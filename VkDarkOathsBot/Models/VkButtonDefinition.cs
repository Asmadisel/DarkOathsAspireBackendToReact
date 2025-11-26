// VkDarkOathsBot/Models/VkButtonDefinition.cs
namespace VkDarkOathsBot.Models;

public class VkButtonDefinition
{
    public required string Label { get; init; }

    // Только для open_link кнопок
    public string? Url { get; init; }

    // Только для text кнопок  
    public string? Payload { get; init; }

    // Цвет применяется только к text кнопкам
    public string Color { get; init; } = "primary";

    public string Type => Url != null ? "open_link" : "text";
}