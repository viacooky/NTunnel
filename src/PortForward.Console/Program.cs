using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PortForward.Console;
using Serilog;
using Serilog.Events;
using System.Net;




// logger
var logTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message}{NewLine}{Exception}";
using var host = Host.CreateDefaultBuilder(args)
                     .UseSerilog((_, configuration) =>
                      {
                          configuration.MinimumLevel.Debug()
                                       .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                                       .Enrich.FromLogContext()
                                       .WriteTo.Console(outputTemplate: logTemplate);
                      })
                     .Build();

Log.Information("开始启动");

// 配置
var appSettings = host.Services
                      .GetRequiredService<IConfiguration>()
                      .Get<AppSettings>();


var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

try
{

    appSettings.Validate();

    var instances = appSettings.Forwards!
                               .Select(f =>
                                {
                                    var localEndPoint  = IPEndPoint.Parse($"{f.LocalHost}:{f.LocalPort}");
                                    var targetEndPoint = IPEndPoint.Parse($"{f.TargetHost}:{f.TargetPort}");
                                    return new
                                    {
                                        f.Name,
                                        PortForward = new PortForward.PortForward(localEndPoint, targetEndPoint)
                                    };
                                })
                               .ToList();

    foreach (var o in instances)
    {
        Log.Information($"[{o.Name}] {o.PortForward.LocalEndPoint} -> {o.PortForward.TargetEndPoint}");
        o.PortForward.Start(cts.Token);
    }
}
catch (Exception e)
{
    Log.Error(e.Message);
    Environment.Exit(0);
}


await host.RunAsync(cts.Token);