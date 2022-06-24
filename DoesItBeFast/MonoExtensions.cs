using Mono.Cecil;
using System.Reflection;

namespace DoesItBeFast
{
	public static class TypeExtensions
	{
		public static string ToNetName(this TypeReference type)
		{
			var elementType = type.GetElementType();
			if (elementType != null && type is GenericInstanceType genericInstance)
			{
				string generics = string.Join(", ", genericInstance.GenericArguments.Select(ToNetName));
				return $"{elementType}[{generics}]";
			}

			return type.ToString() == "System.Void" ? "Void" : type.ToString();
		}
		public static bool IsEqual(this MethodReference monoMethod, MethodInfo method)
		{
			return method.ToString() == 
				$"{monoMethod.ReturnType.ToNetName()} {monoMethod.Name}({string.Join(", ", monoMethod.Parameters.Select(x => x.ParameterType.ToNetName()))})";
		}
		public static bool IsEqual(this TypeReference monoType, Type type)
		{
			return type.ToString() == monoType.ToNetName();
		}
		public static long GetGenericHashCode(this MethodReference monoMethod)
		{
			return ((long)monoMethod.FullName.GetHashCode()) + int.MaxValue; // Ensure always positive
		}
	}
}
