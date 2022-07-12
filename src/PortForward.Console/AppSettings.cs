using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

// ReSharper disable ReplaceWithSingleCallToAny

namespace PortForward.Console;

public class AppSettings
{
    public Forward[]? Forwards { get; set; }

    public class Forward
    {
        public string Name { get; set; } = "";

        /// <summary>
        /// 本地监听地址
        /// </summary>
        public string LocalHost { get; set; } = "127.0.0.1";

        /// <summary>
        /// 本地监听端口
        /// </summary>
        public int LocalPort { get; set; }

        /// <summary>
        /// 转发目标地址
        /// </summary>
        public string TargetHost { get; set; } = "127.0.0.1";

        /// <summary>
        /// 转发目标端口
        /// </summary>
        public int TargetPort { get; set; }
    }
}

public static class AppSettingsExtensions
{
    public static void Validate(this AppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings.Forwards);

        var isRepeat = appSettings.Forwards
                                  .GroupBy(f => f.Name)
                                  .Where(g => g.Count() > 1)
                                  .Any();
        if (isRepeat) throw new ValidationException("配置文件校验失败，名称重复");


        isRepeat = appSettings.Forwards
                              .GroupBy(f => $"{f.LocalHost}:{f.LocalPort}")
                              .Where(g => g.Count() > 1)
                              .Any();
        if (isRepeat) throw new ValidationException("配置文件校验失败，本地监听地址/端口重复");


        foreach (var f in appSettings.Forwards)
            if (PortAvailable(f.LocalPort))
                throw new ValidationException($"配置文件校验失败，本地监听端口[{f.LocalPort}]已使用");
    }

    public static bool PortAvailable(int port)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        return properties.GetActiveTcpListeners()
                         .Concat(properties.GetActiveUdpListeners())
                         .Where(p => p.Port == port)
                         .Any();
    }
}