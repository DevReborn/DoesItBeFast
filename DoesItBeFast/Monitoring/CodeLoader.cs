using Mono.Cecil;
using Mono.Cecil.Cil;
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
			for (int i = 0; i < body.Instructions.Count; i++)
			{
				var instruction = body.Instructions[i];
				switch (instruction.OpCode.Code)
				{
					case Code.Call:
					case Code.Calli:
					case Code.Callvirt:
						i += WrapAroundMethodCall(monitored, monitorType, il, body, (MethodReference)instruction.Operand, i);
						break;
				}
			}
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
			var method = callingMethod.Method;
			long calledMethodHash = calledMethod.GetGenericHashCode();

			InsertMonitoringBeforeCall(monitorType, il, callingMethod, index, method, calledMethodHash);
			InsertMonitoringAfterCall(monitorType, il, callingMethod, index, method, calledMethodHash);
			Update(callingMethod.Instructions[0]);

			monitoredMethods.TryAdd(calledMethodHash, calledMethod);

			if (_parameters.IncludedAssemblies.Contains(calledMethod.Resolve().Module))
			{
				MonitorMethod(calledMethod.Resolve(), monitorType, monitoredMethods);
			}

			return 12;
		}

		private void InsertMonitoringAfterCall(MonitorTypeDefintion monitorType, ILProcessor il, MethodBody callingMethod, int index, MethodDefinition method, long calledMethodHash)
		{
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.HashAddMethod())));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldc_I8, -calledMethodHash));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.HashField)));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.TimeAddMethod())));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Call, method.Module.ImportReference(_datetimeNow)));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.TimeField)));
		}

		private void InsertMonitoringBeforeCall(MonitorTypeDefintion monitorType, ILProcessor il, MethodBody callingMethod,
			int index, MethodDefinition method, long calledMethodHash)
		{
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.TimeAddMethod())));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Call, method.Module.ImportReference(_datetimeNow)));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.TimeField)));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.HashAddMethod())));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldc_I8, calledMethodHash));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.HashField)));
		}

		//private static void InsertBefore(ILProcessor il, Instruction instruction1, Instruction instruction2)
		//{
		//	il.InsertBefore(instruction1, instruction2);
		//	Update(instruction2);
		//}

		//private static void InsertAfter(ILProcessor il, Instruction instruction1, Instruction instruction2)
		//{
		//	il.InsertAfter(instruction1, instruction2);
		//	Update(instruction2);
		//}
		private static void Update(Instruction instruction)
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
	}
}
