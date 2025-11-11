using System.Text.Json.Serialization;

namespace SwiftDashboard.Models.DTOs;

public class UpdateInfoRequest
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
