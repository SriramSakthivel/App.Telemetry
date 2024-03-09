using Microsoft.Extensions.Caching.Memory;
using NLog.Extensions.Logging;

namespace App.TelemetryService
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
      logger.Info("App Started");

      var builder = WebApplication.CreateBuilder(args);

      builder.Logging.ClearProviders();
      builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
      builder.Logging.AddNLog();

      // Add services to the container.

      builder.Services.AddSingleton<IMemoryCache, MemoryCache>();




      builder.Services.AddControllers();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      //app.UseHttpsRedirection();

      app.UseAuthorization();

      app.MapControllers();

      app.Run();
    }
  }
}