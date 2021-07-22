using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

using Ookii.Dialogs.Wpf;
using SharpCompress.Readers;
using SharpCompress.Common;

namespace Archive_Extraction_tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string rootPath = string.Empty;
        private int maxCount = 0;
        private readonly Stopwatch stopwatch = new Stopwatch();       
        private long elapsed;


        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        private string total;

        public string Total
        {
            get => total;
            set 
            {
                total = value;
                OnPropertyChanged("Total");
            }
        }

        private string text;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        private string progress;

        public string Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private string currentProgress;

        public string CurrentProgress
        {
            get => currentProgress;
            set
            {
                currentProgress = value;
                OnPropertyChanged("CurrentProgress");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Select parent folder with archives.";
            dialog.UseDescriptionForTitle = true;

            if ((bool)dialog.ShowDialog())
            {
                rootPath = dialog.SelectedPath;
                txtBrowse.Text = rootPath;
                await LoadFiles();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }


        private async Task LoadFiles()
        {
            var fileList = new List<string>();

            try
            {
                await Task.Run(() =>
                {
                    fileList = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories).Where(file => file.EndsWith("*.zip") || file.EndsWith("*.rar") || file.EndsWith("*.7zip") || file.EndsWith("*.xz") || file.EndsWith("*.tar") || file.EndsWith("*.bzip2")).ToList();
                });

                prgProgress.Minimum = 0;
                prgProgress.Maximum = fileList.Count;
                maxCount = fileList.Count;

                await Extract(fileList);
            }
            catch (ArgumentNullException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (ArgumentException)
            {

            }
            catch (UnauthorizedAccessException)
            {

            }
            catch (DirectoryNotFoundException)
            {

            }
            catch (PathTooLongException)
            {

            }
            catch (IOException)
            {

            }
            catch (Exception)
            {
                
            }
        }


        private async Task Extract(List<string> fileList)
        {
            await Task.Run(() => 
            {
                foreach (var file in fileList)
                {
                    try
                    {
                        stopwatch.Start();
                        
                        using (Stream stream = File.OpenRead(file))
                        {
                            using (var reader = ReaderFactory.Open(stream))
                            {
                                while (reader.MoveToNextEntry())
                                {
                                    if (!reader.Entry.IsDirectory)
                                    {
                                        reader.WriteEntryToDirectory(Path.GetFileNameWithoutExtension(file), new ExtractionOptions()
                                        {
                                            ExtractFullPath = true,
                                            Overwrite = true
                                        });
                                    }
                                }
                            }
                        }

                        elapsed = stopwatch.ElapsedMilliseconds;
                        stopwatch.Reset();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

    }
}
