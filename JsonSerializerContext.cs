using System.Text.Json.Serialization;

namespace GhostRadio;

[JsonSerializable(typeof(RadioStationMap))]
[JsonSerializable(typeof(RadioStation))]
[JsonSerializable(typeof(List<RadioStation>))]
public partial class GhostRadioJsonContext : JsonSerializerContext
{
}