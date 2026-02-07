using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Yousician.Expo.Messages.Inbound
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CameraView
	{
		[EnumMember(Value = "day")]
		Day,

		[EnumMember(Value = "simulation")]
		Simulation,
	}
}
