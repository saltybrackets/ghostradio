using System.Text.Json.Serialization;

namespace GhostRadio;

public class RadioStation
{
    [JsonPropertyName("min")]
    public double MinTunerValue { get; set; }
    
    [JsonPropertyName("max")]
    public double MaxTunerValue { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}