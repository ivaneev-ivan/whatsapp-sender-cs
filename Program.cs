namespace whatsapp_sender;

internal class Program
{
    [Obsolete("Obsolete")]
    private static void Main()
    {
        // var deviceManager = new WhatsappSender();
        var messageReader = new MessageReader();
        Console.WriteLine(messageReader.GetTimeNowItems());
        // deviceManager.StopWhatsapp();
        // deviceManager.OpenChat("+79952680540");
        // deviceManager.GetMessageBox();
        // deviceManager.SendText("привет мир!!");
        // deviceManager.SendMessage();
        // deviceManager.Close();
    }
}