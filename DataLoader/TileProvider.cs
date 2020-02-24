using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataLoader
{
    public class TileProvider
    {
        public const string DATA_DIR = @"D:\Manuel\Desktop\MASTER_THESIS\DATA\TILES";
        public const string FILE_HEIGHT_RGB = "height_rgb.png";
        public const string FILE_HEIGHT_GS = "height_gs.png";
        public const string FILE_SAT_MAPBOX = "sat_1.png";
        public const string FILE_SAT_HERE = "sat_2.png";
        public const string FILE_SAT_MAPTILER_256 = "sat_3_256.jpg";
        public const string FILE_SAT_MAPTILER_512 = "sat_3_512.jpg";

        private const string API_1_KEY = "pk.eyJ1IjoiZWluc3RlaW5zZXJiZSIsImEiOiJjazZxenZxaTkwMHBjM2xtZXgwYmlwaTZmIn0.GSzUYV7Os83_w8vt2_QpbQ";
        private const string API_1_BASE_URL = "https://api.mapbox.com/v4/mapbox.";

        private const string API_2_KEY = "mzTfdBcaAcYbJSMsqrunPYhmkWTL9JJ4qgjzqYcYl8c";
        private const string API_2_BASE_URL = "https://2.aerial.maps.ls.hereapi.com/maptile/2.1/maptile/newest/";

        private const string API_3_KEY = "ZUcadfx4h73DnNLSYie8";
        private const string API_3_BASE_URL = "https://api.maptiler.com/tiles/";

        public static TileProvider MAPBOX = new TileProvider(FILE_SAT_MAPBOX,
            (p, z) =>
            {
                return API_1_BASE_URL + "satellite/" + z + "/" + p.X + "/" + p.Y + "@2x.pngraw?access_token=" + API_1_KEY;
            },
            (p, z) =>
            {
                return API_1_BASE_URL + "terrain-rgb/" + z + "/" + p.X + "/" + p.Y + "@2x.pngraw?access_token=" + API_1_KEY;
            });

        public static TileProvider HERE = new TileProvider(FILE_SAT_HERE,
            (p, z) =>
            {
                return API_2_BASE_URL + "satellite.day/" + z + "/" + p.X + "/" + p.Y + "/512/png?apiKey=" + API_2_KEY;
            },
            (p, z) =>
            {
                return null;
            });

        public static TileProvider MAPTILER = new TileProvider(FILE_SAT_MAPTILER_256,
            (p, z) =>
            {
                return API_3_BASE_URL + "satellite" + "/" + z + "/" + p.X + "/" + p.Y + ".jpg?key=" + API_3_KEY;
            },
            (p, z) =>
            {
                return API_3_BASE_URL + "terrain-rgb" + "/" + z + "/" + p.X + "/" + p.Y + ".png?key=" + API_3_KEY;
            });


        private Func<Point, int, string> downloadSatUrl;
        private Func<Point, int, string> downloadHeightUrl;
        private string satFileName;

        public TileProvider(string satFileName, Func<Point, int, string> downloadSatUrl, Func<Point, int, string> downloadHeightUrl)
        {
            this.satFileName = satFileName;
            this.downloadHeightUrl = downloadHeightUrl;
            this.downloadSatUrl = downloadSatUrl;
        }

        private string CreateBaseDir(Point p, int zoom)
        {
            string path = Path.Combine(DATA_DIR, zoom.ToString(), p.X.ToString(), p.Y.ToString());
            Directory.CreateDirectory(path);
            return path;
        }

        private void CheckFile(string path, Point p, string type)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine(type + "-tile " + p.X + "/" + p.Y + " doesn't exist!");
            }
        }

        public string DownloadSat(Point p, int zoom)
        {
            string path = CreateBaseDir(p, zoom);
            string pathS = Path.Combine(path, satFileName);

            if (!File.Exists(pathS) && !File.Exists(Path.Combine(path, "NOTILE")))
            {
                //Console.WriteLine("Download Sat: " + p.X + "/" + p.Y);
                HttpDownload(downloadSatUrl(p, zoom), pathS);
            }

            CheckFile(pathS, p, "sat");

            return pathS;
        }

        public string DownloadHeight(Point p, int zoom, bool grayscale = true)
        {
            string path = CreateBaseDir(p, zoom);
            string pathH = Path.Combine(path, FILE_HEIGHT_RGB);
            string pathG = Path.Combine(path, FILE_HEIGHT_GS);

            if (!File.Exists(pathH) && !File.Exists(Path.Combine(path, "NOTILE")))
            {
                //Console.WriteLine("Download RGB: " + p.X + "/" + p.Y);
                HttpDownload(downloadHeightUrl(p, zoom), pathH);
            }

            CheckFile(pathH, p, "rgb");

            if (!File.Exists(pathG) && File.Exists(pathH) && grayscale)
            {
                //Console.WriteLine("Create GS: " + p.X + "/" + p.Y);
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
                        int gray = Math.Max(Math.Min((int)(height / 5000.0 * 255), 255), 0);
                        d.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }

                d.Save(pathG);
            }

            return pathH;
        }

        private static void HttpDownload(string uri, string path)
        {
            //Console.WriteLine(uri);
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
    }
}
