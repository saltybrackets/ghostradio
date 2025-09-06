using System.Text.Json;
using GhostRadio.Models;

namespace GhostRadio.Services;

public class StationService
{
    private readonly List<RadioStation> _stations;

    public StationService(string stationFilePath = "stations.json")
    {
        _stations = LoadStationMap(stationFilePath);
    }

    private static List<RadioStation> LoadStationMap(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Station file not found: {filePath}");
                return new List<RadioStation>();
            }

            var jsonContent = File.ReadAllText(filePath);
            var stationMap = JsonSerializer.Deserialize<StationMap>(jsonContent);
            
            return stationMap?.Stations ?? new List<RadioStation>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading station map: {ex.Message}");
            return new List<RadioStation>();
        }
    }

    public string? GetStationUrl(double tunerValue)
    {
        foreach (var station in _stations)
        {
            if (tunerValue >= station.Min && tunerValue <= station.Max)
            {
                return station.Url;
            }
        }
        
        return null; // No station found for this tuner value
    }

    public RadioStation? GetStation(double tunerValue)
    {
        return _stations.FirstOrDefault(s => tunerValue >= s.Min && tunerValue <= s.Max);
    }

    public IReadOnlyList<RadioStation> GetAllStations() => _stations.AsReadOnly();
}