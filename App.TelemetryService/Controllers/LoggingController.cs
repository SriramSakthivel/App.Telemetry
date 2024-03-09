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
    public async Task<ActionResult<string>> CreateLogSessionAsync([FromBody] SessionRequest request)
    {
      if (string.IsNullOrEmpty(request.UserName) || request.SessionTime == DateTime.MinValue)
      {
        return BadRequest();
      }

      string sessionId = $"{request.UserName}-{request.SessionTime:s}-{Guid.NewGuid()}";
      //LogManager.Configuration.AddTarget();

      await cache.GetOrCreateAsync<SessionRequest>(sessionId, entry => Task.FromResult(request));
      return Ok(sessionId);
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
      var session = cache.Get<SessionRequest>(sessionId);

      LogEventInfo logEventInfo = new LogEventInfo()
      {
        Message = request["message"]?.ToString(),
        Level = LogLevel.FromString(request["level"]?.ToString()),
        LoggerName = request["logger"]?.ToString(),
        Properties =
        {
          { "sessionId", sessionId },
          { "userName", session.UserName },
        },
      };
      LogManager.GetLogger(sessionId).Log(logEventInfo);
      Console.WriteLine(request);


      return Ok();
    }
  }
}