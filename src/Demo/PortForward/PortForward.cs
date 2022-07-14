using System.Net;
using System.Net.Sockets;

namespace PortForward;

/// <summary>
/// 端口转发
/// https://www.cnblogs.com/zhouyz/articles/2192915.html
/// </summary>
public class PortForward : IDisposable
{
    private       TcpListener? _localTcpListener;
    public        IPEndPoint   LocalEndPoint;
    public        IPEndPoint   TargetEndPoint;
    private const int          Timeout = 3_000; // ms

    private readonly CancellationTokenSource _cts = new();


    public PortForward(IPEndPoint localEndPoint, IPEndPoint targetEndPoint)
    {
        LocalEndPoint  = localEndPoint;
        TargetEndPoint = targetEndPoint;
    }


    public async void Start(CancellationToken token)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);

        _localTcpListener ??= new TcpListener(LocalEndPoint);
        _localTcpListener.Start();
        while (!cts.IsCancellationRequested)
            try
            {
                await Task.Yield();
                await TransportAsync(cts.Token);
            }
            catch (Exception)
            {
                cts.Cancel();
                throw;
            }
    }

    private async Task TransportAsync(CancellationToken token)
    {
        using var targetClient = new TcpClient(TargetEndPoint.Address.ToString(), TargetEndPoint.Port);
        using var localClient  = await _localTcpListener!.AcceptTcpClientAsync(token);

        localClient.SendTimeout     = Timeout;
        localClient.ReceiveTimeout  = Timeout;
        targetClient.SendTimeout    = Timeout;
        targetClient.ReceiveTimeout = Timeout;

        var sourceStream = localClient.GetStream();
        var targetStream = targetClient.GetStream();
        var task1        = sourceStream.CopyToAsync(targetStream, token);
        var task2        = targetStream.CopyToAsync(sourceStream, token);
        await Task.WhenAny(task1, task2);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _localTcpListener?.Stop();
    }
}