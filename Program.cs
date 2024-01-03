namespace whatsapp_sender;

internal class Program
{
    [Obsolete("Obsolete")]
    private static void Main()
    {
        var deviceManager = new WhatsappSender();
        var messageReader = new MessageReader();
        var messages = messageReader.GetTimeNowItems();
        if (messages == null)
        {
            Console.WriteLine("Сообщение не найдено");
            return;
        }
        Console.WriteLine(messages[0].Message);
        deviceManager.StopWhatsapp();
        deviceManager.OpenChat("+79952680540");
        deviceManager.GetMessageBox();
        deviceManager.SendText(messages[0].Message);
        deviceManager.SendMessage();
        deviceManager.Close();
    }
}