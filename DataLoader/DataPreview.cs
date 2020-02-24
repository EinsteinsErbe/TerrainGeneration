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
    public partial class DataPreview : Form
    {
        public DataPreview()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(TileProvider.DATA_DIR, textBox3.Text);
            if (Directory.Exists(path))
            {
                string[] xs = Directory.GetDirectories(path);
                string x = Path.Combine(path, xs[new Random().Next(xs.Length)]);
                if (Directory.Exists(x))
                {
                    string[] ys = Directory.GetDirectories(x);
                    string y = Path.Combine(x, ys[new Random().Next(ys.Length)]);
                    if (Directory.Exists(y))
                    {
                        string rgb = Path.Combine(y, TileProvider.FILE_HEIGHT_RGB);
                        string gs = Path.Combine(y, TileProvider.FILE_HEIGHT_GS);
                        string sat = Path.Combine(y, TileProvider.FILE_SAT_MAPTILER_512);

                        if (File.Exists(rgb))
                        {
                            pictureBox1.Image = new Bitmap(rgb);
                        }
                        if (File.Exists(gs))
                        {
                            pictureBox2.Image = new Bitmap(gs);
                        }
                        if (File.Exists(sat))
                        {
                            pictureBox3.Image = new Bitmap(sat);
                        }
                    }
                }
            }
        }
    }
}
