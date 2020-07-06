using System;
using System.IO;
using Jpeg2000Library.IO;

namespace Jpeg2000Library
{
   
    public class Logger
    {
        public enum Severity
        {
            Error, 
            Warning,
            Debug
        }
        public static Severity MaxSeverity = Severity.Debug;
        public static void Log(Severity severity, string message)
        {
            if (severity > MaxSeverity)
                return;

            switch (severity)
            {
                case Severity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Severity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }
            System.Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Log(Severity.Error, message);
        }
        public static void Warning(string message)
        {
            Log(Severity.Warning, message);
        }
        public static void Debug(string message)
        {
            Log(Severity.Debug, message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            if (args.Length == 0)
            {
                Logger.Error("No input file provided.");
                return;
            }
            var inputFileName = args[0];
            if (!File.Exists(inputFileName))
            {
                Logger.Error($@"Input file doesn't exists. Filename provided: {inputFileName}");
                return;
            }
            Logger.Debug($"Processing file: {inputFileName}");
            try
            {
                var inputFile = File.OpenRead(inputFileName);

                var decoder = new Decoder(new Util.ParameterList());
                var randomAccessFile = new RandomAccessFile(inputFile);
                //var decoded = decoder.Decode(randomAccessFile);
                decoder.Decode(randomAccessFile);
            }
            catch (Exception ex)
            {
                Logger.Error($"Problem reading input file.\nException: {ex.Message}");
            }
        }
    }
}
