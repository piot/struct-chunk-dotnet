using System;

namespace StructChunk
{
    public static class Util
    {
        public static string ByteArrayToString(byte[] octets)
        {
            return BitConverter.ToString(octets).Replace("-", "");
        }
    }
}