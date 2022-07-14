// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

Console.WriteLine("Dotnetty.Client");

var eventLoopGroup = new MultithreadEventLoopGroup();
var bootstrap = new Bootstrap()
               .Group(eventLoopGroup)
               .Channel<TcpSocketChannel>()
               .Option(ChannelOption.SoKeepalive, true)
               .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new ClientHandler());
                }));
Console.WriteLine("回车开始连接");
Console.ReadLine();
Console.WriteLine("连接中");
var channel = await bootstrap.ConnectAsync(IPEndPoint.Parse("127.0.0.1:5000"));
Console.WriteLine("连接成功");
do
{
    var result = Console.ReadLine();
    if (result == "exit") break;

    var buffer = Unpooled.Buffer(1024);
    buffer.WriteString(result,Encoding.UTF8);
    await channel.WriteAndFlushAsync(buffer);

} while (true);

await channel.CloseAsync();


public class ClientHandler : ChannelHandlerAdapter
{
    public override void ChannelActive(IChannelHandlerContext context)
    {
        base.ChannelActive(context);
    }

    public override void ChannelRegistered(IChannelHandlerContext context)
    {
        base.ChannelRegistered(context);
    }

    public override void Read(IChannelHandlerContext context)
    {
        base.Read(context);
    }

    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        base.ChannelReadComplete(context);
    }

    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        // base.ChannelRead(ctx, msg);
        if (msg is IByteBuffer buffer)
        {
            Console.WriteLine($"接收到消息: {buffer.ToString(Encoding.UTF8)}");
        }
    }
}