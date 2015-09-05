using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LicencePlateDetection
{
    public partial class Form1 : Form
    {
        private Bitmap Bitmap { get; set; }
        private bool Applied { get; set; }

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
            });
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
            });
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
            });           
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
            Parallel.For(0, heightInPixels, i =>
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int gValue = currentLine[j + 1] - value;
                    int rValue = currentLine[j + 2] - value;
                    currentLine[j + 1] = Convert.ToByte(gValue < 0 ? 0 : gValue);
                    currentLine[j + 2] = Convert.ToByte(rValue < 0 ? 0 : rValue);
                }
            });
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
            Parallel.For(0, heightInPixels, i =>
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int bValue = currentLine[j] - value;
                    int rValue = currentLine[j + 2] - value;
                    currentLine[j] = Convert.ToByte(bValue < 0 ? 0 : bValue);
                    currentLine[j + 2] = Convert.ToByte(rValue < 0 ? 0 : rValue);
                }
            });
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
            Parallel.For(0, heightInPixels, i =>
            {
                byte* currentLine = ptrFirstPixel + (i * bitmapData.Stride);
                for (int j = 0; j < widthInBytes; j = j + bytesPerPixel)
                {
                    int bValue = currentLine[j] - value;
                    int gValue = currentLine[j + 1] - value;
                    currentLine[j] = Convert.ToByte(bValue < 0 ? 0 : bValue);
                    currentLine[j + 1] = Convert.ToByte(gValue < 0 ? 0 : gValue);
                }
            });
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        /*private void MedianFilter()
        {
            Bitmap bitmap = new Bitmap(Bitmap);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            float rgb = 0;
            for (int i = 3; i < rgbValues.Length; i += 4)
            {
                //rgb = rgbValues[i]*0.3f
                //rgb += rgbValues[i - 1]
                int brightness = (rgbValues[i - 3] + rgbValues[i - 2] + rgbValues[i - 1]) / 3;
                if (brightness <= 0)
                {
                    rgbValues[i - 3] = 0;
                    rgbValues[i - 2] = 0;
                    rgbValues[i - 1] = 0;
                }
                else
                {
                    rgbValues[i - 3] = 255;
                    rgbValues[i - 2] = 255;
                    rgbValues[i - 1] = 255;
                }
            }
            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;

        }*/

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap = new Bitmap(pictureBox1.Image);
            Applied = false;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (!Applied)
            {
                Binarization(trackBar1.Value);
            }
            else
            {
                Applied = false;
            }
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            if (!Applied)
            {
                Brightness(trackBar2.Value);
            }
            else
            {
                Applied = false;
            }
        }

        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            if (!Applied)
            {
                BlueFiltering(trackBar3.Value);
            }
            else
            {
                Applied = false;
            }
        }

        private void trackBar4_ValueChanged(object sender, EventArgs e)
        {
            if (!Applied)
            {
                GreenFiltering(trackBar4.Value);
            }
            else
            {
                Applied = false;
            }
        }

        private void trackBar5_ValueChanged(object sender, EventArgs e)
        {
            if (!Applied)
            {
                RedFiltering(trackBar5.Value);
            }
            else
            {
                Applied = false;
            }
        }

        private void trackBar6_ValueChanged(object sender, EventArgs e)
        {
            if (!Applied)
            {
                Contrast(trackBar6.Value);
            }
            else
            {
                Applied = false;
            }
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
    }
}