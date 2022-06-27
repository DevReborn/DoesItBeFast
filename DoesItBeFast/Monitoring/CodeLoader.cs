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
		private readonly MethodDefinition _entryMethod;
		private readonly MonitorTypeDefintion _monitorType;
		private readonly IDictionary<long, MethodReference> _monitoredMethods;

		public CodeLoader(CodeParameters parameters)
		{
			_parameters = parameters;
			_datetimeNow = typeof(DateTime)?.GetProperty("Now")?.GetGetMethod() ?? throw new Exception();
			_entryMethod = _parameters.EntryMethod.Method;
			_monitorType = CreateMonitoringType(_entryMethod.Module);
			_monitoredMethods = new Dictionary<long, MethodReference>();
		}
		public MonitoredCode Load()
		{
			MonitorMethod(_entryMethod);

			var assembly = _entryMethod.Module;
			assembly.Write($"{assembly.Assembly.Name.Name}.updated.dll");
			var targetAssembly = Assembly.LoadFile(Directory.GetCurrentDirectory() + $"/{assembly.Assembly.Name.Name}.updated.dll");
			return new MonitoredCode(targetAssembly, new Dictionary<long, MethodReference>(_monitoredMethods));
		}

		private void MonitorMethod(MethodDefinition method)
		{
			var body = method.Body;
			var il = body.GetILProcessor();
			long methodHash = method.GetGenericHashCode();
			var instructions = body.Instructions;
			var handlers = new List<ExceptionHandler>();

			var extra = InsertMonitoringBeforeCall(il, body, 0, methodHash);

			for (int i = extra; i < instructions.Count; i++)
			{
				var instruction = instructions[i];
				switch (instruction.OpCode.Code)
				{
					case Code.Call:
					case Code.Calli:
					case Code.Callvirt:
					case Code.Ldftn:
						i += WrapAroundMethodCall(il, body, (MethodReference)instruction.Operand, i);
						break;
					case Code.Throw:
						i += InsertMonitoringBeforeThrow(method, body, il, i, handlers);
						break;
					case Code.Ret:
						i += InsertMonitoringAfterCall(il, body, i - 1, methodHash);
						break;
				}
			}

			UpdateExceptionHandlerStart(body.ExceptionHandlers, instructions[0], il);
			UpdateExceptionHandlerEnd(body.ExceptionHandlers, instructions.Last());
			UpdateBreakToMethod(instructions, instructions.Last());
			UpdateShortFormCodes(instructions[0]);
		}

		private void UpdateExceptionHandlerStart(IList<ExceptionHandler> handlers, Instruction instruction, ILProcessor il)
		{
			if (handlers.Count == 0)
				return;

			var current = instruction;
			while(current != null)
			{
				var handler = handlers.SingleOrDefault(x => x.HandlerStart.Equals(current));
				if (handler != null)
				{
					InsertMonitoringAtHandler(handler, il);
				}
				current = current.Next;
			}
		}

		private int InsertMonitoringAtHandler(ExceptionHandler handler, ILProcessor il)
		{
			var newStart = il.Create(OpCodes.Ldsfld, _monitorType.HashField);
			var start = handler.HandlerStart;
			il.InsertBefore(start, newStart);
			il.InsertBefore(start, il.Create(OpCodes.Dup));
			il.InsertBefore(start, il.Create(OpCodes.Callvirt, _monitorType.HashCountMethod()));
			il.InsertBefore(start, il.Create(OpCodes.Ldc_I4_M1));
			il.InsertBefore(start, il.Create(OpCodes.Add));
			il.InsertBefore(start, il.Create(OpCodes.Callvirt, _monitorType.HashRemoveAtMethod()));

			il.InsertBefore(start, il.Create(OpCodes.Ldsfld, _monitorType.TimeField));
			il.InsertBefore(start, il.Create(OpCodes.Dup));
			il.InsertBefore(start, il.Create(OpCodes.Callvirt, _monitorType.TimeCountMethod()));
			il.InsertBefore(start, il.Create(OpCodes.Ldc_I4_M1));
			il.InsertBefore(start, il.Create(OpCodes.Add));
			il.InsertBefore(start, il.Create(OpCodes.Callvirt, _monitorType.TimeRemoveAtMethod()));
			handler.HandlerStart = newStart;
			handler.TryEnd = newStart;
			return 12;
		}

		private int InsertMonitoringBeforeThrow(MethodDefinition method, MethodBody body, 
			ILProcessor il, int i, List<ExceptionHandler> handlers)
		{
			var handler = body.ExceptionHandlers.SingleOrDefault(x => IsThrowInsideHandler(x, body.Instructions[i]));
			if (handler != null)
			{
				handlers.Add(handler);
			} 
			return InsertMonitoringAfterCall(il, body, i - 1, method.GetGenericHashCode());
		}

		private bool IsThrowInsideHandler(ExceptionHandler handler, Instruction instruction)
		{
			var current = handler.TryStart;
			while(current != null && current != handler.TryEnd)
			{
				if (current == instruction)
					return true;
				current = current.Next;
			}
			return false;
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

		private int WrapAroundMethodCall(ILProcessor il,
			MethodBody callingMethod,
			MethodReference calledMethod,
			int index)
		{
			long calledMethodHash = calledMethod.GetGenericHashCode();

			if(MethodShouldBeMonitored(calledMethod))
			{
				// Wrap around methods that we don't own
				if (!MethodCanBeEdited(calledMethod))
				{
					var before = InsertMonitoringBeforeCall(il, callingMethod, index, calledMethodHash);
					var after = InsertMonitoringAfterCall(il, callingMethod, index + before, calledMethodHash);
					UpdateBreakToMethod(callingMethod.Instructions, callingMethod.Instructions[index + before]);
					UpdateShortFormCodes(callingMethod.Instructions[0]);

					_monitoredMethods.TryAdd(calledMethodHash, calledMethod);

					return before + after;
				}
				// Only go inside included methods that we havene't visited yet.
				else if (_monitoredMethods.TryAdd(calledMethodHash, calledMethod))
				{
					MonitorMethod(calledMethod.Resolve());
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

		private int InsertMonitoringAfterCall(ILProcessor il, MethodBody callingMethod, 
			int index, long calledMethodHash)
		{
			il.InsertAfter(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, _monitorType.HashAddMethod()));
			il.InsertAfter(callingMethod.Instructions[index], il.Create(OpCodes.Ldc_I8, -calledMethodHash));
			il.InsertAfter(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, _monitorType.HashField));
			il.InsertAfter(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, _monitorType.TimeAddMethod()));
			il.InsertAfter(callingMethod.Instructions[index], il.Create(OpCodes.Call, _entryMethod.Module.ImportReference(_datetimeNow)));
			il.InsertAfter(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, _monitorType.TimeField));
			return 6;
		}

		private int InsertMonitoringBeforeCall(ILProcessor il, MethodBody callingMethod,
			int index, long calledMethodHash)
		{
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, _monitorType.TimeAddMethod()));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Call, _entryMethod.Module.ImportReference(_datetimeNow)));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, _monitorType.TimeField));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, _monitorType.HashAddMethod()));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldc_I8, calledMethodHash));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, _monitorType.HashField));
			return 6;
		}

		private static void UpdateShortFormCodes(Instruction instruction)
		{
			var inst = instruction;
			while (inst != null)
			{
				inst.Offset = inst.Previous == null ? 0 : (inst.Previous.Offset + inst.Previous.GetSize());
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
					case Code.Leave_S:
						ConvertShortForm(inst, OpCodes.Leave); break;
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

		private static void UpdateBreakToMethod(Collection<Instruction> instructions, Instruction instructionToBreakTo)
		{
			foreach(var inst in instructions)
			{
				switch (inst.OpCode.Code)
				{
					// Other break methods
					case Code.Jmp:
					case Code.Switch:
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
						// Not sure if i have to do extra due to exception handling?
					case Code.Leave:
					case Code.Leave_S:
						if (inst.Operand.Equals(instructionToBreakTo))
							inst.Operand = instructionToBreakTo.Previous.Previous.Previous.Previous.Previous.Previous;
						if (inst.Operand is Instruction instruction && instruction.OpCode.Code == Code.Throw)
							throw new NotImplementedException();
							//inst.Operand = instruction.Previous.Previous.Previous.Previous.Previous.Previous;
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
					case Code.Ldnull:
					case Code.Ldc_I4_0:
					case Code.Ldc_I4_1:
					case Code.Ldc_I4_2:
					case Code.Ldc_I4_3:
					case Code.Ldc_I4_4:
					case Code.Ldc_I4_5:
					case Code.Ldc_I4_6:
					case Code.Ldc_I4_7:
					case Code.Ldc_I4_8:
					case Code.Ldc_I4_M1:
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
					case Code.Conv_I8:
					case Code.Conv_U2:
					case Code.Callvirt:
					case Code.Call:
					case Code.Add:
					case Code.Rem:
					case Code.Cgt_Un:
					case Code.Ceq:
					case Code.Ret:
					case Code.Dup:
					case Code.Pop:
						break;

					// unsure
					case Code.Ldarga_S:
					case Code.Ldloca_S:
					case Code.Ldftn:
					case Code.Throw:
						break;
					default:
						throw new NotImplementedException("Unsupported Opcode: " + inst.OpCode.Code);
				}
			}
		}

		private static void UpdateExceptionHandlerEnd(Collection<ExceptionHandler> exceptionHandlers, Instruction instructionAfterEx)
		{
			foreach (var handler in exceptionHandlers)
			{
				switch (handler.HandlerType)
				{
					case ExceptionHandlerType.Catch:
						if (handler.HandlerEnd.Equals(instructionAfterEx))
							handler.HandlerEnd = instructionAfterEx.Previous.Previous.Previous.Previous.Previous.Previous;
						break;
					case ExceptionHandlerType.Filter:
					case ExceptionHandlerType.Finally:
					case ExceptionHandlerType.Fault:
						throw new NotImplementedException();
				}
			}
		}
	}
}
