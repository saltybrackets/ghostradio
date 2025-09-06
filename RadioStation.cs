using System.Text.Json.Serialization;

namespace GhostRadio.Models;

public class RadioStation
{
    [JsonPropertyName("min")]
    public double Min { get; set; }
    
    [JsonPropertyName("max")]
    public double Max { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class StationMap
{
    [JsonPropertyName("stations")]
    public List<RadioStation> Stations { get; set; } = new();
}