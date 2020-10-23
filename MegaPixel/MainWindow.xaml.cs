using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
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
        public bool imageOutputSet, wrongFormat;

        public MainWindow()
        {
            InitializeComponent();
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
                if (CheckBoxWebpNearLossless.IsChecked == true)
                {
                    allSettingsWebp = "-preset " + ComboBoxWebpPreset.Text + " -near_lossless " + TextBoxWebpNearLossless.Text + " -z " + ComboBoxWebpLosslessPreset.Text;
                }
                else if (CheckBoxAvifLossless.IsChecked == true)
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
                string quant;
                if(CheckBoxMozjpegQuant.IsChecked == true) { quant = " -quant-table " + ComboBoxMozjpegQuantTable.SelectedIndex; } else { quant = ""; }
                allSettingsMozjpeg = "-quality " + TextBoxWebpQuality.Text + " -" + ComboBoxMozjpegTune.Text + " -smooth " + TextBoxMozjpegSmoothing.Text + quant;
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

        private void AutoRemoveNonJpeg()
        {
            List<string> list = new List<string>();
            foreach (var element in ListBoxImagesToConvert.Items)
            {
                if (Path.GetExtension(element.ToString()) != ".jpg" && Path.GetExtension(element.ToString()) != ".jpeg")
                {
                    list.Add(element.ToString());
                }
            }
            foreach (var element in list)
            {
                ListBoxImagesToConvert.Items.Remove(element);
            }
        }

        private void ListBoxImagesToConvert_Drop(object sender, DragEventArgs e)
        {
            //Allows to drag and drop files directly into the ListBox
            List<string> filepaths = new List<string>();
            foreach (var s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                {
                    //Add files from folder
                    filepaths.AddRange(Directory.GetFiles(s));
                }
                else
                {
                    //Add filepath
                    filepaths.Add(s);
                }
            }
            foreach (string fileName in filepaths)
            {
                ListBoxImagesToConvert.Items.Add(fileName);
                //if (Path.GetExtension(fileName.ToString()) == ".jpg" || Path.GetExtension(fileName.ToString()) == ".jpeg")
                //{
                //                   
                //}                
            }
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
                if (CheckBoxPlayFinishedSound.IsChecked == true)
                {
                    SoundPlayer playSound = new SoundPlayer(Properties.Resources.finished);
                    playSound.Play();
                }
            }
            catch { }
        }

        private void ButtonSaveTo_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browseOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            if (browseOutputFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imageOutput = browseOutputFolder.SelectedPath;
                LabelOutput.Content = imageOutput;
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
                    if (ComboBoxEncoder.SelectedIndex == 4)
                    {
                        foreach (var element in ListBoxImagesToConvert.Items)
                        {
                            if (Path.GetExtension(element.ToString()) != ".jpg" && Path.GetExtension(element.ToString()) != ".jpeg") { wrongFormat = true; }
                        }
                        if (wrongFormat) {

                            if (MessageBox.Show("You have elements in the Queue, which are not jpg/jpeg.\n\nMozJpeg only reduces file sizes of JPEG images!\n\nAuto-Remove non jpeg/jpg files from List?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                AutoRemoveNonJpeg();
                            }                        
                        }
                    }
                    wrongFormat = false;
                }
            }

        }

        private void ButtonRemoveFromList_Click(object sender, RoutedEventArgs e)
        {
            //Removes every selected item from the listbox
            var selected = ListBoxImagesToConvert.SelectedItems.Cast<Object>().ToArray();
            foreach (var item in selected) ListBoxImagesToConvert.Items.Remove(item);
        }

        private void ButtonOpenGithubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Alkl58/MegaPixel");
        }

        private void ButtonOpenDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/HSBxne3");
        }

        private void ButtonOpenReddit_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.reddit.com/user/Al_kl");
        }

        private void ComboBoxEncoder_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboBoxEncoder.SelectedIndex == 4)
            {
                foreach (var element in ListBoxImagesToConvert.Items)
                {
                    if (Path.GetExtension(element.ToString()) != ".jpg"  && Path.GetExtension(element.ToString()) != ".jpeg") { wrongFormat = true; }
                }
                if (wrongFormat)
                {

                    if (MessageBox.Show("You have elements in the Queue, which are not jpg/jpeg.\n\nMozJpeg only reduces file sizes of JPEG images!\n\nAuto-Remove non jpeg/jpg files from List?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        AutoRemoveNonJpeg();
                    }
                }
            }
            wrongFormat = false;
        }

        private void CheckImageOutput(string Path)
        {
            if (CheckBoxRemoveFinishedItems.IsChecked == true)
            {
                if (File.Exists(Path))
                {
                    try
                    {
                        ListBoxImagesToConvert.Items.Remove(Path);
                    } catch { }
                }
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
                                    startInfo.Arguments = "/C cjpeg.exe " + allSettingsMozjpeg + " -outfile " + '\u0022' + imageOutputTemp + "jpg" + '\u0022' + " " + '\u0022' + items + '\u0022';
                                    break;
                                case "ect":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "ect");
                                    startInfo.Arguments = "/C ect.exe " + allSettingsEct + " " + '\u0022' + items + '\u0022';
                                    break;
                                default:
                                    break;
                            }
                            //Console.WriteLine(startInfo.Arguments);
                            process.StartInfo = startInfo;
                            process.Start();
                            process.WaitForExit();
                            LabelProgressbar.Dispatcher.Invoke(() => CheckImageOutput(items.ToString()), DispatcherPriority.Background);
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
