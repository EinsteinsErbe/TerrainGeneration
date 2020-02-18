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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataLoader
{
    public partial class Form1 : Form
    {
        const string API_KEY = "pk.eyJ1IjoiZWluc3RlaW5zZXJiZSIsImEiOiJjazZxenZxaTkwMHBjM2xtZXgwYmlwaTZmIn0.GSzUYV7Os83_w8vt2_QpbQ";
        const string API_BASE_URL = "https://api.mapbox.com/v4/mapbox.";
        const string DATA_DIR = @"D:\Manuel\Desktop\MASTER_THESIS\DATA\TILES";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float lat = float.Parse(textBox1.Text);
            float lon = float.Parse(textBox2.Text);
            int z = int.Parse(textBox3.Text);

            Point p = WorldToTile(lon, lat, z);

            DownloadTile(p, z);
        }

        private void DownloadTile(Point p, int zoom)
        {
            string path = Path.Combine(DATA_DIR, zoom.ToString(), p.X.ToString(), p.Y.ToString());
            string pathH = Path.Combine(path, "height_rgb.png");
            string pathS = Path.Combine(path, "satellite.png");
            string pathG = Path.Combine(path, "height_g.png");
            Directory.CreateDirectory(path);
            string s = "/" + zoom + "/" + p.X + "/" + p.Y + "@2x.pngraw?access_token=" + API_KEY;

            if (!File.Exists(pathH) && !File.Exists(Path.Combine(path, "NOTILE")))
            {
                string url = API_BASE_URL + "terrain-rgb" + s;
                Console.WriteLine("Download RGB: " + p.X + "/" + p.Y);
                HttpDownload(url, pathH);
            }

            if (!File.Exists(pathS) && File.Exists(pathH))
            {
                string url = API_BASE_URL + "satellite" + s;
                Console.WriteLine("Download Sat: " + p.X + "/" + p.Y);
                HttpDownload(url, pathS);
            }

            if (!File.Exists(pathG) && File.Exists(pathH))
            {
                Console.WriteLine("Create G: " + p.X + "/" + p.Y);
                Bitmap c = new Bitmap(pathH);
                //Bitmap d = new Bitmap(c.Width, c.Height, PixelFormat.Format16bppGrayScale);
                Bitmap d = new Bitmap(c.Width, c.Height, c.PixelFormat);
                
                // Loop through the images pixels to reset color.
                for (int x = 0; x < c.Width; x++)
                {
                    for (int y = 0; y < c.Height; y++)
                    {
                        Color pixelColor = c.GetPixel(x, y);
                        double height = -10000 + ((pixelColor.R * 256 * 256 + pixelColor.G * 256 + pixelColor.B) * 0.1);
                        int gray = Math.Max(Math.Min((int)(height/5000.0*255),255),0);
                        d.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }

                d.Save(pathG);
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

        public static void HttpDownload(string uri, string path)
        {
            // use the httpclient
            using (var client = new HttpClient())
            {
                // connect to the REST endpoint        
                HttpResponseMessage response = client.GetAsync(uri).Result;

                // check to see if we have a succesfull respond
                if (response.IsSuccessStatusCode)
                {
                    Stream stream = response.Content.ReadAsStreamAsync().Result;
                    using (var fileStream = File.Create(path))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }
                    stream.Close();
                }
                else
                {
                    Console.WriteLine(response.StatusCode + ": " + response.ReasonPhrase);
                    if (response.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        File.Create(Path.Combine(Path.GetDirectoryName(path), "NOTILE"));
                    }
                }
            }
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

        private void button2_Click(object sender, EventArgs e)
        {
            int x = int.Parse(textBox4.Text);
            int y = int.Parse(textBox5.Text);
            int z = int.Parse(textBox3.Text);

            DownloadTile(new Point(x, y), z);
        }
    }
}
