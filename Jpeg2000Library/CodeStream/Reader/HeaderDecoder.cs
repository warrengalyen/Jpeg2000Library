using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Jpeg2000Library.Entropy;
using Jpeg2000Library.Exceptions;
using Jpeg2000Library.IO;
using Jpeg2000Library.ROI;
using Jpeg2000Library.Util;
using Jpeg2000Library.Wavelet;
using Jpeg2000Library.Wavelet.Synthesis;

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
        private NullableDictionary<string, byte[]> _dictionary;
        private int _nComp;
        private int _nTiles;
        private DecoderSpecs _decSpec;
        private int _nCOCMarkSeg;
        private int _cb0x = -1;
        private int _cb0y = -1;
        private bool _precinctPartitionIsUsed;
        private byte[][] _pPMMarkerData;
        private List<int> _tileOfTileParts;

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
            // marker segment is found
            _nfMarkSeg = 0;
            do
            {
                ExtractMainMarkSeg(_input.ReadShort());
            } while ((_nfMarkSeg & SOT_FOUND) == 0); //Stop when SOT is found
            _input.Seek(_input.Position - 2); // Realign codestream on SOT marker

            // Second pass: Read each marker segment previously found
            ReadFoundMainMarkSeg();
        }

        private void ExtractMainMarkSeg(ushort marker)
        {
            if (_nfMarkSeg == 0)
            { // First non-delimiting marker of the header
              // JPEG 2000 part 1 specify that it must be SIZ
                if (marker != Markers.SIZ)
                {
                    throw new CorruptedCodestreamException("First marker after SOC must be SIZ " + Convert.ToString(marker, 16));
                }
            }

            string dictKey = ""; // Name used as a key for the hash-table
            if (_dictionary == null)
            {
                _dictionary = new NullableDictionary<string, byte[]>();
            }

            switch (marker)
            {
                case Markers.SIZ:
                    if ((_nfMarkSeg & SIZ_FOUND) != 0)
                    {
                        throw
                          new CorruptedCodestreamException("More than one SIZ marker segment found in main header");
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
                    dictKey = "COC" + (_nCOCMarkSeg++);
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
                        throw new CorruptedCodestreamException("More than one PLM marker found iain header");
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
                int markSegLen = _input.ReadUnsignedShort();
                byte[] buf = new byte[markSegLen];

                // Copy data (after re-insertion of the marker segment length);
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

        /** 
        * Retrieves and reads all marker segments found in the main header during
        * the first pass.
        * */
        private void ReadFoundMainMarkSeg()
        {
            //DataInputStream dis;
            MemoryStream bais;

            // SIZ marker segment
            if ((_nfMarkSeg & SIZ_FOUND) != 0)
            {
                bais = new MemoryStream(_dictionary["SIZ"]);
                ReadSIZ(new BEBinaryReader(bais));
            }

            // COM marker segments
            if ((_nfMarkSeg & COM_FOUND) != 0)
            {
                for (int i = 0; i < _nCOMMarkSeg; i++)
                {
                    bais = new MemoryStream(_dictionary["COM" + i]);
                    ReadCOM(new BEBinaryReader(bais), true, 0, i);
                }
            }

            // CRG marker segment
            if ((_nfMarkSeg & CRG_FOUND) != 0)
            {
                bais = new MemoryStream(_dictionary["CRG"]);
                ReadCRG(new BEBinaryReader(bais));
            }

            // COD marker segment
            if ((_nfMarkSeg & COD_FOUND) != 0)
            {
                bais = new MemoryStream(_dictionary["COD"]);
                ReadCOD(new BEBinaryReader(bais), true, 0, 0);
            }

            // COC marker segments
            if ((_nfMarkSeg & COC_FOUND) != 0)
            {
                for (int i = 0; i < _nCOCMarkSeg; i++)
                {
                    bais = new MemoryStream(_dictionary["COC" + i]);
                    ReadCOC(new BEBinaryReader(bais), true, 0, 0);
                }
            }

            // RGN marker segment
            if ((_nfMarkSeg & RGN_FOUND) != 0)
            {
                for (int i = 0; i < _nRGNMarkSeg; i++)
                {
                    bais = new MemoryStream(_dictionary["RGN" + i]);
                    ReadRGN(new BEBinaryReader(bais), true, 0, 0);
                }
            }

            // // QCD marker segment
            // if ((_nfMarkSeg & QCD_FOUND) != 0)
            // {
            //     bais = new MemoryStream(_dictionary["QCD"]);
            //     ReadQCD(new BEBinaryReader(bais), true, 0, 0);
            // }

            // // QCC marker segments
            // if ((_nfMarkSeg & QCC_FOUND) != 0)
            // {
            //     for (int i = 0; i < _nQCCMarkSeg; i++)
            //     {
            //         bais = new MemoryStream(_dictionary["QCC" + i]);
            //         ReadQCC(new BEBinaryReader(bais), true, 0, 0);
            //     }
            // }

            // POC marker segment
            if ((_nfMarkSeg & POC_FOUND) != 0)
            {
                bais = new MemoryStream(_dictionary["POC"]);
                ReadPOC(new BEBinaryReader(bais), true, 0, 0);
            }

            // PPM marker segments
            if ((_nfMarkSeg & PPM_FOUND) != 0)
            {
                for (int i = 0; i < _nPPMMarkSeg; i++)
                {
                    bais = new MemoryStream(_dictionary["PPM" + i]);
                    ReadPPM(new BEBinaryReader(bais));
                }
            }

            // Reset the hashtable
            _dictionary = null;
        }

        private void ReadSIZ(BEBinaryReader ehs)
        {
            HeaderInfo.SIZ ms = new HeaderInfo.SIZ();
            _headerInfo.siz = ms;

            // Read the length of SIZ marker segment (Lsiz)
            ms.lsiz = ehs.ReadUnsignedShort();

            // Read the capability of the codestream (Rsiz)
            ms.rsiz = ehs.ReadUnsignedShort();
            if (ms.rsiz > 2)
            {
                throw new Exception("Codestream capabiities not JPEG 2000 - Part I compliant");
            }

            // Read image size
            ms.xsiz = ehs.ReadInt();
            ms.ysiz = ehs.ReadInt();
            if (ms.xsiz <= 0 || ms.ysiz <= 0)
            {
                throw new Exception("JJ2000 does not support images whose width and/or height not in the range: 1 -- (2^31)-1");
            }

            // Read image offset
            ms.x0siz = ehs.ReadInt();
            ms.y0siz = ehs.ReadInt();
            if (ms.x0siz < 0 || ms.y0siz < 0)
            {
                throw new Exception("JJ2000 does not support images offset not in the range: 0 -- (2^31)-1");
            }

            // Read size of tile
            ms.xtsiz = ehs.ReadInt();
            ms.ytsiz = ehs.ReadInt();
            if (ms.xtsiz <= 0 || ms.ytsiz <= 0)
            {
                throw new Exception("JJ2000 does not support tiles whose width and/or height are not in the range: 1 -- (2^31)-1");
            }

            // Read upper-left tile offset
            ms.xt0siz = ehs.ReadInt();
            ms.yt0siz = ehs.ReadInt();
            if (ms.xt0siz < 0 || ms.yt0siz < 0)
            {
                throw new Exception("JJ2000 does not support tiles whose offset is not in the range: 0 -- (2^31)-1");
            }

            // Read number of components and initialize related arrays
            _nComp = ms.csiz = ehs.ReadUnsignedShort();
            if (_nComp < 1 || _nComp > 16384)
            {
                throw new ArgumentException("Number of component out of range 1--16384: " + _nComp);
            }

            ms.ssiz = new int[_nComp];
            ms.xrsiz = new int[_nComp];
            ms.yrsiz = new int[_nComp];

            // Read bit-depth and down-sampling factors of each component
            for (int i = 0; i < _nComp; i++)
            {
                ms.ssiz[i] = ehs.ReadUnsignedByte();
                ms.xrsiz[i] = ehs.ReadUnsignedByte();
                ms.yrsiz[i] = ehs.ReadUnsignedByte();
            }

            // // Check marker length
            CheckMarkerLength(ehs, "SIZ marker");

            // Create needed ModuleSpec
            _nTiles = ms.getNumTiles();

            // Finish initialization of decSpec
            _decSpec = new DecoderSpecs(_nTiles, _nComp);
        }
        private void ReadCOM(BEBinaryReader ehs, bool mainh, int tileIdx,
                          int comIdx)
        {
            HeaderInfo.COM ms = new HeaderInfo.COM();

            // Read length of COM field
            ms.lcom = ehs.ReadUnsignedShort();

            // Read the registration value of the COM marker segment
            ms.rcom = ehs.ReadUnsignedShort();
            switch (ms.rcom)
            {
                case Markers.RCOM_GEN_USE:
                    ms.ccom = new byte[ms.lcom - 4];
                    for (int i = 0; i < ms.lcom - 4; i++)
                    {
                        ms.ccom[i] = ehs.ReadByte();
                    }
                    break;
                default:
                    // --- Unknown or unsupported markers ---
                    // (skip them and see if we can get way with it)
                    Logger.Warning(
                                 "COM marker registered as 0x" + Convert.ToString(ms.rcom, 16) +
                                 " unknown, ignoring (this might crash the decoder or decode a quality degraded or even useless image)");
                    ehs.SkipBytes(ms.lcom - 4); //Ignore this field for the moment
                    break;
            }

            if (mainh)
            {
                _headerInfo.com.Add("main_" + comIdx, ms);
            }
            else
            {
                _headerInfo.com.Add("t" + tileIdx + "_" + comIdx, ms);
            }

            // Check marker length
            CheckMarkerLength(ehs, "COM marker");
        }

        private void ReadCRG(BEBinaryReader ehs)
        {
            HeaderInfo.CRG ms = new HeaderInfo.CRG();
            _headerInfo.crg = ms;

            ms.lcrg = ehs.ReadUnsignedShort();
            ms.xcrg = new int[_nComp];
            ms.ycrg = new int[_nComp];

            Logger.Warning("Information in CRG marker segment not taken into account. This may affect the display of the decoded image.");
            for (int c = 0; c < _nComp; c++)
            {
                ms.xcrg[c] = ehs.ReadUnsignedShort();
                ms.ycrg[c] = ehs.ReadUnsignedShort();
            }

            // Check marker length
            CheckMarkerLength(ehs, "CRG marker");
        }

        private void ReadCOD(BEBinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
        {
            int cstyle;         // The block style
            SynWTFilter[] hfilters, vfilters;
            int l;
            int[] cblk;
            String errMsg;
            bool sopUsed = false;
            bool ephUsed = false;
            HeaderInfo.COD ms = new HeaderInfo.COD();

            // Lcod (marker length)
            ms.lcod = ehs.ReadUnsignedShort();

            // Scod (block style)
            // We only support wavelet transformed data
            cstyle = ms.scod = ehs.ReadUnsignedByte();

            if ((cstyle & Markers.SCOX_PRECINCT_PARTITION) != 0)
            {
                _precinctPartitionIsUsed = true;
                // Remove flag
                cstyle &= ~(Markers.SCOX_PRECINCT_PARTITION);
            }
            else
            {
                _precinctPartitionIsUsed = false;
            }

            // SOP markers
            if (mainh)
            {
                _headerInfo.cod.Add("main", ms);

                if ((cstyle & Markers.SCOX_USE_SOP) != 0)
                {
                    // SOP markers are used
                    _decSpec.sops.setDefault(true);
                    sopUsed = true;
                    // Remove flag
                    cstyle &= ~(Markers.SCOX_USE_SOP);
                }
                else
                {
                    // SOP markers are not used
                    _decSpec.sops.setDefault(false);
                }
            }
            else
            {
                _headerInfo.cod.Add("t" + tileIdx, ms);

                if ((cstyle & Markers.SCOX_USE_SOP) != 0)
                {
                    // SOP markers are used
                    _decSpec.sops.setTileDef(tileIdx, true);
                    sopUsed = true;
                    // Remove flag
                    cstyle &= ~(Markers.SCOX_USE_SOP);
                }
                else
                {
                    // SOP markers are not used
                    _decSpec.sops.setTileDef(tileIdx, false);
                }
            }

            // EPH markers
            if (mainh)
            {
                if ((cstyle & Markers.SCOX_USE_EPH) != 0)
                {
                    // EPH markers are used
                    _decSpec.ephs.setDefault(true);
                    ephUsed = true;
                    // Remove flag
                    cstyle &= ~(Markers.SCOX_USE_EPH);
                }
                else
                {
                    // EPH markers are not used
                    _decSpec.ephs.setDefault(false);
                }
            }
            else
            {
                if ((cstyle & Markers.SCOX_USE_EPH) != 0)
                {
                    // EPH markers are used
                    _decSpec.ephs.setTileDef(tileIdx, true);
                    ephUsed = true;
                    // Remove flag
                    cstyle &= ~(Markers.SCOX_USE_EPH);
                }
                else
                {
                    // EPH markers are not used
                    _decSpec.ephs.setTileDef(tileIdx, false);
                }
            }

            // Code-block partition origin
            if ((cstyle & (Markers.SCOX_HOR_CB_PART | Markers.SCOX_VER_CB_PART)) != 0)
            {
                Logger.Warning("Code-block partition origin different from (0,0). This is defined in JPEG 2000 part 2 and may not be supported by all JPEG 2000 decoders.");
            }
            if ((cstyle & Markers.SCOX_HOR_CB_PART) != 0)
            {
                if (_cb0x != -1 && _cb0x == 0)
                {
                    throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
                }
                _cb0x = 1;
                cstyle &= ~(Markers.SCOX_HOR_CB_PART);
            }
            else
            {
                if (_cb0x != -1 && _cb0x == 1)
                {
                    throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
                }
                _cb0x = 0;
            }
            if ((cstyle & Markers.SCOX_VER_CB_PART) != 0)
            {
                if (_cb0y != -1 && _cb0y == 0)
                {
                    throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
                }
                _cb0y = 1;
                cstyle &= ~(Markers.SCOX_VER_CB_PART);
            }
            else
            {
                if (_cb0y != -1 && _cb0y == 1)
                {
                    throw new ArgumentException("Code-block partition origin redefined in new COD marker segment. Not supported by JJ2000");
                }
                _cb0y = 0;
            }

            // SGcod
            // Read the progressive order
            ms.sgcod_po = ehs.ReadUnsignedByte();

            // Read the number of layers
            ms.sgcod_nl = ehs.ReadUnsignedShort();
            if (ms.sgcod_nl <= 0 || ms.sgcod_nl > 65535)
            {
                throw new CorruptedCodestreamException("Number of layers out of range: 1--65535");
            }

            // Multiple component transform
            ms.sgcod_mct = ehs.ReadUnsignedByte();

            // SPcod
            // decomposition levels
            int mrl = ms.spcod_ndl = ehs.ReadUnsignedByte();
            if (mrl > 32)
            {
                throw new CorruptedCodestreamException("Number of decomposition levels out of range: 0--32");
            }

            // Read the code-blocks dimensions
            cblk = new int[2];
            ms.spcod_cw = ehs.ReadUnsignedByte();
            cblk[0] = (1 << (ms.spcod_cw + 2));
            if (cblk[0] < StdEntropyCoderOptions.MIN_CB_DIM ||
                 cblk[0] > StdEntropyCoderOptions.MAX_CB_DIM)
            {
                errMsg = "Non-valid code-block width in SPcod field, COD marker";
                throw new CorruptedCodestreamException(errMsg);
            }
            ms.spcod_ch = ehs.ReadUnsignedByte();
            cblk[1] = (1 << (ms.spcod_ch + 2));
            if (cblk[1] < StdEntropyCoderOptions.MIN_CB_DIM ||
                 cblk[1] > StdEntropyCoderOptions.MAX_CB_DIM)
            {
                errMsg = "Non-valid code-block height in SPcod field, COD marker";
                throw new CorruptedCodestreamException(errMsg);
            }
            if ((cblk[0] * cblk[1]) > StdEntropyCoderOptions.MAX_CB_AREA)
            {
                errMsg = "Non-valid code-block area in SPcod field, COD marker";
                throw new CorruptedCodestreamException(errMsg);
            }
            if (mainh)
            {
                _decSpec.cblks.setDefault(cblk);
            }
            else
            {
                _decSpec.cblks.setTileDef(tileIdx, cblk);
            }

            // Style of the code-block coding passes
            int ecOptions = ms.spcod_cs = ehs.ReadUnsignedByte();
            if ((ecOptions &
                 ~(StdEntropyCoderOptions.OPT_BYPASS | StdEntropyCoderOptions.OPT_RESET_MQ | StdEntropyCoderOptions.OPT_TERM_PASS |
               StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL | StdEntropyCoderOptions.OPT_PRED_TERM | StdEntropyCoderOptions.OPT_SEG_SYMBOLS)) != 0)
            {
                throw
                    new CorruptedCodestreamException("Unknown \"code-block style\" in SPcod field, COD marker: 0x" + Convert.ToString(ecOptions, 16));
            }

            // Read wavelet filter for tile or image
            hfilters = new SynWTFilter[1];
            vfilters = new SynWTFilter[1];
            hfilters[0] = ReadFilter(ehs, ms.spcod_t);
            vfilters[0] = hfilters[0];

            // Fill the filter spec
            // If this is the main header, set the default value, if it is the
            // tile header, set default for this tile 
            SynWTFilter[][] hvfilters = new SynWTFilter[2][];
            hvfilters[0] = hfilters;
            hvfilters[1] = vfilters;

            // Get precinct partition sizes
            var v = new List<int>[2];
            v[0] = new List<int>();
            v[1] = new List<int>();
            int val = Markers.PRECINCT_PARTITION_DEF_SIZE;
            if (!_precinctPartitionIsUsed)
            {
                int w, h;
                w = (1 << (val & 0x000F));
                v[0].Add(w);
                h = (1 << (((val & 0x00F0) >> 4)));
                v[1].Add(h);
            }
            else
            {
                ms.spcod_ps = new int[mrl + 1];
                for (int rl = mrl; rl >= 0; rl--)
                {
                    int w, h;
                    val = ms.spcod_ps[mrl - rl] = ehs.ReadUnsignedByte();
                    w = (1 << (val & 0x000F));
                    v[0].Insert(0, w);
                    h = (1 << (((val & 0x00F0) >> 4)));
                    v[1].Insert(0, h);
                }
            }
            if (mainh)
            {
                _decSpec.pss.setDefault(v);
            }
            else
            {
                _decSpec.pss.setTileDef(tileIdx, v);
            }
            _precinctPartitionIsUsed = true;

            // Check marker length
            CheckMarkerLength(ehs, "COD marker");

            // Store specifications in decSpec
            if (mainh)
            {
                _decSpec.wfs.setDefault(hvfilters);
                _decSpec.dls.setDefault(mrl);
                _decSpec.ecopts.setDefault(ecOptions);
                _decSpec.cts.setDefault(ms.sgcod_mct);
                _decSpec.nls.setDefault(ms.sgcod_nl);
                _decSpec.pos.setDefault(ms.sgcod_po);
            }
            else
            {
                _decSpec.wfs.setTileDef(tileIdx, hvfilters);
                _decSpec.dls.setTileDef(tileIdx, mrl);
                _decSpec.ecopts.setTileDef(tileIdx, ecOptions);
                _decSpec.cts.setTileDef(tileIdx, ms.sgcod_mct);
                _decSpec.nls.setTileDef(tileIdx, ms.sgcod_nl);
                _decSpec.pos.setTileDef(tileIdx, ms.sgcod_po);
            }
        }

        /**
         * Reads the COC marker segment and realigns the codestream where the next
         * marker should be found.
         *
         * @param ehs The encoder header stream.
         *
         * @param mainh Flag indicating whether or not this marker segment is read
         * from the main header.
         *
         * @param tileIdx The index of the current tile
         *
         * @param tpIdx Tile-part index
         *
         * @exception IOException If an I/O error occurs while reading from the
         * encoder header stream
         * */
        private void ReadCOC(BEBinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
        {
            int cComp;         // current component
            SynWTFilter[] hfilters, vfilters;
            int tmp, l;
            int ecOptions;
            int[] cblk;
            String errMsg;
            HeaderInfo.COC ms = new HeaderInfo.COC();

            // Lcoc (marker length)
            ms.lcoc = ehs.ReadUnsignedShort();

            // Ccoc
            if (_nComp < 257)
            {
                cComp = ms.ccoc = ehs.ReadUnsignedByte();
            }
            else
            {
                cComp = ms.ccoc = ehs.ReadUnsignedShort();
            }
            if (cComp >= _nComp)
            {
                throw new CorruptedCodestreamException("Invalid component index in QCC marker");
            }

            // Scoc (block style)
            int cstyle = ms.scoc = ehs.ReadUnsignedByte();
            if ((cstyle & Markers.SCOX_PRECINCT_PARTITION) != 0)
            {
                _precinctPartitionIsUsed = true;
                // Remove flag
                cstyle &= ~(Markers.SCOX_PRECINCT_PARTITION);
            }
            else
            {
                _precinctPartitionIsUsed = false;
            }

            // SPcoc

            // decomposition levels
            int mrl = ms.spcoc_ndl = ehs.ReadUnsignedByte();

            // Read the code-blocks dimensions
            cblk = new int[2];
            ms.spcoc_cw = ehs.ReadUnsignedByte();
            cblk[0] = (1 << (ms.spcoc_cw + 2));
            if (cblk[0] < StdEntropyCoderOptions.MIN_CB_DIM ||
                 cblk[0] > StdEntropyCoderOptions.MAX_CB_DIM)
            {
                errMsg = "Non-valid code-block width in SPcod field, COC marker";
                throw new CorruptedCodestreamException(errMsg);
            }
            ms.spcoc_ch = ehs.ReadUnsignedByte();
            cblk[1] = (1 << (ms.spcoc_ch + 2));
            if (cblk[1] < StdEntropyCoderOptions.MIN_CB_DIM ||
                 cblk[1] > StdEntropyCoderOptions.MAX_CB_DIM)
            {
                errMsg = "Non-valid code-block height in SPcod field, COC marker";
                throw new CorruptedCodestreamException(errMsg);
            }
            if ((cblk[0] * cblk[1]) >
                 StdEntropyCoderOptions.MAX_CB_AREA)
            {
                errMsg = "Non-valid code-block area in SPcod field, COC marker";
                throw new CorruptedCodestreamException(errMsg);
            }
            if (mainh)
            {
                _decSpec.cblks.setCompDef(cComp, cblk);
            }
            else
            {
                _decSpec.cblks.setTileCompVal(tileIdx, cComp, cblk);
            }

            // Read entropy block mode options
            // NOTE: currently OPT_SEG_SYMBOLS is not included here
            ecOptions = ms.spcoc_cs = ehs.ReadUnsignedByte();
            if ((ecOptions &
                 ~(StdEntropyCoderOptions.OPT_BYPASS | StdEntropyCoderOptions.OPT_RESET_MQ | StdEntropyCoderOptions.OPT_TERM_PASS |
               StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL | StdEntropyCoderOptions.OPT_PRED_TERM | StdEntropyCoderOptions.OPT_SEG_SYMBOLS)) != 0)
            {
                throw
                    new CorruptedCodestreamException("Unknown \"code-block context\" in SPcoc field, COC marker: 0x" +
                                                     Convert.ToString(ecOptions, 16));
            }

            // Read wavelet filter for tile or image
            hfilters = new SynWTFilter[1];
            vfilters = new SynWTFilter[1];
            hfilters[0] = ReadFilter(ehs, ms.spcoc_t);
            vfilters[0] = hfilters[0];

            // Fill the filter spec
            // If this is the main header, set the default value, if it is the
            // tile header, set default for this tile 
            SynWTFilter[][] hvfilters = new SynWTFilter[2][];
            hvfilters[0] = hfilters;
            hvfilters[1] = vfilters;

            // Get precinct partition sizes
            var v = new List<int>[2];
            v[0] = new List<int>();
            v[1] = new List<int>();
            int val = Markers.PRECINCT_PARTITION_DEF_SIZE;
            if (!_precinctPartitionIsUsed)
            {
                int w, h;
                w = (1 << (val & 0x000F));
                v[0].Add(w);
                h = (1 << (((val & 0x00F0) >> 4)));
                v[1].Add(h);
            }
            else
            {
                ms.spcoc_ps = new int[mrl + 1];
                for (int rl = mrl; rl >= 0; rl--)
                {
                    int w, h;
                    val = ms.spcoc_ps[rl] = ehs.ReadUnsignedByte();
                    w = (1 << (val & 0x000F));
                    v[0].Insert(0, w);
                    h = (1 << (((val & 0x00F0) >> 4)));
                    v[1].Insert(0, h);
                }
            }
            if (mainh)
            {
                _decSpec.pss.setCompDef(cComp, v);
            }
            else
            {
                _decSpec.pss.setTileCompVal(tileIdx, cComp, v);
            }
            _precinctPartitionIsUsed = true;

            // Check marker length
            CheckMarkerLength(ehs, "COD marker");

            if (mainh)
            {
                _headerInfo.coc.Add("main_c" + cComp, ms);
                _decSpec.wfs.setCompDef(cComp, hvfilters);
                _decSpec.dls.setCompDef(cComp, mrl);
                _decSpec.ecopts.setCompDef(cComp, ecOptions);
            }
            else
            {
                _headerInfo.coc.Add("t" + tileIdx + "_c" + cComp, ms);
                _decSpec.wfs.setTileCompVal(tileIdx, cComp, hvfilters);
                _decSpec.dls.setTileCompVal(tileIdx, cComp, mrl);
                _decSpec.ecopts.setTileCompVal(tileIdx, cComp, ecOptions);
            }
        }

        /** 
         * Reads the POC marker segment and realigns the codestream where the next
         * marker should be found.
         *
         * @param ehs The encoder header stream.
         *
         * @param mainh Flag indicating whether or not this marker segment is read
         * from the main header.
         *
         * @param t The index of the current tile
         *
         * @param tpIdx Tile-part index
         *
         * @exception IOException If an I/O error occurs while reading from the
         * encoder header stream
         * */
        private void ReadPOC(BEBinaryReader ehs, bool mainh, int t, int tpIdx)
        {

            bool useShort = (_nComp >= 256) ? true : false;
            int tmp;
            int nOldChg = 0;
            HeaderInfo.POC ms;
            if (mainh || _headerInfo.poc["t" + t] == null)
            {
                ms = new HeaderInfo.POC();
            }
            else
            {
                ms = (HeaderInfo.POC)_headerInfo.poc["t" + t];
                nOldChg = ms.rspoc.Length;
            }

            // Lpoc
            ms.lpoc = ehs.ReadUnsignedShort();

            // Compute the number of new progression changes
            // newChg = (lpoc - Lpoc(2)) / (RSpoc(1) + CSpoc(2) +
            //  LYEpoc(2) + REpoc(1) + CEpoc(2) + Ppoc (1) )
            int newChg = (ms.lpoc - 2) / (5 + (useShort ? 4 : 2));
            int ntotChg = nOldChg + newChg;

            int[][] change;
            if (nOldChg != 0)
            {
                // Creates new arrays
                change = new int[ntotChg][];
                for (int i = 0; i < change.Length; i++) change[i] = new int[6];
                int[] tmprspoc = new int[ntotChg];
                int[] tmpcspoc = new int[ntotChg];
                int[] tmplyepoc = new int[ntotChg];
                int[] tmprepoc = new int[ntotChg];
                int[] tmpcepoc = new int[ntotChg];
                int[] tmpppoc = new int[ntotChg];

                // Copy old values
                int[][] prevChg = (int[][])_decSpec.pcs.getTileDef(t);
                for (int chg = 0; chg < nOldChg; chg++)
                {
                    change[chg] = prevChg[chg];
                    tmprspoc[chg] = ms.rspoc[chg];
                    tmpcspoc[chg] = ms.cspoc[chg];
                    tmplyepoc[chg] = ms.lyepoc[chg];
                    tmprepoc[chg] = ms.repoc[chg];
                    tmpcepoc[chg] = ms.cepoc[chg];
                    tmpppoc[chg] = ms.ppoc[chg];
                }
                ms.rspoc = tmprspoc;
                ms.cspoc = tmpcspoc;
                ms.lyepoc = tmplyepoc;
                ms.repoc = tmprepoc;
                ms.cepoc = tmpcepoc;
                ms.ppoc = tmpppoc;
            }
            else
            {
                change = new int[newChg][];
                for (int i = 0; i < change.Length; i++) change[i] = new int[6];
                ms.rspoc = new int[newChg];
                ms.cspoc = new int[newChg];
                ms.lyepoc = new int[newChg];
                ms.repoc = new int[newChg];
                ms.cepoc = new int[newChg];
                ms.ppoc = new int[newChg];
            }

            for (int chg = nOldChg; chg < ntotChg; chg++)
            {
                // RSpoc
                change[chg][0] = ms.rspoc[chg] = ehs.ReadUnsignedByte();

                // CSpoc
                if (useShort)
                {
                    change[chg][1] = ms.cspoc[chg] = ehs.ReadUnsignedShort();
                }
                else
                {
                    change[chg][1] = ms.cspoc[chg] = ehs.ReadUnsignedByte();
                }

                // LYEpoc
                change[chg][2] = ms.lyepoc[chg] = ehs.ReadUnsignedShort();
                if (change[chg][2] < 1)
                {
                    throw new CorruptedCodestreamException
                        ("LYEpoc value must be greater than 1 in POC marker " +
                         "segment of tile " + t + ", tile-part " + tpIdx);
                }

                // REpoc
                change[chg][3] = ms.repoc[chg] = ehs.ReadUnsignedByte();
                if (change[chg][3] <= change[chg][0])
                {
                    throw new CorruptedCodestreamException
                        ("REpoc value must be greater than RSpoc in POC marker " +
                         "segment of tile " + t + ", tile-part " + tpIdx);
                }

                // CEpoc
                if (useShort)
                {
                    change[chg][4] = ms.cepoc[chg] = ehs.ReadUnsignedShort();
                }
                else
                {
                    tmp = ms.cepoc[chg] = ehs.ReadUnsignedByte();
                    if (tmp == 0)
                    {
                        change[chg][4] = 0;
                    }
                    else
                    {
                        change[chg][4] = tmp;
                    }
                }
                if (change[chg][4] <= change[chg][1])
                {
                    throw new CorruptedCodestreamException
                        ("CEpoc value must be greater than CSpoc in POC marker " +
                         "segment of tile " + t + ", tile-part " + tpIdx);
                }

                // Ppoc
                change[chg][5] = ms.ppoc[chg] = ehs.ReadUnsignedByte();
            }

            // Check marker length
            CheckMarkerLength(ehs, "POC marker");

            // Register specifications
            if (mainh)
            {
                _headerInfo.poc.Add("main", ms);
                _decSpec.pcs.setDefault(change);
            }
            else
            {
                _headerInfo.poc.Add("t" + t, ms);
                _decSpec.pcs.setTileDef(t, change);
            }
        }

        private SynWTFilter ReadFilter(BEBinaryReader ehs, int[] filtIdx)
        {
            int kid; // the filter id

            kid = filtIdx[0] = ehs.ReadUnsignedByte();
            if (kid >= (1 << 7))
            {
                throw new NotImplementedException("Custom filters not supported");
            }
            // Return filter based on ID
            switch (kid)
            {
                case FilterTypes.W9X7:
                    return new SynWTFilterFloatLift9x7();
                case FilterTypes.W5X3:
                    return new SynWTFilterIntLift5x3();
                default:
                    throw new CorruptedCodestreamException("Specified wavelet filter not JPEG 2000 part I compliant");
            }
        }

        /**
             * Reads the PPM marker segment of the main header.
             *
             * @param ehs The encoder header stream.
             *
             * @exception IOException If an I/O error occurs while reading from the
             * encoder header stream
             * */
        private void ReadPPM(BEBinaryReader ehs)
        {
            int curMarkSegLen;
            int i, indx, len, off;
            int remSegLen;
            byte[] b;

            // If first time readPPM method is called allocate arrays for packed
            // packet data
            if (_pPMMarkerData == null)
            {
                _pPMMarkerData = new byte[_nPPMMarkSeg][];
                _tileOfTileParts = new List<int>();
                _decSpec.pphs.setDefault(true);
            }

            // Lppm (marker length)
            curMarkSegLen = ehs.ReadUnsignedShort();
            remSegLen = curMarkSegLen - 3;

            // Zppm (index of PPM marker)
            indx = ehs.ReadUnsignedByte();

            // Read Nppm and Ippm data 
            _pPMMarkerData[indx] = new byte[remSegLen];
            ehs.Read(_pPMMarkerData[indx], 0, remSegLen);

            // Check marker length
            CheckMarkerLength(ehs, "PPM marker");
        }

        /**
         * Reads the RGN marker segment of the codestream header.
         *
         * <p>May be used in tile or main header. If used in main header, it
         * refers to the maxshift value of a component in all tiles. When used in
         * tile header, only the particular tile-component is affected.</p>
         *
         * @param ehs The encoder header stream.
         *
         * @param mainh Flag indicating whether or not this marker segment is read
         * from the main header.
         *
         * @param tileIdx The index of the current tile
         *
         * @param tpIdx Tile-part index
         *
         * @exception IOException If an I/O error occurs while reading from the
         * encoder header stream
         * */
        private void ReadRGN(BEBinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
        {
            int comp;           // ROI component
            int i;              // loop variable
            int tempComp;       // Component for
            HeaderInfo.RGN ms = new HeaderInfo.RGN();

            // Lrgn (marker length)
            ms.lrgn = ehs.ReadUnsignedShort();

            // Read component
            ms.crgn = comp = (_nComp < 257) ? ehs.ReadUnsignedByte() :
                ehs.ReadUnsignedShort();
            if (comp >= _nComp)
            {
                throw new CorruptedCodestreamException("Invalid component " +
                                                      "index in RGN marker" +
                                                      comp);
            }

            // Read type of RGN.(Srgn) 
            ms.srgn = ehs.ReadUnsignedByte();

            // Check that we can handle it.
            if (ms.srgn != Markers.SRGN_IMPLICIT)
                throw new CorruptedCodestreamException("Unknown or unsupported " +
                                                      "Srgn parameter in ROI " +
                                                      "marker");

            if (_decSpec.rois == null)
            { // No maxshift spec defined
              // Create needed ModuleSpec
                _decSpec.rois = new MaxShiftSpec(_nTiles, _nComp,
                                                  ModuleSpec.SPEC_TYPE_TILE_COMP);
            }

            // SPrgn
            ms.sprgn = ehs.ReadUnsignedByte();

            if (mainh)
            {
                _headerInfo.rgn.Add("main_c" + comp, ms);
                _decSpec.rois.setCompDef(comp, ms.sprgn);
            }
            else
            {
                _headerInfo.rgn.Add("t" + tileIdx + "_c" + comp, ms);
                _decSpec.rois.setTileCompVal(tileIdx, comp, ms.sprgn);
            }

            // Check marker length
            CheckMarkerLength(ehs, "RGN marker");
        }

        private void CheckMarkerLength(BEBinaryReader ehs, String str)
        {
            if (ehs.Available != 0)
            {
                Logger.Warning(str + " length was short, attempting to resync.");
            }
        }
    }
}
