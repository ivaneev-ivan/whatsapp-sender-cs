

using System.Text.Json;

namespace whatsapp_sender;

internal static class ConfigManager
{

    public static TimeDelay ReadConfigFile()
    {
        using var file = new FileStream("data.json", FileMode.OpenOrCreate);
        Dictionary<string, int> item = JsonSerializer.Deserialize<Dictionary<string, int>>(file);
        if (item == null)
        {
            throw new ConfigNotFound("Файл data.json не найден");
        }
        return new TimeDelay(start: item["StartDelay"], stop: item["StopDelay"]);
    }

    public static void WriteDevicesToConfig(IEnumerable<DeviceItem> devices)
    {
        var data = "";
        foreach (var device in devices)
        {
            data += device.ToString() + "\n";
        }

        if (data[data.Length - 1] == '\n')
        {
            data = data.Substring(0, data.Length - 1);
        }
        File.WriteAllText("devices.txt",data);
    }
    
}

class ConfigNotFound : Exception
{
    public ConfigNotFound(string? message) : base(message)
    {
    }
}