namespace whatsapp_sender;

internal class Program
{
    [Obsolete("Obsolete")]
    private static async Task Main()
    {
        var messageReader = new MessageReader();
        var message = messageReader.GetRandomRandomizedMessage();
        if (message == null) return;

        var deviceManager = new WhatsappSender();
        await deviceManager.StopWhatsapp();
        await deviceManager.OpenChat("+79952680540");
        await deviceManager.GetMessageBox();
        await deviceManager.SendText(message);
        await deviceManager.SendMessage();
        await deviceManager.Close();
    }
}