using System;

namespace Jpeg2000Library.Util
{
    public class StringTokenizer
    {
        private int pos;
        private String str;
        private int len;
        private String delim;
        private bool retDelims;

        public StringTokenizer(String str) : this(str, " \t\n\r\f", false)
        {
        }

        public StringTokenizer(String str, String delim) : this(str, delim, false)
        {
        }

        public StringTokenizer(String str, String delim, bool retDelims)
        {
            len = str.Length;
            this.str = str;
            this.delim = delim;
            this.retDelims = retDelims;
            this.pos = 0;
        }

        public bool HasMoreTokens()
        {
            if (!retDelims)
            {
                while (pos < len && delim.IndexOf(str[pos]) >= 0)
                    pos++;
            }
            return pos < len;
        }

        public String NextToken(String delim)
        {
            this.delim = delim;
            return NextToken();
        }

        public String NextToken()
        {
            if (pos < len && delim.IndexOf(str[pos]) >= 0)
            {
                if (retDelims)
                    return str.Substring(pos++, 1);
                while (++pos < len && delim.IndexOf(str[pos]) >= 0) ;
            }
            if (pos < len)
            {
                int start = pos;
                while (++pos < len && delim.IndexOf(str[pos]) < 0) ;

                return str.Substring(start, pos - start);
            }
            throw new IndexOutOfRangeException();
        }

        public int CountTokens()
        {
            int count = 0;
            int delimiterCount = 0;
            bool tokenFound = false;
            int tmpPos = pos;

            while (tmpPos < len)
            {
                if (delim.IndexOf(str[tmpPos++]) >= 0)
                {
                    if (tokenFound)
                    {
                        count++;
                        tokenFound = false;
                    }
                    delimiterCount++;
                }
                else
                {
                    tokenFound = true;
                    while (tmpPos < len
                        && delim.IndexOf(str[tmpPos]) < 0)
                        ++tmpPos;
                }
            }
            if (tokenFound)
                count++;
            return retDelims ? count + delimiterCount : count;
        }
    }
}
