namespace Dinomopabot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Dinomopabot.Model;
    using Dinomopabot.Utils;
    using Mono.Options;

    class Program
    {
        static int Main(string[] args)
        {
            bool show_help = false;
            var settings = new Settings();
            var p = new OptionSet() {
                    { "p|path=",    "the folder to scan (defaults to current directory)", v => settings.Path = v},
                    { "s|subdirs",  "include subdirectories", v =>  settings.Subdirectories = v != null ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly},
                    { "f|filter=",  "search filter (defaults to *.*)", v => settings.Filter = v},
                    { "a|all",      "show ALL checksums of files, even non-copies", v => settings.All = v != null},
#if DEBUG
                    { "d|debug",    "Show debug information", v => Debug.Listeners.Add(new ConsoleTraceListener())},
#endif
                    { "?|h|help",   "show this message and exit", v => show_help = v != null },
            };

 #pragma warning disable 219
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

            var duplicates = new Dictionary<string, DuplicateSet>();
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
                    c = FileUtils.GetPartialChecksum(f, 4096);
                }
                catch (Exception)
                {
                    errors++;
                    continue;
                }
                finally 
                {
                    Debug.WriteLine("Files: {0}, Dupes: {1}, Errors: {2}", fileNum, dupes, errors);
                }

                Debug.WriteLine("- Checking: " + f);
                DuplicateSet dupe = duplicates.ContainsKey(c) ? duplicates[c] : null;

                if (dupe != null)
                {
                    if (FileUtils.IsPartialChecksum(c))
                    {
                        Debug.WriteLine("Partial checksum coliding: " + f);
                        string fullCheckSum;
                        FileInfo fileInfo;
                        if (dupe.Locations.Count == 1)
                        {
                            // Now handle the FullCheckSum for the file, already in the Dictionary 
                            string fileName = dupe.Locations[0];
                            Debug.WriteLine("Calculating first full checksum: " + fileName);
                            fullCheckSum = FileUtils.GetChecksum(fileName);

                            fileInfo = new FileInfo(fileName);
                            DuplicateSet dupeSet = new DuplicateSet { 
                                CheckSum = fullCheckSum, 
                                Locations = new List<string>() { fileName }, 
                                FileSize = fileInfo.Length 
                            };
                            duplicates.Add(fullCheckSum, dupeSet);
                        }

                        // Now handle the FullCheckSum for the current (2nd, 3rd, ...) file 
                        fullCheckSum = FileUtils.GetChecksum(f);
                        // Add the location to the partial checksum
                        dupe.Locations.Add(f); 

                        if (duplicates.ContainsKey(fullCheckSum))
                        {
                            Debug.WriteLine("True dupe for: " + f);
                            dupe = duplicates[fullCheckSum];
                        }
                        else
                        {
                            Debug.WriteLine("False dupe for: " + f);
                            fileInfo = new FileInfo(f);
                            dupe = new DuplicateSet { 
                                CheckSum = fullCheckSum, 
                                Locations = new List<string>() { f }, 
                                FileSize = fileInfo.Length 
                            };
                            duplicates.Add(fullCheckSum, dupe);
                            continue;
                        }
                        c = fullCheckSum;
                    }
                    dupe.Locations.Add(f);
                }

                if (dupe != null)
                {
                    dupes++;
                    Debug.WriteLine("** " + f);

                    if (dupe.Locations.Count == 2)
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
                        Console.Write(dupe.FileSize);
                        Console.Write("|");
                        Console.WriteLine(dupe.Locations[0]);
                    }

                    //write out the found file.
                    Console.Write(c);
                    Console.Write("|");
                    Console.Write(dupe.Locations.Count - 1);
                    Console.Write("|");
                    Console.Write(dupe.FileSize);
                    Console.Write("|");
                    Console.WriteLine(f);
                }
                else
                {
                    var fi = new FileInfo(f);
                    //var b = fi.Length;
                    var n = new DuplicateSet { CheckSum = c, Locations = new List<string>(), FileSize = fi.Length };
                    n.Locations.Add(f);
                    duplicates.Add(c, n);
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
                Console.WriteLine(" Find duplicate files, by calculating checksums.\r\n");
            }

            Console.Write("Usage: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Dupes.exe ");
            Console.ResetColor();
            Console.WriteLine("[options]");
            Console.WriteLine("Tip: redirect output to a .csv file, and manipulate with NimbleText.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

    }

    
}
