namespace NoPasaranTD.Utilities
{
    public static class Binary
    {

        public static void WriteBytes(short obj, byte[] data, int offset)
        {
            for (; offset < 2; offset++)
                data[offset] = (byte)((obj >> (offset * 2)) & 0xFF);
        }

        public static void WriteBytes(ushort obj, byte[] data, int offset)
        {
            for (; offset < 2; offset++)
                data[offset] = (byte)((obj >> (offset * 2)) & 0xFF);
        }

        public static void WriteBytes(int obj, byte[] data, int offset)
        {
            for (; offset < 4; offset++)
                data[offset] = (byte)((obj >> (offset * 4)) & 0xFF);
        }

        public static void WriteBytes(uint obj, byte[] data, int offset)
        {
            for (; offset < 4; offset++)
                data[offset] = (byte)((obj >> (offset * 4)) & 0xFF);
        }

        public static void WriteBytes(long obj, byte[] data, int offset)
        {
            for (; offset < 8; offset++)
                data[offset] = (byte)((obj >> (offset * 8)) & 0xFF);
        }

        public static void WriteBytes(ulong obj, byte[] data, int offset)
        {
            for (; offset < 8; offset++)
                data[offset] = (byte)((obj >> (offset * 8)) & 0xFF);
        }

    }
}
