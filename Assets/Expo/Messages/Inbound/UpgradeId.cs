using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Yousician.Expo.Messages.Inbound
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum UpgradeId
	{
		[EnumMember(Value = "signCardboardSign")]
		SignCardboardSign,

		[EnumMember(Value = "coolStyrofoamBox")]
		CoolStyrofoamBox,

		[EnumMember(Value = "storExtraCrate")]
		StorExtraCrate,
	}
}
