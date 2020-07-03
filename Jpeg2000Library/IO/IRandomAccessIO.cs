using System;
using System.Collections.Generic;
using System.Text;

namespace Jpeg2000Library.IO
{
    public interface IRandomAccessIO
    {
        long Position { get; }
        long Length { get; }
        void Seek(long offset);
        uint ReadInt();
        ushort ReadShort();
        ulong ReadLong();
    }
}
