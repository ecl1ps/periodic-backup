using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace PeriodicBackup
{
    public class BackupPackage
    {
        private List<FileInfo> Files = new List<FileInfo>();

        public void AddFile(FileInfo fileInfo)
        {
            Files.Add(fileInfo);
        }

        public void Pack(string outPathName, int compressionLevel)
        {
            var zipStream = new ZipOutputStream(File.Create(outPathName));

            zipStream.SetLevel(compressionLevel); //0-9, 9 being the highest level of compression

            CompressFiles(zipStream);

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        private void CompressFiles(ZipOutputStream zipStream)
        {
            foreach (var fi in Files)
            {
                var newEntry = new ZipEntry(fi.Name)
                {
                    DateTime = fi.LastWriteTime,
                    Size = fi.Length
                };

                zipStream.PutNextEntry(newEntry);

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = fi.OpenRead())
                    StreamUtils.Copy(streamReader, zipStream, buffer);

                zipStream.CloseEntry();
            }
        }
    }
}