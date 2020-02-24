using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataLoader
{
    public class ButtonState
    {
        string text;
        Button b;
        Label l;
        ProgressBar p;

        int total = 0;
        int i;

        public ButtonState(Button b, Label l = null, ProgressBar p = null)
        {
            this.b = b;
            this.l = l;
            this.p = p;
        }

        public void Start(int total = 0)
        {
            this.total = total;
            text = b.Text;
            b.Enabled = false;
            b.Text = "working...";
            if (l != null)
            {
                l.Text = "working...";
            }
            if (p != null)
            {
                p.Value = 0;
            }
        }

        public void Update(string msg = "")
        {
            double percent = Math.Round(i / ((float)total) * 100, 2);
            if (l != null && total > 0)
            {
                l.Text = percent + " % " + msg;
            }
            if (p != null && total > 0)
            {
                p.Value = (int)percent;
            }
            i++;
        }

        public void End()
        {
            b.Text = text;
            b.Enabled = true;
            if (l != null)
            {
                l.Text = "done";
            }
            if (p != null)
            {
                p.Value = 100;
            }
        }
    }
}
