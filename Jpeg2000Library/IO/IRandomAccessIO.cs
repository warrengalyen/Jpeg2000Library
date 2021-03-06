﻿using System;
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
        ushort ReadUnsignedShort();
        ulong ReadLong();
        void ReadFully(byte[] buffer, long offset, long length);
    }
}
