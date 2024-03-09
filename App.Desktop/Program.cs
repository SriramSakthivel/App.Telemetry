using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Layouts;
using LogManager = Common.Logging.LogManager;

namespace App.Desktop
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string userName = args.Length > 0 ? args[0] : Environment.UserName;
      string machineName = args.Length > 1 ? args[1] : Environment.MachineName;

      GlobalDiagnosticsContext.Set("UserName", userName);
      GlobalDiagnosticsContext.Set("MachineName", machineName);

      var logger = LogManager.GetLogger<Program>();
      Thread.Sleep(10000);

      logger.Info($"Hello from {userName} {machineName}");

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
            logger.Error($"Something failed {number}", e);
          }
        }
        else
        {
          logger.Info($"Hello {number} from user User={userName}, Machine={machineName}");
        }

        Thread.Sleep(250);
        number++;
      }
      logger.Info("Done, waiting to be closed.");
      Console.ReadLine();
      NLog.LogManager.Shutdown();
    }
  }
}
