using System.Diagnostics;

namespace Dinomopabot.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    public static class FileUtils
    {
		/// <summary>
		/// Gets the checksum for the first part of the file.
		/// </summary>
		/// <returns>
		/// The checksum. If it's not the checksum of the complete file, and underscore ('_') is appended.
		/// </returns>
		/// <param name='filePath'>
		/// File path.
		/// </param>
		/// <param name='count'>
		/// Count.
		/// </param>
		public static string GetPartialChecksum(string filePath, int count)
        {
			bool isPartial;
			byte[] checksum;
			using (var file = File.OpenRead(filePath))
			{
				SHA256Managed sha = new SHA256Managed();
				byte[] buffer = new byte[count];
				int bytesRead = file.Read(buffer, 0, count);
				checksum = sha.ComputeHash(buffer, 0, bytesRead);
				isPartial = file.Length > bytesRead;
			}
			return BitConverter.ToString(checksum).Replace("-", String.Empty) + (isPartial ? "_" : String.Empty);

        }

		public static string GetChecksum(string filePath)
		{
			SHA256Managed sha = new SHA256Managed();
			byte[] checksum;
			using (var stream = new BufferedStream(File.OpenRead(filePath), 1200000))
			{
				checksum = sha.ComputeHash(stream); 
			}
			return BitConverter.ToString(checksum).Replace("-", String.Empty);
		}

		public static bool IsPartialChecksum(string checksum)
		{
			return checksum.EndsWith("_");
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

		#region Poor man's unit tests
		static void Test_PartialChecksum_OneFile(int fileSize, int offset)
		{
			// Setup: create files
			string fileName = Path.GetTempFileName();
			using(var file = File.CreateText(fileName))
			{
				var rnd = new Random();
				for(var i=0; i<fileSize; i++)
				{
					char ch = (char)rnd.Next(' ', '~');
					file.Write(ch);
				}
			}

			// Test
			string c1 = GetChecksum(fileName);
			string c2 = GetPartialChecksum(fileName, fileSize);
			Debug.Assert(c1 == c2, "count==fileSize should not give partial, but full checksum");

			Debug.Assert(offset > 0, "parameter offset should be > 0");
			c2 = GetPartialChecksum(fileName, fileSize - offset);
			Debug.Assert(IsPartialChecksum(c2), "Filesize minus something should give a partial hash");

			c2 = GetPartialChecksum(fileName, fileSize + offset);
			Debug.Assert(c1 == c2, "count==fileSize+N (N>0) should not give partial, but full checksum");

			// Tear down:
			File.Delete(fileName);
		}

		static void Test_PartialChecksum_TwoFile(int sizePartA, int sizePartB)
		{
			// Setup: create files
			int fileSize = sizePartA + sizePartB;
			string fileName1 = Path.GetTempFileName();
			string fileName2 = Path.GetTempFileName();
			using(var file1 = File.CreateText(fileName1))
			{
				var rnd = new Random();
				using(var file2 = File.CreateText(fileName2))
				{
					// Write same content to both files
					for(var i=0; i<sizePartA; i++)
					{
						char ch = (char)rnd.Next(' ', '~');
						file1.Write(ch);
						file2.Write(ch);
					}
					// Write different content
					// Make sure rnd doesn't give exact the same sequence for file1 and file2
					file1.Write('1');
					file2.Write('2');
					for(var i=1; i<sizePartB; i++)
					{
						char ch1 = (char)rnd.Next(' ', '~');
						char ch2 = (char)rnd.Next(' ', '~');
						file1.Write(ch1);
						file2.Write(ch2);
					}
				}
			}
			
			// Test
			string c1 = GetPartialChecksum(fileName1, sizePartA);
			string c2 = GetPartialChecksum(fileName1, sizePartA);
			Debug.Assert(IsPartialChecksum(c1), "c1 should be partial");
			Debug.Assert(IsPartialChecksum(c2), "c2 should be partial");
			Debug.Assert(c1 == c2, "First part should have same checksum");

			c1 = GetPartialChecksum(fileName1, fileSize);
			c2 = GetPartialChecksum(fileName2, fileSize);
			Debug.Assert(!IsPartialChecksum(c1), "c1 should not be partial");
			Debug.Assert(!IsPartialChecksum(c2), "c2 should not be partial");
			Debug.Assert(c1 != c2, "Whole file should have different checksum");

			string c1f = GetChecksum(fileName1);
			string c2f = GetChecksum(fileName2);
			Debug.Assert(c1 == c1f, "c1 GetPartialChecksum should give same as GetCheckSum");
			Debug.Assert(c2 == c2f, "c2 GetPartialChecksum should give same as GetCheckSum");

			// Tear down:
			File.Delete(fileName1);
			File.Delete(fileName2);
		}

		static int TestMain(string[] args)
		{
			Debug.Listeners.Add(new ConsoleTraceListener());
			Test_PartialChecksum_OneFile(1234, 1);
			Test_PartialChecksum_OneFile(4096, 256);
			Test_PartialChecksum_TwoFile(256, 128);
			Test_PartialChecksum_TwoFile(1024, 2048);
			Debug.WriteLine("Done testing, bravo!");
			return 0;
		}
		#endregion
    }
}
