using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Yousician.Expo.Json
{
	/// <summary>
	/// A JsonConverter that reads a discriminator field from the JSON object and deserializes
	/// to the correct concrete subtype of <typeparamref name="TBase"/>.
	/// <para>
	/// The mapping from discriminator value to concrete type is driven by
	/// <see cref="JsonTypeDiscriminatorAttribute"/> on the fields of <typeparamref name="TEnum"/>.
	/// </para>
	/// </summary>
	public sealed class DiscriminatorJsonConverter<TBase, TEnum> : JsonConverter<TBase>
		where TEnum : struct, Enum
	{
		// Thread-static flags prevent infinite recursion when the serializer re-enters
		// this converter while serializing/deserializing the concrete subtype
		// (which inherits the [JsonConverter] attribute from TBase).
		// Each closed generic type gets its own static fields, so flags for
		// InboundMessage and OutboundMessage are independent.
		[ThreadStatic] private static bool s_isReading;
		[ThreadStatic] private static bool s_isWriting;

		private readonly string _discriminatorField;
		private readonly Dictionary<TEnum, Type> _enumToType;
		private readonly Dictionary<Type, TEnum> _typeToEnum;
		private readonly StringEnumConverter _enumConverter = new StringEnumConverter();

		public override bool CanRead => !s_isReading;
		public override bool CanWrite => !s_isWriting;

		public DiscriminatorJsonConverter(string discriminatorField)
		{
			_discriminatorField = discriminatorField;
			_enumToType = new Dictionary<TEnum, Type>();
			_typeToEnum = new Dictionary<Type, TEnum>();
			BuildTypeMaps();
		}

		private void BuildTypeMaps()
		{
			foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				var attr = field.GetCustomAttribute<JsonTypeDiscriminatorAttribute>();
				if (attr == null) continue;

				var enumValue = (TEnum)field.GetValue(null);
				_enumToType[enumValue] = attr.Type;
				_typeToEnum[attr.Type] = enumValue;
			}
		}

		public override TBase ReadJson(
			JsonReader reader,
			Type objectType,
			TBase existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return default;

			var jObject = JObject.Load(reader);

			var discriminatorToken = jObject[_discriminatorField];
			if (discriminatorToken == null)
				throw new JsonSerializationException(
					$"Missing discriminator field '{_discriminatorField}' in JSON.");

			var enumValue = discriminatorToken.ToObject<TEnum>(
				JsonSerializer.Create(new JsonSerializerSettings
				{
					Converters = { _enumConverter }
				}));

			if (!_enumToType.TryGetValue(enumValue, out var targetType))
				throw new JsonSerializationException(
					$"Unknown discriminator value '{discriminatorToken}' for type {typeof(TBase).Name}.");

			s_isReading = true;
			try
			{
				using var subReader = jObject.CreateReader();
				return (TBase)serializer.Deserialize(subReader, targetType);
			}
			finally
			{
				s_isReading = false;
			}
		}

		public override void WriteJson(JsonWriter writer, TBase value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			var type = value.GetType();
			if (!_typeToEnum.TryGetValue(type, out var enumValue))
				throw new JsonSerializationException(
					$"No discriminator mapping for type {type.Name}.");

			s_isWriting = true;
			try
			{
				// Serialize the concrete object (CanWrite returns false, so default
				// serialization is used and this converter is not re-entered).
				var jObject = JObject.FromObject(value, serializer);

				// Insert the discriminator field at the top
				var enumString = JsonConvert.SerializeObject(enumValue, _enumConverter)
					.Trim('"');
				jObject.AddFirst(new JProperty(_discriminatorField, enumString));

				jObject.WriteTo(writer);
			}
			finally
			{
				s_isWriting = false;
			}
		}
	}
}
