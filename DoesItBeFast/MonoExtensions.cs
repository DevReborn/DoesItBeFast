using Mono.Cecil;
using System.Reflection;

namespace DoesItBeFast
{
	public static class TypeExtensions
	{
		public static bool IsEqual(this MethodReference monoMethod, MethodInfo method)
		{
			if (!monoMethod.Name.Equals(method.Name))
				return false;
			if (!monoMethod.ReturnType.IsEqual(method.ReturnType))
				return false;
			if (!monoMethod.DeclaringType.IsEqual(method.DeclaringType))
				return false;
			var parameters = method.GetParameters();
			var monoParameters = monoMethod.Parameters;
			if (parameters.Length != monoParameters.Count)
				return false;

			for (int i = 0; i < parameters.Length; i++)
			{
				if (!monoParameters[i].ParameterType.IsEqual(parameters[i].ParameterType))
					return false;
			}

			return true;
		}

		public static bool IsEqual(this TypeReference monoType, Type type)
		{
			if (!type.Name.Equals(monoType.Name))
				return false;
			if (!type.Namespace.Equals(monoType.Namespace))
				return false;

			var genericArgs = type.GetGenericArguments();
			var monoGenericArgs = monoType is GenericInstanceType monoGenericType ? monoGenericType.GenericArguments.ToArray() : Array.Empty<TypeReference>();
			if (genericArgs.Length != monoGenericArgs.Length)
				return false;

			for (int i = 0; i < genericArgs.Length; i++)
			{
				if (!monoGenericArgs[i].IsEqual(genericArgs[i]))
					return false;
			}

			if (type.IsNested || monoType.IsNested)
				throw new NotImplementedException();

			if (!(type.IsNested == monoType.IsNested))
				return false;

			return true;
		}

		public static long GetGenericHashCode(this MethodReference monoMethod)
		{
			return ((long)monoMethod.FullName.GetHashCode()) + int.MaxValue; // Ensure always positive
		}
	}
}
