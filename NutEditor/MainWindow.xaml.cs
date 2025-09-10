using NutFileLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NutEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary> 
public class NutFileEntry
{
    public string FileName { get; set; }
    public string FullPath { get; set; }
}


public partial class MainWindow : Window
{
    int _imagePosition = 0;
    public int ImagePosition
    {
        get => _imagePosition;
        set
        {
            _imagePosition = value;
            if (ImagePosition < NutFile.Images.Count && ImagePosition >= 0)
            {
                ImageIndexInput.Text = value.ToString();
                ImageFormatLabel.Content = "Image Format: " + NutFile?.Images[ImagePosition].ImageFormat.ToString();
                ImageWidthLabel.Content = "Image Width: " + NutFile?.Images[ImagePosition].Width;
                ImageHeightLabel.Content = "Image Height: " + NutFile?.Images[ImagePosition].Height;
            }
        }
    }

    // Binded to UI so we can scroll thumbnails
    public ObservableCollection<BitmapImage> Thumbnails { get; set; } = new ObservableCollection<BitmapImage>();
    public ObservableCollection<NutFileEntry> NutFileEntries { get; set; } = new ObservableCollection<NutFileEntry>();



    NutFile NutFile;

    string CurrentNutFileName = "";

    public MainWindow()
    {
        InitializeComponent();
        KeyDown += new System.Windows.Input.KeyEventHandler(MainWindow_KeyDown);
        NutFile NutFile = new NutFile();
        DataContext = this;
    }
    void Open_Directory(object sender, RoutedEventArgs e)
    {
        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Select Donkey Konga Root Directory";
        folderDialog.ShowNewFolderButton = false;

        DialogResult result = folderDialog.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            NutFileEntries.Clear();
            string selectedFolder = folderDialog.SelectedPath;

            List<string> nutFiles = FindNutFiles(selectedFolder);
            foreach (var file in nutFiles)
            {
                NutFileEntries.Add(new NutFileEntry
                {
                    FileName = Path.GetFileName(file),
                    FullPath = file
                });
            }
        }
    }

    public List<string> FindNutFiles(string rootFolder)
    {
        try
        {
            return Directory.EnumerateFiles(rootFolder, "*.nut", SearchOption.AllDirectories).ToList();
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Windows.MessageBox.Show($"Access denied to some folders: {ex.Message}");
            return new List<string>();
        }
    }

    private void NutFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.DataContext is NutFileEntry nutFile)
        {
            Open_Nut_File_Based_On_Path(nutFile.FileName, nutFile.FullPath);
        }
    }

    void Open_Nut_File(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Nut files (*.nut)|*.nut",
            Title = "Open Nut File"
        };
        DialogResult result = dialog.ShowDialog();

        if (result == System.Windows.Forms.DialogResult.OK)
        {
            Open_Nut_File_Based_On_Path(dialog.SafeFileName, dialog.FileName);
        }
    }

    void Open_Nut_File_Based_On_Path(string fileName, string filePath)
    {
        NutFile = new NutFile();
        CurrentNutFileName = fileName;
        byte[] nutBytes = File.ReadAllBytes(filePath);
        NutFile.ReadNutFileBytes(nutBytes);

        ImageDisplay.Source = BitmapToImageSource(NutFile.Images[0].ImageBitMap);
        ImageDisplay.Width = NutFile.Images[0].Width;
        ImageDisplay.Height = NutFile.Images[0].Height;

        ImagePosition = 0;
        NutFile.FileName = fileName;
        NuteFileName.Content = fileName;
        NutMaxImages.Content = "Images: " + NutFile.Images.Count;

        Thumbnails.Clear();
        foreach (var image in NutFile.Images)
        {
            Thumbnails.Add(BitmapToImageSource(image.ImageBitMap));
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

                // a horribly janky way to refresh the UI lol, god I hope no one looks at this
                // I promise I know how to set up WPF apps properly, but I didn't when I first made it and there is no way I'm refactoring 
                ImagePosition = ImagePosition;
                RefreshThumbnails();
            }
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }

    private void RefreshThumbnails()
    {
        // please pretend you don't see this, I am so sorry.
        Thumbnails.Clear();
        foreach (var image in NutFile.Images)
        {
            Thumbnails.Add(BitmapToImageSource(image.ImageBitMap));
        }
    }

    private void Bulk_Replace_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] selectedFiles = dialog.FileNames;
                foreach (string file in selectedFiles)
                {
                    Bitmap bitmap = new Bitmap(file);
                    NutFile.Images[ImagePosition].UpdateImage(bitmap);

                    ImageDisplay.Source = BitmapToImageSource(NutFile.Images[ImagePosition].ImageBitMap);
                    ImageDisplay.Width = NutFile.Images[ImagePosition].Width;
                    ImageDisplay.Height = NutFile.Images[ImagePosition].Height;
                    // Go to next image, will handle if we are above the number of images
                    Next_Click(null, null);

                    RefreshThumbnails();
                }
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
        dialog.FileName = CurrentNutFileName;
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

    private void Export_All_Click(object sender, RoutedEventArgs e)
    {
        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Exported images will be saved in their own folder.";
        folderDialog.ShowNewFolderButton = true;

        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string selectedPath = folderDialog.SelectedPath;
            // You can now create a new folder inside selectedPath
            string newFolderPath = Path.Combine(selectedPath, NutFile.FileName + " Export");
            Directory.CreateDirectory(newFolderPath);
            int index = 0;
            foreach(NutImage image in NutFile.Images)
            {
                string fileName = $"{NutFile.FileName}_{index}.png";
                string fullPath = Path.Combine(newFolderPath, fileName);
                image.ImageBitMap.Save(fullPath);
                index++;
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

    private void Thumbnail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.DataContext is BitmapImage clickedImage)
        {
            ImagePosition = Thumbnails.IndexOf(clickedImage);
            ImageDisplay.Source = BitmapToImageSource(NutFile.Images[ImagePosition].ImageBitMap);
            ImageDisplay.Width = NutFile.Images[ImagePosition].Width;
            ImageDisplay.Height = NutFile.Images[ImagePosition].Height;
        }
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

    private void Image_Index_Change(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (int.TryParse(ImageIndexInput.Text, out int parsedInt))
        {
            // Success: result now holds the integer value
        }
        else
        {

            ImageIndexInput.Text = ImagePosition.ToString();
            System.Windows.MessageBox.Show("Could not convert input to a number");
            return;
        }

        // Bounds checking
        if (parsedInt >= NutFile.Header.NumberOfTextures)
        {
            ImageIndexInput.Text = ImagePosition.ToString();
            System.Windows.MessageBox.Show("Number is too high");
            return; 
        }
        if (parsedInt < 0)
        {
            ImageIndexInput.Text = ImagePosition.ToString();
            System.Windows.MessageBox.Show("Number is too low");
            return;
        }

        ImagePosition = parsedInt;

        ImageDisplay.Source = BitmapToImageSource(NutFile.Images[ImagePosition].ImageBitMap);
        ImageDisplay.Width = NutFile.Images[ImagePosition].Width;
        ImageDisplay.Height = NutFile.Images[ImagePosition].Height;
    }
}
