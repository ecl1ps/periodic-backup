using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeriodicBackup
{
    class Program
    {
        private static string originalFileName;
        private static string outputDirectory;
        private static DateTime lastChange;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("PeriodicBackup <time in minutes> <file to backup>[ <directory to backup>]");
                return;
            }

            int minutes;
            if (!int.TryParse(args[0], out minutes))
            {
                Console.WriteLine("Time '{0}' is not a valit number", args[0]);
                return;
            }

            if (!File.Exists(args[1]))
            {
                Console.WriteLine("File '{0}' not found", args[1]);
                return;
            }

            originalFileName = args[1];
            var originalFile = new FileInfo(originalFileName);

            if (args.Length > 2)
            {
                if (!Directory.Exists(args[2]))
                {
                    Console.WriteLine("Directory '{0}' not found", args[2]);
                    return;                    
                }
                else
                {
                    outputDirectory = args[2];
                }
            }
            else
            {
                outputDirectory = Path.Combine(originalFile.DirectoryName, "PeriodicBackup");
            }

            Console.WriteLine("Periodic backups of '{0}' every {1} minutes to '{2}' started", originalFile.Name, minutes, outputDirectory);
            Console.WriteLine("------------------------------------------------------------------");

            AutoResetEvent autoEvent = new AutoResetEvent(false);
            Timer t = new Timer(Tick, autoEvent, TimeSpan.FromMinutes(minutes), TimeSpan.FromMinutes(minutes));
            autoEvent.WaitOne();
        }

        static void Tick(object o)
        {
            var originalFile = new FileInfo(originalFileName);
            if (originalFile.LastWriteTime != lastChange)
            {
                Console.WriteLine("Creating backup in {0}", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                using (FileStream inFile = originalFile.OpenRead())
                {
                    using (FileStream outFile = File.Create(Path.Combine(outputDirectory,
                        string.Format("{0}-{1}{2}.gz", Path.GetFileNameWithoutExtension(originalFile.Name), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), originalFile.Extension))))
                    {
                        using (GZipStream compress = new GZipStream(outFile, CompressionMode.Compress))
                        {
                            inFile.CopyTo(compress);
                        }
                    }
                }

                lastChange = originalFile.LastWriteTime;
            }
            else
            {
                Console.WriteLine("File has not changed, backup skipped in {0}", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            }
        }
    }
}
