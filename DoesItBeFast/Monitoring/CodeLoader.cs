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
						i += InjectMonitoringCode(monitored, monitorType, il, body, (MethodReference)instruction.Operand, i);
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

		private int InjectMonitoringCode(IDictionary<long, MethodReference> monitoredMethods,
			MonitorTypeDefintion monitorType,
			ILProcessor il,
			MethodBody callingMethod,
			MethodReference calledMethod,
			int index)
		{
			if (_parameters.IncludedAssemblies != null
				&& !_parameters.IncludedAssemblies.Contains(calledMethod.Resolve().Module))
				return 0;

			var method = callingMethod.Method;
			long calledMethodHash = calledMethod.GetGenericHashCode();

			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.TimeAddMethod())));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Call, method.Module.ImportReference(_datetimeNow)));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.TimeField)));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.HashAddMethod())));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldc_I8, calledMethodHash));
			il.InsertBefore(callingMethod.Instructions[index], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.HashField)));

			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.HashAddMethod())));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldc_I8, -calledMethodHash));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.HashField)));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Callvirt, method.Module.ImportReference(monitorType.TimeAddMethod())));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Call, method.Module.ImportReference(_datetimeNow)));
			il.InsertAfter(callingMethod.Instructions[index + 6], il.Create(OpCodes.Ldsfld, method.Module.ImportReference(monitorType.TimeField)));

			monitoredMethods.TryAdd(calledMethodHash, calledMethod);

			MonitorMethod(calledMethod.Resolve(), monitorType, monitoredMethods);

			return 12;
		}
	}
}
