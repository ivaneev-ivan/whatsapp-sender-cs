#nullable enable
using System.Text.Json;

namespace whatsapp_sender;

internal static class ConfigManager
{

    public static TimeDelay ReadConfigFile()
    {
        using var file = new FileStream("data.json", FileMode.OpenOrCreate);
        Dictionary<string, int>? item = JsonSerializer.Deserialize<Dictionary<string, int>>(file);
        if (item == null)
        {
            throw new ConfigNotFound("Файл data.json не найден");
        }
        return new TimeDelay(start: item["StartDelay"], stop: item["StopDelay"]);
    }

    /// <summary>
    /// Записывает списко девайсов в конфиг файл
    /// </summary>
    /// <param name="devices">Коллекция девайсов</param>
    public static void WriteDevicesToConfig(List<DeviceItem> devices)
    {
        var data = "";
        foreach (var device in devices)
        {
            data += device + "\n";
        }
        if ( data.Length > 1 && data[^1] == '\n')
        {
            data = data.Substring(0, data.Length - 1);
        }
        File.WriteAllText("devices.txt",data);
    }

    /// <summary>
    /// Полученение всех устройств из конфига
    /// </summary>
    /// <returns>Список девайсов</returns>
    public static List<DeviceItem> GetDevicesFromConfig()
    {
        try
        {
            string data = File.ReadAllText("devices.txt");
            Console.WriteLine(data);
            List<DeviceItem> devices = new();
            if (string.IsNullOrEmpty(data))
            {
                return new List<DeviceItem>();
            }
            foreach (var element in data.Split("\n"))
            {
                devices.Add(DeviceItem.FromString(element));
            }

            return devices;
        }
        catch (FileNotFoundException)
        {
            return new List<DeviceItem>();
        }
    }
    
}

class ConfigNotFound(string? message) : Exception(message);