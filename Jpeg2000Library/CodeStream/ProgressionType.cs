namespace Jpeg2000Library.CodeStream
{
    public class ProgressionType
    {

        /** The codestream is Layer/Resolution/Component/Position progressive : 0
         * */
        public const int LY_RES_COMP_POS_PROG = 0;

        /** The codestream is Resolution/Layer/Component/Position progressive : 1
         * */
        public const int RES_LY_COMP_POS_PROG = 1;

        /** The codestream is Resolution/Position/Component/Layer progressive : 2
         * */
        public const int RES_POS_COMP_LY_PROG = 2;

        /** The codestream is Position/Component/Resolution/Layer progressive : 3
         * */
        public const int POS_COMP_RES_LY_PROG = 3;

        /** The codestream is Component/Position/Resolution/Layer progressive : 4
         * */
        public const int COMP_POS_RES_LY_PROG = 4;
    }
}