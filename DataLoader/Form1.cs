using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void DownloadTile(Point p, int zoom)
        {
            TileProvider.MAPBOX.DownloadHeight(p, zoom);
            string path = TileProvider.MAPTILER.DownloadSat(p, zoom);

            if (checkBox1.Checked)
            {
                TileProvider.MAPBOX.DownloadSat(p, zoom);
                TileProvider.HERE.DownloadSat(p, zoom);
            }

            //upscale Maptiler Satellite Image
            string pathScaled = Path.Combine(Path.GetDirectoryName(path), TileProvider.FILE_SAT_MAPTILER_512);
            if (zoom < 20 && !File.Exists(pathScaled) && checkBox2.Checked)
            {
                //Console.WriteLine("Create scaled Sat: " + p.X + "/" + p.Y);

                Bitmap d = new Bitmap(512, 512);

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        string file = TileProvider.MAPTILER.DownloadSat(new Point(p.X * 2 + i, p.Y * 2 + j), zoom + 1);
                        if (File.Exists(file))
                        {
                            Bitmap c = new Bitmap(file);
                            for (int x = 0; x < c.Width; x++)
                            {
                                for (int y = 0; y < c.Height; y++)
                                {
                                    Color pixelColor = c.GetPixel(x, y);
                                    d.SetPixel(x + i * 256, y + j * 256, pixelColor);
                                }
                            }
                        }
                    }
                }

                d.Save(pathScaled);
            }
        }

        private byte[] ConvertBitmap(Bitmap bitmap)
        {
            //Code excerpted from Microsoft Robotics Studio v1.5
            BitmapData raw = null;  //used to get attributes of the image
            byte[] rawImage = null; //the image as a byte[]

            try
            {
                //Freeze the image in memory
                raw = bitmap.LockBits(
                    new Rectangle(0, 0, (int)bitmap.Width, (int)bitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb
                );

                int size = raw.Height * raw.Stride;
                rawImage = new byte[size];

                //Copy the image into the byte[]
                System.Runtime.InteropServices.Marshal.Copy(raw.Scan0, rawImage, 0, size);
            }
            finally
            {
                if (raw != null)
                {
                    //Unfreeze the memory for the image
                    bitmap.UnlockBits(raw);
                }
            }
            return rawImage;
        }

        public Point WorldToTile(double lon, double lat, int zoom)
        {
            Point p = new Point();
            p.X = (int)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;
        }

        public PointF WorldToTilePos(double lon, double lat, int zoom)
        {
            PointF p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;
        }

        public PointF TileToWorldPos(double tile_x, double tile_y, int zoom)
        {
            PointF p = new Point();
            double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

            p.X = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return p;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float lat = float.Parse(textBox1.Text);
            float lon = float.Parse(textBox2.Text);
            int z = int.Parse(textBox3.Text);

            Point p = WorldToTile(lon, lat, z);

            DownloadTile(p, z);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int x = int.Parse(textBox4.Text);
            int y = int.Parse(textBox5.Text);
            int z = int.Parse(textBox3.Text);

            DownloadTile(new Point(x, y), z);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            float lat = float.Parse(textBox1.Text);
            float lon = float.Parse(textBox2.Text);
            int z = int.Parse(textBox3.Text);

            float lat2 = float.Parse(textBox6.Text);
            float lon2 = float.Parse(textBox7.Text);

            Point p = WorldToTile(lon, lat, z);
            Point p2 = WorldToTile(lon2, lat2, z);

            if (p.X > p2.X || p.Y > p2.Y)
            {
                Console.WriteLine("Enter Coords from top left to bottom right");
                return;
            }

            int total = (p2.X - p.X + 1) * (p2.Y - p.Y + 1);
            label7.Text = total.ToString();
            Console.WriteLine("Download " + total + " tiles");
            progressBar1.Value = 0;

            ButtonState bs = new ButtonState(button3, label6, progressBar1);
            bs.Start(total);
            Task.Run(() =>
            {
                Parallel.For(p.X, p2.X + 1, x =>
                {
                    for (int y = p.Y; y <= p2.Y; y++)
                    {
                        DownloadTile(new Point(x, y), z);
                        Invoke(new Action(() => bs.Update(x + "/" + y)));
                    }
                });

                Invoke(new Action(() => bs.End()));
                Console.WriteLine("Done");
            });
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DataPreview().ShowDialog();
        }

        private void tileMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new TileMap().ShowDialog();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            float lat = float.Parse(textBox1.Text);
            float lon = float.Parse(textBox2.Text);
            int z = int.Parse(textBox3.Text);

            float lat2 = float.Parse(textBox6.Text);
            float lon2 = float.Parse(textBox7.Text);

            Point p = WorldToTile(lon, lat, z);
            Point p2 = WorldToTile(lon2, lat2, z);

            if (p.X > p2.X || p.Y > p2.Y)
            {
                Console.WriteLine("Enter Coords from top left to bottom right");
                return;
            }

            int total = (p2.X - p.X + 1) * (p2.Y - p.Y + 1);
            label7.Text = total.ToString();
        }
    }
}
