namespace Jpeg2000Library.Image
{
    public class Coord
    {
        /** The horizontal coordinate */
        public int X { get; set; }

        /** The vertical coordinate */
        public int Y { get; set; }

        /**
         * Creates a new coordinate object given with the (0,0) coordinates
         * */
        public Coord() { }

        /**
         * Creates a new coordinate object given the two coordinates.
         *
         * @param x The horizontal coordinate.
         *
         * @param y The vertical coordinate.
         * */
        public Coord(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /**
         * Creates a new coordinate object given another Coord object i.e. copy 
         * constructor
         *
         * @param c The Coord object to be copied.
         * */
        public Coord(Coord c)
        {
            this.X = c.X;
            this.Y = c.Y;
        }

        /**
         * Returns a string representation of the object coordinates
         *
         * @return The vertical and the horizontal coordinates
         * */
        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
    }
}
