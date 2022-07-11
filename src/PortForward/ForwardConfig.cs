namespace PortForward;

public class ForwardConfig
{
    public const string Section = "Forwards";

    public string Name { get; set; }
    public string SourceHost { get; set; }
    public int SourcePort { get; set; }
    public string TargetHost { get; set; }
    public int TargetPort { get; set; }
}