using System;

namespace Jpeg2000Library.Exceptions
{
    public class EndOfFileExpception : Exception
    {
        public EndOfFileExpception() : base("End of file reached!")
        {

        }
    }
}
