namespace whatsapp_sender;

internal class Program
{
    [Obsolete("Obsolete")]
    private static void Main(string[] args)
    {
        var deviceManager = new DeviceManager();
        deviceManager.StopWhatsapp();
        deviceManager.OpenChat("+79952680540");
        deviceManager.GetMessageBox();
        deviceManager.SendText("привет мир!!");
        deviceManager.SendMessage();
        deviceManager.Close();
    }
}