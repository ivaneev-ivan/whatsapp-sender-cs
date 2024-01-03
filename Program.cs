namespace whatsapp_sender;

public class TaskQueue
{
    private readonly SemaphoreSlim semaphore;

    public TaskQueue()
    {
        semaphore = new SemaphoreSlim(1);
    }

    public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
    {
        await semaphore.WaitAsync();
        try
        {
            return await taskGenerator();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task Enqueue(Func<Task> taskGenerator)
    {
        await semaphore.WaitAsync();
        try
        {
            await taskGenerator();
        }
        finally
        {
            semaphore.Release();
        }
    }
}

internal class Program
{
    [Obsolete("Obsolete")]
    private static async Task Main(string[] args)
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