using DirectoryScan.Controller;
using DirectoryScan.Model;
using Jil;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DirectoryScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DirectoryController dc;
        private ConverterController converter;

        private List<PageFileModel> renderModel;
        private Stack<List<PageFileModel>> prevPages;
        private SortingModel sortingModel;
        private object _lock_render = new object();

        private string globalPath = string.Empty;
        private bool completeScanDisks = false;
        public MainWindow()
        {

            InitializeComponent();
            prevPages = new Stack<List<PageFileModel>>();

            dc = new DirectoryController();
            converter = new ConverterController();
            sortingModel = new SortingModel();

            dc.OnReadyRender += Dc_OnReadyRender;

            Task.Run(LoadAllDisk);

            MainDirectory.MouseDoubleClick += NextFolder;
        }

        private void NextFolder(object sender, MouseButtonEventArgs e)
        {

            var listView = ((ListView)sender);

            var item = listView.SelectedItem as PageFileModel;
            if (item != null)
            {
                RenderChild(item.FileModel);
                // MessageBox.Show(item.Name + " Double Click handled!");
            }

        }

        private void Dc_OnReadyRender(bool canRender)
        {
            if (canRender)
            {
                RenderBase();
            }
        }
        private void RenderChild(FileModel parent)
        {
            if (!parent.IsDirectory)
            {
                return;
            }
            lock (_lock_render)
            {
                prevPages.Push(renderModel);
                renderModel = new List<PageFileModel>();
                var children = parent.FilesChildren;
                RenderAllPage(children, parent);
                MainDirectory.ItemsSource = renderModel;
            }
        }
        private void RenderBase()
        {
            lock (_lock_render)
            {
                var disks = dc.GetCurrentFileManager();


                renderModel = new List<PageFileModel>();
                RenderAllPage(disks.ToList());

                SortFile("FileName");
                Dispatcher.Invoke(() =>
                {
                    MainDirectory.ItemsSource = renderModel;
                });
            }
        }
        private void RenderAllPage(List<FileModel> renderList, FileModel parent = null)
        {
            if (renderList.Count > 0)
            {
                globalPath = RenderPath(renderList[0], true);
              
                for (int i = 0; i < renderList.Count; i++)
                {
                    Render(renderList[i]);
                }
            }
            if (parent != null)
            {
                globalPath = RenderPath(parent, false);
            }

            Dispatcher.Invoke(() => { FullPath.Text = globalPath; });

        }
        private string RenderPath(FileModel file, bool current)
        {
            string fileName = current ? "" : file.Name;
            if (file.Parent != null)
            {
                return RenderPath(file.Parent, false) + "\\" + fileName;
            }
            else
            {
                return fileName;
            }
        }
        private void RenderBack(List<PageFileModel> pageFiles)
        {
            lock (_lock_render)
            {
                var files = pageFiles.Select(x => x.FileModel).ToList();

                renderModel = new List<PageFileModel>();
                RenderAllPage(files);
                MainDirectory.ItemsSource = renderModel;

            }
        }

        private void Render(FileModel file)
        {
            if (file != null)
            {
                var folder = file;

                string name = folder.Name;
                string size = folder.Size == 0 ? PlaceHolderSize() : converter.ConvertByteToNormal(folder.Size);
                long sortedSize = folder.Size;

                renderModel.Add(new PageFileModel
                {
                    Name = name,
                    SizeNormal = size,
                    SizeForSort = sortedSize,
                    FileModel = folder,
                });
            }
        }
        private string PlaceHolderSize()
        {
            if (completeScanDisks)
            {
                return "Empty";
            }
            else
            {
                return "Loading...";
            }
        }
        public Task LoadAllDisk()
        {
            bool res = dc.GetFileManagerSystem();
            if (res)
            {
                MessageBox.Show("Загрузка дисков успешно завершена");
                completeScanDisks = true;
                ReRenderGlobal();
            }
            else
            {
                MessageBox.Show("Загрузка дисков провалена");
            }
            return Task.CompletedTask;
        }

        private void AddPrevPage(List<FileModel> fileModels)
        {
            List<PageFileModel> pageFileModels = new List<PageFileModel>();

            for (int i = 0; i < fileModels.Count; i++)
            {
                pageFileModels.Add(new PageFileModel
                {
                    Name = fileModels[i].Name,
                    SizeForSort = fileModels[i].Size,
                    SizeNormal = converter.ConvertByteToNormal(fileModels[i].Size),
                    FileModel = fileModels[i],
                });
            }

            prevPages.Push(pageFileModels);
        }

        private void ReRenderGlobal()
        {
            lock (_lock_render)
            {
                var disks = dc.GetCurrentFileManager();

                string path = globalPath.Replace(":\\", "*");
                string[] paths = path.Split("\\").Where(x => x != string.Empty).ToArray();
                if (paths.Length > 0)
                {
                    paths[0] = paths[0].Replace("*", ":\\");
                    FileModel folder = disks.FirstOrDefault(x => x.Name == paths[0]);
                    prevPages.Clear();
                    AddPrevPage(disks.ToList());

                    for (int i = 1; i < paths.Length; i++)
                    {

                        AddPrevPage(folder.FilesChildren);

                        folder = folder.FilesChildren.FirstOrDefault(x => x.Name == paths[i]);

                    }

                    AddPrevPage(folder.FilesChildren);

                    var currentPage = prevPages.Pop();

                    var files = currentPage.Select(x => x.FileModel).ToList();

                    renderModel = new List<PageFileModel>();
                    RenderAllPage(files, folder);

                }
                else
                {
                    renderModel = new List<PageFileModel>();
                    RenderAllPage(disks.ToList());
                }
                Dispatcher.Invoke(() => { MainDirectory.ItemsSource = renderModel; });

            }
        }

        private void OnSortColumn(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            if (column != null)
            {
                MainDirectory.SelectedItem = null;
                String field = column.Tag as String;

                SortFile(field);

                MainDirectory.ItemsSource = renderModel;

            }
        }
        private void OnBackPage(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            if (column != null)
            {
                MainDirectory.SelectedItem = null;

                if (prevPages.Count > 0)
                {
                    RenderBack(prevPages.Pop());
                }

                MainDirectory.ItemsSource = renderModel;

            }
        }
        private void SortFile(string column)
        {
            if (sortingModel.Column == null || sortingModel.Column != column)
            {
                sortingModel.Column = column;
                sortingModel.IsAscending = true;
            }
            else
            {
                sortingModel.IsAscending = !sortingModel.IsAscending;
            }

            if (sortingModel.Column == "FileName")
            {
                renderModel = renderModel.OrderBy(x => x.Name).ToList();
            }
            else if (sortingModel.Column == "Size")
            {
                renderModel = renderModel.OrderBy(x => x.SizeForSort).ToList();
            }

            if (!sortingModel.IsAscending)
            {
                renderModel.Reverse();
            }
        }
    }
}