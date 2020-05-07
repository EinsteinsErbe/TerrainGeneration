using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataLoader
{
    public partial class TileZoom : Form
    {
        public TileZoom()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int zoom = int.Parse(textBox3.Text);
            string path = Path.Combine(TileProvider.DATA_DIR, zoom.ToString());

            foreach (string xDir in Directory.GetDirectories(path))
            {
                int x = int.Parse(Path.GetFileNameWithoutExtension(xDir));
                if (x % 2 == 0)
                {
                    foreach (string yDir in Directory.GetDirectories(xDir))
                    {
                        int y = int.Parse(Path.GetFileNameWithoutExtension(yDir));
                        if (y % 2 == 0)
                        {
                            Point p = new Point(x / 2, y / 2);
                            Console.WriteLine("Zoom tile: " + p.X + "/" + p.Y);
                            ZoomTile(p, zoom, TileProvider.FILE_SAT_MAPTILER_512, TileProvider.FILE_SAT_MAPTILER_1024);
                            ZoomTile(p, zoom, TileProvider.FILE_HEIGHT_GS, TileProvider.FILE_HEIGHT_GS_1024);
                            ZoomTile(p, zoom, TileProvider.FILE_HEIGHT_RGB, TileProvider.FILE_HEIGHT_RGB_1024);
                        }
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private void ZoomTile(Point p, int zoom, string partName, string targetName)
        {
            string path = TileProvider.CreateBaseDir(p, zoom - 1);
            string pathScaled = Path.Combine(path, targetName);

            bool allParts = true;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    allParts &= File.Exists(Path.Combine(TileProvider.CreateBaseDir(new Point(p.X * 2 + i, p.Y * 2 + j), zoom ), partName));
                }
            }

            if (!File.Exists(pathScaled) && allParts)
            {
                Bitmap d = new Bitmap(1024, 1024);

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        string file = Path.Combine(TileProvider.CreateBaseDir(new Point(p.X * 2 + i, p.Y * 2 + j), zoom), partName);
                        if (File.Exists(file))
                        {
                            Bitmap c = new Bitmap(file);
                            for (int x = 0; x < c.Width; x++)
                            {
                                for (int y = 0; y < c.Height; y++)
                                {
                                    Color pixelColor = c.GetPixel(x, y);
                                    d.SetPixel(x + i * 512, y + j * 512, pixelColor);
                                }
                            }
                        }
                    }
                }

                d.Save(pathScaled);
            }
        }
    }
}
