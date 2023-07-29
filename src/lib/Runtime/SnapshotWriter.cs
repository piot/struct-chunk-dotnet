using System;
using Piot.Blitser;
using UnityEngine;

namespace Piot.StructChunk
{
    public struct EntityId
    {
        public ulong value; // index and version
    }

    public interface ISnapshotWriter
    {
        public void WriteEntity(EntityId entity);
        public void WriteComponentData<T>(T componentData) where T : unmanaged;
        public void WriteTagComponentData<T>() where T : unmanaged;
        public Span<byte> Close();
    }

    public class SnapshotWriter : ISnapshotWriter
    {
        private readonly Memory<byte> targetBuffer = new byte[1024];
        private readonly byte[] tempBuffer = new byte[1024];
        private int position;

        public void WriteEntity(EntityId entity)
        {
            //Debug.Log($"Writing entity {entity}");
            BitConverter.TryWriteBytes(targetBuffer.Span.Slice(position, sizeof(ulong)), entity.value);
            position += sizeof(ulong);
        }

        public void WriteComponentData<T>(T componentData) where T : unmanaged
        {
            var componentTypeId = DataIdLookup<T>.value;
            Debug.Log($"found componentType {componentTypeId}");
            BitConverter.TryWriteBytes(targetBuffer.Span.Slice(position, sizeof(ushort)), componentTypeId);
            position += sizeof(ushort);

            var octetCount = DataCopy.ToBytes(tempBuffer, ref componentData);
            var copy = new byte[octetCount];

            copy.CopyTo(targetBuffer.Span.Slice(position, octetCount));
            position += octetCount;

            Debug.Log($"wrote {Util.ByteArrayToString(copy)}");
        }

        public void WriteTagComponentData<T>() where T : unmanaged
        {
            var componentTypeId = DataIdLookup<T>.value;
            Debug.Log($"found tag componentType {componentTypeId}");
            BitConverter.TryWriteBytes(targetBuffer.Span.Slice(position, sizeof(ushort)), componentTypeId);
            position += sizeof(ushort);

            Debug.Log($"wrote tag component data");
        }

        public Span<byte> Close()
        {
            var x = new byte[position];
            Array.Copy(targetBuffer.ToArray(), x, position);
            return x;
        }
    }
}