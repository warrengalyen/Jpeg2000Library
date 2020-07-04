using System;
using System.IO;

namespace Jpeg2000Library.IO
{
    public class BEBinaryReader
    {
        private readonly BinaryReader _reader;
        public BEBinaryReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public long Available => _reader.BaseStream.Length - _reader.BaseStream.Position;
        public int ReadUnsignedShort()
        {
            int ch1 = _reader.ReadByte();
            int ch2 = _reader.ReadByte();
            if ((ch1 | ch2) < 0)
                throw new EndOfStreamException();
            return (ch1 << 8) + (ch2 << 0);
        }

        public int ReadUnsignedByte()
        {
            return _reader.ReadByte();
        }
        public int ReadInt()
        {
            int ch1 = _reader.ReadByte();
            int ch2 = _reader.ReadByte();
            int ch3 = _reader.ReadByte();
            int ch4 = _reader.ReadByte();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
                throw new EndOfStreamException();
            return ((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0));
        }
        public byte ReadByte()
        {
            int ch = _reader.ReadByte();
            if (ch < 0)
                throw new EndOfStreamException();
            return (byte)(ch);
        }
        public int SkipBytes(int n)
        {
            int total = 0;
            int cur = 0;

            while ((total < n) && ((cur = (int)Skip(n - total)) > 0))
            {
                total += cur;
            }

            return total;
        }

        private const int MAX_SKIP_BUFFER_SIZE = 2048;

        public long Skip(long n)
        {

            long remaining = n;
            int nr;

            if (n <= 0)
            {
                return 0;
            }

            int size = (int)Math.Min(MAX_SKIP_BUFFER_SIZE, remaining);
            byte[] skipBuffer = new byte[size];
            while (remaining > 0)
            {
                nr = _reader.Read(skipBuffer, 0, (int)Math.Min(size, remaining));
                if (nr < 0)
                {
                    break;
                }
                remaining -= nr;
            }

            return n - remaining;
        }
    }
}