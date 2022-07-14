using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
// ReSharper disable ReplaceWithSingleCallToFirstOrDefault

Console.WriteLine("Dotnetty.Server");

var eventLoopGroup = new MultithreadEventLoopGroup();
var bootstrap = new ServerBootstrap()
               .Group(eventLoopGroup)
               .Channel<TcpServerSocketChannel>()
               .Option(ChannelOption.SoBacklog, 1024)
               .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
               .Option(ChannelOption.RcvbufAllocator, new FixedRecvByteBufAllocator(1024 * 8))
               .ChildOption(ChannelOption.SoKeepalive, true)
               .ChildOption(ChannelOption.TcpNodelay, true)
               .ChildOption(ChannelOption.SoReuseport, true)
               .Handler(new LoggingHandler("server_listen"))
               .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new ServerHandler());
                }));
var channel = await bootstrap.BindAsync(5000);
Console.WriteLine("启动完成");
do
{
    var result = Console.ReadLine();
    if (result == "exit") break;

} while (true);


public static class Global
{
    public static List<IChannel> Channels = new List<IChannel>();
}

public class ServerHandler : ChannelHandlerAdapter
{

    

    public override void ChannelRegistered(IChannelHandlerContext context)
    {
        base.ChannelRegistered(context);

        var msg = $"客户端注册：{context.Channel.Id}";
        Console.WriteLine(msg);
        Global.Channels.Add(context.Channel);
    }

    public override void ChannelUnregistered(IChannelHandlerContext context)
    {
        base.ChannelUnregistered(context);
    }


    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IByteBuffer buffer)
        {
            Console.WriteLine($"接收到消息: [{context.Channel.Id}] - {buffer.ToString(Encoding.UTF8)}");
        }

        var aa = Global.Channels.Where(c => c.Id != context.Channel.Id).FirstOrDefault();
        Console.WriteLine($"消息转发 [{context.Channel.Id}] -> [{aa?.Id}]");
        // context.WriteAsync(message);
        aa?.WriteAndFlushAsync(message);



    }

    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        // base.ChannelReadComplete(context);
        context.Flush();
    }

    public override Task WriteAsync(IChannelHandlerContext context, object message)
    {
        return base.WriteAsync(context, message);
    }
}