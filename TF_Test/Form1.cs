using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tensorflow;
using NumSharp;
using static Tensorflow.Binding;

namespace TF_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            var str = "Hello, TensorFlow.NET!";
            var hello = tf.constant(str);
            
            // tf.Tensor: shape=(), dtype=string, numpy=b'Hello, TensorFlow.NET!'
            print(hello);

            string tensor = "";//hello.numpy();
            */
            Console.WriteLine(tf.VERSION);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var nd = np.full(5, 12); //[5, 5, 5 .. 5]
            nd = np.zeros(12); //[0, 0, 0 .. 0]
            nd = np.arange(12); //[0, 1, 2 .. 11]

            // create a matrix
            nd = np.zeros((3, 4)); //[0, 0, 0 .. 0]
            nd = np.arange(12).reshape(3, 4);
            Console.WriteLine(nd.Shape);
        }
    }
}
