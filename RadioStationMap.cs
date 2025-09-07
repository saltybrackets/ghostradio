using System.Text.Json;
using System.Text.Json.Serialization;

namespace GhostRadio;

public class RadioStationMap
{
    [JsonPropertyName("stations")]
    public List<RadioStation> Stations { get; set; } = [];
    
    /// <summary>
    /// Return the RadioStation object mapped to a given tuner value.
    /// </summary>
    /// <param name="tunerValue">Value of tuner to get associated radio station URL.</param>
    /// <returns>RadioStation mapped to the tuner value, or null if nothing is mapped.</returns>
    public RadioStation? GetStation(double tunerValue)
    {
        foreach (var radioStation in Stations)
        {
            if (tunerValue >= radioStation.MinTunerValue
                && tunerValue <= radioStation.MaxTunerValue)
            {
                return radioStation;
            }
        }

        return null;
    }
    
    /// <summary>
    /// Return the URL mapped to a given tuner value.
    /// </summary>
    /// <param name="tunerValue">Value of tuner to get associated radio station URL.</param>
    /// <returns>URL if tuner value maps to a station, or empty if nothing is mapped.</returns>
    public string GetStationUrl(double tunerValue)
    {
        return GetStation(tunerValue)?.Url ?? string.Empty;
    }
    
    public IReadOnlyList<RadioStation> GetAllStations() => Stations.AsReadOnly();
    
    /// <summary>
    /// Deserialize a RadioStationMap from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to .json file.</param>
    /// <returns>A deserialized RadioStationMap.</returns>
    public static RadioStationMap Load(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Station file not found: {filePath}");
                return new RadioStationMap();
            }

            string jsonContent = File.ReadAllText(filePath);
            RadioStationMap? stationMap = JsonSerializer.Deserialize<RadioStationMap>(jsonContent, GhostRadioJsonContext.Default.RadioStationMap);
            
            return stationMap ?? new RadioStationMap();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading station map: {ex.Message}");
            return new RadioStationMap();
        }
    }
}