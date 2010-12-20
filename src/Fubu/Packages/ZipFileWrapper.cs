using System;
using Ionic.Zip;

namespace Fubu.Packages
{
    public class ZipFileWrapper : IZipFile
    {
        private readonly ZipFile _file;

        public ZipFileWrapper(ZipFile file)
        {
            _file = file;
        }

        public void AddFile(string fileName)
        {
            _file.AddFile(fileName, "");
        }

        public void AddFile(string fileName, string zipFolder)
        {
            Console.WriteLine("  Adding {0} to zip folder '{1}'", fileName, zipFolder);
            _file.AddFile(fileName, zipFolder);
        }

        public void AddFiles(ZipFolderRequest request)
        {
            request.WriteToZipFile(this);
        }
    }
}