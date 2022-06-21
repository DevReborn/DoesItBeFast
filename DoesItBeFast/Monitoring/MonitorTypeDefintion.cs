using Mono.Cecil;

namespace DoesItBeFast.Monitoring
{
	public class MonitorTypeDefintion
	{
		private MethodReference? _hashAddMethod;
		private MethodReference? _timeAddMethod;

		public MonitorTypeDefintion(TypeDefinition monitorType, FieldDefinition hashField, FieldDefinition timeField)
		{
			MonitorType = monitorType;
			HashField = hashField;
			TimeField = timeField;
		}

		public TypeDefinition MonitorType;
		public FieldDefinition HashField;
		public FieldDefinition TimeField;

		public MethodReference HashAddMethod()
		{
			if (_hashAddMethod != null)
				return _hashAddMethod;

			var methodReference = MonitorType.Module.ImportReference(typeof(List<long>).GetMethod("Add"));
			return _hashAddMethod = methodReference;
		}
		public MethodReference TimeAddMethod()
		{
			if (_timeAddMethod != null)
				return _timeAddMethod;

			var methodReference = MonitorType.Module.ImportReference(typeof(List<DateTime>).GetMethod("Add"));
			return _timeAddMethod = methodReference;
		}
	}
}
