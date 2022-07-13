using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebSocket.Server.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;

    private List<System.Net.WebSockets.WebSocket> _sockets = new();


    public WebSocketController(ILogger<WebSocketController> logger)
    {
        _logger = logger;
    }


    [HttpGet("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _sockets.Add(webSocket);
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task Echo(System.Net.WebSockets.WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            var receiveMsg = Encoding.UTF8.GetString(buffer.Take(result.Count).ToArray());
            Console.WriteLine($"接收客户端消息:{receiveMsg}");
            if(receiveMsg.Trim() == "close") webSocket.Abort();
            

            var serverMsg = Encoding.UTF8.GetBytes($"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} - {receiveMsg}");
            await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

            // 继续等待
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}