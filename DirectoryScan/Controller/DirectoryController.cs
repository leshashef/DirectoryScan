using DirectoryScan.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DirectoryScan.Controller
{
    internal class DirectoryController
    {
        public event Action<bool> OnReadyRender;
        public int errorCount = 0;
        public int countFilesAll = 0;
        private FileModel[] disks;

        public DirectoryController()
        {

        }

        public FileModel[] GetCurrentFileManager()
        {
            return disks;
        }

        public bool GetFileManagerSystem()
        {
            try
            {
                DriveInfo[] drivers = DriveInfo.GetDrives();
                disks = new FileModel[drivers.Length];

                Parallel.For(0, drivers.Length, (index) =>
                {
                    GetAllDirectoryesDisk(drivers[index], disks, index);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        public void GetAllDirectoryesDisk(DriveInfo driver, FileModel[] disks, int index) 
        {

            DirectoryInfo folder = new DirectoryInfo(driver.Name);


            disks[index] = new FileModel()
            {
                Name = folder.Name,
                Size = driver.TotalSize - driver.TotalFreeSpace,
                Parent = null,
                IsDirectory = true
            };

            OnReadyRender?.Invoke(true);

            long AllSize = FolderFulPack(folder, disks[index], true);

        }
        private long FolderFulPack(DirectoryInfo folder, FileModel current, bool root = false)
        {

            long totalSizeOfDir = 0;
            object lockObj = new object();

            // Get all files into the directory 
            FileInfo[] allFiles = folder.GetFiles();



            // Loop through every file and get size of it 
            foreach (FileInfo file in allFiles)
            {
                try
                {

                    var fileInDir = new FileModel
                    {
                        Name = file.Name + file.Extension,
                        Parent = current,
                        Size = file.Length,
                        IsDirectory = false
                    };

                    current.FilesChildren.Add(fileInDir);
                    totalSizeOfDir += fileInDir.Size;

                }
                //catch (UnauthorizedAccessException ex)
                //{

                //}
                catch (Exception ex)
                {
                    errorCount++;
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
                        FileModel next = new FileModel()
                        {
                            Name = dir.Name,
                            Parent = current,
                            Size = 0,
                            IsDirectory = true
                        };
                        next.Size = FolderFulPack(dir, next);
                        lock (lockObj)
                        {
                            totalSizeOfDir += next.Size;
                        }
                        current.FilesChildren.Add(next);
                    }
                    //catch (UnauthorizedAccessException ex)
                    //{

                    //}
                    catch (Exception ex)
                    {
                        errorCount++;
                    }
                });
            }
            else
            {
                foreach (DirectoryInfo dir in subFolders)
                {
                    try
                    {
                        FileModel next = new FileModel()
                        {
                            Name = dir.Name,
                            Parent = current,
                            Size = 0,
                            IsDirectory = true
                        };
                        next.Size = FolderFulPack(dir, next);
                        totalSizeOfDir += next.Size;
                        current.FilesChildren.Add(next);
                    }
                    //catch (UnauthorizedAccessException ex)
                    //{

                    //}
                    catch (Exception ex)
                    {
                        errorCount++;
                    }

                }
            }



            return totalSizeOfDir;
        }


    }
}
