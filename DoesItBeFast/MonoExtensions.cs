using Mono.Cecil;
using System.Reflection;

namespace DoesItBeFast
{
	public static class TypeExtensions
	{
		public static bool IsEqual(this MethodReference monoMethod, MethodInfo method)
		{
			return method.ToString() == $"{monoMethod.ReturnType} {monoMethod.Name}({string.Join(", ", monoMethod.Parameters.Select(x => x.ParameterType.ToString()))})";
		}
		public static bool IsEqual(this TypeReference monoType, Type type)
		{
			return type.FullName == monoType.FullName;
		}
		public static long GetGenericHashCode(this MethodReference monoMethod)
		{
			return ((long)monoMethod.FullName.GetHashCode()) + int.MaxValue; // Ensure always positive
		}
	}
}
