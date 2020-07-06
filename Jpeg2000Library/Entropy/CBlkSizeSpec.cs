/*
 * CVS identifier:
 *
 * $Id: CBlkSizeSpec.java,v 1.11 2001/02/14 10:38:18 grosbois Exp $
 *
 * Class:                   CBlkSizeSpec
 *
 * Description:             Specification of the code-blocks size
 *
 *
 *
 * COPYRIGHT:
 * 
 * This software module was originally developed by Rapha�l Grosbois and
 * Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
 * Askel�f (Ericsson Radio Systems AB); and Bertrand Berthelot, David
 * Bouchard, F�lix Henry, Gerard Mozelle and Patrice Onno (Canon Research
 * Centre France S.A) in the course of development of the JPEG2000
 * standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
 * software module is an implementation of a part of the JPEG 2000
 * Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
 * Systems AB and Canon Research Centre France S.A (collectively JJ2000
 * Partners) agree not to assert against ISO/IEC and users of the JPEG
 * 2000 Standard (Users) any of their rights under the copyright, not
 * including other intellectual property rights, for this software module
 * with respect to the usage by ISO/IEC and Users of this software module
 * or modifications thereof for use in hardware or software products
 * claiming conformance to the JPEG 2000 Standard. Those intending to use
 * this software module in hardware or software products are advised that
 * their use may infringe existing patents. The original developers of
 * this software module, JJ2000 Partners and ISO/IEC assume no liability
 * for use of this software module or modifications thereof. No license
 * or right to this software module is granted for non JPEG 2000 Standard
 * conforming products. JJ2000 Partners have full right to use this
 * software module for his/her own purpose, assign or donate this
 * software module to any third party and to inhibit third parties from
 * using this software module for non JPEG 2000 Standard conforming
 * products. This copyright notice must be included in all copies or
 * derivative works of this software module.
 * 
 * Copyright (c) 1999/2000 JJ2000 Partners.
 * */
using System;
using Jpeg2000Library.Util;

namespace Jpeg2000Library.Entropy
{

    /**
     * This class extends ModuleSpec class for code-blocks sizes holding purposes.
     * 
     * <P>It stores the size a of code-block. 
     * */
    public class CBlkSizeSpec : ModuleSpec
    {

        /** Name of the option */
        private const string optName = "Cblksiz";

        /** The maximum code-block width */
        private int maxCBlkWidth = 0;

        /** The maximum code-block height */
        private int maxCBlkHeight = 0;

        /**
         * Creates a new CBlkSizeSpec object for the specified number of tiles and
         * components.
         *
         * @param nt The number of tiles
         *
         * @param nc The number of components
         *
         * @param type the type of the specification module i.e. tile specific,
         * component specific or both.
         * */
        public CBlkSizeSpec(int nt, int nc, byte type) : base(nt, nc, type)
        {
        }

        /**
         * Creates a new CBlkSizeSpec object for the specified number of tiles and
         * components and the ParameterList instance.
         *
         * @param nt The number of tiles
         *
         * @param nc The number of components
         *
         * @param type the type of the specification module i.e. tile specific,
         * component specific or both.
         *
         * @param imgsrc The image source (used to get the image size)
         *
         * @param pl The ParameterList instance
         * */
        public CBlkSizeSpec(int nt, int nc, byte type, ParameterList pl) : base(nt, nc, type)
        {

            bool firstVal = true;
            string param = pl.getParameter(optName);

            // Precinct partition is used : parse arguments
            StringTokenizer stk = new StringTokenizer(param);
            byte curSpecType = SPEC_DEF; // Specification type of the
                                         // current parameter
            bool[] tileSpec = null; // Tiles concerned by the specification
            bool[] compSpec = null; // Components concerned by the specification
            int i, xIdx, ci, ti;
            string word = null; // current word
            string errMsg = null;

            while (stk.HasMoreTokens())
            {
                word = stk.NextToken();

                switch (word[0])
                {

                    case 't': // Tiles specification
                        tileSpec = parseIdx(word, nTiles);
                        if (curSpecType == SPEC_COMP_DEF)
                        {
                            curSpecType = SPEC_TILE_COMP;
                        }
                        else
                        {
                            curSpecType = SPEC_TILE_DEF;
                        }
                        break;

                    case 'c': // Components specification
                        compSpec = parseIdx(word, nComp);
                        if (curSpecType == SPEC_TILE_DEF)
                        {
                            curSpecType = SPEC_TILE_COMP;
                        }
                        else
                        {
                            curSpecType = SPEC_COMP_DEF;
                        }
                        break;

                    default:
                        if (!char.IsDigit(word[0]))
                        {
                            errMsg = "Bad construction for parameter: " + word;
                            throw new ArgumentException(errMsg);
                        }
                        int[] dim = new int[2];
                        // Get code-block's width
                        try
                        {
                            dim[0] = int.Parse(word);
                            // Check that width is not >
                            // StdEntropyCoderOptions.MAX_CB_DIM
                            if (dim[0] > StdEntropyCoderOptions.MAX_CB_DIM)
                            {
                                errMsg = "'" + optName + "' option : the code-block's " +
                                    "width cannot be greater than " +
                                    StdEntropyCoderOptions.MAX_CB_DIM;
                                throw new ArgumentException(errMsg);
                            }
                            // Check that width is not <
                            // StdEntropyCoderOptions.MIN_CB_DIM
                            if (dim[0] < StdEntropyCoderOptions.MIN_CB_DIM)
                            {
                                errMsg = "'" + optName + "' option : the code-block's " +
                                    "width cannot be less than " +
                                    StdEntropyCoderOptions.MIN_CB_DIM;
                                throw new ArgumentException(errMsg);
                            }
                            // Check that width is a power of 2
                            if (dim[0] !=
                                 (1 << MathUtil.log2(dim[0])))
                            {
                                errMsg = "'" + optName + "' option : the code-block's " +
                                    "width must be a power of 2";
                                throw new ArgumentException(errMsg);
                            }
                        }
                        catch (FormatException e)
                        {
                            errMsg = "'" + optName + "' option : the code-block's " +
                                "width could not be parsed.";
                            throw new ArgumentException(errMsg);
                        }
                        // Get the next word in option
                        try
                        {
                            word = stk.NextToken();
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            errMsg = "'" + optName + "' option : could not parse the " +
                                "code-block's height";
                            throw new ArgumentException(errMsg);

                        }
                        // Get the code-block's height
                        try
                        {
                            dim[1] = int.Parse(word);
                            // Check that height is not >
                            // StdEntropyCoderOptions.MAX_CB_DIM
                            if (dim[1] > StdEntropyCoderOptions.MAX_CB_DIM)
                            {
                                errMsg = "'" + optName + "' option : the code-block's " +
                                    "height cannot be greater than " +
                                    StdEntropyCoderOptions.MAX_CB_DIM;
                                throw new ArgumentException(errMsg);
                            }
                            // Check that height is not <
                            // StdEntropyCoderOptions.MIN_CB_DIM
                            if (dim[1] < StdEntropyCoderOptions.MIN_CB_DIM)
                            {
                                errMsg = "'" + optName + "' option : the code-block's " +
                                    "height cannot be less than " +
                                    StdEntropyCoderOptions.MIN_CB_DIM;
                                throw new ArgumentException(errMsg);
                            }
                            // Check that height is a power of 2
                            if (dim[1] !=
                                 (1 << MathUtil.log2(dim[1])))
                            {
                                errMsg = "'" + optName + "' option : the code-block's " +
                                    "height must be a power of 2";
                                throw new ArgumentException(errMsg);
                            }
                            // Check that the code-block 'area' (i.e. width*height) is
                            // not greater than StdEntropyCoderOptions.MAX_CB_AREA
                            if (dim[0] * dim[1] >
                                 StdEntropyCoderOptions.MAX_CB_AREA)
                            {
                                errMsg = "'" + optName + "' option : The " +
                                    "code-block's area (i.e. width*height) " +
                                    "cannot be greater than " +
                                    StdEntropyCoderOptions.MAX_CB_AREA;
                                throw new ArgumentException(errMsg);
                            }
                        }
                        catch (FormatException e)
                        {
                            errMsg = "'" + optName + "' option : the code-block's height " +
                                "could not be parsed.";
                            throw new ArgumentException(errMsg);
                        }

                        // Store the maximum dimensions if necessary
                        if (dim[0] > maxCBlkWidth)
                        {
                            maxCBlkWidth = dim[0];
                        }

                        if (dim[1] > maxCBlkHeight)
                        {
                            maxCBlkHeight = dim[1];
                        }

                        if (firstVal)
                        {
                            // This is the first time a value is given so we set it as
                            // the default one 
                            setDefault(dim);
                            firstVal = false;
                        }

                        switch (curSpecType)
                        {
                            case SPEC_DEF:
                                setDefault(dim);
                                break;
                            case SPEC_TILE_DEF:
                                for (ti = tileSpec.Length - 1; ti >= 0; ti--)
                                {
                                    if (tileSpec[ti])
                                    {
                                        setTileDef(ti, dim);
                                    }
                                }
                                break;
                            case SPEC_COMP_DEF:
                                for (ci = compSpec.Length - 1; ci >= 0; ci--)
                                {
                                    if (compSpec[ci])
                                    {
                                        setCompDef(ci, dim);
                                    }
                                }
                                break;
                            default:
                                for (ti = tileSpec.Length - 1; ti >= 0; ti--)
                                {
                                    for (ci = compSpec.Length - 1; ci >= 0; ci--)
                                    {
                                        if (tileSpec[ti] && compSpec[ci])
                                        {
                                            setTileCompVal(ti, ci, dim);
                                        }
                                    }
                                }
                                break;
                        }
                        break;
                } // end switch
            }
        }

        /**
         * Returns the maximum code-block's width
         * 
         */
        public int getMaxCBlkWidth()
        {
            return maxCBlkWidth;
        }

        /**
         * Returns the maximum code-block's height
         * 
         */
        public int getMaxCBlkHeight()
        {
            return maxCBlkHeight;
        }

        /**
         * Returns the code-block width :
         *
         * <ul>
         * <li>for the specified tile/component</li>
         * <li>for the specified tile</li>
         * <li>for the specified component</li>
         * <li>default value</li>
         * </ul>
         * 
         * The value returned depends on the value of the variable 'type' which
         * can take the following values :<br>
         *
         * <ul> 
         * <li>SPEC_DEF -> Default value is returned. t and c values are
         * ignored</li> 
         * <li>SPEC_COMP_DEF -> Component default value is returned. t value is
         * ignored</li>
         * <li>SPEC_TILE_DEF -> Tile default value is returned. c value is
         * ignored</li>
         * <li>SPEC_TILE_COMP -> Tile/Component value is returned.</li>
         * </ul>
         * 
         * @param type The type of the value we want to be returned
         * 
         * @param t The tile index
         * 
         * @param c the component index
         *
         * @return The code-block width for the specified tile and component
         * */
        public int getCBlkWidth(byte type, int t, int c)
        {
            int[] dim = null;
            switch (type)
            {
                case SPEC_DEF:
                    dim = (int[])getDefault();
                    break;
                case SPEC_COMP_DEF:
                    dim = (int[])getCompDef(c);
                    break;
                case SPEC_TILE_DEF:
                    dim = (int[])getTileDef(t);
                    break;
                case SPEC_TILE_COMP:
                    dim = (int[])getTileCompVal(t, c);
                    break;
            }
            return dim[0];
        }

        /**
         * Returns the code-block height:
         *
         * <ul>
         * <li>for the specified tile/component</li>
         * <li>for the specified tile</li>
         * <li>for the specified component</li>
         * <li>default value</li>
         * </ul>
         * 
         * The value returned depends on the value of the variable 'type' which
         * can take the following values :
         *
         * <ul> 
         * <li>SPEC_DEF -> Default value is returned. t and c values are
         * ignored</li> 
         * <li>SPEC_COMP_DEF -> Component default value is returned. t value is
         * ignored</li>
         * <li>SPEC_TILE_DEF -> Tile default value is returned. c value is
         * ignored</li>
         * <li>SPEC_TILE_COMP -> Tile/Component value is returned.</li>
         * </ul>
         * 
         * @param type The type of the value we want to be returned
         * 
         * @param t The tile index
         * 
         * @param c the component index
         *
         * @return The code-block height for the specified tile and component
         * */
        public int getCBlkHeight(byte type, int t, int c)
        {
            int[] dim = null;
            switch (type)
            {
                case SPEC_DEF:
                    dim = (int[])getDefault();
                    break;
                case SPEC_COMP_DEF:
                    dim = (int[])getCompDef(c);
                    break;
                case SPEC_TILE_DEF:
                    dim = (int[])getTileDef(t);
                    break;
                case SPEC_TILE_COMP:
                    dim = (int[])getTileCompVal(t, c);
                    break;
            }
            return dim[1];
        }

        /** 
         * Sets default value for this module 
         *
         * @param value Default value
         * */
        public new void setDefault(object value)
        {
            base.setDefault(value);

            // Store the biggest code-block dimensions
            storeHighestDims((int[])value);
        }

        /** 
         * Sets default value for specified tile and specValType tag if allowed by
         * its priority.
         *
         * @param c Tile index.
         *
         * @param value Tile's default value
         *  */
        public new void setTileDef(int t, object value)
        {
            base.setTileDef(t, value);

            // Store the biggest code-block dimensions
            storeHighestDims((int[])value);
        }

        /** 
         * Sets default value for specified component and specValType tag if
         * allowed by its priority.
         *
         * @param c Component index 
         *
         * @param value Component's default value
         *  */
        public new void setCompDef(int c, object value)
        {
            base.setCompDef(c, value);

            // Store the biggest code-block dimensions
            storeHighestDims((int[])value);
        }

        /** 
         * Sets value for specified tile-component.
         *
         * @param t Tie index 
         * 
         * @param c Component index 
         *
         * @param value Tile-component's value
         *  */
        public new void setTileCompVal(int t, int c, object value)
        {
            base.setTileCompVal(t, c, value);

            // Store the biggest code-block dimensions
            storeHighestDims((int[])value);
        }

        /** 
         * Stores the highest code-block width and height
         *
         * @param dim The 2 elements array that contains the code-block width and
         * height.
         *  */
        private void storeHighestDims(int[] dim)
        {
            // Store the biggest code-block dimensions
            if (dim[0] > maxCBlkWidth)
            {
                maxCBlkWidth = dim[0];
            }
            if (dim[1] > maxCBlkHeight)
            {
                maxCBlkHeight = dim[1];
            }
        }
    }

}
