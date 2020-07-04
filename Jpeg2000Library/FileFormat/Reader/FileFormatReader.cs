using System;
using System.Collections.Generic;
using Jpeg2000Library.Exceptions;
using Jpeg2000Library.IO;

namespace Jpeg2000Library.FileFormat.Reader
{
    public class FileFormatReader
    {
        private class CodeStreamBox
        {
            public CodeStreamBox(long position, long length)
            {
                Length = length;
                Position = position;
            }
            public long Length { get; set; }
            public long Position { get; set; }
        }
        private List<CodeStreamBox> _codeStreamBoxes;
        private readonly IRandomAccessIO _input;
        public bool JP2FFUsed { get; private set; }
        public long FirstCodeStreamPosition => _codeStreamBoxes == null ? 0 : _codeStreamBoxes[0].Position;
        public FileFormatReader(IRandomAccessIO input)
        {
            _input = input;

            try
            {
                if (input.ReadInt() != 0x0000000c ||
                    input.ReadInt() != FileFormatBoxes.JP2_SIGNATURE_BOX ||
                    input.ReadInt() != 0x0d0a870a)
                {
                    input.Seek(0);
                    var marker = input.ReadShort();
                    if (marker != Markers.SOC)
                    {
                        throw new Exception("File is neither valid JP2 file nor valid JPEG 2000 codestream");
                    }
                    JP2FFUsed = false;
                    input.Seek(0);
                    return;
                }

                JP2FFUsed = true;

                // Read File Type box
                if (!ReadFileTypeBox())
                {
                    // Not a valid JP2 file or codestream
                    throw new Exception("Invalid JP2 file: File Type box missing");
                }

                var lastBoxFound = false;
                var jp2HeaderBoxFound = false;
                // Read all remaining boxes 
                while (!lastBoxFound)
                {
                    var pos = input.Position;
                    var length = input.ReadInt();
                    ulong longLength = 0;
                    if ((pos + length) == input.Length)
                        lastBoxFound = true;

                    var box = input.ReadInt();
                    if (length == 0)
                    {
                        lastBoxFound = true;
                        length = (uint)(input.Length - input.Position);
                    }
                    else if (length == 1)
                    {
                        longLength = input.ReadLong();
                        throw new Exception("File too long.");
                    }
                    else longLength = (long)0;

                    switch (box)
                    {
                        case FileFormatBoxes.CONTIGUOUS_CODESTREAM_BOX:
                            if (!jp2HeaderBoxFound)
                            {
                                throw new Exception("Invalid JP2 file: JP2Header box not found before Contiguous codestream box");
                            }
                            ReadContiguousCodeStreamBox(pos, length, longLength);
                            break;
                        case FileFormatBoxes.JP2_HEADER_BOX:
                            if (jp2HeaderBoxFound)
                                throw new Exception("Invalid JP2 file: Multiple JP2Header boxes found");
                            ReadJP2HeaderBox(pos, length, longLength);
                            jp2HeaderBoxFound = true;
                            break;
                        case FileFormatBoxes.INTELLECTUAL_PROPERTY_BOX:
                            ReadIntPropertyBox(length);
                            break;
                        case FileFormatBoxes.XML_BOX:
                            ReadXMLBox(length);
                            break;
                        case FileFormatBoxes.UUID_BOX:
                            ReadUUIDBox(length);
                            break;
                        case FileFormatBoxes.UUID_INFO_BOX:
                            ReadUUIDInfoBox(length);
                            break;
                        default:
                            //     FacilityManager.getMsgLogger().
                            //         printmsg(MsgLogger.WARNING,"Unknown box-type: 0x"+
                            //  Integer.toHexString(box));
                            break;
                    }

                    if (!lastBoxFound)
                        input.Seek(pos + length);
                }
            }
            catch (EndOfFileException e)
            {
                throw new Exception("EOF reached before finding Contiguous Codestream Box");
            }

            if (_codeStreamBoxes.Count == 0)
            {
                // Not a valid JP2 file or codestream
                throw new Exception("Invalid JP2 file: Contiguous codestream box missing");
            }
        }

        private bool ReadFileTypeBox()
        {
            var pos = _input.Position;
            // Read box length (LBox)
            var length = _input.ReadInt();
            ulong longLength;
            if (length == 0)
            { // This can not be last box
                throw new Exception("Zero-length of Profile Box.");
            }

            // Check that this is a File Type box (TBox)
            if (_input.ReadInt() != FileFormatBoxes.FILE_TYPE_BOX)
            {
                return false;
            }

            // Check for XLBox
            if (length == 1)
            {
                longLength = _input.ReadLong();
                throw new Exception("File too long.");
            }

            // Read Brand field
            _input.ReadInt();

            // Read MinV field
            _input.ReadInt();

            bool foundComp = false;
            // Check that there is at least one FT_BR entry in in
            // compatibility list
            var nComp = (length - 16) / 4;
            for (var i = nComp; i > 0; i--)
            {
                if (_input.ReadInt() == FileFormatBoxes.FT_BR)
                    foundComp = true;
            }
            return foundComp;
        }

        private bool ReadContiguousCodeStreamBox(long pos, long length, ulong longLength)
        {

            // Add new codestream position to position vector
            var ccpos = _input.Position;

            if (_codeStreamBoxes == null)
                _codeStreamBoxes = new List<CodeStreamBox>();
            _codeStreamBoxes.Add(new CodeStreamBox(ccpos, length));

            return true;
        }

        private bool ReadJP2HeaderBox(long pos, long length, ulong longLength)
        {
            if (length == 0)
            { // This can not be last box
                throw new Exception("Zero-length of JP2Header Box");
            }

            // Here the JP2Header data (DBox) would be read if we were to use it

            return true;
        }

        /** 
        * This method reads the contents of the Intellectual property box
        * */
        private void ReadIntPropertyBox(long length)
        {
        }

        /** 
         * This method reads the contents of the XML box
         * */
        private void ReadXMLBox(long length)
        {
        }

        /** 
         * This method reads the contents of the Intellectual property box
         * */
        private void ReadUUIDBox(long length)
        {
        }

        /** 
         * This method reads the contents of the Intellectual property box
         * */
        private void ReadUUIDInfoBox(long length)
        {
        }
    }
}