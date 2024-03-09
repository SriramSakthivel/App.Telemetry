namespace App.TelemetryService.Controllers;

public class SessionInfo
{
  public SessionInfo(string sessionId, string userName, string machineName, DateTime sessionCreatedTime)
  {
    SessionId = sessionId;
    UserName = userName;
    MachineName = machineName;
    SessionCreatedTime = sessionCreatedTime;
  }

  public string SessionId { get; }
  public string UserName { get; }
  public string MachineName { get; }
  public DateTime SessionCreatedTime { get; }
}