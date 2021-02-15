using System;
using System.Collections.Generic;

namespace Examples.XConnect.Copy.Config
{
    public class Arguments
    {
        public Arguments(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
            {
                WriteHelp = true;
            }

            for (int i = 0; i < args.Count; i += 2)
            {
                switch (args[i])
                {
                    case "-mode":
                        if (Enum.TryParse(SafeGet(args, i + 1), true, out ReadMode readMode))
                        {
                            CurrentReadMode = readMode;
                        }

                        break;

                    case "-file":
                        FileName = SafeGet(args, i + 1);
                        break;
                    case "-writer":
                        if (Enum.TryParse(SafeGet(args, i + 1), true, out WriteMode writeMode))
                        {
                            CurrentWriteMode = writeMode;
                        }

                        break;
                    case "-xdbmodel":
                        WriteXdbModel = true;
                        break;
                    case "-?":
                        WriteHelp = true;
                        break;
                }
            }
        }

        public enum ReadMode
        {
            DataExtraction,
            IdList
        }

        public enum WriteMode
        {
            XConnect
        }

        public ReadMode CurrentReadMode { get; set; }

        public WriteMode CurrentWriteMode { get; set; }

        public bool WriteXdbModel { get; set; }

        public bool WriteHelp { get; set; }

        public string FileName { get; set; }

        private static string SafeGet(IReadOnlyList<string> array, int position)
        {
            string result = null;
            if (array.Count > position)
            {
                result = array[position];
            }

            return result;
        }
    }
}
