using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket.Client;

public class WsClient
{
    public           string           ClientId;
    private          ClientWebSocket? _socket = new();
    private readonly Uri              _uri;

    public WebSocketState State => _socket.State;

    // public event EventHandler? OnConnected;
    //
    // public event EventHandler? OnDisconnect;
    //
    public event EventHandler? OnReceive;
    //
    // public event EventHandler? OnSend;
    //
    // public event EventHandler? OnError;

    public WsClient(string clientId, string url)
    {
        ClientId = clientId;
        _uri     = new Uri(url);
    }

    public async Task StartAsync(CancellationToken token)
    {
        Console.WriteLine("Client 启动");
        while (!token.IsCancellationRequested)
            try
            {
                await ConnectAsync(token);
                await ReceiveAsync(token);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
    }

    public async Task StopAsync(CancellationToken token)
    {
        Console.WriteLine("Client Stop");
        _socket?.Abort();
    }

    public async Task Send(string msg,CancellationToken token)
    {
        var msgBytes = Encoding.UTF8.GetBytes(msg);
        await _socket.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, token);
    }

    private async Task ReceiveAsync(CancellationToken token)
    {
        // 管道方式处理TCP消息
        // 具体看官网 https://docs.microsoft.com/zh-cn/dotnet/standard/io/pipelines
        // var pipe    = new Pipe();
        // var writing = FillPipeAsync(pipe.Writer, token);
        // var reading = ReadPipeAsync(pipe.Reader, token);
        //
        // await Task.WhenAll(reading, writing);


        var buffer        = new byte[1024 * 4];
        var receiveResult = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
        while (!receiveResult.CloseStatus.HasValue)
        {
            if (receiveResult.EndOfMessage)
            {
                Console.WriteLine($"服务端返回: {Encoding.UTF8.GetString(buffer.Take(receiveResult.Count).ToArray())}");
            }

            receiveResult = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
        }


    }

    private async Task ReadPipeAsync(PipeReader reader, CancellationToken token)
    {
        while (true)
        {
            var readResult = await reader.ReadAsync(token);
            var buffer     = readResult.Buffer;

            while (TryReadLine(ref buffer, out var line))
            {
                // todo 处理行
            }

            bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
            {
                // Look for a EOL in the buffer.
                var position = buffer.PositionOf((byte) '\n');
                if (position == null)
                {
                    line = default;
                    return false;
                }

                // Skip the line + the \n.
                line   = buffer.Slice(0, position.Value);
                buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                return true;
            }

            reader.AdvanceTo(buffer.Start, buffer.End);
            if (readResult.IsCompleted) break;
        }

        await reader.CompleteAsync();
    }

    private async Task FillPipeAsync(PipeWriter writer, CancellationToken token)
    {
        const int bufferSize = 512;
        while (true)
        {
            try
            {
                var buffer        = writer.GetMemory(bufferSize);
                var receiveResult = await _socket.ReceiveAsync(buffer, token);

                writer.Advance(receiveResult.Count);
            }
            catch (Exception)
            {
                break;
            }

            var flushResult = await writer.FlushAsync(token);
            if (flushResult.IsCompleted) break;
        }

        await writer.CompleteAsync();
        
    }

    private async Task ConnectAsync(CancellationToken token)
    {
        _socket ??= new ClientWebSocket();

        _socket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        _socket.Options.SetRequestHeader("NT_ClientId", ClientId);
        Console.WriteLine("正在连接...");
        await _socket.ConnectAsync(_uri, token);
        Console.WriteLine("连接成功...");
    }
}