namespace whatsapp_sender;
public struct TimeDelay(int start, int stop)
{
    private readonly Random _random = new();
    public int Start = start;
    public int Stop = stop;

    /// <summary>
    /// Получение задержки в секундах
    /// </summary>
    /// <returns>задержка</returns>
    public readonly int GetDelay()
    {
        return _random.Next(Start, Stop + 1) * 1000;
    }

    public override string ToString()
    {
        return $"{Start}-{Stop}";
    }
}