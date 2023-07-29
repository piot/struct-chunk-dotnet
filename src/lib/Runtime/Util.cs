using System;

namespace Piot.StructChunk
{
    public static class Util
    {
        public static string ByteArrayToString(byte[] octets)
        {
            return BitConverter.ToString(octets).Replace("-", "");
        }

        public static string ByteArrayToString(Span<byte> octets)
        {
            return BitConverter.ToString(octets.ToArray()).Replace("-", "");
        }
    }
}