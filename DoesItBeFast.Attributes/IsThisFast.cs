namespace DoesItBeFast.Attributes
{
	[AttributeUsage(AttributeTargets.Constructor 
		| AttributeTargets.Method 
		| AttributeTargets.Property, AllowMultiple = false, 
		Inherited = false)]
	public class IsThisFastAttribute : Attribute
	{
	}
}
