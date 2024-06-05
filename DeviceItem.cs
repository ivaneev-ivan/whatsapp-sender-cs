namespace whatsapp_sender;

public struct DeviceItem (string model, string name)
{
    public string Model = model;
    public string Name = name;

    public string ToString()
    {
        return $"{Model}:{Name}";
    }

    public static DeviceItem FromString(string data)
    {
        var split = data.Split(":");
        if (split.Length != 2)
        {
            throw new Exception("Неправильный формат в файле конфигурации телефонов");
        }
        return new DeviceItem(split[0], split[1]);
    }
}