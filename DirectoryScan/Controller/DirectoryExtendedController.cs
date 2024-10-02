using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScan.Controller
{
    internal class DirectoryExtendedController
    {
        public long GetSizeDirectory(string path) //C:\Users\leshashef\Desktop\ТестоваяПапкаВес
        {
            // Get the directory information using directoryInfo() method 
            DirectoryInfo folder = new DirectoryInfo(path);

            // Calling a folderSize() method 
            long totalFolderSize = FolderSize(folder);

            return totalFolderSize;
        }
        public DriveInfo[] GetDrives()
        {
            return DriveInfo.GetDrives();
        }
        private long FolderSize(DirectoryInfo folder, bool root = false)
        {

            long totalSizeOfDir = 0;

            // Get all files into the directory 
            FileInfo[] allFiles = folder.GetFiles();

            // Loop through every file and get size of it 
            foreach (FileInfo file in allFiles)
            {
                try
                {
                    totalSizeOfDir += file.Length;
                }
                catch (UnauthorizedAccessException ex)
                {

                }
                catch (Exception ex)
                {

                }

            }

            // Find all subdirectories 
            DirectoryInfo[] subFolders = folder.GetDirectories();

            // Loop through every subdirectory and get size of each 

            if (root)
            {
                Parallel.ForEach(subFolders, (dir) =>
                {
                    try
                    {
                        totalSizeOfDir += FolderSize(dir);
                    }
                    catch (UnauthorizedAccessException ex)
                    {

                    }
                    catch (Exception ex)
                    {

                    }
                });

            }
            else
            {
                foreach (DirectoryInfo dir in subFolders)
                {
                    try
                    {
                        totalSizeOfDir += FolderSize(dir);
                    }
                    catch (UnauthorizedAccessException ex)
                    {

                    }
                    catch (Exception ex)
                    {

                    }

                }
            }


            // Return the total size of folder 
            return totalSizeOfDir;
        }
    }
}
