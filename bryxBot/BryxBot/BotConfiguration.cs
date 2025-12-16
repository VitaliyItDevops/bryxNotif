namespace BryxBot;

public class BotConfiguration
{
    public string BotToken { get; set; } = string.Empty;
    public string CrmApiUrl { get; set; } = string.Empty;
    public List<string> AllowedUsers { get; set; } = new();
}
