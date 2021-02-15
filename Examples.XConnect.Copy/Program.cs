using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Examples.XConnect.Copy.Config;
using Examples.XConnect.Copy.Model;
using Examples.XConnect.Copy.Read;
using Examples.XConnect.Copy.Status;
using Examples.XConnect.Copy.Write;

using Serilog;

using Sitecore.XConnect.Serialization;

namespace Examples.XConnect.Copy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.File(
                "logs\\log.txt",
                fileSizeLimitBytes: 10485760,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true).CreateLogger();

            Arguments arguments = new Arguments(args);
            if (arguments.WriteHelp)
            {
                WriteHelp();
            }
            else if (arguments.WriteXdbModel)
            {
                WriteXdbModel();
            }
            else
            {
                MainAsync(arguments).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            
            WriteLine("END OF PROGRAM.", ConsoleColor.White);
            Console.ReadKey();
        }

        private static async Task MainAsync(Arguments arguments)
        {
            try
            {
                List<Task> executionTasks = new List<Task>();
                XConnectClientFactory readFactory = new XConnectClientFactory("read");
                Reader reader;
                switch (arguments.CurrentReadMode)
                {
                    case Arguments.ReadMode.IdList:
                        Configuration config = readFactory.Configuration;
                        config.IdListFileName = arguments.FileName;
                        reader = new IdListReader(readFactory.BuildClient(), config);
                        break;
                    default:
                        reader = new DataExtractionReader(readFactory.BuildClient(true), readFactory.Configuration);
                        break;
                }

                Task readingTask = reader.Start();
                executionTasks.Add(readingTask);

                Writer writer;
                switch (arguments.CurrentWriteMode)
                {
                    default:
                        XConnectClientFactory writeFactory = new XConnectClientFactory("write");
                        for (int i = 0; i < Configuration.WriterThreads; i++)
                        {
                            writer = new XConnectWriter(writeFactory.BuildClient(), writeFactory.Configuration);
                            Task writingTask = writer.Start();
                            executionTasks.Add(writingTask);
                        }

                        break;
                }

                while (CurrentStatus.IsReading() || !CurrentStatus.IsQueueEmpty())
                {
                    WriteLine(CurrentStatus.GetStatus(), ConsoleColor.White);
                    Thread.Sleep(5000);
                }

                await Task.WhenAll(executionTasks);
            }
            catch (Exception ex)
            {
                WriteLine($"FATAL: {ex.Message} at {ex.StackTrace}", ConsoleColor.Red);
            }
        }

        private static void Write(string text, ConsoleColor color)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = original;
        }

        private static void WriteLine(string text, ConsoleColor color)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = original;
        }

        private static void WriteHeader()
        {
            WriteLine(string.Empty, ConsoleColor.White);
            WriteLine(@"    ______                           __           _  ________                            __   ______                 ", ConsoleColor.Cyan);
            WriteLine(@"   / ____/  ______ _____ ___  ____  / /__  _____ | |/ / ____/___  ____  ____  ___  _____/ /_ / ____/___  ____  __  __", ConsoleColor.Cyan);
            WriteLine(@"  / __/ | |/_/ __ `/ __ `__ \/ __ \/ / _ \/ ___/ |   / /   / __ \/ __ \/ __ \/ _ \/ ___/ __// /   / __ \/ __ \/ / / /", ConsoleColor.Cyan);
            WriteLine(@" / /____>  </ /_/ / / / / / / /_/ / /  __(__  ) /   / /___/ /_/ / / / / / / /  __/ /__/ /__/ /___/ /_/ / /_/ / /_/ / ", ConsoleColor.Cyan);
            WriteLine(@"/_____/_/|_|\__,_/_/ /_/ /_/ .___/_/\___/____(_)_/|_\____/\____/_/ /_/_/ /_/\___/\___/\__(_)____/\____/ .___/\__, /  ", ConsoleColor.Cyan);
            WriteLine(@"                          /_/                                                                        /_/    /____/   ", ConsoleColor.Cyan);
            WriteLine(string.Empty, ConsoleColor.White);
        }

        private static void WriteHelp()
        {
            WriteHeader();
            WriteLine("This program is build to copy XConnect data from 1 XConnect instance to another. Supporting different Readers and Writers.", ConsoleColor.White);
            WriteLine(string.Empty, ConsoleColor.White);
            WriteLine("Usage: Examples.Xconnect.Copy.exe [-mode DataExtraction|IdList] [-file path] [-writer XConnect] [-xdbmodel] [-?]", ConsoleColor.White);
            WriteLine(string.Empty, ConsoleColor.White);
            WriteLine("Parameters", ConsoleColor.White);
            WriteLine("-mode".PadRight(10) + "DataExtraction or IdList, IdList needs the -file.", ConsoleColor.White);
            WriteLine("-file".PadRight(10) + "Path to a file containing a list of GUID (1 per line) to load and copy.", ConsoleColor.White);
            WriteLine("-writer".PadRight(10) + "Completely optional, available for when you want to add custom writers.", ConsoleColor.White);
            WriteLine("-xdbmodel".PadRight(10) + "Writes a file for the CopyXdbModel to install into XConnect. No other parameters needed.", ConsoleColor.White);
            WriteLine("-?".PadRight(10) + "Writes the help you're reading.", ConsoleColor.White);
            WriteLine(string.Empty, ConsoleColor.White);
        }

        private static void WriteXdbModel()
        {
            WriteHeader();
            Write("Serializing the CopyXdbModel...", ConsoleColor.White);

            string json = XdbModelWriter.Serialize(CopyXdbModel.Model);
            File.WriteAllText(CopyXdbModel.Model.FullName + ".json", json);

            WriteLine(" Done.", ConsoleColor.White);
        }
    }
}
