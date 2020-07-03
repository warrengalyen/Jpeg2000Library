public class Markers
{

    // ----> Delimiting markers and marker segments <----

    /** Start of codestream (SOC): 0xFF4F */
    public const ushort SOC = 0xff4f;

    /** Start of tile-part (SOT): 0xFF90 */
    public const ushort SOT = 0xff90;

    /** Start of data (SOD): 0xFF93 */
    public const ushort SOD = 0xff93;

    /** End of codestream (EOC): 0xFFD9 */
    public const ushort EOC = 0xffd9;

    // ----> Fixed information marker segments <----

    // ** SIZ marker **

    /** SIZ marker (Image and tile size): 0xFF51 */
    public const ushort SIZ = 0xff51;

    /** No special capabilities (baseline) in codestream, in Rsiz field of SIZ
     * marker: 0x00. All flag bits are turned off */
    public const int RSIZ_BASELINE = 0x00;
    /** Error resilience marker flag bit in Rsiz field in SIZ marker: 0x01 */
    public const int RSIZ_ER_FLAG = 0x01;
    /** ROI present marker flag bit in Rsiz field in SIZ marker: 0x02 */
    public const int RSIZ_ROI = 0x02;
    /** Component bitdepth bits in Ssiz field in SIZ marker: 7 */
    public const int SSIZ_DEPTH_BITS = 7;
    /** The maximum number of component bitdepth */
    public const int MAX_COMP_BITDEPTH = 38;


    // ----> Functional marker segments <----

    // ** COD/COC marker **

    /** Coding style default (COD): 0xFF52 */
    public const ushort COD = 0xff52;

    /** Coding style component (COC): 0xFF53 */
    public const ushort COC = 0xff53;

    /** Precinct used flag */
    public const int SCOX_PRECINCT_PARTITION = 1;
    /** Use start of packet marker */
    public const int SCOX_USE_SOP = 2;
    /** Use end of packet header marker */
    public const int SCOX_USE_EPH = 4;
    /** Horizontal code-block partition origin is at x=1 */
    public const int SCOX_HOR_CB_PART = 8;
    /** Vertical code-block partition origin is at y=1 */
    public const int SCOX_VER_CB_PART = 16;
    /** The default size exponent of the precincts */
    public const int PRECINCT_PARTITION_DEF_SIZE = 0xffff;

    // ** RGN marker segment **
    /** Region-of-interest (RGN): 0xFF5E */
    public const ushort RGN = 0xff5e;

    /** Implicit (i.e. max-shift) ROI flag for Srgn field in RGN marker
        segment: 0x00 */
    public const int SRGN_IMPLICIT = 0x00;

    // ** QCD/QCC markers **

    /** Quantization default (QCD): 0xFF5C */
    public const ushort QCD = 0xff5c;

    /** Quantization component (QCC): 0xFF5D */
    public const ushort QCC = 0xff5d;

    /** Guard bits shift in SQCX field: 5 */
    public const int SQCX_GB_SHIFT = 5;
    /** Guard bits mask in SQCX field: 7 */
    public const int SQCX_GB_MSK = 7;
    /** No quantization (i.e. embedded reversible) flag for Sqcd or Sqcc
     * (Sqcx) fields: 0x00. */
    public const int SQCX_NO_QUANTIZATION = 0x00;
    /** Scalar derived (i.e. LL values only) quantization flag for Sqcd or
     * Sqcc (Sqcx) fields: 0x01. */
    public const int SQCX_SCALAR_DERIVED = 0x01;
    /** Scalar expounded (i.e. all values) quantization flag for Sqcd or Sqcc
     * (Sqcx) fields: 0x02. */
    public const int SQCX_SCALAR_EXPOUNDED = 0x02;
    /** Exponent shift in SPQCX when no quantization: 3 */
    public const int SQCX_EXP_SHIFT = 3;
    /** Exponent bitmask in SPQCX when no quantization: 3 */
    public const int SQCX_EXP_MASK = (1 << 5) - 1;
    /** The "SOP marker segments used" flag within Sers: 1 */
    public const int ERS_SOP = 1;
    /** The "segmentation symbols used" flag within Sers: 2 */
    public const int ERS_SEG_SYMBOLS = 2;

    // ** Progression order change **
    public const ushort POC = 0xff5f;

    // ----> Pointer marker segments <----

    /** Tile-part lengths (TLM): 0xFF55 */
    public const ushort TLM = 0xff55;

    /** Packet length, main header (PLM): 0xFF57 */
    public const ushort PLM = 0xff57;

    /** Packet length, tile-part header (PLT): 0xFF58 */
    public const ushort PLT = 0xff58;

    /** Packed packet headers, main header (PPM): 0xFF60 */
    public const ushort PPM = 0xff60;

    /** Packed packet headers, tile-part header (PPT): 0xFF61 */
    public const ushort PPT = 0xff61;

    /** Maximum length of PPT marker segment */
    public const int MAX_LPPT = 65535;

    /** Maximum length of PPM marker segment */
    public const int MAX_LPPM = 65535;


    // ----> In bit stream markers and marker segments <----

    /** Start pf packet (SOP): 0xFF91 */
    public const ushort SOP = 0xff91;

    /** Length of SOP marker (in bytes) */
    public const ushort SOP_LENGTH = 6;

    /** End of packet header (EPH): 0xFF92 */
    public const ushort EPH = 0xff92;

    /** Length of EPH marker (in bytes) */
    public const ushort EPH_LENGTH = 2;

    // ----> Informational marker segments <----

    /** Component registration (CRG): 0xFF63 */
    public const ushort CRG = 0xff63;

    /** Comment (COM): 0xFF64 */
    public const ushort COM = 0xff64;

    /** General use registration value (COM): 0x0001 */
    public const ushort RCOM_GEN_USE = 0x0001;
}
