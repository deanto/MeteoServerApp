using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;




namespace MeteoServer.Components.FileEditor
{
    public partial class WinFormEditor : Form
    {
        int IntoPos;
        int BordPos;
        bool mousPressed;

        int selectedOne;

        private string[] Buffer;
        public string[] FILEBuf { set { Buffer = value; } get { return Buffer; } }

        private string[] BackGround;
        public string[] BackgroundMap { set { BackGround = value; } }

        List<Ring> BackGroundrings;
        Rectangle BackGroundreq;



        List<Ring> rings;
        Rectangle req;


        public WinFormEditor()
        {
            InitializeComponent();
        }



        private void WinFormEditor_Load(object sender, EventArgs e)
        {

            // эта секция отвечает за то, чтобы если файл пуст - создать его
            int newx=0,newy=0;

            if (Buffer.Length==0)
            {
                if (BackGround == null)
                {// если нам не прислали фон - значит либо ошибка произошла либо это просто файл карты.
                    MessageBox.Show("Файл еще несодержит данных. Пожалуйста укажите размерность карты.");
                    Dilog1 d = new Dilog1();
                    d.ShowDialog();

                    newx = d.x; newy = d.y;
                }
                else
                {// значит нам прислали фон. это точно погода. если она пустая - размерность возьмем из карты местности
                    // в первой строчке указаны размеры.
                    string[] lines = BackGround[0].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                    int X = Convert.ToInt32(lines[0]);
                    int Y = Convert.ToInt32(lines[1]);

                    newx = X;
                    newy = Y;

                }


            }

            if (BackGround!=null)
            {
             BackGroundrings = new List<Ring>();

                    for (int i = 1; i < BackGround.Length; i++)
                    {
                        Ring tmp = new Ring();

                        string[] lines1 = BackGround[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                        tmp.X = Convert.ToDouble(lines1[0]);
                        tmp.Y = Convert.ToDouble(lines1[1]);
                        tmp.R = Convert.ToDouble(lines1[2]);
                        tmp.VALUE = Convert.ToDouble(lines1[3]);


                        BackGroundrings.Add(tmp);
                    }
            }
            //



            rings = new List<Ring>();

            for (int i = 1; i < Buffer.Length; i++)
            {
                Ring tmp = new Ring();

                string[] lines = Buffer[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                tmp.X = Convert.ToDouble(lines[0]);
                tmp.Y = Convert.ToDouble(lines[1]);
                tmp.R = Convert.ToDouble(lines[2]);
                tmp.VALUE = Convert.ToDouble(lines[3]);


                rings.Add(tmp);
            }

            
               
            req = new Rectangle();

            if (Buffer.Length != 0)
            {
                string[] l = Buffer[0].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            

                req.X = Convert.ToDouble(l[0]);
                req.Y = Convert.ToDouble(l[1]);
            } else
            {
                req.X=newx;
                req.Y=newy;
            }


            // заполнили структуры данных кольцами + прямоугольник есть тоже

            ShowData();

        }


        class Ring // собственный класс для описания кругов на карте (или циклонов на погоде)
        {
            private double x, y, r, value; // характеристики круга
            public double X { get { return x; } set { x = value; } }
            public double Y { get { return y; } set { y = value; } }
            public double R { get { return r; } set { r = value; } }
            public double VALUE { get { return value; } set { this.value = value; } }

            public bool Focus(double ax, double ay)
            { // на границе
            
                  // x2+y2=r2 уравнение круга.   
                double ar = Math.Sqrt((x - ax) * (x - ax) + (y - ay) * (y - ay));
                   if ( Math.Abs(ar-r) < 5) return true;
                else return false;
            }
            public bool Into(double ax, double ay)
            {

                // x2+y2=r2 уравнение круга.   
                double ar = Math.Sqrt((x - ax) * (x - ax) + (y - ay) * (y - ay));
                if (ar<r-5) return true;
                else return false;
            }

            public void ChangeR(double ax, double ay)
            { 
                //поменять радиус до указанных значений
                double ar = Math.Sqrt((x - ax) * (x - ax) + (y - ay) * (y - ay));
                r = ar;
            }

            public void ChangeXY(double ax, double ay)
            { // клиент тыкнул в этих точках

                // нужно туда сместиться - сделаем просто центр теперь там ок

                x = ax;
                y = ay;
                
            }


        }

        class Rectangle // класс для описания самой карты
        {
            private double x, y;

            public double X { get { return x; } set { x = value; } }
            public double Y { get { return y; } set { y = value; } }

        }
        

        private void ShowData()
        {
            // если есть фон - заполним его.

            Bitmap cadr=null;

            if (BackGround != null)
            {
                cadr = new Bitmap((int)req.X, (int)req.Y);

                Graphics gr1 = Graphics.FromImage(cadr);
                gr1.FillRectangle(new SolidBrush(Color.White), 0, 0, (int)req.X, (int)req.Y);

                for (int i = 0; i < BackGroundrings.Count; i++)
                {
                    gr1.FillEllipse(new SolidBrush(Color.FromArgb((int)BackGroundrings[i].VALUE, (int)BackGroundrings[i].VALUE, (int)BackGroundrings[i].VALUE)), (int)BackGroundrings[i].X - (int)(BackGroundrings[i].R), (int)BackGroundrings[i].Y - (int)(BackGroundrings[i].R), (int)BackGroundrings[i].R * 2, (int)BackGroundrings[i].R * 2);
                    gr1.DrawString(Convert.ToString(BackGroundrings[i].VALUE), new Font("Arial", 5), new SolidBrush(Color.Black), (int)BackGroundrings[i].X, (int)BackGroundrings[i].Y);


                }

                pictureBox1.Image = cadr;
            }



            // отрисуем все из List<Ring> rings и  Rectangle req на окошечко

            if (cadr==null)
            cadr = new Bitmap((int)req.X, (int)req.Y);

            Graphics gr = Graphics.FromImage(cadr);

            if (BackGround == null) gr.FillRectangle(new SolidBrush(Color.White), 0, 0, (int)req.X, (int)req.Y);

            for (int i = 0; i < rings.Count; i++)
            {
                gr.FillEllipse(new SolidBrush(Color.FromArgb((int)rings[i].VALUE, 255 - (int)rings[i].VALUE, 255 - (int)rings[i].VALUE)), (int)rings[i].X - (int)(rings[i].R), (int)rings[i].Y - (int)(rings[i].R), (int)rings[i].R*2, (int)rings[i].R*2);
                gr.DrawString(Convert.ToString(rings[i].VALUE), new Font("Arial", 10), new SolidBrush(Color.Black), (int)rings[i].X,(int)rings[i].Y);

                if (selectedOne == i) gr.DrawEllipse(new Pen(new SolidBrush(Color.Black),10), (int)rings[i].X - (int)(rings[i].R), (int)rings[i].Y - (int)(rings[i].R), (int)rings[i].R * 2, (int)rings[i].R * 2);
            }

            pictureBox1.Image = cadr;
            
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            IntoPos=-1;
            BordPos = -1;

            Point p = pictureBox1.PointToClient(System.Windows.Forms.Cursor.Position);

            for (int i = 0; i < rings.Count; i++)
                if (rings[i].Into(p.X,p.Y)) IntoPos = i;

            for (int i = 0; i < rings.Count; i++)
                if (rings[i].Focus(p.X, p.Y)) BordPos = i;

            mousPressed = true;
            selectedOne = IntoPos;

            ShowData();

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousPressed)
            { 
                // если кнопка зажата
                Point p = pictureBox1.PointToClient(System.Windows.Forms.Cursor.Position);

                if (IntoPos != -1)
                { 
                    // мы внутри круга
                    rings[IntoPos].ChangeXY(p.X, p.Y);
                    

                }
                if (BordPos != -1)
                { 
                    // мы на границе круга
                    rings[BordPos].ChangeR(p.X, p.Y);
                    
                }
                ShowData();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mousPressed = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // запишем в буффер новые данные

            Buffer = new string[1 + rings.Count];
            Buffer[0] = req.X + " " + req.Y;

            for (int i = 0; i < rings.Count; i++)
            {
                string tmp = rings[i].X + " " + rings[i].Y + " " + rings[i].R + " " + rings[i].VALUE;

                Buffer[i+1] = tmp;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Ring tmp = new Ring();
            tmp.X = 100;
            tmp.Y = 100;
            tmp.R = 50;
            tmp.VALUE =Convert.ToDouble( textBox4.Text.ToString());

            rings.Add(tmp);
            ShowData();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedOne != -1)
            {
                rings.RemoveAt(selectedOne);
                ShowData();
            }
        }



    }
}
