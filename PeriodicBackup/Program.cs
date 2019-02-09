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
    class FileData
    {
        public string FilePath { get; }
        public DateTime LastChange { get; set; }

        public FileData(string filePath)
        {
            FilePath = filePath;
        }
    }

    class Program
    {
        private static List<FileData> backupedFiles;
        private static string outputDirectory;
        private static string outputFileName;
        private static string outputExtension;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("PeriodicBackup <time in minutes> <files to backup separated by semicolon> [<directory to backup>] [<name of output file>]");
                return;
            }

            if (!int.TryParse(args[0], out int minutes))
            {
                Console.WriteLine("Time '{0}' is not a valit number", args[0]);
                return;
            }

            var files = args[1].Split(';');
            if (files.Length == 0)
            {
                Console.WriteLine("No files for backup specified");
                return;
            }

            if (args.Length < 4 && files.Length > 1)
            {
                Console.WriteLine("Output file name must be specified when backing-up multiple files");
                return;
            }

            backupedFiles = new List<FileData>();
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine("File '{0}' not found", file);
                    return;
                }

                backupedFiles.Add(new FileData(file));
            }

            outputFileName = args.Length == 4 ? args[3] : Path.GetFileNameWithoutExtension(backupedFiles.First().FilePath);
            outputExtension = string.Empty;

            if (args.Length > 2)
            {
                outputDirectory = args[2];
            }
            else
            {
                var originalFile = new FileInfo(backupedFiles.FirstOrDefault().FilePath);
                outputDirectory = Path.Combine(originalFile.DirectoryName, "PeriodicBackup");
            }

            Console.WriteLine("Started periodic backups every {0} minutes to '{1}' of files:", minutes, outputDirectory);
            backupedFiles.ForEach(f => Console.WriteLine(new FileInfo(f.FilePath).Name));
            Console.WriteLine("------------------------------------------------------------------");

            AutoResetEvent autoEvent = new AutoResetEvent(false);
            Timer t = new Timer(Tick, autoEvent, TimeSpan.FromMinutes(minutes), TimeSpan.FromMinutes(minutes));
            autoEvent.WaitOne();
        }

        static void Tick(object o)
        {
            var info = backupedFiles.Select(bf => new { BackupInfo = bf, FileInfo = new FileInfo(bf.FilePath) });
            var now = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            if (info.All(i => i.BackupInfo.LastChange >= i.FileInfo.LastWriteTime))
            {
                Console.WriteLine("Files has not changed, backup skipped in {0}", now);
                return;
            }

            Console.WriteLine("Creating backup in {0}", now);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var package = new BackupPackage();

            foreach (var i in info)
                package.AddFile(i.FileInfo);

            package.Pack(Path.Combine(outputDirectory, string.Format("{0}-{1}{2}.zip", outputFileName, now, outputExtension)), 6);

            foreach (var i in info)
                i.BackupInfo.LastChange = i.FileInfo.LastWriteTime;
        }
    }
}