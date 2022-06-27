using Mono.Cecil;

namespace DoesItBeFast.Monitoring
{
	public class MonitorTypeDefintion
	{
		private MethodReference? _hashAddMethod;
		private MethodReference? _hashCountMethod;
		private MethodReference? _hashRemoveAtMethod;
		private MethodReference? _timeAddMethod;
		private MethodReference? _timeCountMethod;
		private MethodReference? _timeRemoveAtMethod;

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
		public MethodReference HashCountMethod()
		{
			if (_hashCountMethod != null)
				return _hashCountMethod;

			var methodReference = MonitorType.Module.ImportReference(typeof(List<long>).GetProperty("Count").GetGetMethod());
			return _hashCountMethod = methodReference;
		}
		public MethodReference HashRemoveAtMethod()
		{
			if (_hashRemoveAtMethod != null)
				return _hashRemoveAtMethod;

			var methodReference = MonitorType.Module.ImportReference(typeof(List<long>).GetMethod("RemoveAt"));
			return _hashRemoveAtMethod = methodReference;
		}
		public MethodReference TimeCountMethod()
		{
			if (_timeCountMethod != null)
				return _timeCountMethod;

			var methodReference = MonitorType.Module.ImportReference(typeof(List<DateTime>).GetProperty("Count").GetGetMethod());
			return _timeCountMethod = methodReference;
		}
		public MethodReference TimeRemoveAtMethod()
		{
			if (_timeRemoveAtMethod != null)
				return _timeRemoveAtMethod;

			var methodReference = MonitorType.Module.ImportReference(typeof(List<DateTime>).GetMethod("RemoveAt"));
			return _timeRemoveAtMethod = methodReference;
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
