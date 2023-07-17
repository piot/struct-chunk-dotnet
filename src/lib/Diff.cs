using System;
using System.Runtime.CompilerServices;

namespace StructChunk
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
                        throw new Exception("out of memory");
                    }

                    Unsafe.CopyBlock(unsafeTargetMutablePtr, (byte*)&entityIdToWrite, sizeof(ushort));
                    unsafeTargetMutablePtr += sizeof(ushort);
                    *unsafeTargetMutablePtr++ = (byte)modification;
                }
            }

            return 0;
        }
    }
}