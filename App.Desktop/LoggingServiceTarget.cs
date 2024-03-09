using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Common;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace App.Desktop
{
  [Target("LoggingService", IsWrapper = true)]
  public class LoggingServiceTarget : WrapperTargetBase
  {
    private readonly HttpClient client = new HttpClient()
    {
      BaseAddress = new Uri("https://localhost:7027/api/Logging/"),
      DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("*/*") } }
    };

    private Task<SessionResponse> sessionIdTask;

    public LoggingServiceTarget()
    {

    }

    protected override void InitializeTarget()
    {
      sessionIdTask = CreateSessionAsync();
      base.InitializeTarget();
    }

    protected override void CloseTarget()
    {
      var response = client.PostAsync("EndLogSession", new StringContent(DefaultJsonSerializer.Instance.SerializeObject(new
      {
        SessionId = sessionIdTask.Result.SessionId
      }), Encoding.UTF8, "application/json"));
      base.CloseTarget();
    }

    private async Task<SessionResponse> CreateSessionAsync()
    {
      string jsonContent = JsonConvert.SerializeObject(new
      {
        UserName = GlobalDiagnosticsContext.GetObject("UserName"),
        MachineName = GlobalDiagnosticsContext.GetObject("MachineName"),
        LocalTime = DateTime.Now
      }, new JsonSerializerSettings() {ContractResolver = new CamelCasePropertyNamesContractResolver()});
      var response = await client.PostAsync("CreateLogSession", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
      response.EnsureSuccessStatusCode();
      string responseText = await response.Content.ReadAsStringAsync();
      var sessionResponse = JsonConvert.DeserializeObject<SessionResponse>(responseText);
      return sessionResponse;
    }

    protected override void Write(AsyncLogEventInfo logEvent)
    {
      if (sessionIdTask.IsCompleted)
      {
        WriteInternal(logEvent, sessionIdTask.Result);
      }
      else
      {
        sessionIdTask.ContinueWith(x =>
        {
          WriteInternal(logEvent, x.Result);
        });
      }
    }

    private void WriteInternal(AsyncLogEventInfo logEvent, SessionResponse sessionResponse)
    {
      logEvent.LogEvent.Properties["sessionId"] = sessionResponse.SessionId;
      WrappedTarget.WriteAsyncLogEvent(logEvent);
    }

    public class SessionResponse
    {
      public string SessionId { get; set; }
      public string UserName { get; set; }
      public string MachineName { get; set; }
      public DateTime SessionCreatedTime { get; set; }
    }
  }
}