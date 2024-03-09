using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;
using NLog;
using NLog.Layouts;
using LogLevel = NLog.LogLevel;

namespace App.TelemetryService.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class LoggingController : ControllerBase
  {
    private readonly IMemoryCache cache;
    private readonly ILogger<LoggingController> logger;

    public LoggingController(ILogger<LoggingController> logger, IMemoryCache cache)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    [HttpPost]
    [Consumes("application/json")]
    [Route("CreateLogSession")]
    public async Task<ActionResult<SessionInfo>> CreateLogSessionAsync([FromBody] SessionRequest request)
    {
      if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.MachineName) || request.LocalTime == DateTime.MinValue)
      {
        return BadRequest();
      }

      string sessionId = $"{request.UserName}-{request.MachineName}-{request.LocalTime:yyyyMMddHHmmssfff}-{Guid.NewGuid()}";
      var sessionInfo = await cache.GetOrCreateAsync<SessionInfo>(sessionId, entry =>
      {
        return Task.FromResult(new SessionInfo(sessionId, request.UserName, request.MachineName, DateTime.UtcNow));
      });
      Console.WriteLine($"Session Created. SessionId={sessionInfo.SessionId}, User={sessionInfo.UserName}, Machine={sessionInfo.MachineName}");
      return Ok(sessionInfo);
    }

    [HttpPost]
    [Consumes("application/json")]
    [Route("EndLogSession")]
    public async Task<IActionResult> EndLogSessionAsync([FromBody] SessionEndRequest request)
    {
      if (string.IsNullOrWhiteSpace(request.SessionId))
      {
        return BadRequest();
      }
      LogManager.Flush();
      var session = cache.Get<SessionInfo>(request.SessionId);
      cache.Remove(request.SessionId);
      Console.WriteLine($"Session Ended. SessionId={request.SessionId}, User={session?.UserName}, Machine={session?.MachineName}");
      return Ok();
    }

    [HttpPost]
    [Consumes("application/json")]
    [Route("LogEvent")]
    public async Task<ActionResult<string>> NewLogEventAsync([FromBody] JsonObject request)
    {
      //if (string.IsNullOrEmpty(request.UserName) || request.SessionTime == DateTime.MinValue)
      //{
      //  return BadRequest();
      //}
      string sessionId = request["sessionId"]?.ToString();
      var session = cache.Get<SessionInfo>(sessionId);

      LogEventInfo logEventInfo = new LogEventInfo()
      {
        Message = request["message"]?.ToString(),
        Level = LogLevel.FromString(request["level"]?.ToString()),
        LoggerName = request["logger"]?.ToString(),
        Properties =
        {
          { "sessionId", sessionId },
          { "userName", session.UserName },
        }
      };
      LogManager.GetLogger(sessionId).Log(logEventInfo);
      Console.WriteLine(request);
      return Ok();
    }
  }
}