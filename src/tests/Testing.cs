using System;
using Piot.Blitser;
//using BenchmarkDotNet.Attributes;
using StructChunk;

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

public class StructCopyBenchmarks
{
    readonly byte[] target = new byte[32];
    
  //  [Benchmark]
    public int ConvertToBytes()
    {
        return StructToOctets.ConvertToOctets(new Test(), target);
    }
}



public static class Testing
{
    public static void TestOut()
    {
        var converted = new byte[32];
        var count = StructToOctets.ConvertToOctets(new Test { x = 42, y = -0.32f , c = 0xca, a = 0xfe}, converted);

        var copy = new byte[count];
        Array.Copy(converted, copy, count);
        Console.WriteLine(Util.ByteArrayToString(copy));
    }
    
    public static void TestOutMultiple()
    {
        var converted = new byte[32];

        DataMetaInfo.infos = new MetaInfo[]
        {
            new (DataType.Ghost, typeof(Test)),
            new (DataType.Ghost, typeof(AnotherTest))
        };
        
        var componentDatas = new ValueType[]
        {
            new Test { x = 42, y = -0.32f, c = 0xca, a = 0xfe },
            new AnotherTest { a = 0x18, c = 0x19 }
        };
        
        var count = StructToOctets.ConvertMultipleToOctets(componentDatas, converted);

        var copy = new byte[count];
        Array.Copy(converted, copy, count);
        Console.WriteLine(Util.ByteArrayToString(copy));
    }
    
    public static void TestOutMultipleWithHeader()
    {
        var converted = new byte[32];

        DataMetaInfo.infos = new MetaInfo[]
        {
            new (DataType.Ghost, typeof(Test)),
            new (DataType.Ghost, typeof(AnotherTest))
        };
        
        var componentDatas = new ValueType[]
        {
            new Test { x = 42, y = -0.32f, c = 0xca, a = 0xfe },
            new AnotherTest { a = 0x18, c = 0x19 }
        };
        
        var count = StructToOctets.ConvertMultipleToOctetsWithHeader(componentDatas, converted);

        var copy = new byte[count];
        Array.Copy(converted, copy, count);
        Console.WriteLine(Util.ByteArrayToString(copy));
    }
    
    public static void TestOutMultipleWithEntityHeader()
    {
        var firstEntitySnapshot = new byte[120];
        var temp = new byte[32];

        DataMetaInfo.infos = new MetaInfo[]
        {
            new (DataType.Ghost, typeof(Test)),
            new (DataType.Ghost, typeof(AnotherTest))
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
        
        var count2 = StructToOctets.ConvertMultipleWithEntityToOctetsWithHeader(0xee, componentDatasForSecond,temp);
        Array.Copy(temp, 0, firstEntitySnapshot, position, count);
        position += count2;
        
        var copy = new byte[position];
        Array.Copy(firstEntitySnapshot, copy, position);
        Console.WriteLine(Util.ByteArrayToString(copy));
    }
}

