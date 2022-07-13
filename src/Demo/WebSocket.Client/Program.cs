// See https://aka.ms/new-console-template for more information

using System.Net.WebSockets;
using System.Text;
using WebSocket.Client;

Console.WriteLine("WebSocket WsClient");

var url = "ws://127.0.0.1:5000/ws";

var ws = new WsClient("01", url);
Console.ReadLine();
ws.StartAsync(CancellationToken.None);

while (true)
{
    Console.WriteLine("输入消息");
    var msg = Console.ReadLine();
    await ws.Send(msg, CancellationToken.None);
    Console.WriteLine("send finish");
}



// var client = new ClientWebSocket();
// var uri    = new Uri("ws://127.0.0.1:5000/ws");
//
// Console.WriteLine("开始连接");
// Console.ReadLine();
//
// await client.ConnectAsync(uri, CancellationToken.None);
//
// while (true)
//     if (client.State == WebSocketState.Open)
//     {
//         // Console.WriteLine("发送消息");
//         // var msg         = Console.ReadLine();
//         // var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{msg}"));
//         // await client.SendAsync(bytesToSend, WebSocketMessageType.Text, WebSocketMessageFlags.None, CancellationToken.None);
//
//
//         var bytesReceived = new ArraySegment<byte>(new byte[1024]);
//         var receiveResult = await client.ReceiveAsync(bytesReceived, CancellationToken.None);
//         Console.WriteLine(Encoding.UTF8.GetString(bytesReceived.Array, 0, receiveResult.Count));
//     }
//     else
//     {
//         await client.ConnectAsync(uri, CancellationToken.None);
//     }