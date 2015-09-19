using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LicencePlateDetection
{
    public partial class Form1 : Form
    {
        private enum WindowSize { ThreeOnThree = 3, FiveOnFive = 5, SevenOnSeven = 7, NineOnNine = 9 }
        private Bitmap Bitmap { get; set; }
        private object LockObject { get; set; }

        public Form1()
        {
            InitializeComponent();
            LockObject = new object();
        }

        private double Dispersion(int[] array, int fromInclusive, int toExclusive)
        {
            double average = 0.0;
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                average += array[i];
            }
            average /= Convert.ToDouble(toExclusive - fromInclusive);
            double dispersion = 0.0;
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                dispersion += Math.Pow(array[i] - average, 2);
            }
            dispersion /= Convert.ToDouble(toExclusive - fromInclusive);
            return dispersion;
        }

        private unsafe void AdaptiveBinarization()
        {
            lock (LockObject)
            {
                button1.BeginInvoke(new Action(() => { button1.Enabled = false; }));
                GC.Collect();
                Bitmap bitmap = new Bitmap(Bitmap);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                int minBrightness = (ptrFirstPixel[0] + ptrFirstPixel[1] + ptrFirstPixel[2]) / 3;
                int maxBrightness = minBrightness;
                for (int i = 0; i < heightInPixels; i++)
                {
                    byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                    for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                    {
                        int brightness = (currentLine[j] + currentLine[j + 1] + currentLine[j + 2]) / 3;
                        if (brightness < minBrightness)
                        {
                            minBrightness = brightness;
                        }
                        if (brightness > maxBrightness)
                        {
                            maxBrightness = brightness;
                        }
                    }
                }
                int[] histogram = new int[maxBrightness - minBrightness + 1];
                for (int i = 0; i < heightInPixels; i++)
                {
                    byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                    for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                    {
                        int brightness = (currentLine[j] + currentLine[j + 1] + currentLine[j + 2]) / 3;
                        histogram[brightness - minBrightness]++;
                    }
                }
                int m = 0;
                int n = 0;
                for (int t = 0; t <= maxBrightness - minBrightness; t++)
                {
                    m += t * histogram[t];
                    n += histogram[t];
                }
                double maxSigma = -1.0;
                int threshold = 0;
                int alpha = 0;
                int beta = 0;
                for (int t = 0; t <= maxBrightness - minBrightness; t++)
                {
                    alpha += t * histogram[t];
                    beta += histogram[t];
                    double w = Convert.ToDouble(beta) / Convert.ToDouble(n);
                    double a = Convert.ToDouble(alpha) / Convert.ToDouble(beta) - Convert.ToDouble(m - alpha) / Convert.ToDouble(n - beta);
                    double sigma = w * (1 - w) * a * a;
                    if (sigma > maxSigma)
                    {
                        maxSigma = sigma;
                        threshold = t;
                    }
                }
                threshold += minBrightness;
                double progress = 0.0;
                Parallel.For(0, heightInPixels, i =>
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
                    Interlocked.Exchange(ref progress, progress + 1.0 / heightInPixels);
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = Convert.ToInt32(100.0 * progress); }));
                });
                bitmap.UnlockBits(bitmapData);
                pictureBox1.Image = bitmap;
                button1.BeginInvoke(new Action(() => { button1.Enabled = true; }));
            }
        }

        private unsafe void Binarization(int threshold)
        {
            lock (LockObject)
            {
                button1.BeginInvoke(new Action(() => { button1.Enabled = false; }));
                GC.Collect();
                Bitmap bitmap = new Bitmap(Bitmap);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                double progress = 0.0;
                Parallel.For(0, heightInPixels, i =>
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
                    Interlocked.Exchange(ref progress, progress + 1.0 / heightInPixels);
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = Convert.ToInt32(100.0 * progress); }));
                });
                bitmap.UnlockBits(bitmapData);
                pictureBox1.Image = bitmap;
                button1.BeginInvoke(new Action(() => { button1.Enabled = true; }));
            }
        }

        private unsafe void Contrast(int value)
        {
            lock (LockObject)
            {
                button1.BeginInvoke(new Action(() => { button1.Enabled = false; }));
                GC.Collect();
                double newValue = (100.0 + value) / 100.0;
                newValue *= newValue;
                Bitmap bitmap = new Bitmap(Bitmap);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                double progress = 0.0;
                Parallel.For(0, heightInPixels, i =>
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
                    Interlocked.Exchange(ref progress, progress + 1.0 / heightInPixels);
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = Convert.ToInt32(100.0 * progress); }));
                });
                bitmap.UnlockBits(bitmapData);
                pictureBox1.Image = bitmap;
                button1.BeginInvoke(new Action(() => { button1.Enabled = true; }));
            }
        }

        private unsafe void Brightness(int value)
        {
            lock (LockObject)
            {
                button1.BeginInvoke(new Action(() => { button1.Enabled = false; }));
                GC.Collect();
                Bitmap bitmap = new Bitmap(Bitmap);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                double progress = 0.0;
                Parallel.For(0, heightInPixels, i =>
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
                    Interlocked.Exchange(ref progress, progress + 1.0 / heightInPixels);
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = Convert.ToInt32(100.0 * progress); }));              
                });
                bitmap.UnlockBits(bitmapData);
                pictureBox1.Image = bitmap;
                button1.BeginInvoke(new Action(() => { button1.Enabled = true; }));
            }
        }

        private unsafe void MedianFiltering(WindowSize windowSize)
        {
            lock (LockObject)
            {
                button1.BeginInvoke(new Action(() => { button1.Enabled = false; }));
                GC.Collect();
                Bitmap bitmap = new Bitmap(Bitmap);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                for (int i = 0; i <= heightInPixels - Convert.ToInt32(windowSize); i++)
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
                    progressBar1.BeginInvoke(new Action(() => { progressBar1.Value = 100 * i / (heightInPixels - Convert.ToInt32(windowSize)); }));
                }
                bitmap.UnlockBits(bitmapData);
                pictureBox1.Image = bitmap;
                button1.BeginInvoke(new Action(() => { button1.Enabled = true; }));
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            int threshold = trackBar1.Value;
            Task.Factory.StartNew(new Action(() => { Binarization(threshold); }));
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            int value = trackBar2.Value;
            Task.Factory.StartNew(new Action(() => { Brightness(value); }));
        }

        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            int value = trackBar3.Value;
            Task.Factory.StartNew(new Action(() => { Contrast(value); }));
        }

        private void trackBar4_ValueChanged(object sender, EventArgs e)
        {
            WindowSize windowSize = (WindowSize)(trackBar4.Value * 2 + 1);
            Task.Factory.StartNew(() => { MedianFiltering(windowSize); });
        }

        private void trackBar5_ValueChanged(object sender, EventArgs e)
        {
            if (trackBar5.Value == 0)
            {
                pictureBox1.Image = Bitmap;
            }
            else
            {
                AdaptiveBinarization();
            }
        }  

        private void button1_Click(object sender, EventArgs e)
        {
            GC.Collect();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap = (Bitmap)Image.FromStream(new MemoryStream(File.ReadAllBytes(openFileDialog1.FileName)));
                if (Bitmap.Width > pictureBox1.Width || Bitmap.Height > pictureBox1.Height)
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                }
                pictureBox1.Image = Bitmap;
                trackBar1.Enabled = true;
                trackBar2.Enabled = true;
                trackBar3.Enabled = true;
                trackBar4.Enabled = true;
                trackBar5.Enabled = true;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                Bitmap bitmap = new Bitmap(Width, Height);
                this.DrawToBitmap(bitmap, new Rectangle(0, 0, Width, Height));
                bitmap.Save("Screenshot.jpg", ImageFormat.Jpeg);
            }
        }       
    }
}