using System.Text.Json;

namespace GhostRadio.Services;

public class StationManagementService
{
    private readonly string _stationFilePath;
    private RadioStationMap _radioStationMap;

    public StationManagementService(string stationFilePath, RadioStationMap radioStationMap)
    {
        _stationFilePath = stationFilePath;
        _radioStationMap = radioStationMap;
    }

    public IReadOnlyList<RadioStation> GetAllStations()
    {
        return _radioStationMap.GetAllStations();
    }

    public void ReloadStations()
    {
        try
        {
            // Load new data from disk
            var newMap = RadioStationMap.Load(_stationFilePath);

            // Update the existing singleton instance in-place
            // This ensures GhostRadioController sees the updated data
            _radioStationMap.Stations.Clear();
            foreach (var station in newMap.Stations)
            {
                _radioStationMap.Stations.Add(station);
            }

            Console.WriteLine($"Station data reloaded from disk. {_radioStationMap.Stations.Count} stations loaded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reloading stations: {ex.Message}");
            throw;
        }
    }

    public void AddStation(double minTunerValue, double maxTunerValue, string url)
    {
        var newStation = new RadioStation
        {
            MinTunerValue = minTunerValue,
            MaxTunerValue = maxTunerValue,
            Url = url
        };

        _radioStationMap.Stations.Add(newStation);
        SaveStations();
    }

    public void UpdateStation(int index, double minTunerValue, double maxTunerValue, string url)
    {
        if (index >= 0 && index < _radioStationMap.Stations.Count)
        {
            _radioStationMap.Stations[index].MinTunerValue = minTunerValue;
            _radioStationMap.Stations[index].MaxTunerValue = maxTunerValue;
            _radioStationMap.Stations[index].Url = url;
            SaveStations();
        }
    }

    public void DeleteStation(int index)
    {
        if (index >= 0 && index < _radioStationMap.Stations.Count)
        {
            _radioStationMap.Stations.RemoveAt(index);
            SaveStations();
        }
    }

    public void MoveStation(int fromIndex, int toIndex)
    {
        if (fromIndex >= 0 && fromIndex < _radioStationMap.Stations.Count &&
            toIndex >= 0 && toIndex < _radioStationMap.Stations.Count &&
            fromIndex != toIndex)
        {
            var station = _radioStationMap.Stations[fromIndex];
            _radioStationMap.Stations.RemoveAt(fromIndex);
            _radioStationMap.Stations.Insert(toIndex, station);
            SaveStations();
        }
    }

    private void SaveStations()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(_radioStationMap, GhostRadioJsonContext.Default.RadioStationMap);
            File.WriteAllText(_stationFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving stations: {ex.Message}");
            throw;
        }
    }
}
