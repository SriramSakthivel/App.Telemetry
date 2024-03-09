using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using LogManager = Common.Logging.LogManager;

namespace App.Desktop
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var logger = LogManager.GetLogger<Program>();
 
      Thread.Sleep(10000);

      logger.Info("Hello");

      int number = 0;
      while (number < 100)
      {
        if (number % 5 == 0)
        {
          try
          {
            throw new InvalidOperationException("some random exception");
          }
          catch (Exception e)
          {
            logger.Error("Something failed", e);
          }
       
        }
        else
        {
          logger.Info("Hellooooooo " + DateTime.Now);
        }

        Thread.Sleep(1000);
        number++;
      }

      Console.ReadLine();
    }
  }

  [Target("LoggingService", IsWrapper = true)]
  public class LoggingServiceTarget : WrapperTargetBase
  {
    public LoggingServiceTarget()
    {

    }

    private Task<string> sessionIdTask;
    protected override void InitializeTarget()
    {
      base.InitializeTarget();

      sessionIdTask = CreateSessionAsync();
    }

    private async Task<string> CreateSessionAsync()
    {
      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
      var response = await client.PostAsync("https://localhost:7027/api/Logging/CreateLogSession", new StringContent(DefaultJsonSerializer.Instance.SerializeObject(new
      {
        UserName = Environment.UserName,
        SessionTime = DateTime.Now,
      }), Encoding.UTF8, "application/json"));
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadAsStringAsync();
    }

    protected override void Write(AsyncLogEventInfo logEvent)
    {
      logEvent.LogEvent.Properties["sessionId"] = sessionIdTask.Result;
      logEvent.LogEvent.Properties["userName"] = Environment.UserName;
      WrappedTarget.WriteAsyncLogEvent(logEvent);
      //base.Write(logEvent);
    }

    //protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
    //{
    //  //string sessionId = await sessionIdTask;
    //  //Target.WriteAsyncLogEvent(new AsyncLogEventInfo(logEvent, exception => { }));
    //  return;
    //}
  }
}
