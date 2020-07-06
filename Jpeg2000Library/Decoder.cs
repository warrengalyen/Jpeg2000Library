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


        //return null;
    }
}
