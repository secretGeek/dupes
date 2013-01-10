using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Dinomopabot.Utils
{
    public static class FileUtils
    {
        public static string GetChecksum(string filePath)
        {
            using (var stream = new BufferedStream(File.OpenRead(filePath), 1200000))
            {
                //MD5M ss = new MD5();
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path)
                                        .SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
