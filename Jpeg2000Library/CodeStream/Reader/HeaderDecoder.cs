using System;
using System.Collections.Generic;
using Jpeg2000Library.Exceptions;
using Jpeg2000Library.IO;

namespace Jpeg2000Library.CodeStream.Reader
{
    public class HeaderDecoder
    {
        /** Flag bit for SIZ marker segment found */
        private const int SIZ_FOUND = 1;

        /** Flag bit for COD marker segment found */
        private const int COD_FOUND = 1 << 1;

        /** Flag bit for COC marker segment found */
        private const int COC_FOUND = 1 << 2;

        /** Flag bit for QCD marker segment found */
        private const int QCD_FOUND = 1 << 3;

        /** Flag bit for TLM marker segment found */
        private const int TLM_FOUND = 1 << 4;

        /** Flag bit for PLM marker segment found */
        private const int PLM_FOUND = 1 << 5;

        /** Flag bit for SOT marker segment found */
        private const int SOT_FOUND = 1 << 6;

        /** Flag bit for PLT marker segment found */
        private const int PLT_FOUND = 1 << 7;

        /** Flag bit for QCC marker segment found */
        private const int QCC_FOUND = 1 << 8;

        /** Flag bit for RGN marker segment found */
        private const int RGN_FOUND = 1 << 9;

        /** Flag bit for POC marker segment found */
        private const int POC_FOUND = 1 << 10;

        /** Flag bit for COM marker segment found */
        private const int COM_FOUND = 1 << 11;

        /** Flag bit for SOD marker segment found */
        public const int SOD_FOUND = 1 << 13;

        /** Flag bit for SOD marker segment found */
        public const int PPM_FOUND = 1 << 14;

        /** Flag bit for SOD marker segment found */
        public const int PPT_FOUND = 1 << 15;

        /** Flag bit for CRG marker segment found */
        public const int CRG_FOUND = 1 << 16;

        private IRandomAccessIO _input;
        private ParameterList _parameterList;
        private HeaderInfo _headerInfo;
        private long _mainHeadOff = 0;
        private int _nfMarkSeg = 0;
        private int _nQCCMarkSeg = 0;
        private int _nRGNMarkSeg = 0;
        private int _nCOMMarkSeg = 0;
        private int _nPPMMarkSeg = 0;
        private Dictionary<string, byte[]> _dictionary;

        public HeaderDecoder(IRandomAccessIO input, ParameterList parameterList, HeaderInfo headerInfo)
        {
            _input = input;
            _parameterList = parameterList;
            _headerInfo = headerInfo;

            _mainHeadOff = input.Position;
            if (_input.ReadShort() != Markers.SOC)
            {
                throw new CorruptedCodestreamException("SOC marker segment not found at the beginning of the codestream.");
            }

            // First Pass: Decode and store main header information until the SOT
            // marker segment is found.
            _nfMarkSeg = 0;
            do
            {
                ExtractMainMarkSeg(_input.ReadShort());
            } while ((_nfMarkSeg & SOT_FOUND) == 0); // Stop when SOT is found
            _input.Seek(_input.Position - 2); // Realign codestream on SOT marker

            // Second pass: Read each marker segment previously found
            //ReadFoundMainMarkSeg();
        }

        private void ExtractMainMarkSeg(ushort marker)
        {
            if (_nfMarkSeg == 0)
            {
                // First non-delimiting marker of the header
                // JPEG 2000 part specifies that is must be SIZ
                if (marker != Markers.SIZ)
                {
                    throw new CorruptedCodestreamException("First marker after SOC must be SIZ " + Convert.ToString(marker, 16));
                }
            }

            string dictKey = ""; // Name used as a key for the hash-table
            if (_dictionary == null)
            {
                _dictionary = new Dictionary<string, byte[]>();
            }

            switch (marker)
            {
                case Markers.SIZ:
                    if ((_nfMarkSeg & SIZ_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one SIZ marker segment found in main header");
                    }
                    _nfMarkSeg |= SIZ_FOUND;
                    dictKey = "SIZ";
                    break;
                case Markers.SOD:
                    throw new CorruptedCodestreamException("SOD found in main header");
                case Markers.EOC:
                    throw new CorruptedCodestreamException("EOC found in main header");
                case Markers.SOT:
                    if ((_nfMarkSeg & SOT_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one SOT marker found right after main or tile header");
                    }
                    _nfMarkSeg |= SOT_FOUND;
                    return;
                case Markers.COD:
                    if ((_nfMarkSeg & COD_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one COD marker found in main header");
                    }
                    _nfMarkSeg |= COD_FOUND;
                    dictKey = "COD";
                    break;
                case Markers.COC:
                    _nfMarkSeg |= COC_FOUND;
                    dictKey = "COC" + (_nfMarkSeg++);
                    break;
                case Markers.QCD:
                    if ((_nfMarkSeg & QCD_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one QCD marker found in main header");
                    }
                    _nfMarkSeg |= QCD_FOUND;
                    dictKey = "QCD";
                    break;
                case Markers.QCC:
                    _nfMarkSeg |= QCC_FOUND;
                    dictKey = "QCC" + (_nQCCMarkSeg++);
                    break;
                case Markers.RGN:
                    _nfMarkSeg |= RGN_FOUND;
                    dictKey = "RGN" + (_nRGNMarkSeg++);
                    break;
                case Markers.COM:
                    _nfMarkSeg |= COM_FOUND;
                    dictKey = "COM" + (_nCOMMarkSeg++);
                    break;
                case Markers.CRG:
                    if ((_nfMarkSeg & CRG_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one CRG marker found in main header");
                    }
                    _nfMarkSeg |= CRG_FOUND;
                    dictKey = "CRG";
                    break;
                case Markers.PPM:
                    _nfMarkSeg |= PPM_FOUND;
                    dictKey = "PPM" + (_nPPMMarkSeg++);
                    break;
                case Markers.TLM:
                    if ((_nfMarkSeg & TLM_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one TLM marker found in main header");
                    }
                    _nfMarkSeg |= TLM_FOUND;
                    break;
                case Markers.PLM:
                    if ((_nfMarkSeg & PLM_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one PLM marker found main header");
                    }
                    Logger.Warning("PLM marker segment found but not used by by JJ2000 decoder.");
                    _nfMarkSeg |= PLM_FOUND;
                    dictKey = "PLM";
                    break;
                case Markers.POC:
                    if ((_nfMarkSeg & POC_FOUND) != 0)
                    {
                        throw new CorruptedCodestreamException("More than one POC marker segment found in main header");
                    }
                    _nfMarkSeg |= POC_FOUND;
                    dictKey = "POC";
                    break;
                case Markers.PLT:
                    throw new CorruptedCodestreamException("PLT found in main header");
                case Markers.PPT:
                    throw new CorruptedCodestreamException("PPT found in main header");
                default:
                    dictKey = "UNKNOWN";
                    Logger.Warning("Non recognized marker segment (0x" + Convert.ToString(marker, 16) + ") in main header!");
                    break;
            }

            if (marker < 0xff30 || marker > 0xff3f)
            {
                // Read marker segment length and create corresponding byte buffer
                int markSegLen = _input.ReadShort();
                byte[] buf = new byte[markSegLen];

                // Copy data (after re-insertion of the marker segment length)
                buf[0] = (byte)((markSegLen >> 8) & 0xFF);
                buf[1] = (byte)(markSegLen & 0xFF);
                _input.ReadFully(buf, 2, markSegLen - 2);

                if (dictKey != "UNKNOWN")
                {
                    // Store array in hashTable
                    _dictionary.Add(dictKey, buf);
                }
            }
        }
    }
}
