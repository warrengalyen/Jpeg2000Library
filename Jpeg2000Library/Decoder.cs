using System;
using System.IO;
using Jpeg2000Library.CodeStream;
using Jpeg2000Library.CodeStream.Reader;
using Jpeg2000Library.Exceptions;
using Jpeg2000Library.FileFormat.Reader;
using Jpeg2000Library.IO;
using Jpeg2000Library;
using System.Collections.Generic;
using System.Collections;
using Jpeg2000Library.Util;

public class Decoder
{
    public ParameterList ParameterList { get; private set; }
    public Decoder(ParameterList pl)
    {
        this.ParameterList = pl;
    }

    public void Decode(IRandomAccessIO input)
    {
        var fileFormatReader = new FileFormatReader(input);
        if (fileFormatReader.JP2FFUsed)
        {
            input.Seek(fileFormatReader.FirstCodeStreamPosition);
        }

        bool verbose = true;
        // **** Header decoder ****
        // Instantiate header decoder and read main header 
        var headerInfo = new HeaderInfo();
        HeaderDecoder headerDecoder = null;
        try
        {
            headerDecoder = new HeaderDecoder(input, ParameterList, headerInfo);
        }
        catch (EndOfFileException e)
        {
            Logger.Error("Codestream too short or bad header, unable to decode.");
            if (ParameterList.getParameter("debug") == "on")
            {
                Logger.Warning(e.StackTrace);
            }
            else
            {
                Logger.Error("Use '-debug' option for more details");
            }
            return;
        }

        int nCompCod = headerDecoder.getNumComps();
        int nTiles = headerInfo.siz.getNumTiles();
        var decSpec = headerDecoder.getDecoderSpecs();

        // Report information
        if (verbose)
        {
            String info = nCompCod + " component(s) in codestream, " + nTiles +
        " tile(s)\n";
            info += "Image dimension: ";
            for (int c = 0; c < nCompCod; c++)
            {
                info += headerInfo.siz.getCompImgWidth(c) + "x" +
                    headerInfo.siz.getCompImgHeight(c) + " ";
            }

            if (nTiles != 1)
            {
                info += "\nNom. Tile dim. (in canvas): " +
                    headerInfo.siz.xtsiz + "x" + headerInfo.siz.ytsiz;
            }
            Logger.Debug(info);
        }
        //if (pl.getBooleanParameter("cdstr_info"))
        {
            Logger.Debug("Main header:\n" + headerInfo.toStringMainHeader());
        }

        // Get demixed bitdepths
        var depth = new int[nCompCod];
        for (var i = 0; i < nCompCod; i++) { depth[i] = headerDecoder.getOriginalBitDepth(i); }

        // **** Bit stream reader ****
        // try
        // {
        //     breader = BitstreamReaderAgent.
        //         createInstance(in, hd, pl, decSpec,
        //                        pl.getBooleanParameter("cdstr_info"), hi);
        // }
        // catch (IOException e)
        // {
        //     error("Error while reading bit stream header or parsing " +
        //   "packets" + ((e.getMessage() != null) ?
        //      (":\n" + e.getMessage()) : ""), 4);
        //     if (pl.getParameter("debug").equals("on"))
        //     {
        //         e.printStackTrace();
        //     }
        //     else
        //     {
        //         error("Use '-debug' option for more details", 2);
        //     }
        //     return;
        // }
        // catch (IllegalArgumentException e)
        // {
        //     error("Cannot instantiate bit stream reader" +
        //               ((e.getMessage() != null) ?
        //                (":\n" + e.getMessage()) : ""), 2);
        //     if (pl.getParameter("debug").equals("on"))
        //     {
        //         e.printStackTrace();
        //     }
        //     else
        //     {
        //         error("Use '-debug' option for more details", 2);
        //     }
        //     return;
        // }
        //return null;
    }
}
