using System.Net;
using System.Net.Sockets;

namespace PortForward;

public class PortForwarder
{
    private ForwardConfig[] _configs = { };


    public PortForwarder WithConfigs(params ForwardConfig[] configs)
    {
        _configs = configs;
        return this;
    }

    public void Start(CancellationToken token)
    {
        var forwards = _configs.Select(c => new Forward(c)).ToList();
        foreach (var forward in forwards) forward.Start(token);
    }

    private class Forward
    {
        private readonly ForwardConfig _config;
        private readonly TcpListener   _targetTcpListener;
        private const    int           Timeout = 3_000; // ms

        public Forward(ForwardConfig config)
        {
            _config = config;

            var endPoint = IPEndPoint.Parse($"{_config.TargetHost}:{_config.TargetPort}");
            _targetTcpListener = new TcpListener(endPoint);
        }

        public async void Start(CancellationToken token)
        {
            Console.WriteLine($"开始转发 [{_config.Name}] {_config.TargetHost}:{_config.TargetPort} <- {_config.SourceHost}:{_config.SourcePort}");
            _targetTcpListener.Start();
            while (true)
                try
                {
                    await Task.Yield();
                    await TransportAsync(token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
        }

        private async Task TransportAsync(CancellationToken token)
        {
            using var sourceTcpClient = new TcpClient(_config.SourceHost, _config.SourcePort);
            using var targetTcpClient = await _targetTcpListener.AcceptTcpClientAsync(token);
            targetTcpClient.SendTimeout    = Timeout;
            targetTcpClient.ReceiveTimeout = Timeout;
            sourceTcpClient.SendTimeout    = Timeout;
            sourceTcpClient.ReceiveTimeout = Timeout;
            var targetStream = targetTcpClient.GetStream();
            var sourceStream = sourceTcpClient.GetStream();
            var task1        = targetStream.CopyToAsync(sourceStream, token);
            var task2        = sourceStream.CopyToAsync(targetStream, token);
            await Task.WhenAny(task1, task2);
        }
    }
}