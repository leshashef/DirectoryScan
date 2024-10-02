using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScan.Controller
{
    class ConverterController
    {

        public decimal ConvertByteToGByte(long bytes)
        {
            
            return Math.Round(bytes / 1_073_741_824m, 2);
        }

        public string ConvertByteToNormal(long bytes)
        {
            if(bytes >= 1_073_741_824m * 1024m)
            {
                return Math.Round(bytes / (1_073_741_824m * 1024m), 2).ToString() + " ТБайт"; // тера байты
            }
            else if(bytes >= 1_073_741_824m)
            {
                return Math.Round(bytes / 1_073_741_824m, 2).ToString() + " ГБайт"; // ГБайт
            }
            else if(bytes >= 1_048_576m)
            {
                return Math.Round(bytes / 1_048_576m, 2).ToString() + " МБайт"; // МБайт
            }
            else if(bytes >= 1024m)
            {
                return Math.Round(bytes / 1024m, 2).ToString() + " КБайт"; // кБайт
            }
            return bytes.ToString() + " Байт";
        }
    }
}
