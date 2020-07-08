namespace Jpeg2000Library.ROI
{
    public class MaxShiftSpec : ModuleSpec
    {

        /**
         * Constructs a 'ModuleSpec' object, initializing all the components and 
         * tiles to the 'SPEC_DEF' spec type, for the specified number of 
         * components and tiles.
         *
         * @param nt The number of tiles
         *
         * @param nc The number of components
         *
         * @param type the type of the specification module i.e. tile specific,
         * component specific or both.
         * */
        public MaxShiftSpec(int nt, int nc, byte type) : base(nt, nc, type)
        {

        }
    }

}