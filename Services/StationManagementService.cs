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

    public void AddStation(double minTunerValue, double maxTunerValue, string url, string? title = null)
    {
        var newStation = new RadioStation
        {
            MinTunerValue = minTunerValue,
            MaxTunerValue = maxTunerValue,
            Url = url,
            Title = title
        };

        _radioStationMap.Stations.Add(newStation);
        SaveStations();
    }

    public void UpdateStation(int index, double minTunerValue, double maxTunerValue, string url, string? title = null)
    {
        if (index >= 0 && index < _radioStationMap.Stations.Count)
        {
            _radioStationMap.Stations[index].MinTunerValue = minTunerValue;
            _radioStationMap.Stations[index].MaxTunerValue = maxTunerValue;
            _radioStationMap.Stations[index].Url = url;
            _radioStationMap.Stations[index].Title = title;
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
