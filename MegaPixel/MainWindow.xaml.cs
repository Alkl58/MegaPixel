using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MegaPixel
{
    public partial class MainWindow : Window
    {
        public string imageOutput, encoder, allSettingsLibavif;
        public int workerCount, imageChunksCount;
        public bool imageOutputSet;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ButtonStartEncoding_Click(object sender, RoutedEventArgs e)
        {
            setParams();
            try
            {
                ProgressBar.Value = 0;
                ProgressBar.Maximum = imageChunksCount;
                await Task.Run(() => ParallelEncode());
            }
            catch { }
        }

        private void ButtonSaveTo_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browseOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            if (browseOutputFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imageOutput = browseOutputFolder.SelectedPath;
                imageOutputSet = true;
            }
        }

        private void CheckBoxCustomSettings_Checked(object sender, RoutedEventArgs e)
        {
            if (ComboBoxEncoder.SelectedIndex == 0) { SetLibavifParams(true); TextBoxCustomSettings.Text = allSettingsLibavif; }
        }

        private void ButtonOpenSource_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxBatchEncoding.IsChecked == false)
            {
                OpenFileDialog openImageFileDialog = new OpenFileDialog();
                openImageFileDialog.Filter = "Image Files|*.png;*.jpg;|All Files|*.*";
                Nullable<bool> result = openImageFileDialog.ShowDialog();
                if (result == true) { ListBoxImagesToConvert.Items.Add(openImageFileDialog.FileName); ListBoxImagesToConvert.SelectedIndex = 0; }
            }
            else
            {
                System.Windows.Forms.FolderBrowserDialog browseSourceFolder = new System.Windows.Forms.FolderBrowserDialog();
                if (browseSourceFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DirectoryInfo queueFiles = new DirectoryInfo(browseSourceFolder.SelectedPath);
                    foreach (var file in queueFiles.GetFiles())
                    {
                        ListBoxImagesToConvert.Items.Add(file.FullName);
                    }
                }
            }

        }

        private void setParams()
        {
            encoder = ComboBoxEncoder.Text;
            workerCount = Int16.Parse(TextBoxWorkerCount.Text);
            imageChunksCount = 0;
            foreach (var file in ListBoxImagesToConvert.Items)
            {
                imageChunksCount += 1;
            }
            if (ComboBoxEncoder.SelectedIndex == 0) { SetLibavifParams(false); }
        }

        private void SetLibavifParams(bool temp)
        {
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                allSettingsLibavif = TextBoxCustomSettings.Text;
            }
            else
            {
                allSettingsLibavif = " --speed " + ComboBoxAvifSpeed.Text + " --jobs " + TextBoxAvifThreads.Text + " --depth " + ComboBoxAvifDepth.Text + " --yuv " + ComboBoxAvifColorFormat.Text + " --range " + ComboBoxAvifColorRange.Text + " --min " + TextBoxAvifMinQ.Text + " --max " + TextBoxAvifMaxQ.Text + " --tilerowslog2 " + ComboBoxAvifTileRows.Text + " --tilecolslog2 " + ComboBoxAvifTileColumns.Text + " ";
            }
            
        }

        private void ParallelEncode()
        {
            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(workerCount))
            {
                List<Task> tasks = new List<Task>();
                foreach (var items in ListBoxImagesToConvert.Items)
                {
                    string imageOutputTemp;
                    if (imageOutputSet == false)
                    {
                        imageOutputTemp = items.ToString() + "_converted.";
                    }
                    else
                    {
                        string imageName = Path.GetFileNameWithoutExtension(items.ToString());
                        imageOutputTemp = Path.Combine(imageOutput, imageName + ".");
                    }

                    concurrencySemaphore.Wait();
                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Process process = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.UseShellExecute = true;
                            startInfo.FileName = "cmd.exe";
                            
                            switch (encoder)
                            {
                                case "avif":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "avif");
                                    startInfo.Arguments = "/C avifenc.exe " + '\u0022' + items + '\u0022' + allSettingsLibavif + '\u0022' + imageOutputTemp + "avif" + '\u0022';
                                    break;
                                default:
                                    break;
                            }
                            Console.WriteLine(startInfo.Arguments);
                            process.StartInfo = startInfo;
                            process.Start();
                            process.WaitForExit();
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                            ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value += 1, DispatcherPriority.Background);
                            LabelProgressbar.Dispatcher.Invoke(() => LabelProgressbar.Content = ProgressBar.Value + " / " + imageChunksCount.ToString(), DispatcherPriority.Background);
                        }
                    });
                    tasks.Add(t);

                }
                Task.WaitAll(tasks.ToArray());
            }
        }

    }
}
