namespace App.TelemetryService.Controllers;

public class SessionRequest
{
  public string UserName { get; set; }
  public string MachineName { get; set; }
  public DateTime LocalTime { get; set; }
}