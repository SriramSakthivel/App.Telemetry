namespace App.TelemetryService.Controllers;

public class LogEvent
{
  public string SessionId { get; set; }
  public string? Message { get; set; }
  public string? Level { get; set; }
  public string? Logger { get; set; }
}