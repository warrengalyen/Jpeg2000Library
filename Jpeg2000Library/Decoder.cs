using System;
using System.IO;
using Jpeg2000Library.Exceptions;
using Jpeg2000Library.FileFormat.Reader;
using Jpeg2000Library.IO;

namespace Jpeg2000Library
{
    public class Decoder
    {

        public Decoder() { }

        public Image Decode(IRandomAccessIO input)
        {
            var fileFormatReader = new FileFormatReader(input);
            if (fileFormatReader.JP2FFUsed)
            {
                input.Seek(fileFormatReader.FirstCodeStreamPosition);
            }

            return null;
        }
    }
}
