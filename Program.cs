namespace Dinomopabot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using Mono.Options;
    using Dinomopabot.Model;
    using Dinomopabot.Utils;

    class Settings
    {
        public Settings()
        {
            Path = @"C:\";
            Subdirectories = SearchOption.AllDirectories ;
            Filter = "*.*";
            All = false;
        }

        public string Path { get; set; }
        public SearchOption Subdirectories { get; set; }
        public string Filter { get; set; }
        public bool All { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            bool show_help = false;
            var settings = new Settings();
            var p = new OptionSet() {
                    { "p|path=",    "the folder to scan", v => settings.Path = v},
                    { "s|subdirs",  "include subdirectories", v =>  settings.Subdirectories = v != null ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly},
                    { "f|filter=",  "search filter (defaults to *.*)", v => settings.Filter = v},
                    { "a|all",      "show ALL checksums of files, even non-copies", v => settings.All = v != null},
                    { "?|h|help",   "show this message and exit", v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\nError parsing arguments");

                Console.WriteLine(e.Message);
                Console.ResetColor();
                return 1;
            }

            if (args.Length == 0) show_help = true;

            if (p.UnrecognizedOptions != null && p.UnrecognizedOptions.Count > 0)
            {
                ShowHelp(p);
                return 1;
            }

            if (show_help)
            {
                ShowHelp(p);
                return 0;
            }

            Dictionary<String, DuplicateSet> h = new Dictionary<string, DuplicateSet>();
            int fileNum = 0;
            int dupes = 0;
            int errors = 0;

            bool headersWritten = false;

            foreach (var f in FileUtils.EnumerateFiles(settings.Path, settings.Filter, settings.Subdirectories))
            {
                fileNum++;
                string c;

                try
                {
                    c = FileUtils.GetChecksum(f);
                }
                catch (IOException)
                {
                    errors++;
                    continue;
                }

                Debug.WriteLine("Files: {0}, Dupes: {1}, Errors: {2}", fileNum, dupes, errors);

                if (h.ContainsKey(c))
                {
                    dupes++;
                    h[c].Locations.Add(f);
                    Debug.WriteLine("** " + f);

                    if (h[c].Locations.Count == 2)
                    {
                        if (!headersWritten)
                        {
                            Console.WriteLine("CheckSum|DuplicateNum|Filesize|Path");
                            headersWritten = true;
                        }


                        //write out the first file.
                        Console.Write(c);
                        Console.Write("|");
                        Console.Write(0);
                        Console.Write("|");
                        Console.Write(h[c].FileSize);
                        Console.Write("|");
                        Console.WriteLine(h[c].Locations[0]);
                    }

                    //write out the found file.
                    Console.Write(c);
                    Console.Write("|");
                    Console.Write(h[c].Locations.Count - 1);
                    Console.Write("|");
                    Console.Write(h[c].FileSize);
                    Console.Write("|");
                    Console.WriteLine(f);
                }
                else
                {
                    var fi = new FileInfo(f);
                    //var b = fi.Length;
                    var n = new DuplicateSet { CheckSum = c, Locations = new List<string>(), FileSize = fi.Length };
                    n.Locations.Add(f);
                    h.Add(c, n);
                }
            }

            return 0;
        }

        static void ShowHelp(OptionSet p)
        {
            if (p.UnrecognizedOptions != null && p.UnrecognizedOptions.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\nError. Unrecognized commandline option" + (p.UnrecognizedOptions.Count > 1 ? "s" : "") + ".");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.ResetColor();
                foreach (var s in p.UnrecognizedOptions)
                {
                    Console.Write("Unrecognized: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(s);
                    Console.ResetColor();
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please see below for all valid options.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\r\nDupes"); Console.ResetColor();
                //Console.WriteLine(" version " + AssemblyAttributes.AssemblyVersion.ToString());
                Console.WriteLine(" Find duplicate files, by calculating checksums.\r\n");
            }

            Console.Write("Usage: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Dupes.exe ");
            Console.ResetColor();
            Console.WriteLine("[options]");
            Console.WriteLine("Tip: redirect output to a .csv file, and manipulate with excel.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

    }

    
}
