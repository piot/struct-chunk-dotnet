using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Piot.Blitser;

namespace StructChunk
{
    public class StructToOctets
    {
        public static unsafe int ConvertToOctets<T>(T value, Span<byte> target) where T : struct
        {
#if true
            fixed (byte* unsafeTarget = target)
            {
                Unsafe.CopyBlock(unsafeTarget, (byte*)&value, (uint)sizeof(T));
            }
#else
            var pointer = (byte*)&value;
            for (var i = 0; i < sizeof(T); i++)
            {
                target[i] = pointer[i];
            }
#endif
            return sizeof(T);
        }

        public static unsafe int ConvertMultipleToOctets(ValueType[] componentDatas, Span<byte> target)
        {
            var position = 0;
            fixed (byte* unsafeTarget = target)
            {
                foreach (var componentData in componentDatas)
                {
                    var octetSize = Marshal.SizeOf(componentData);

                    Unsafe.CopyBlock(unsafeTarget + position, (byte*)&componentData, (uint)octetSize);

                    position += octetSize;
                }
            }

            return position;
        }

        public static byte GetId(object x)
        {
            var typeToLookFor = x.GetType();
            byte index = 0;
            foreach (var metaInfo in DataMetaInfo.infos)
            {
                if (metaInfo.type == typeToLookFor)
                {
                    return index;
                }

                index++;
            }

            throw new Exception("could not find anything for this object");
        }

        public static unsafe int ConvertMultipleToOctetsWithHeader(ValueType[] componentDatas, Span<byte> target)
        {
            var position = 0;
            target[0] = (byte)componentDatas.Length;
            position++;

            foreach (var componentData in componentDatas)
            {
                target[position] = GetId(componentData);
                position++;
            }

            fixed (byte* unsafeTarget = target)
            {
                foreach (var componentData in componentDatas)
                {
                    var octetSize = Marshal.SizeOf(componentData);
                    Unsafe.CopyBlock(unsafeTarget + position, (byte*)&componentData, (uint)octetSize);
                    position += octetSize;
                }
            }

            return position;
        }
        
        public interface IComponentData
        {
            
        }

        public static unsafe int ConvertMultipleWithEntityToOctetsWithHeader(ushort entityId, IComponentData[] componentDatas,
            Span<byte> target)
        {
            var position = 0;
            fixed (byte* unsafeTarget = target)
            {
                Unsafe.CopyBlock(unsafeTarget, (byte*)&entityId, sizeof(ushort));
            }

            position += sizeof(ushort);

            target[position] = (byte)componentDatas.Length;
            position++;

            foreach (var componentData in componentDatas)
            {
                target[position] = GetId(componentData);
                position++;
            }

            fixed (byte* unsafeTarget = target)
            {
                foreach (var componentData in componentDatas)
                {
                    IntPtr ptr = Marshal.AllocHGlobal(sizeof(componentData));
                    Marshal.StructureToPtr(componentData, ptr, true);
                    ConvertToOctets(componentData!, target);
                    fixed (byte* unsafeComponentData = Unmanaged..Unbox(componentDatas[i]))
                    {
                        var octetSize = Marshal.SizeOf(componentData);
                        Unsafe.CopyBlock(unsafeTarget + position, unsafeComponentData, (uint)octetSize);
                        position += octetSize;
                    }
                }
            }

            return position;
        }
    }
}