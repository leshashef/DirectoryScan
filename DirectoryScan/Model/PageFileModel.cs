using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScan.Model
{
    internal class PageFileModel
    {
        public string Name { get; set; }
        public string SizeNormal { get; set; }

        public long SizeForSort { get; set; }

        public FileModel FileModel { get; set; }

        public string SpaceColumn { get; set; } = string.Empty;
    }
}
