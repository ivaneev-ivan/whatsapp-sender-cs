namespace whatsapp_sender;

internal class Program
{
    private static void Main()
    {
        var messageReader = new MessageReader();
        // var message = messageReader.GetRandomRandomizedMessage();
        var message = messageReader.GetRandomRandomizedMessage();
        if (message == null) return;
        var deviceManager = new WhatsappSender();
        while (true)
        {
            deviceManager.StopWhatsapp();
            deviceManager.OpenChat("+79952680540");
            deviceManager.GetMessageBox();
            deviceManager.SendTypeWordText(message);
            deviceManager.SendMessage();
            // deviceManager.Close();
        }
    }
}