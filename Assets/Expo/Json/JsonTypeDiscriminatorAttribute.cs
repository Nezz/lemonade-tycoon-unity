using System;

namespace Yousician.Expo.Json
{
	/// <summary>
	/// Marks an enum field with the concrete type it maps to for discriminator-based deserialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class JsonTypeDiscriminatorAttribute : Attribute
	{
		public Type Type { get; }

		public JsonTypeDiscriminatorAttribute(Type type)
		{
			Type = type;
		}
	}
}
