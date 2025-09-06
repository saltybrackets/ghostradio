using System.Text.Json;
using System.Text.Json.Serialization;

namespace GhostRadio;

public class RadioStationMap
{
    [JsonPropertyName("stations")]
    public List<RadioStation> Stations { get; } = [];
    
    /// <summary>
    /// Return the URL mapped to a given tuner value.
    /// </summary>
    /// <param name="tunerValue">Value of tuner to get associated radio station URL.</param>
    /// <returns>URL if tuner value maps to a station, or empty if nothing is mapped.</returns>
    public string GetStationUrl(double tunerValue)
    {
        foreach (RadioStation station in Stations)
        {
            if (tunerValue >= station.MinTunerValue
                && tunerValue <= station.MaxTunerValue)
            {
                return station.Url;
            }
        }
        
        return string.Empty;
    }
    
    public RadioStation? GetStation(double tunerValue)
    {
        return Stations.FirstOrDefault(radioStation => 
            tunerValue >= radioStation.MinTunerValue 
            && tunerValue <= radioStation.MaxTunerValue);
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
            RadioStationMap? stationMap = JsonSerializer.Deserialize<RadioStationMap>(jsonContent);
            
            return stationMap ?? new RadioStationMap();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading station map: {ex.Message}");
            return new RadioStationMap();
        }
    }
}