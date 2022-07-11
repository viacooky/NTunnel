using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PortForward;

using var host = Host.CreateDefaultBuilder(args).Build();


var config = host.Services.GetRequiredService<IConfiguration>();

var forwardConfigs = config.GetSection(ForwardConfig.Section).Get<ForwardConfig[]>();

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    cts.Cancel();
    eventArgs.Cancel = true;
};

new PortForwarder().WithConfigs(forwardConfigs).Start(cts.Token);

await host.RunAsync();