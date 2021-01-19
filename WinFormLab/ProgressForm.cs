using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormLab
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public void UpdateProgress(ProgressInfo pi)
        {
            this.Invoke(new Action(() =>
            {
                progressBar1.Value = pi.Total == 0 ? 0 : (int)Math.Round((double)(100 * pi.Processed) / pi.Total);
                label1.Text = $"{progressBar1.Value} %";
                label2.Text = pi.Message;
            }));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = DateTime.Now.ToString("HH-mm-ss");
        }
    }

    public class ProgressInfo
    {
        public long Total { get; set; }
        public long Processed { get; set; }
        public string Message { get; set; }

        public ProgressInfo(long total, long processed, string message)
        {
            Total = total;
            Processed = processed;
            Message = message;
        }
    }
}
