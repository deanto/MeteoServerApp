using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;

using MeteoServer.Components.WeatherCalculating;
using MeteoServer.Objects;

namespace MeteoServer.Components.WeatherCalculating
{
    public partial class WinFormWeatherDisplay : Form
    {
        private WeatherCalculating wc;
        public WeatherCalculating WC { set { wc = value; } }

        private string map, weather;
        public string MAP { set { map = value; } }
        public string WEATHER { set { weather = value; } }

        public IUserID user;
        public IUserID USER { set { user = value; } }

        List<WeatherCadr> initialvideo; // ролик в начале.

        public WinFormWeatherDisplay()
        {
            InitializeComponent();
            
        }

        private void WinFormWeatherDisplay_Load(object sender, EventArgs e)
        {
            initialvideo = wc.GetWeatherFromBegin(user, map, weather);
            
        }


        




        Thread th; 

        void ShowVideoThreaded()
        {
            th = new Thread(new ThreadStart(ShowVideo));
            th.Start();
        }
        
       void ShowVideo()
        {

            for (int i = 0; i < initialvideo.Count; i++)
            {

                WeatherCadr now = initialvideo[i];

                Bitmap cadr = new Bitmap((int)now.height, (int)now.weight);

               
                Graphics g = Graphics.FromImage(cadr);

                g.FillRectangle(new SolidBrush(Color.White), 0, 0, cadr.Width, cadr.Height);

                pictureBox1.Image = cadr;
                
                g.FillRectangle(new SolidBrush(Color.Red), (int)now.Weather[0].X, (int)now.Weather[0].Y, (int)now.Weather[0].R, (int)now.Weather[0].R);

                pictureBox1.Image = cadr;
               // pictureBox1.Refresh();
                

                System.Threading.Thread.Sleep(Convert.ToInt32(textBox1.Text));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (th != null)
                th.Abort();
            
            

            ShowVideoThreaded();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int t = Convert.ToInt32(textBox1.Text);
            t++;
            textBox1.Text = t.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int t = Convert.ToInt32(textBox1.Text);
            if (t!=0) t--;
            textBox1.Text = t.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (th != null)
                th.Abort();
        }

        
    }

    

}
