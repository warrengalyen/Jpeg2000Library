using System;
using System.IO;
using Jpeg2000Library.CodeStream;
using Jpeg2000Library.CodeStream.Reader;
using Jpeg2000Library.Exceptions;
using Jpeg2000Library.FileFormat.Reader;
using Jpeg2000Library.IO;
using System.Collections.Generic;
using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.Metadata;

namespace Jpeg2000Library
{
    public class ParameterList : IReadOnlyDictionary<string, object>
    {
        public ParameterList(IDictionary<string, object> parameters)
        {
            _backingDict = parameters != null ? new Dictionary<string, object>(parameters) : new Dictionary<string, object>();
        }
        private Dictionary<string, object> _backingDict;
        public object this[string key] => _backingDict.ContainsKey(key) ? _backingDict[key] : null;

        public IEnumerable<string> Keys => _backingDict.Keys;

        public IEnumerable<object> Values => _backingDict.Values;

        public int Count => _backingDict.Count;

        public bool ContainsKey(string key) => _backingDict.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _backingDict.GetEnumerator();

        public bool TryGetValue(string key, out object value) => _backingDict.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _backingDict.GetEnumerator();

        public bool Debug => ((string)this["debug"] ?? "on") == "on";

    }

    public class Decoder 
    { 
        public ParameterList ParameterList { get; private set; }
        public Decoder(IDictionary<string, object> parameterList = null)
        {
            ParameterList = new ParameterList(parameterList);
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
                if (ParameterList.Debug)
                {
                    Logger.Warning(e.StackTrace);
                }
                else
                {
                    Logger.Error("Use '-debug' option for more details");
                }
                return;
            }

            // return null
        }
    }
}
