using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LicencePlateDetection
{
    public partial class Form1 : Form
    {
        private enum WindowSize { ThreeOnThree = 3, FiveOnFive = 5, SevenOnSeven = 7, NineOnNine = 9 }
        private Bitmap Bitmap { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private unsafe void Binarization(int threshold)
        {
            GC.Collect();
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
            for (int i = 0; i < heightInPixels; i++)
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int brightness = (currentLine[j] + currentLine[j + 1] + currentLine[j + 2]) / 3;
                    if (brightness <= threshold)
                    {
                        currentLine[j] = 0;
                        currentLine[j + 1] = 0;
                        currentLine[j + 2] = 0;
                    }
                    else
                    {
                        currentLine[j] = 255;
                        currentLine[j + 1] = 255;
                        currentLine[j + 2] = 255;
                    }
                }
            }
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        private unsafe void Contrast(int value)
        {
            GC.Collect();
            double newValue = (100.0 + value) / 100.0;
            newValue *= newValue;
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
            for (int i = 0; i < heightInPixels; i++)
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    double bValue = ((((currentLine[j] / 255.0) - 0.5) * newValue) + 0.5) * 255.0;
                    double gValue = ((((currentLine[j + 1] / 255.0) - 0.5) * newValue) + 0.5) * 255.0;
                    double rValue = ((((currentLine[j + 2] / 255.0) - 0.5) * newValue) + 0.5) * 255.0;
                    currentLine[j] = Convert.ToByte((bValue < 0 ? 0 : (bValue > 255 ? 255 : bValue)));
                    currentLine[j + 1] = Convert.ToByte((gValue < 0 ? 0 : (gValue > 255 ? 255 : gValue)));
                    currentLine[j + 2] = Convert.ToByte((rValue < 0 ? 0 : (rValue > 255 ? 255 : rValue)));
                }
            }
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        private unsafe void Brightness(int value)
        {
            GC.Collect();
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
            for (int i = 0; i < heightInPixels; i++)
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int bValue = currentLine[j] + value;
                    int gValue = currentLine[j + 1] + value;
                    int rValue = currentLine[j + 2] + value;
                    currentLine[j] = Convert.ToByte((bValue < 0 ? 0 : (bValue > 255 ? 255 : bValue)));
                    currentLine[j + 1] = Convert.ToByte((gValue < 0 ? 0 : (gValue > 255 ? 255 : gValue)));
                    currentLine[j + 2] = Convert.ToByte((rValue < 0 ? 0 : (rValue > 255 ? 255 : rValue)));
                }
            }         
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        private unsafe void BlueFiltering(int value)
        {
            GC.Collect();
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
            for (int i = 0; i < heightInPixels; i++)
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int gValue = currentLine[j + 1] - value;
                    int rValue = currentLine[j + 2] - value;
                    currentLine[j + 1] = Convert.ToByte(gValue < 0 ? 0 : gValue);
                    currentLine[j + 2] = Convert.ToByte(rValue < 0 ? 0 : rValue);
                }
            }
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        private unsafe void GreenFiltering(int value)
        {
            GC.Collect();
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
            for (int i = 0; i < heightInPixels; i++)
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int bValue = currentLine[j] - value;
                    int rValue = currentLine[j + 2] - value;
                    currentLine[j] = Convert.ToByte(bValue < 0 ? 0 : bValue);
                    currentLine[j + 2] = Convert.ToByte(rValue < 0 ? 0 : rValue);
                }
            }
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        private unsafe void RedFiltering(int value)
        {
            GC.Collect();
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
            for (int i = 0; i < heightInPixels; i++)
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int bValue = currentLine[j] - value;
                    int gValue = currentLine[j + 1] - value;
                    currentLine[j] = Convert.ToByte(bValue < 0 ? 0 : bValue);
                    currentLine[j + 1] = Convert.ToByte(gValue < 0 ? 0 : gValue);
                }
            }
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        private unsafe void MedianFiltering(WindowSize windowSize)
        {
            GC.Collect();
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
            for (int i = 0; i < heightInPixels - Convert.ToInt32(windowSize) + 1; i++)
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes - Convert.ToInt32(windowSize) * bytesPerPixel + bytesPerPixel; j = j + bytesPerPixel)
                {
                    List<Color> colors = new List<Color>();
                    for (int k = 0; k < Convert.ToInt32(windowSize); k++)
                    {
                        byte* currentWindowLine = currentLine + (k * bitmapData.Stride);
                        for (int l = j; l < j + Convert.ToInt32(windowSize) * bytesPerPixel; l = l + bytesPerPixel)
                        {
                            colors.Add(Color.FromArgb(currentWindowLine[l + 2], currentWindowLine[l + 1], currentWindowLine[l]));
                        }
                    }
                    colors.Sort((Color left, Color right) =>
                    {
                        int leftBrightness = (left.R + left.G + left.B) / 3;
                        int rightBrightness = (right.R + right.G + right.B) / 3;
                        return leftBrightness.CompareTo(rightBrightness);
                    });
                    currentLine[j + (Convert.ToInt32(windowSize) / 2 * bitmapData.Stride) + Convert.ToInt32(windowSize) / 2 * bytesPerPixel] = Convert.ToByte(colors[colors.Count / 2].B);
                    currentLine[j + (Convert.ToInt32(windowSize) / 2 * bitmapData.Stride) + Convert.ToInt32(windowSize) / 2 * bytesPerPixel + 1] = Convert.ToByte(colors[colors.Count / 2].G);
                    currentLine[j + (Convert.ToInt32(windowSize) / 2 * bitmapData.Stride) + Convert.ToInt32(windowSize) / 2 * bytesPerPixel + 2] = Convert.ToByte(colors[colors.Count / 2].R);
                }
            }
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap = new Bitmap(pictureBox1.Image);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            Binarization(trackBar1.Value);
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            Brightness(trackBar2.Value);
        }

        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            BlueFiltering(trackBar3.Value);
        }

        private void trackBar4_ValueChanged(object sender, EventArgs e)
        {
            GreenFiltering(trackBar4.Value);
        }

        private void trackBar5_ValueChanged(object sender, EventArgs e)
        {
            RedFiltering(trackBar5.Value);
        }

        private void trackBar6_ValueChanged(object sender, EventArgs e)
        {
            Contrast(trackBar6.Value);
        }

        private void trackBar7_ValueChanged(object sender, EventArgs e)
        {
            MedianFiltering((WindowSize)(trackBar7.Value * 2 + 1));
        }   

        private void trackBar1_Leave(object sender, EventArgs e)
        {
            trackBar1.Value = 128;
        }

        private void trackBar2_Leave(object sender, EventArgs e)
        {
            trackBar2.Value = 0;
        }

        private void trackBar3_Leave(object sender, EventArgs e)
        {
            trackBar3.Value = 0;
        }

        private void trackBar4_Leave(object sender, EventArgs e)
        {
            trackBar4.Value = 0;
        }

        private void trackBar5_Leave(object sender, EventArgs e)
        {
            trackBar5.Value = 0;
        }

        private void trackBar6_Leave(object sender, EventArgs e)
        {
            trackBar6.Value = 0;
        }

        private void trackBar7_Leave(object sender, EventArgs e)
        {
            trackBar6.Value = 1;
        }       
    }
}