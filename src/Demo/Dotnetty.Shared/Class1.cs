namespace Dotnetty.Shared;

public class Package<T>
{
    public int CheckBit { get; set; }

    public int Length { get; set; }

    public int Cmd { get; set; }

    public T Body { get; set; }

    public byte[] Serialize()
    {
        CheckBit = 0x1F;
        Length   = 4 * 3;
        var bodyArray =
            Length += bodyArray.Length;
        return bodyArray;
    }
}

public class SerializerUtils
{
    public static byte[] Serialize<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
        }
    }
}