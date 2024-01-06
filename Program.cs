namespace whatsapp_sender;

internal class Program
{
    private static void Main()
    {
        var exelReader = new ExelReader("base.xlsx");
        // var deviceManager = new WhatsappSender();
        while (true)
        {
            UserData data = exelReader.GetPhoneNumber();
            if (data.Phone == "")
            {
                break;
            }
            Console.WriteLine(data);
            // exelReader.WriteStatusPhone(new UserData());
            Console.ReadKey();
            var messageReader = new MessageReader();
            var message = messageReader.GetRandomRandomizedMessage();
            if (message == null) return;
            // deviceManager.StopWhatsapp();
            // deviceManager.OpenChat("+79952680540");
            // deviceManager.GetMessageBox();
            // deviceManager.SendTypeWordText(message);
            // deviceManager.SendMessage();
        }
        // deviceManager.Close();
    }
}