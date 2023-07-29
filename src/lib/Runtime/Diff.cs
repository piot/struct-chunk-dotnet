using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Piot.StructChunk
{
    public static class Differ
    {
        public static ushort ReservedIllegalEntityId = 0xffff;

        public enum EntityModification
        {
            Created,
            Updated,
            Deleted
        }

        public static unsafe int Diff(Span<byte> before, Span<byte> after, byte[] target)
        {
            fixed (byte* beforeP = before, afterP = after, unsafeTarget = target)
            {
                var beforeCount = before.Length;
                var afterCount = after.Length;
                ushort beforeEntityId = ReservedIllegalEntityId;
                ushort afterEntityId = ReservedIllegalEntityId;
                byte* beforeLast = beforeP + beforeCount;
                byte* afterLast = afterP + afterCount;
                byte* unsafeEndTarget = unsafeTarget + target.Length;
                byte* unsafeTargetMutablePtr = unsafeTarget;

                while (beforeP != beforeLast || afterP != afterLast)
                {
                    if (beforeP != beforeLast)
                    {
                        beforeEntityId = (ushort)*beforeP;
                    }
                    else
                    {
                        beforeEntityId = ReservedIllegalEntityId;
                    }

                    var modification = EntityModification.Updated;
                    var entityIdToWrite = afterEntityId;
                    if (beforeEntityId != afterEntityId)
                    {
                        if (afterEntityId < beforeEntityId)
                        {
                            // Since they are sorted, if there is an earlier one, it must be new
                            modification = EntityModification.Created;
                        }
                        else
                        {
                            modification = EntityModification.Deleted;
                            entityIdToWrite = beforeEntityId;
                        }
                    }

                    if (unsafeTargetMutablePtr + sizeof(ushort) > unsafeEndTarget)
                    {
                        throw new Exception("Differ: out of memory");
                    }
#if UNITY || true
                    UnsafeUtility.MemCpy(unsafeTargetMutablePtr, (byte*)&entityIdToWrite, sizeof(ushort));
#else
                    Unsafe.CopyBlock(unsafeTargetMutablePtr, (byte*)&entityIdToWrite, sizeof(ushort));
#endif
                    unsafeTargetMutablePtr += sizeof(ushort);
                    *unsafeTargetMutablePtr++ = (byte)modification;
                }
            }

            return 0;
        }


        public static int Diff2(Span<byte> before, Span<byte> after, Span<byte> target)
        {
            var beforeCount = before.Length;
            var afterCount = after.Length;
            
            ulong beforeEntityId = ReservedIllegalEntityId;
            ulong afterEntityId = ReservedIllegalEntityId;

            var beforePosition = 0;
            var afterPosition = 0;
            var writePosition = 0;

            while (beforePosition < beforeCount || afterPosition < afterCount)
            {
                if (beforePosition < beforeCount)
                {
                    beforeEntityId = BitConverter.ToUInt64(before.Slice(beforePosition, sizeof(ulong)));
                    beforePosition += sizeof(ulong);
                }
                else
                {
                    beforeEntityId = ReservedIllegalEntityId;
                }
                if (afterPosition < afterCount)
                {
                    afterEntityId = BitConverter.ToUInt64(after.Slice(afterPosition, sizeof(ulong)));
                    afterPosition += sizeof(ulong);
                }
                else
                {
                    afterEntityId = ReservedIllegalEntityId;
                }

                var modification = EntityModification.Updated;
                var entityIdToWrite = afterEntityId;
                if (afterEntityId != beforeEntityId)
                {
                    if (afterEntityId < beforeEntityId)
                    {
                        // Since they are sorted, if there is an earlier one, it must be new
                        modification = EntityModification.Created;
                    }
                    else
                    {
                        modification = EntityModification.Deleted;
                        entityIdToWrite = beforeEntityId;
                    }
                }

                if (writePosition + sizeof(ulong) >= target.Length)
                {
                    throw new Exception("out of memory");
                }
                BitConverter.TryWriteBytes(target.Slice(writePosition, sizeof(ulong)), entityIdToWrite);
                writePosition += sizeof(ulong);
            }

            return writePosition;
        }
    }
}