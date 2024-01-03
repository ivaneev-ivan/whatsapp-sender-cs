namespace whatsapp_sender;

internal class Program
{
    [Obsolete("Obsolete")]
    private static void Main()
    {
        var deviceManager = new WhatsappSender();
        var messageReader = new MessageReader();
        var message = messageReader.GetRandomRandomizedMessage();
        if (message == null)
        {
            return;
        }

        while (true)
        {
            deviceManager.StopWhatsapp();
            deviceManager.OpenChat("+79952680540");
            deviceManager.GetMessageBox();
            deviceManager.SendText(message);
            deviceManager.SendMessage();
            deviceManager.Close();
        }
    }
}