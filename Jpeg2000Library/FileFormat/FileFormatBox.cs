namespace Jpeg2000Library.FileFormat
{

    public class FileFormatBoxes
    {

        /**** Main boxes ****/

        public const int JP2_SIGNATURE_BOX = 0x6a502020;

        public const int FILE_TYPE_BOX = 0x66747970;

        public const int JP2_HEADER_BOX = 0x6a703268;

        public const int CONTIGUOUS_CODESTREAM_BOX = 0x6a703263;

        public const int INTELLECTUAL_PROPERTY_BOX = 0x64703269;

        public const int XML_BOX = 0x786d6c20;

        public const int UUID_BOX = 0x75756964;

        public const int UUID_INFO_BOX = 0x75696e66;

        /** JP2 Header boxes */
        public const int IMAGE_HEADER_BOX = 0x69686472;

        public const int BITS_PER_COMPONENT_BOX = 0x62706363;

        public const int COLOUR_SPECIFICATION_BOX = 0x636f6c72;

        public const int PALETTE_BOX = 0x70636c72;

        public const int COMPONENT_MAPPING_BOX = 0x636d6170;

        public const int CHANNEL_DEFINITION_BOX = 0x63646566;

        public const int RESOLUTION_BOX = 0x72657320;

        public const int CAPTURE_RESOLUTION_BOX = 0x72657363;

        public const int DEFAULT_DISPLAY_RESOLUTION_BOX = 0x72657364;

        /** End of JP2 Header boxes */

        /** UUID Info Boxes */
        public const int UUID_LIST_BOX = 0x75637374;

        public const int URL_BOX = 0x75726c20;
        /** end of UUID Info boxes */

        /** Image Header Box Fields */
        public const int IMB_VERS = 0x0100;

        public const int IMB_C = 7;

        public const int IMB_UnkC = 1;

        public const int IMB_IPR = 0;
        /** end of Image Header Box Fields*/

        /** Colour Specification Box Fields */
        public const int CSB_METH = 1;

        public const int CSB_PREC = 0;

        public const int CSB_APPROX = 0;

        public const int CSB_ENUM_SRGB = 16;

        public const int CSB_ENUM_GREY = 17;
        /** en of Colour Specification Box Fields */

        /** File Type Fields */
        public const int FT_BR = 0x6a703220;
    }
}