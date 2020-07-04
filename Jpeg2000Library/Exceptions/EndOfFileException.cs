using System;

namespace Jpeg2000Library.Exceptions
{
    public class EndOfFileException : Exception
    {
        public EndOfFileException() : base("End of file reached!")
        {

        }
    }
}
