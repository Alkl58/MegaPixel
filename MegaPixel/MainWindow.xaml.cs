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
        public string imageOutput, encoder, allSettingsLibavif, allSettingsWebp, allSettingsJpegxl, allSettingsMozjpeg, allSettingsEct;
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
                LabelProgressbar.Content = "0 / " + imageChunksCount;
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
            if (ComboBoxEncoder.SelectedIndex == 1) { SetWebpParams(true); TextBoxCustomSettings.Text = allSettingsWebp; }
            if (ComboBoxEncoder.SelectedIndex == 2) { SetJpegxlParams(true); TextBoxCustomSettings.Text = allSettingsJpegxl; }
            if (ComboBoxEncoder.SelectedIndex == 4) { SetMozjpegParams(true); TextBoxCustomSettings.Text = allSettingsMozjpeg; }
            if (ComboBoxEncoder.SelectedIndex == 5) { SetEctParams(true); TextBoxCustomSettings.Text = allSettingsEct; }
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

        private void ButtonRemoveFromList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ListBoxImagesToConvert.Items.RemoveAt(ListBoxImagesToConvert.SelectedIndex);
            }
            catch { }
        }

        private void ComboBoxEncoder_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboBoxEncoder.SelectedIndex == 4) 
            {  
                foreach (var element in ListBoxImagesToConvert.Items)
                {
                    if (Path.GetExtension(element.ToString()) != "jpg" )
                    {
                        MessageBox.Show("You have elements in the Queue, which are not jpg/jpg. \n\nMozJpeg is only reduces file sizes of JPEG images! \n\nPlease check your Queue and remove non jpeg elements.");
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
            if (ComboBoxEncoder.SelectedIndex == 1) { SetWebpParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 2) { SetJpegxlParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 4) { SetMozjpegParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 5) { SetEctParams(false); }
        }

        private void SetLibavifParams(bool temp)
        {
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                allSettingsLibavif = TextBoxCustomSettings.Text;
            }
            else
            {
                if (CheckBoxAvifLossless.IsChecked == true)
                {
                    allSettingsLibavif = "--lossless --speed " + ComboBoxAvifSpeed.Text + " --jobs " + TextBoxAvifThreads.Text + " --depth " + ComboBoxAvifDepth.Text + " --yuv " + ComboBoxAvifColorFormat.Text + " --range " + ComboBoxAvifColorRange.Text + " --tilerowslog2 " + ComboBoxAvifTileRows.Text + " --tilecolslog2 " + ComboBoxAvifTileColumns.Text;
                }
                else
                {
                    allSettingsLibavif = "--speed " + ComboBoxAvifSpeed.Text + " --jobs " + TextBoxAvifThreads.Text + " --depth " + ComboBoxAvifDepth.Text + " --yuv " + ComboBoxAvifColorFormat.Text + " --range " + ComboBoxAvifColorRange.Text + " --min " + TextBoxAvifMinQ.Text + " --max " + TextBoxAvifMaxQ.Text + " --tilerowslog2 " + ComboBoxAvifTileRows.Text + " --tilecolslog2 " + ComboBoxAvifTileColumns.Text;
                }
            }
        }

        private void SetWebpParams(bool temp)
        {
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                allSettingsWebp = TextBoxCustomSettings.Text;
            }
            else
            {
                if (CheckBoxAvifLossless.IsChecked == true)
                {
                    allSettingsWebp = "-preset " + ComboBoxWebpPreset.Text + " -lossless -m " + ComboBoxWebpSpeed.SelectedIndex;
                }
                else
                {
                    allSettingsWebp = "-preset " + ComboBoxWebpPreset.Text + " -q " + TextBoxWebpQuality.Text + " -m " + ComboBoxWebpSpeed.SelectedIndex;
                }
                if (CheckBoxWebpMultiThreading.IsChecked == true)
                {
                    allSettingsWebp += " -mt";
                }
            }
        }

        private void SetJpegxlParams(bool temp)
        {
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                allSettingsJpegxl = TextBoxCustomSettings.Text;
            }
            else
            {
                allSettingsJpegxl = "-q " + TextBoxJpegxlQuality.Text + " -s " + ComboBoxJpegxlSpeed.Text;
            }
        }

        private void SetMozjpegParams(bool temp)
        {
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                allSettingsMozjpeg = TextBoxCustomSettings.Text;
            }
            else
            {
                allSettingsMozjpeg = "-quality " + TextBoxWebpQuality.Text;
            }
        }

        private void SetEctParams(bool temp)
        {
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                allSettingsEct = TextBoxCustomSettings.Text;
            }
            else
            {
                allSettingsEct = "-" + ComboBoxEctCompressionLevel.Text;
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
                                    startInfo.Arguments = "/C avifenc.exe " + '\u0022' + items + '\u0022' + " " + allSettingsLibavif + " " + '\u0022' + imageOutputTemp + "avif" + '\u0022';
                                    break;
                                case "webp":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "webp");
                                    startInfo.Arguments = "/C cwebp.exe " + allSettingsWebp + " " + '\u0022' + items + '\u0022' + " -o " + '\u0022' + imageOutputTemp + "webp" + '\u0022';
                                    break;
                                case "jpegxl":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "jpegxl");
                                    startInfo.Arguments = "/C cjpegxl.exe " + '\u0022' + items + '\u0022' + " " + '\u0022' + imageOutputTemp + "jxl" + '\u0022' + " " + allSettingsJpegxl;
                                    break;
                                case "jpegxl decoder":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "jpegxl");
                                    startInfo.Arguments = "/C djpegxl.exe " + '\u0022' + items + '\u0022' + " " + '\u0022' + imageOutputTemp + "png" + '\u0022';
                                    break;
                                case "mozjpeg":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "mozjpeg");
                                    startInfo.Arguments = "/C cjpeg.exe " + allSettingsMozjpeg + " " +'\u0022' + items + '\u0022' + " > " + '\u0022' + imageOutputTemp + "jpg" + '\u0022';
                                    break;
                                case "ect":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "ect");
                                    startInfo.Arguments = "/C ect.exe " + allSettingsEct + " " + '\u0022' + items + '\u0022';
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
