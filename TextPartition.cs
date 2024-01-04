using System.Text.RegularExpressions;

namespace whatsapp_sender;

public struct TimeDelay(int start, int stop)
{
    private readonly Random _random = new Random();
    public int Start = start;
    public int Stop = stop;

    public int GetDelay()
    {
        return _random.Next(Start, Stop);
    }

    public override string ToString()
    {
        return $"{Start}-{Stop}";
    }
}

public struct TextPartition(int start, int stop, string message)
{
    public TimeDelay Delay = new TimeDelay(start, stop);
    public string Message = message;

    public static List<TextPartition> GetTextPartition(string message)
    {
        Regex regex = new(@"<(\d+), ?(\d+)>");
        List<TextPartition> parts = new();
        var matches = regex.Matches(message);
        foreach (Match match in matches)
        {
            var matchString = match.ToString();
            var splitMessage = message.Split(matchString).ToList();
            var countSplit = Regex.Matches(message, matchString).Count;
            var mess = splitMessage[0];
            int start = Convert.ToInt32(match.Groups[1].ToString());
            int stop = Convert.ToInt32(match.Groups[2].ToString());
            message = splitMessage[1];
            if (countSplit > 1)
            {
                int i = 2;
                List<string> a = new();
                while (i < splitMessage.Count)
                {
                    a.Add(splitMessage[i]);
                    i++;
                }

                message += matchString + string.Join(matchString, a);
            }

            parts.Add(new TextPartition(start, stop, mess));
        }

        parts.Add(new TextPartition(0, 0, message));
        return parts;
    }

    public override string ToString()
    {
        return $"{Delay.ToString()} {Message}";
    }
}