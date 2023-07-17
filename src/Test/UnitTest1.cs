using Piot.Blitser;
using StructChunk;

namespace Test;

public struct Test
{
    public int x;
    public float y;
    public byte c;
    public byte a;
}

public struct AnotherTest
{
    public byte c;
    public byte a;
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var firstEntitySnapshot = new byte[120];
        var temp = new byte[32];

        DataMetaInfo.infos = new MetaInfo[]
        {
            new(DataType.Ghost, typeof(Test)),
            new(DataType.Ghost, typeof(AnotherTest))
        };

        var componentDatasForFirst = new ValueType[]
        {
            new Test { x = 42, y = -0.32f, c = 0xca, a = 0xfe },
            new AnotherTest { a = 0x18, c = 0x19 }
        };

        var componentDatasForSecond = new ValueType[]
        {
            new AnotherTest { a = 0x1a, c = 0x19 }
        };

        var position = 0;
        var count = StructToOctets.ConvertMultipleWithEntityToOctetsWithHeader(0x999, componentDatasForFirst, temp);
        Array.Copy(temp, 0, firstEntitySnapshot, position, count);
        position += count;

        var count2 = StructToOctets.ConvertMultipleWithEntityToOctetsWithHeader(0xee, componentDatasForSecond, temp);
        Array.Copy(temp, 0, firstEntitySnapshot, position, count2);
        position += count2;

        var copy = new byte[position];
        Array.Copy(firstEntitySnapshot, copy, position);
        var resultString = Util.ByteArrayToString(copy);
        Assert.Equal("9909020001109B1E400100000000000000309BEE000101689B", resultString);
    }
}