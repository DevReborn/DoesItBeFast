using Mono.Cecil;
using System.Reflection;

namespace DoesItBeFast.Monitoring
{
	public class MonitoredCode
	{
		public MonitoredCode(Assembly targetAssembly, IDictionary<long, MethodReference> monitoredMethods)
		{
			TargetAssembly = targetAssembly;
			MonitoredMethods = monitoredMethods;
		}

		public Assembly TargetAssembly { get; }
		public IDictionary<long, MethodReference> MonitoredMethods { get; }
	}
}
