using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;

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
      await cache.GetOrCreateAsync(sessionId, entry => Task.FromResult(""));
      return Ok(sessionId);
    }

    [HttpPost]
    [Consumes("application/json")]
    [Route("LogEvent")]
    public async Task<ActionResult<string>> NewLogEventAsync([FromBody] LogEvent request)
    {
      //if (string.IsNullOrEmpty(request.UserName) || request.SessionTime == DateTime.MinValue)
      //{
      //  return BadRequest();
      //}

      Console.WriteLine(request.Message);
      

      return Ok();
    }
  }

  public class SessionRequest
  {
    public string UserName { get; set; }
    public DateTime SessionTime { get; set; }
  }

  public class LogEvent
  {
    public string? SessionId { get; set; }
    public string? Message { get; set; }
    public string? Level { get; set; }
    public string? Logger { get; set; }
  }
}