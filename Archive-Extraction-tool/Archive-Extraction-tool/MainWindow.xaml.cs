using Ookii.Dialogs.Wpf;
using Serilog;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Archive_Extraction_tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>  

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private string rootPath = string.Empty;
        private int maxCount = 0;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private long elapsed;
        private double estimated;
        private int timelapse = 0;
        private Timer timer = new Timer(1000);
        private List<string> fileList;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();
            InitializeComponent();
            DataContext = this;
            timer.Elapsed += Timer_Tick;
            Log.Information("Application has started!");

            Dispatcher.Invoke(() =>
            {
                CurrentProgress = $"Progress:  0/0";
                Progress = "Unknown";
                EstimatedTime = "00h:00m:00s";
            });
        }

        private void Timer_Tick(object source, ElapsedEventArgs e)
        {
            timelapse++;
            TimeSpan t = TimeSpan.FromSeconds(timelapse);

            Dispatcher.Invoke(() =>
            {
                EstimatedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
            });
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

        private string estimatedTime;

        public string EstimatedTime
        {
            get => estimatedTime;
            set
            {
                estimatedTime = value;
                OnPropertyChanged("EstimatedTime");
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
            Log.Information("Selecting rootpath!");
            if ((bool)dialog.ShowDialog())
            {
                rootPath = dialog.SelectedPath;
                txtBrowse.Text = rootPath;
                timer.Start();
                await LoadFiles();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }


        private async Task LoadFiles()
        {
            Log.Information("Loading files!");

            try
            {
                await Task.Run(() =>
                {

                    fileList = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories).Where(file => file.EndsWith(".zip") || file.EndsWith(".rar") || file.EndsWith(".7zip") || file.EndsWith(".xz") || file.EndsWith(".tar") || file.EndsWith(".bzip2")).ToList();
                });

                prgProgress.Minimum = 0;
                prgProgress.Value = prgProgress.Minimum;
                prgProgress.Maximum = fileList.Count;
                maxCount = fileList.Count;
            }
            catch (ArgumentNullException ex)
            {
                Log.Fatal(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Fatal(ex.Message);
            }
            catch (ArgumentException ex)
            {
                Log.Fatal(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Fatal(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Fatal(ex.Message);
            }
            catch (PathTooLongException ex)
            {
                Log.Fatal(ex.Message);
            }
            catch (IOException ex)
            {
                Log.Fatal(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
            }
        }


        private async Task Extract(List<string> fileList)
        {
            Log.Information("Extracting files.");

            await Task.Run(() =>
            {
                foreach (var file in fileList)
                {
                    Log.Debug($"Current file extracting: {file}");
                    var extension = Path.GetExtension(file);
                    var extractionPath = file.Remove(file.Length - extension.Length, extension.Length);
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
                                        reader.WriteEntryToDirectory(extractionPath, new ExtractionOptions()
                                        {
                                            ExtractFullPath = true,
                                            Overwrite = true
                                        });
                                    }
                                }
                            }
                        }

                        elapsed = stopwatch.ElapsedMilliseconds;
                        Dispatcher.Invoke(() => { prgProgress.Value++; });
                        stopwatch.Reset();
                    }
                    catch (Exception ex)
                    {
                        Log.Fatal(ex.Message);
                        Dispatcher.Invoke(() => { prgProgress.Value++; });
                    }
                }
            });

            if ((bool)chkDeleteArchive.IsChecked)
            {
                await Delete(fileList);
            }
            else
            {
                timer.Stop();
            }
        }

        private async Task Delete(List<string> fileList)
        {
            Log.Information("Deleting files!");
            await Task.Run(() =>
            {
                foreach (var file in fileList)
                {
                    Log.Debug($"Deleting file: {file}");
                    File.Delete(file);
                }
            });
            timer.Stop();
        }

        private void prgProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Log.Debug("progressbar value updated!");
            Dispatcher.Invoke(() =>
            {
                CurrentProgress = $"Progress: {prgProgress.Value}/{maxCount}";
            });

            double currentProgressValue = prgProgress.Value;
            double lastElapsedTime = elapsed;
            Log.Debug($"current iteration took {elapsed}ms");

            //all iterations - current iteration = iteration to go
            double iterations = prgProgress.Maximum - prgProgress.Value;
            estimated = iterations * lastElapsedTime;

            Dispatcher.Invoke(() =>
           {
               Progress = $"Estimated: {(int)(estimated / 1000) / 60} Min";
           });


        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= Timer_Tick;
            timer.Dispose();
            Log.CloseAndFlush();
        }

        private async void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            await Extract(fileList);
        }
    }
}
