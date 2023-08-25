using NutFileLibrary;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NutEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int ImagePosition = 0;
        NutFile NutFile;

        string CurrentNuteFileName = "";

        public MainWindow()
        {
            InitializeComponent();
            KeyDown += new System.Windows.Input.KeyEventHandler(MainWindow_KeyDown);
            NutFile NutFile = new NutFile();
        }

        void Open_Nut_File(object sender, RoutedEventArgs e)
        {
            ImagePosition = 0;
            NutFile = new NutFile();
            var dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                CurrentNuteFileName = dialog.SafeFileName;
                string filePath = dialog.FileName;
                byte[] nutBytes = File.ReadAllBytes(filePath);
                NutFile.ReadNutFileBytes(nutBytes);
                
                ImageDisplay.Source = BitmapToImageSource(NutFile.Images[0].ImageBitMap);
                ImageDisplay.Width = NutFile.Images[0].Width;
                ImageDisplay.Height = NutFile.Images[0].Height;
            }
        }

        // no clue what this does, it just werks
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Bitmap bitmap = new Bitmap(dialog.FileName);
                    NutFile.Images[ImagePosition].UpdateImage(bitmap);

                    ImageDisplay.Source = BitmapToImageSource(NutFile.Images[ImagePosition].ImageBitMap);
                    ImageDisplay.Width = NutFile.Images[ImagePosition].Width;
                    ImageDisplay.Height = NutFile.Images[ImagePosition].Height;
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
            
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = CurrentNuteFileName;
            dialog.Filter = "NUT File (*.nut)|*.nut|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = dialog.FileName;
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] nutFileBytes = NutFile.ConvertToBytes();
                    fs.Write(nutFileBytes, 0, nutFileBytes.Length);
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "export.png";
            dialog.Filter = "PNG Files (*.png)|*.png|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = dialog.FileName;
                NutFile.Images[ImagePosition].ImageBitMap.Save(filePath);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            ImagePosition++;
            if(ImagePosition == NutFile.Header.NumberOfTextures)
            {
                ImagePosition = 0;
            }
            ImageDisplay.Source = BitmapToImageSource(NutFile.Images[ImagePosition].ImageBitMap);
            ImageDisplay.Width = NutFile.Images[ImagePosition].Width;
            ImageDisplay.Height = NutFile.Images[ImagePosition].Height;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            ImagePosition--;
            if (ImagePosition < 0)
            {
                ImagePosition = NutFile.Header.NumberOfTextures - 1;
            }
            ImageDisplay.Source = BitmapToImageSource(NutFile.Images[ImagePosition].ImageBitMap);
            ImageDisplay.Width = NutFile.Images[ImagePosition].Width;
            ImageDisplay.Height = NutFile.Images[ImagePosition].Height;
        }

        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Next_Click(null, null);
            }

            if (e.Key == Key.Down)
            {
                Previous_Click(null, null);
            }
        }
    }
}
