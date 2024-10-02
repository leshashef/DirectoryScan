using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScan.Model
{
    internal class FileModel
    {
        public string Name { get; set; } = null!;
        public long Size { get; set; }

        public bool IsDirectory { get; set; }

        public List<FileModel> FilesChildren { get; set; }
        public FileModel Parent { get; set; } = null!;
        public FileModel()
        {
            FilesChildren = new List<FileModel>();
        }
    }
}
