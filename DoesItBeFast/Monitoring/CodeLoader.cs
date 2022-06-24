using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Reflection;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace DoesItBeFast.Monitoring
{
	public class CodeLoader
	{
		private readonly CodeParameters _parameters;
		private readonly MethodInfo _datetimeNow;

		public CodeLoader(CodeParameters parameters)
		{
			_parameters = parameters;
			_datetimeNow = typeof(DateTime)?.GetProperty("Now")?.GetGetMethod() ?? throw new Exception();
		}
		public MonitoredCode Load()
		{
			var method = _parameters.EntryMethod.Method;
			var monitorType = CreateMonitoringType(method.Module);
			var monitoredMethods = new Dictionary<long, MethodReference>();

			MonitorMethod(method, monitorType, monitoredMethods);

			var assembly = method.Module;
			assembly.Write($"{assembly.Assembly.Name.Name}.updated.dll");
			var targetAssembly = Assembly.LoadFile(Directory.GetCurrentDirectory() + $"/{assembly.Assembly.Name.Name}.updated.dll");
			return new MonitoredCode(targetAssembly, monitoredMethods);
		}

		private void MonitorMethod(MethodDefinition method, MonitorTypeDefintion monitorType, IDictionary<long, MethodReference> monitored)
		{
			var body = method.Body;
			var il = body.GetILProcessor();

			InsertMonitoringBeforeCall(monitorType, il, body, 0, method, method.GetGenericHashCode());

			for (int i = 6; i < body.Instructions.Count; i++)
			{
				var instruction = body.Instructions[i];
				switch (instruction.OpCode.Code)
				{
					case Code.Call:
					case Code.Calli:
					case Code.Callvirt:
					case Code.Ldftn:
						i += WrapAroundMethodCall(monitored, monitorType, il, body, (MethodReference)instruction.Operand, i);
						break;
				}
			}

			InsertMonitoringAfterCall(monitorType, il, body, body.Instructions.Count - 1 - 6 - 1, method, method.GetGenericHashCode());
			UpdateBreakToMethod(body.Instructions, body.Instructions.Last());
			UpdateShortFormCodes(body.Instructions[0]);
		}

		private static MonitorTypeDefintion CreateMonitoringType(ModuleDefinition module)
		{
			var monitorType = new TypeDefinition("DontUseThisNamespaceOtherwiseBadThingsHappenStupid", "Monitor",
				TypeAttributes.Public | TypeAttributes.SpecialName, module.ImportReference(typeof(object)));
			var hashField = new FieldDefinition("_hash", FieldAttributes.Public | FieldAttributes.Static, module.ImportReference(typeof(List<long>)));
			var timeField = new FieldDefinition("_time", FieldAttributes.Public | FieldAttributes.Static, module.ImportReference(typeof(List<DateTime>)));

			monitorType.Fields.Add(hashField);
			monitorType.Fields.Add(timeField);

			module.Types.Add(monitorType);

			return new MonitorTypeDefintion(monitorType, hashField, timeField);
		}

		private int WrapAroundMethodCall(IDictionary<long, MethodReference> monitoredMethods,
			MonitorTypeDefintion monitorType,
			ILProcessor il,
			MethodBody callingMethod,
			MethodReference calledMethod,
			int index)
		{
			long calledMethodHash = calledMethod.GetGenericHashCode();
			var method = callingMethod.Method;

			if(MethodShouldBeMonitored(calledMethod))
			{
				// Wrap around methods that we don't own
				if (!MethodCanBeEdited(calledMethod))
				{
					InsertMonitoringBeforeCall(monitorType, il, callingMethod, index, method, calledMethodHash);
					InsertMonitoringAfterCall(monitorType, il, callingMethod, index, method, calledMethodHash);
					UpdateBreakToMethod(callingMethod.Instructions, callingMethod.Instructions[index + 6]);
					UpdateShortFormCodes(callingMethod.Instructions[0]);

					monitoredMethods.TryAdd(calledMethodHash, calledMethod);

					return 12;
				}
				// Only go inside included methods that we havene't visited yet.
				else if (monitoredMethods.TryAdd(calledMethodHash, calledMethod))
				{
					MonitorMethod(calledMethod.Resolve(), monitorType, monitoredMethods);
				}
			}
			return 0;
		}

		private bool MethodShouldBeMonitored(MethodReference calledMethod)
		{
			return !calledMethod.DeclaringType.FullName.Equals("<PrivateImplementationDetails>");
		}

		private bool MethodCanBeEdited(MethodReference calledMethod)
		{
			return _parameters.EditableAssemblies.Contains(calledMethod.Resolve().Module)
				&& !calledMethod.DeclaringType.FullName.Equals("<PrivateImplementationDetails>");
		}

		private void InsertMonitoringAfterCall(MonitorTypeDefintion monitorType, ILProcessor il, MethodBody callingMethod, 
			int index, MethodDefinition method, long calledMethodHash)
		{
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Callvirt, monitorType.HashAddMethod()));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldc_I8, -calledMethodHash));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldsfld, monitorType.HashField));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Callvirt, monitorType.TimeAddMethod()));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Call, method.Module.ImportReference(_datetimeNow)));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldsfld, monitorType.TimeField));
		}

		private void InsertMonitoringBeforeCall(MonitorTypeDefintion monitorType, ILProcessor il, MethodBody callingMethod,
			int index, MethodDefinition method, long calledMethodHash)
		{
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, monitorType.TimeAddMethod()));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Call, method.Module.ImportReference(_datetimeNow)));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, monitorType.TimeField));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, monitorType.HashAddMethod()));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldc_I8, calledMethodHash));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, monitorType.HashField));
		}

		private static void UpdateShortFormCodes(Instruction instruction)
		{
			var inst = instruction;
			while (inst != null)
			{
				inst.Offset = inst.Previous == null ? 0: (inst.Previous.Offset + inst.Previous.GetSize());
				switch (inst.OpCode.Code)
				{
					case Code.Br_S:
						ConvertShortForm(inst, OpCodes.Br); break;
					case Code.Brfalse_S:
						ConvertShortForm(inst, OpCodes.Brfalse); break;
					case Code.Brtrue_S:
						ConvertShortForm(inst, OpCodes.Brtrue); break;
					case Code.Beq_S:
						ConvertShortForm(inst, OpCodes.Beq); break;
					case Code.Bge_S:
						ConvertShortForm(inst, OpCodes.Bge); break;
					case Code.Bgt_S:
						ConvertShortForm(inst, OpCodes.Bgt); break;
					case Code.Ble_S:
						ConvertShortForm(inst, OpCodes.Ble); break;
					case Code.Blt_S:
						ConvertShortForm(inst, OpCodes.Blt); break;
					case Code.Bne_Un_S:
						ConvertShortForm(inst, OpCodes.Bne_Un); break;
					case Code.Bge_Un_S:
						ConvertShortForm(inst, OpCodes.Bge_Un); break;
					case Code.Bgt_Un_S:
						ConvertShortForm(inst, OpCodes.Bgt_Un); break;
					case Code.Ble_Un_S:
						ConvertShortForm(inst, OpCodes.Ble_Un); break;
					case Code.Blt_Un_S:
						ConvertShortForm(inst, OpCodes.Blt_Un); break;
				}
				inst = inst.Next;
			}
		}
		private static void ConvertShortForm(Instruction inst, OpCode newCode)
		{
			var destination = ((Instruction)inst.Operand).Offset;
			var source = inst.Offset;
			var size = destination - (source + inst.GetSize() + 1);
			if (size > 127 || size < -128)
			{
				inst.OpCode = newCode;
			}
		}

		private void UpdateBreakToMethod(Collection<Instruction> instructions, Instruction calledMethod)
		{
			foreach(var inst in instructions)
			{
				switch (inst.OpCode.Code)
				{
					// Other break methods
					case Code.Jmp:
					case Code.Switch:
					case Code.Leave:
					case Code.Leave_S:
						throw new NotImplementedException("Unsupported Opcode: " + inst.OpCode.Code);

					// Supported breaks
					case Code.Br_S:
					case Code.Brfalse_S:
					case Code.Brtrue_S:
					case Code.Beq_S:
					case Code.Bge_S:
					case Code.Bgt_S:
					case Code.Ble_S:
					case Code.Blt_S:
					case Code.Bne_Un_S:
					case Code.Bge_Un_S:
					case Code.Bgt_Un_S:
					case Code.Ble_Un_S:
					case Code.Blt_Un_S:
					case Code.Br:
					case Code.Brfalse:
					case Code.Brtrue:
					case Code.Beq:
					case Code.Bge:
					case Code.Bgt:
					case Code.Ble:
					case Code.Blt:
					case Code.Bne_Un:
					case Code.Bge_Un:
					case Code.Bgt_Un:
					case Code.Ble_Un:
					case Code.Blt_Un:
						if (inst.Operand.Equals(calledMethod))
							inst.Operand = calledMethod.Previous.Previous.Previous.Previous.Previous.Previous;
						break;

					// supported
					case Code.Nop:
					case Code.Newobj:
					case Code.Stloc_0:
					case Code.Stloc_1:
					case Code.Stloc_2:
					case Code.Stloc_3:
					case Code.Stloc_S:
					case Code.Ldarg_0:
					case Code.Ldarg_1:
					case Code.Ldarg_2:
					case Code.Ldarg_3:
					case Code.Ldc_I4_0:
					case Code.Ldc_I4_1:
					case Code.Ldc_I4_2:
					case Code.Ldc_I4_3:
					case Code.Ldc_I4_4:
					case Code.Ldc_I4_5:
					case Code.Ldc_I4_6:
					case Code.Ldc_I4_7:
					case Code.Ldc_I4_8:
					case Code.Ldc_I4_S:
					case Code.Ldc_I4:
					case Code.Ldloc_0:
					case Code.Ldloc_1:
					case Code.Ldloc_2:
					case Code.Ldloc_3:
					case Code.Ldloc_S:
					case Code.Ldstr:
					case Code.Ldsfld:
					case Code.Stsfld:
					case Code.Ldc_I8:
					case Code.Callvirt:
					case Code.Call:
					case Code.Add:
					case Code.Ceq:
					case Code.Ret:
					case Code.Dup:
					case Code.Pop:
						break;

					// unsure
					case Code.Ldarga_S:
					case Code.Ldftn:
						break;
					default:
						throw new NotImplementedException("Unsupported Opcode: " + inst.OpCode.Code);
				}
			}
		}

	}
}
