

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

    private class Item(int startDelay, int stopDelay)
    {
        public int StartDelay = startDelay;
        public int StopDelay = stopDelay;
    }
}

class ConfigNotFound : Exception
{
    public ConfigNotFound(string? message) : base(message)
    {
    }
}