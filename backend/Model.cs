using System.Text.Json.Serialization;

namespace SwiftDashboard;

public class Invoice
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
}

public class Info
{
    public int Id { get; set; }
    public string? Text { get; set; }
}

public class Holiday
{
    public string? Date { get; set; }
    public string? LocalName { get; set; }
    public string? Name { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryName { get; set; }
    public bool Fixed { get; set; }
    public bool Global { get; set; }
    public List<string>? Counties { get; set; }
    public int? LaunchYear { get; set; }
    public List<string>? Types { get; set; }
}

public class UpdateInfoRequest
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class UpdateInvoiceModel
{
    public decimal Amount { get; set; }
}