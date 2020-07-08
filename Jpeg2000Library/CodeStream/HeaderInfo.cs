using System;
using System.Collections.Generic;
using Jpeg2000Library.Wavelet;

namespace Jpeg2000Library.CodeStream
{
    public class HeaderInfo
    {
        public class COC
        {
            public int lcoc;
            public int ccoc;
            public int scoc;
            public int spcoc_ndl; // Number of decomposition levels
            public int spcoc_cw;
            public int spcoc_ch;
            public int spcoc_cs;
            public int[] spcoc_t = new int[1];
            public int[] spcoc_ps;
            /** Display information found in this COC marker segment */
            public String toString()
            {
                String str = "\n --- COC (" + lcoc + " bytes) ---\n";
                str += " Component      : " + ccoc + "\n";
                str += " Coding style   : ";
                if (scoc == 0)
                {
                    str += "Default";
                }
                else
                {
                    if ((scoc & 0x1) != 0) str += "Precints ";
                    if ((scoc & 0x2) != 0) str += "SOP ";
                    if ((scoc & 0x4) != 0) str += "EPH ";
                }
                str += "\n";
                str += " Cblk style     : ";
                if (spcoc_cs == 0)
                {
                    str += "Default";
                }
                else
                {
                    if ((spcoc_cs & 0x1) != 0) str += "Bypass ";
                    if ((spcoc_cs & 0x2) != 0) str += "Reset ";
                    if ((spcoc_cs & 0x4) != 0) str += "Terminate ";
                    if ((spcoc_cs & 0x8) != 0) str += "Vert_causal ";
                    if ((spcoc_cs & 0x10) != 0) str += "Predict ";
                    if ((spcoc_cs & 0x20) != 0) str += "Seg_symb ";
                }
                str += "\n";
                str += " Num. of levels : " + spcoc_ndl + "\n";
                str += " Cblk dimension : " + (1 << (spcoc_cw + 2)) + "x" +
                    (1 << (spcoc_ch + 2)) + "\n";
                switch (spcoc_t[0])
                {
                    case FilterTypes.W9X7:
                        str += " Filter         : 9-7 irreversible\n";
                        break;
                    case FilterTypes.W5X3:
                        str += " Filter         : 5-3 reversible\n";
                        break;
                }
                if (spcoc_ps != null)
                {
                    str += " Precincts      : ";
                    for (int i = 0; i < spcoc_ps.Length; i++)
                    {
                        str += (1 << (spcoc_ps[i] & 0x000F)) + "x" +
                            (1 << (((spcoc_ps[i] & 0x00F0) >> 4))) + " ";
                    }
                }
                str += "\n";
                return str;
            }
        }
        public class CRG
        {
            public int lcrg;
            public int[] xcrg;
            public int[] ycrg;
            /** Display information found in the CRG marker segment */
            public String toString()
            {
                String str = "\n --- CRG (" + lcrg + " bytes) ---\n";
                for (int c = 0; c < xcrg.Length; c++)
                {
                    str += " Component " + c + " offset : " + xcrg[c] + "," + ycrg[c] + "\n";
                }
                str += "\n";
                return str;
            }
        }
        public class COD
        {
            public int lcod;
            public int scod;
            public int sgcod_po; // Progression order
            public int sgcod_nl; // Number of layers
            public int sgcod_mct; // Multiple component transformation
            public int spcod_ndl; // Number of decomposition levels
            public int spcod_cw; // Code-blocks width
            public int spcod_ch; // Code-blocks height
            public int spcod_cs; // Code-blocks style
            public int[] spcod_t = new int[1]; // Transformation
            public int[] spcod_ps; // Precinct size

            // public COD getCopy()
            // {
            //     COD ms = null;
            //     try
            //     {
            //         ms = (COD)this.clone();
            //     }
            //     catch (CloneNotSupportedException e)
            //     {
            //         throw new Error("Cannot clone SIZ marker segment");
            //     }
            //     return ms;
            // }
            /** Display information found in this COD marker segment */
            public override string ToString()
            {
                String str = "\n --- COD (" + lcod + " bytes) ---\n";
                str += " Coding style   : ";
                if (scod == 0)
                {
                    str += "Default";
                }
                else
                {
                    if ((scod & Markers.SCOX_PRECINCT_PARTITION) != 0) str += "Precints ";
                    if ((scod & Markers.SCOX_USE_SOP) != 0) str += "SOP ";
                    if ((scod & Markers.SCOX_USE_EPH) != 0) str += "EPH ";
                    int cb0x = ((scod & Markers.SCOX_HOR_CB_PART) != 0) ? 1 : 0;
                    int cb0y = ((scod & Markers.SCOX_VER_CB_PART) != 0) ? 1 : 0;
                    if (cb0x != 0 || cb0y != 0)
                    {
                        str += "Code-blocks offset";
                        str += "\n Cblk partition : " + cb0x + "," + cb0y;
                    }
                }
                str += "\n";
                str += " Cblk style     : ";
                if (spcod_cs == 0)
                {
                    str += "Default";
                }
                else
                {
                    if ((spcod_cs & 0x1) != 0) str += "Bypass ";
                    if ((spcod_cs & 0x2) != 0) str += "Reset ";
                    if ((spcod_cs & 0x4) != 0) str += "Terminate ";
                    if ((spcod_cs & 0x8) != 0) str += "Vert_causal ";
                    if ((spcod_cs & 0x10) != 0) str += "Predict ";
                    if ((spcod_cs & 0x20) != 0) str += "Seg_symb ";
                }
                str += "\n";
                str += " Num. of levels : " + spcod_ndl + "\n";
                switch (sgcod_po)
                {
                    case ProgressionType.LY_RES_COMP_POS_PROG:
                        str += " Progress. type : LY_RES_COMP_POS_PROG\n";
                        break;
                    case ProgressionType.RES_LY_COMP_POS_PROG:
                        str += " Progress. type : RES_LY_COMP_POS_PROG\n";
                        break;
                    case ProgressionType.RES_POS_COMP_LY_PROG:
                        str += " Progress. type : RES_POS_COMP_LY_PROG\n";
                        break;
                    case ProgressionType.POS_COMP_RES_LY_PROG:
                        str += " Progress. type : POS_COMP_RES_LY_PROG\n";
                        break;
                    case ProgressionType.COMP_POS_RES_LY_PROG:
                        str += " Progress. type : COMP_POS_RES_LY_PROG\n";
                        break;
                }
                str += " Num. of layers : " + sgcod_nl + "\n";
                str += " Cblk dimension : " + (1 << (spcod_cw + 2)) + "x" +
                    (1 << (spcod_ch + 2)) + "\n";
                switch (spcod_t[0])
                {
                    case FilterTypes.W9X7:
                        str += " Filter         : 9-7 irreversible\n";
                        break;
                    case FilterTypes.W5X3:
                        str += " Filter         : 5-3 reversible\n";
                        break;
                }
                str += " Multi comp tr. : " + (sgcod_mct == 1) + "\n";
                if (spcod_ps != null)
                {
                    str += " Precincts      : ";
                    for (int i = 0; i < spcod_ps.Length; i++)
                    {
                        str += (1 << (spcod_ps[i] & 0x000F)) + "x" +
                            (1 << (((spcod_ps[i] & 0x00F0) >> 4))) + " ";
                    }
                }
                str += "\n";
                return str;
            }
        }
        public class COM
        {
            public int lcom;
            public int rcom;
            public byte[] ccom;
            /** Display information found in the COM marker segment */
            public override string ToString()
            {
                String str = "\n --- COM (" + lcom + " bytes) ---\n";
                if (rcom == 0)
                {
                    str += " Registration : General use (binary values)\n";
                }
                else if (rcom == 1)
                {
                    str += " Registration : General use (IS 8859-15:1999 " +
                        "(Latin) values)\n";
                    str += " Text         : " + (System.Text.Encoding.Default.GetString(ccom)/*Convert.ToString(ccom)*/) + "\n";
                }
                else
                {
                    str += " Registration : Unknown\n";
                }
                str += "\n";
                return str;
            }
        }
        /** Returns a new instance of COM */
        public COM getNewCOM() { ncom++; return new COM(); }

        /** Returns the number of found COM marker segments */
        public int getNumCOM() { return ncom; }

        /** Internal class holding information found in the SIZ marker segment */
        public class SIZ : ICloneable
        {
            public int lsiz;
            public int rsiz;
            public int xsiz;
            public int ysiz;
            public int x0siz;
            public int y0siz;
            public int xtsiz;
            public int ytsiz;
            public int xt0siz;
            public int yt0siz;
            public int csiz;
            public int[] ssiz;
            public int[] xrsiz;
            public int[] yrsiz;

            /** Component widths */
            private int[] compWidth = null;
            /** Maximum width among all components */
            private int maxCompWidth = -1;
            /** Component heights */
            private int[] compHeight = null;
            /** Maximum height among all components */
            private int maxCompHeight = -1;
            /** 
             * Width of the specified tile-component
             *
             * @param t Tile index
             *
             * @param c Component index
             * */
            public int getCompImgWidth(int c)
            {
                if (compWidth == null)
                {
                    compWidth = new int[csiz];
                    for (int cc = 0; cc < csiz; cc++)
                    {
                        compWidth[cc] =
                            (int)(Math.Ceiling((xsiz) / (double)xrsiz[cc])
                                  - Math.Ceiling(x0siz / (double)xrsiz[cc]));
                    }
                }
                return compWidth[c];
            }
            public int getMaxCompWidth()
            {
                if (compWidth == null)
                {
                    compWidth = new int[csiz];
                    for (int cc = 0; cc < csiz; cc++)
                    {
                        compWidth[cc] =
                            (int)(Math.Ceiling((xsiz) / (double)xrsiz[cc])
                                  - Math.Ceiling(x0siz / (double)xrsiz[cc]));
                    }
                }
                if (maxCompWidth == -1)
                {
                    for (int c = 0; c < csiz; c++)
                    {
                        if (compWidth[c] > maxCompWidth)
                        {
                            maxCompWidth = compWidth[c];
                        }
                    }
                }
                return maxCompWidth;
            }
            public int getCompImgHeight(int c)
            {
                if (compHeight == null)
                {
                    compHeight = new int[csiz];
                    for (int cc = 0; cc < csiz; cc++)
                    {
                        compHeight[cc] =
                            (int)(Math.Ceiling((ysiz) / (double)yrsiz[cc])
                                  - Math.Ceiling(y0siz / (double)yrsiz[cc]));
                    }
                }
                return compHeight[c];
            }
            public int getMaxCompHeight()
            {
                if (compHeight == null)
                {
                    compHeight = new int[csiz];
                    for (int cc = 0; cc < csiz; cc++)
                    {
                        compHeight[cc] =
                            (int)(Math.Ceiling((ysiz) / (double)yrsiz[cc])
                                  - Math.Ceiling(y0siz / (double)yrsiz[cc]));
                    }
                }
                if (maxCompHeight == -1)
                {
                    for (int c = 0; c < csiz; c++)
                    {
                        if (compHeight[c] != maxCompHeight)
                        {
                            maxCompHeight = compHeight[c];
                        }
                    }
                }
                return maxCompHeight;
            }
            private int numTiles = -1;
            public int getNumTiles()
            {
                if (numTiles == -1)
                {
                    numTiles = ((xsiz - xt0siz + xtsiz - 1) / xtsiz) *
                        ((ysiz - yt0siz + ytsiz - 1) / ytsiz);
                }
                return numTiles;
            }
            private bool[] origSigned = null;
            public bool isOrigSigned(int c)
            {
                if (origSigned == null)
                {
                    origSigned = new bool[csiz];
                    for (int cc = 0; cc < csiz; cc++)
                    {
                        //origSigned[cc] = ((ssiz[cc]>>>Markers.SSIZ_DEPTH_BITS)==1);
                        // >>> = unsigned right shift
                        origSigned[cc] = ((((uint)ssiz[cc]) >> Markers.SSIZ_DEPTH_BITS) == 1);
                    }
                }
                return origSigned[c];
            }
            private int[] origBitDepth = null;
            public int getOrigBitDepth(int c)
            {
                if (origBitDepth == null)
                {
                    origBitDepth = new int[csiz];
                    for (int cc = 0; cc < csiz; cc++)
                    {
                        origBitDepth[cc] = (ssiz[cc] & ((1 << Markers.SSIZ_DEPTH_BITS) - 1)) + 1;
                    }
                }
                return origBitDepth[c];
            }
            public SIZ getCopy()
            {
                SIZ ms = null;
                try
                {
                    ms = (SIZ)this.Clone();
                }
                catch (Exception e)
                {
                    throw new Exception("Cannot clone SIZ marker segment");
                }
                return ms;
            }

            /** Display information found in SIZ marker segment */
            public override string ToString()
            {
                String str = "\n --- SIZ (" + lsiz + " bytes) ---\n";
                str += " Capabilities : " + rsiz + "\n";
                str += " Image dim.   : " + (xsiz - x0siz) + "x" + (ysiz - y0siz) + ", (off=" +
                    x0siz + "," + y0siz + ")\n";
                str += " Tile dim.    : " + xtsiz + "x" + ytsiz + ", (off=" + xt0siz + "," +
                    yt0siz + ")\n";
                str += " Component(s) : " + csiz + "\n";
                str += " Orig. depth  : ";
                for (int i = 0; i < csiz; i++) { str += getOrigBitDepth(i) + " "; }
                str += "\n";
                str += " Orig. signed : ";
                for (int i = 0; i < csiz; i++) { str += isOrigSigned(i) + " "; }
                str += "\n";
                str += " Subs. factor : ";
                for (int i = 0; i < csiz; i++) { str += xrsiz[i] + "," + yrsiz[i] + " "; }
                str += "\n";
                return str;
            }

            public object Clone()
            {
                //todo test
                return this.MemberwiseClone();
            }
        }
        /** Internal class holding information found in the RGN marker segments */
        public class RGN
        {
            public int lrgn;
            public int crgn;
            public int srgn;
            public int sprgn;
            /** Display information found in this RGN marker segment */
            public override string ToString()
            {
                String str = "\n --- RGN (" + lrgn + " bytes) ---\n";
                str += " Component : " + crgn + "\n";
                if (srgn == 0)
                {
                    str += " ROI style : Implicit\n";
                }
                else
                {
                    str += " ROI style : Unsupported\n";
                }
                str += " ROI shift : " + sprgn + "\n";
                str += "\n";
                return str;
            }
        }
        public class POC
        {
            public int lpoc;
            public int[] rspoc;
            public int[] cspoc;
            public int[] lyepoc;
            public int[] repoc;
            public int[] cepoc;
            public int[] ppoc;
            /** Display information found in this POC marker segment */
            public String toString()
            {
                String str = "\n --- POC (" + lpoc + " bytes) ---\n";
                str += " Chg_idx RSpoc CSpoc LYEpoc REpoc CEpoc Ppoc\n";
                for (int chg = 0; chg < rspoc.Length; chg++)
                {
                    str += "   " + chg
                        + "      " + rspoc[chg]
                        + "     " + cspoc[chg]
                        + "     " + lyepoc[chg]
                        + "      " + repoc[chg]
                        + "     " + cepoc[chg];
                    switch (ppoc[chg])
                    {
                        case ProgressionType.LY_RES_COMP_POS_PROG:
                            str += "  LY_RES_COMP_POS_PROG\n";
                            break;
                        case ProgressionType.RES_LY_COMP_POS_PROG:
                            str += "  RES_LY_COMP_POS_PROG\n";
                            break;
                        case ProgressionType.RES_POS_COMP_LY_PROG:
                            str += "  RES_POS_COMP_LY_PROG\n";
                            break;
                        case ProgressionType.POS_COMP_RES_LY_PROG:
                            str += "  POS_COMP_RES_LY_PROG\n";
                            break;
                        case ProgressionType.COMP_POS_RES_LY_PROG:
                            str += "  COMP_POS_RES_LY_PROG\n";
                            break;
                    }
                }
                str += "\n";
                return str;
            }
        }
        /** Internal class holding information found in the QCC marker segments */
        public class QCC
        {
            public int lqcc;
            public int cqcc;
            public int sqcc;
            public int[][] spqcc;

            private int qType = -1;
            public int getQuantType()
            {
                if (qType == -1)
                {
                    qType = sqcc & ~(Markers.SQCX_GB_MSK << Markers.SQCX_GB_SHIFT);
                }
                return qType;
            }
            private int gb = -1;
            public int getNumGuardBits()
            {
                if (gb == -1)
                {
                    gb = (sqcc >> Markers.SQCX_GB_SHIFT) & Markers.SQCX_GB_MSK;
                }
                return gb;
            }

            /** Display information found in this QCC marker segment */
            public override string ToString()
            {
                String str = "\n --- QCC (" + lqcc + " bytes) ---\n";
                str += " Component      : " + cqcc + "\n";
                str += " Quant. type    : ";
                int qt = getQuantType();
                if (qt == Markers.SQCX_NO_QUANTIZATION) str += "No quantization \n";
                else if (qt == Markers.SQCX_SCALAR_DERIVED) str += "Scalar derived\n";
                else if (qt == Markers.SQCX_SCALAR_EXPOUNDED) str += "Scalar expounded\n";
                str += " Guard bits     : " + getNumGuardBits() + "\n";
                if (qt == Markers.SQCX_NO_QUANTIZATION)
                {
                    str += " Exponents   :\n";
                    int exp;
                    for (int i = 0; i < spqcc.Length; i++)
                    {
                        for (int j = 0; j < spqcc[i].Length; j++)
                        {
                            if (i == 0 && j == 0)
                            {
                                exp = (spqcc[0][0] >> Markers.SQCX_EXP_SHIFT) & Markers.SQCX_EXP_MASK;
                                str += "\tr=0 : " + exp + "\n";
                            }
                            else if (i != 0 && j > 0)
                            {
                                exp = (spqcc[i][j] >> Markers.SQCX_EXP_SHIFT) & Markers.SQCX_EXP_MASK;
                                str += "\tr=" + i + ",s=" + j + " : " + exp + "\n";
                            }
                        }
                    }
                }
                else
                {
                    str += " Exp / Mantissa : \n";
                    int exp;
                    double mantissa;
                    for (int i = 0; i < spqcc.Length; i++)
                    {
                        for (int j = 0; j < spqcc[i].Length; j++)
                        {
                            if (i == 0 && j == 0)
                            {
                                exp = (spqcc[0][0] >> 11) & 0x1f;
                                mantissa = (-1f - ((float)(spqcc[0][0] & 0x07ff)) /
                                            (1 << 11)) / (-1 << exp);
                                str += "\tr=0 : " + exp + " / " + mantissa + "\n";
                            }
                            else if (i != 0 && j > 0)
                            {
                                exp = (spqcc[i][j] >> 11) & 0x1f;
                                mantissa = (-1f - ((float)(spqcc[i][j] & 0x07ff)) /
                                            (1 << 11)) / (-1 << exp);
                                str += "\tr=" + i + ",s=" + j + " : " + exp + " / " +
                                    mantissa + "\n";
                            }
                        }
                    }
                }
                str += "\n";
                return str;
            }
        }
        /** Internal class holding information found in the QCD marker segments */
        public class QCD
        {
            public int lqcd;
            public int sqcd;
            public int[][] spqcd;

            private int qType = -1;
            public int getQuantType()
            {
                if (qType == -1)
                {
                    qType = sqcd & ~(Markers.SQCX_GB_MSK << Markers.SQCX_GB_SHIFT);
                }
                return qType;
            }
            private int gb = -1;
            public int getNumGuardBits()
            {
                if (gb == -1)
                {
                    gb = (sqcd >> Markers.SQCX_GB_SHIFT) & Markers.SQCX_GB_MSK;
                }
                return gb;
            }

            /** Display information found in this QCD marker segment */
            public override string ToString()
            {
                String str = "\n --- QCD (" + lqcd + " bytes) ---\n";
                str += " Quant. type    : ";
                int qt = getQuantType();
                if (qt == Markers.SQCX_NO_QUANTIZATION) str += "No quantization \n";
                else if (qt == Markers.SQCX_SCALAR_DERIVED) str += "Scalar derived\n";
                else if (qt == Markers.SQCX_SCALAR_EXPOUNDED) str += "Scalar expounded\n";
                str += " Guard bits     : " + getNumGuardBits() + "\n";
                if (qt == Markers.SQCX_NO_QUANTIZATION)
                {
                    str += " Exponents   :\n";
                    int exp;
                    for (int i = 0; i < spqcd.Length; i++)
                    {
                        for (int j = 0; j < spqcd[i].Length; j++)
                        {
                            if (i == 0 && j == 0)
                            {
                                exp = (spqcd[0][0] >> Markers.SQCX_EXP_SHIFT) & Markers.SQCX_EXP_MASK;
                                str += "\tr=0 : " + exp + "\n";
                            }
                            else if (i != 0 && j > 0)
                            {
                                exp = (spqcd[i][j] >> Markers.SQCX_EXP_SHIFT) & Markers.SQCX_EXP_MASK;
                                str += "\tr=" + i + ",s=" + j + " : " + exp + "\n";
                            }
                        }
                    }
                }
                else
                {
                    str += " Exp / Mantissa : \n";
                    int exp;
                    double mantissa;
                    for (int i = 0; i < spqcd.Length; i++)
                    {
                        for (int j = 0; j < spqcd[i].Length; j++)
                        {
                            if (i == 0 && j == 0)
                            {
                                exp = (spqcd[0][0] >> 11) & 0x1f;
                                mantissa = (-1f - ((float)(spqcd[0][0] & 0x07ff)) /
                                            (1 << 11)) / (-1 << exp);
                                str += "\tr=0 : " + exp + " / " + mantissa + "\n";
                            }
                            else if (i != 0 && j > 0)
                            {
                                exp = (spqcd[i][j] >> 11) & 0x1f;
                                mantissa = (-1f - ((float)(spqcd[i][j] & 0x07ff)) /
                                            (1 << 11)) / (-1 << exp);
                                str += "\tr=" + i + ",s=" + j + " : " + exp + " / " +
                                    mantissa + "\n";
                            }
                        }
                    }
                }
                str += "\n";
                return str;
            }
        }
        public SIZ siz;
        public CRG crg;
        public NullableDictionary<string, COM> com = new NullableDictionary<string, COM>();
        public NullableDictionary<string, COD> cod = new NullableDictionary<string, COD>();
        public NullableDictionary<string, COC> coc = new NullableDictionary<string, COC>();
        public NullableDictionary<string, POC> poc = new NullableDictionary<string, POC>();
        public NullableDictionary<string, RGN> rgn = new NullableDictionary<string, RGN>();
        public NullableDictionary<string, QCC> qcc = new NullableDictionary<string, QCC>();
        public NullableDictionary<string, QCD> qcd = new NullableDictionary<string, QCD>();

        /** Number of found COM marker segment */
        private int ncom = 0;
        /** Display information found in the different marker segments of the main
     * header */
        public String toStringMainHeader()
        {
            int nc = siz.csiz;
            // SIZ
            String str = "" + siz;
            // COD
            if (cod["main"] != null)
            {
                str += "" + (COD)cod["main"];
            }
            // COCs
            for (int c = 0; c < nc; c++)
            {
                if (coc["main_c" + c] != null)
                {
                    str += "" + (COC)coc["main_c" + c];
                }
            }
            // QCD
            if (qcd["main"] != null)
            {
                str += "" + (QCD)qcd["main"];
            }
            // QCCs
            for (int c = 0; c < nc; c++)
            {
                if (qcc["main_c" + c] != null)
                {
                    str += "" + (QCC)qcc["main_c" + c];
                }
            }
            // RGN
            for (int c = 0; c < nc; c++)
            {
                if (rgn["main_c" + c] != null)
                {
                    str += "" + (RGN)rgn["main_c" + c];
                }
            }
            // POC
            if (poc["main"] != null)
            {
                str += "" + (POC)poc["main"];
            }
            // CRG
            if (crg != null)
            {
                str += "" + crg;
            }
            // COM
            for (int i = 0; i < ncom; i++)
            {
                if (com["main_" + i] != null)
                {
                    str += "" + (COM)com["main_" + i];
                }
            }
            return str;
        }
    }
}
