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



        // данные для отображения видео
        List<WeatherCadr> currentBlock; // тут хранится видео, которое сейчас отображается
        int currentBlockStartTime;      // начальный момент времени для этого ролика
        int currentBlockFrames;         // сколько кадров в этом ролике
        int currentBlockCurrentCadr;    // какой кадр мы смотрим в данный момент
        //-----------------------------

        
        // блок функций работы со строкой состояния
        int progressStep = 5; // сколько пикселей на кадр 
        void ProgressClean()
        {
            
            label3.Text = "0";
            label4.Text = Convert.ToString((int)((double)progress.Width / (double)progressStep));

            Bitmap p = new Bitmap(progress.Width, progress.Height);
            Graphics gp = Graphics.FromImage(p);
            gp.FillRectangle(new SolidBrush(Color.WhiteSmoke), 0, 0, progress.Width, progress.Height);
            gp.DrawLine(new Pen(new SolidBrush(Color.Purple), progressStep), 0, 0, 0, progress.Width);

            progress.Image = p;

        }
        void ProgressSetTime(int time)
        {
            Bitmap p = new Bitmap(progress.Width, progress.Height);
            Graphics gp = Graphics.FromImage(p);
            gp.FillRectangle(new SolidBrush(Color.WhiteSmoke), 0, 0, progress.Width, progress.Height);
            gp.DrawLine(new Pen(new SolidBrush(Color.Purple), progressStep), time * progressStep, 0, time * progressStep, progress.Width);
            gp.DrawString(time.ToString(), new Font("Arial", 10), new SolidBrush(Color.Purple), (float)(time * progressStep + 3),4);

            progress.Image = p;
        }
        //-----------------------------



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
        {// смотреть видео с указанного начального кадра в currentBlock
            // это значит мы можем внутри currentBlock начинать смотреть с любого кадра.

            for (int i = currentBlockCurrentCadr; i < currentBlock.Count; i++)
            {

                WeatherCadr now = currentBlock[i];
                Bitmap cadr = new Bitmap((int)now.height, (int)now.weight);
                Graphics g = Graphics.FromImage(cadr);


                //// очистим
                //g.FillRectangle(new SolidBrush(Color.White), 0, 0, cadr.Width, cadr.Height);
                //pictureBox1.Image = cadr;

                // нарисуем землю
                for (int l = 0; l < now.Land.Count; l++)
                {
                    g.FillEllipse(new SolidBrush(Color.DarkMagenta), (int)now.Land[l].X - (int)(now.Land[l].R), (int)now.Land[l].Y - (int)(now.Land[l].R), (int)now.Land[l].R * 2, (int)now.Land[l].R * 2);
                    g.DrawString(Convert.ToString(now.Land[l].V), new Font("Arial", 10), new SolidBrush(Color.Aqua), (int)now.Land[l].X, (int)now.Land[l].Y);

                }
                    // нарисуем циклоны

                for (int w = 0; w < now.Weather.Count; w++)
                {
                    g.FillEllipse(new SolidBrush(Color.DarkGray), (int)now.Weather[w].X - (int)(now.Weather[w].R), (int)now.Weather[w].Y - (int)(now.Weather[w].R), (int)now.Weather[w].R * 2, (int)now.Weather[w].R * 2);
                    g.DrawString(Convert.ToString(now.Weather[w].V), new Font("Arial", 10), new SolidBrush(Color.Black), (int)now.Weather[w].X, (int)now.Weather[w].Y);
                }


                pictureBox1.Image = cadr;

                ProgressSetTime(i);

                System.Threading.Thread.Sleep(Convert.ToInt32(textBox1.Text));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (th != null)
                th.Abort();

            currentBlock = initialvideo;
            currentBlockCurrentCadr = 0;
            currentBlockFrames = currentBlock.Count;
            currentBlockStartTime = 0;


            ProgressClean();

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
