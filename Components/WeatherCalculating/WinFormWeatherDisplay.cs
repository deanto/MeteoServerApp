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

        int globaltime;

        // данные для отображения видео
        List<WeatherCadr> currentBlock; // тут хранится видео, которое сейчас отображается
        int currentBlockStartTime;      // начальный момент времени для этого ролика
        int currentBlockFrames;         // сколько кадров в этом ролике
        int currentBlockCurrentCadr;    // какой кадр мы смотрим в данный момент
        //-----------------------------

        


        // блок функций работы со строкой состояния
        int progressStep = 2; // сколько пикселей на кадр 
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
            // покажем сколько кадров вообще сейчас загружено
            gp.FillRectangle(new SolidBrush(Color.DarkGreen), currentBlock[0].TIME * progressStep, 0,  currentBlockFrames * progressStep, progress.Height);

            
            gp.DrawLine(new Pen(new SolidBrush(Color.Purple), progressStep), time * progressStep, 0, time * progressStep, progress.Width);
            gp.DrawString(time.ToString(), new Font("Arial", 6), new SolidBrush(Color.Purple), (float)(time * progressStep + 3),4);

            progress.Image = p;
        }
        //-----------------------------



        public WinFormWeatherDisplay()
        {
            InitializeComponent();
            
        }

        private void WinFormWeatherDisplay_Load(object sender, EventArgs e)
        {
            currentBlock = wc.GetWeatherFromBegin(user, map, weather);

            currentBlockCurrentCadr = 0;
            currentBlockFrames = currentBlock.Count;
            currentBlockStartTime = 0;

            globaltime = 0;

            ProgressClean();
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

            WeatherCadr last=null;
            int i;
            for (i = currentBlockCurrentCadr; i < currentBlock.Count; i++)
            {
                currentBlockCurrentCadr++;

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

                ProgressSetTime(globaltime);
                globaltime++;
                System.Threading.Thread.Sleep(Convert.ToInt32(textBox1.Text));

                if (i == currentBlock.Count - 1) last = now;

            }

            // если дошли до сюда - значит весь буффер просмотрели что был. нужен следующий
            if (last != null && globaltime < (int)((double)progress.Width / (double)progressStep))
            {
                currentBlock = wc.GetWeatherFromCadr(last,globaltime, user, weather);
                currentBlockStartTime=globaltime;      // начальный момент времени для этого ролика
                currentBlockFrames = currentBlock.Count ;         // сколько кадров в этом ролике
                currentBlockCurrentCadr = 0 ;    // какой кадр мы смотрим в данный момент
                

                revertShow();
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (th != null)
                th.Abort();



            ShowVideoThreaded();
        }

       void revertShow()
        {

           // ProgressClean();

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
