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
    public partial class TileMap : Form
    {
        int zoom = 3;//4
        int xMap = 4;//8
        int yMap = 2;//5

        public TileMap()
        {
            InitializeComponent();
        }

        private void TileMap_Load(object sender, EventArgs e)
        {
            //string map = Path.Combine(TileProvider.DATA_DIR, zoom.ToString(), xMap.ToString(), yMap.ToString(), TileProvider.FILE_SAT_MAPTILER_512);
            //pictureBox1.Image = new Bitmap(map);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ButtonState bs = new ButtonState(button1, label6, progressBar1);


            string map = Path.Combine(TileProvider.DATA_DIR, zoom.ToString(), xMap.ToString(), yMap.ToString(), TileProvider.FILE_SAT_MAPTILER_512);
            Bitmap image = new Bitmap(map);
            int z = int.Parse(textBox3.Text);
            //int offsetScale = (int)Math.Pow(2, zoom);
            int scale = (int)Math.Pow(2, z - zoom);
            int xOffset = xMap * scale;
            int yOffset = yMap * scale;
            int total = 0;

            string xpath = Path.Combine(TileProvider.DATA_DIR, textBox3.Text);
            if (Directory.Exists(xpath))
            {
                string[] xs = Directory.GetDirectories(xpath);

                bs.Start(xs.Length);
                Task.Run(() =>
                {
                    foreach (string tx in xs)
                    {
                        int x = int.Parse(Path.GetFileName(tx));
                        foreach (string ty in Directory.GetDirectories(tx))
                        {
                            int y = int.Parse(Path.GetFileName(ty));
                            if (File.Exists(Path.Combine(ty, TileProvider.FILE_HEIGHT_RGB)))
                            {
                                if (File.Exists(Path.Combine(ty, TileProvider.FILE_SAT_MAPTILER_512)))
                                {
                                    total++;
                                    int pScale = 512 / scale;
                                    int pX = (x - xOffset) * pScale;
                                    int pY = (y - yOffset) * pScale;

                                    if (pX < 0 || pY < 0 || pX >= 512 || pY >= 512)
                                    {
                                        continue;
                                    }

                                    for (int px = 0; px < pScale; px++)
                                    {
                                        for (int py = 0; py < pScale; py++)
                                        {
                                            Color c = image.GetPixel(pX + px, pY + py);
                                            image.SetPixel(pX + px, pY + py, Color.FromArgb(c.R / 2 + 128, c.G / 2, c.B / 2));
                                        }
                                    }
                                }
                            }
                        }
                        Invoke(new Action(() => bs.Update()));
                    }
                    Invoke(new Action(() =>
                    {
                        bs.End();
                        pictureBox1.Image = image;
                        label1.Text = total + " tiles";
                    }));
                    Console.WriteLine("Done");
                });
            }
        }
    }
}
