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
        int PATHEDIT;

        int IntoPos;
        int BordPos;
        bool mousPressed;

        int selectedOne;

        private List<double[]>[] CyclonesPath; // траектории всех кругов [0] номер циклона. потом по два значения - координаты точек в траектории.

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
            PATHEDIT = 0;
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

            // если это у нас погода, то узнаем сколько там циклонов

            rings = new List<Ring>();
            int pos=0;
            for (int i = 1; i < Buffer.Length; i++)
            {
                Ring tmp = new Ring();

                string[] lines = Buffer[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length != 0) // это условие отбросит строчки после пустой(траектории там для циклонов, если это погода)
                {
                    tmp.X = Convert.ToDouble(lines[0]);
                    tmp.Y = Convert.ToDouble(lines[1]);
                    tmp.R = Convert.ToDouble(lines[2]);
                    tmp.VALUE = Convert.ToDouble(lines[3]);


                    rings.Add(tmp);
                }
                else { pos=i; break; }
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
            // если мы работаем с погодой - проинициализируем данные о траекториях
            CyclonesPath = new List<double[]>[rings.Count];

            if (pos != 0)
            {
                
                // значт это погода и есть траектории!
                //pos это пустая строчка в Buffer
                // заполним пути циклонов и ок)

                pos++;

                for (int i = 0; i < Buffer.Length - pos; i++)
                {

                    
                    if (Buffer[i + pos].Length != 0)
                    {
                        CyclonesPath[i] = new List<double[]>();
                        string[] tmp = Buffer[i + pos].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                        for (int t = 0; t < tmp.Length; t += 2)
                        {

                            CyclonesPath[i].Add(new double[2] { Convert.ToDouble(tmp[t]), Convert.ToDouble(tmp[t + 1]) });

                        }
                        CyclonesPath[i].Add(CyclonesPath[i][0]);
                    }

                }

            }

            ShowData();
            
        }


        class Ring // собственный класс для описания кругов на карте (или циклонов на погоде)
        {
            private double x, y, r, value, conductivity; // характеристики круга
            public double X { get { return x; } set { x = value; } }
            public double Y { get { return y; } set { y = value; } }
            public double R { get { return r; } set { r = value; } }
            public double VALUE { get { return value; } set { this.value = value; } }
            public double CONDUCT { get { return conductivity; } set { conductivity = value; } }

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
            
                // круги сами нарисовали. теперь добавим траектории сереньким цветом.

                if ((BackGround!=null)&&(CyclonesPath.Length!=0))

                if (CyclonesPath[i] != null)
                { 
                    Point[] P= new Point[CyclonesPath[i].Count];
                    for (int p = 0; p < CyclonesPath[i].Count; p++)
                        P[p] = new Point((int)CyclonesPath[i][p][0],(int)CyclonesPath[i][p][1]);
                    if (selectedOne!=i)
                        gr.DrawLines(new Pen(new SolidBrush(Color.Silver), 3), P);
                }
            }

            if ((BackGround != null)&&(CyclonesPath.Length!=0))
            for (int i = 0; i < rings.Count; i++)
            {
                if (selectedOne == i)
                    if (CyclonesPath[i] != null)
                    {
                        Point[] P = new Point[CyclonesPath[i].Count];
                        for (int p = 0; p < CyclonesPath[i].Count; p++)
                            P[p] = new Point((int)CyclonesPath[i][p][0], (int)CyclonesPath[i][p][1]);
                        gr.DrawLines(new Pen(new SolidBrush(Color.Red), 4), P);
                    }
            }

            pictureBox1.Image = cadr;

            if ((BackGround != null) && (selectedOne >= 0)) button4.Enabled = true; else button4.Enabled = false;

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (PATHEDIT == 0)
            {// если мы не в режиме редактирования пути

                IntoPos = -1;
                BordPos = -1;

                Point p = pictureBox1.PointToClient(System.Windows.Forms.Cursor.Position);

                for (int i = 0; i < rings.Count; i++)
                    if (rings[i].Into(p.X, p.Y)) IntoPos = i;

                for (int i = 0; i < rings.Count; i++)
                    if (rings[i].Focus(p.X, p.Y)) BordPos = i;

                mousPressed = true;
                selectedOne = IntoPos;

                ShowData();
            }
            else
            { 
            // в режиме редактирования пути и нажали на область окна

                // возьмем список точек из траектории для этого циклона. на предпоследнем месте теперь будет текущая точка.
                // таким образом увеличим траекторию на текущую точку.
                Point p = pictureBox1.PointToClient(System.Windows.Forms.Cursor.Position);

                CyclonesPath[selectedOne].RemoveAt(CyclonesPath[selectedOne].Count - 1);// удалили последнюю точку(центр циклона)
                CyclonesPath[selectedOne].Add(new double[2] { p.X,p.Y}); // добавили текущую
                CyclonesPath[selectedOne].Add(CyclonesPath[selectedOne][0]); // замкнули траекторию

                ShowData();
            }


        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            
                if (mousPressed)
                {
                    // если кнопка зажата
                    Point p = pictureBox1.PointToClient(System.Windows.Forms.Cursor.Position);

                    if (IntoPos != -1)
                    {

                        // вместе с изменением положения круга, нужно изменить и положение траектории (если мы редактируем погоду)
                        if (BackGround != null)
                        {
                            // тоесть у нас всетаки погодная карта редактируется
                            double prevX = rings[IntoPos].X;
                            double prevY = rings[IntoPos].Y;

                            double Xchange = p.X - prevX; // сколько нужно прибавить к коодинатам (может быть отрицательным)
                            double Ychange = p.Y - prevY; //

                            // теперь эти изменения добавим к траектории
                            if (CyclonesPath.Length!=0)
                                if (CyclonesPath[IntoPos] != null)
                            {

                                for (int i = 0; i < CyclonesPath[IntoPos].Count-1; i++)
                                {
                                    CyclonesPath[IntoPos][i][0] += Xchange;
                                    CyclonesPath[IntoPos][i][1] += Ychange;
                                }
                                CyclonesPath[IntoPos][CyclonesPath[IntoPos].Count - 1][0] = CyclonesPath[IntoPos][0][0];
                                CyclonesPath[IntoPos][CyclonesPath[IntoPos].Count - 1][1] = CyclonesPath[IntoPos][0][1];
                            }

                        }

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
            if (PATHEDIT == 0)
            {// если мы не в режиме редактирования пути
                mousPressed = false;
            }
            else
            { 
                
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // запишем в буффер новые данные

            Buffer = new string[1 + rings.Count];
            Buffer[0] = req.X + " " + req.Y;

            for (int i = 0; i < rings.Count; i++)
            {
                string tmp = rings[i].X + " " + rings[i].Y + " " + rings[i].R + " " + rings[i].VALUE +" "+rings[i].CONDUCT;

                Buffer[i+1] = tmp;
            }

            // часть с траекториями будет следовать после части с данными о циклонах. разделитель - пустая строка
            if (BackGround != null)
            {
                string[] oldBuf = new string[Buffer.Length];
                for (int i = 0; i < Buffer.Length; i++)
                    oldBuf[i] = Buffer[i];


                Buffer = new string[oldBuf.Length + 1 + CyclonesPath.Length];
                for (int i = 0; i < oldBuf.Length; i++)
                    Buffer[i] = oldBuf[i];

                // старые данные сохранены
                Buffer[oldBuf.Length] = ""; // разделитель.
                // добавим данные о траекториях циклонов

                for (int i = 0; i < CyclonesPath.Length; i++)
                {
                    string tmp = "";
                    if (CyclonesPath[i]!=null)
                        for (int q = 0; q < CyclonesPath[i].Count; q++)
                        {
                            tmp += CyclonesPath[i][q][0].ToString() + " ";
                            tmp += CyclonesPath[i][q][1].ToString() + " ";
                        }

                    string correcttmp = "";
                    if (tmp!="")
                    for (int q = 0; q < tmp.Length - 1; q++) correcttmp += tmp[q];

                    Buffer[oldBuf.Length +1+ i] = correcttmp;

                }

            }
            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Ring tmp = new Ring();
            tmp.X = 100;
            tmp.Y = 100;
            tmp.R = 50;
            tmp.VALUE =Convert.ToDouble(textBox4.Text.ToString());
            tmp.CONDUCT = Convert.ToDouble(textBox1.Text.ToString());

            rings.Add(tmp);
            // когда создаем круги - нужно под пути создать место.

            List<double[]>[] NewCyclonesPath = new List<double[]>[CyclonesPath.Length+1];

            // копируем туда данные
            if (CyclonesPath.Length != 0)
            {
                for (int i = 0; i < CyclonesPath.Length; i++)
                {
                    List<double[]> tmp1 = new List<double[]>();
                    for (int q = 0; q < CyclonesPath[i].Count; q++)
                    {
                        double[] tmp3 = new double[2];
                        tmp3[0] = CyclonesPath[i][q][0];
                        tmp3[1] = CyclonesPath[i][q][1];

                        tmp1.Add(tmp3);
                    }
                    NewCyclonesPath[i] = tmp1;
                }

                NewCyclonesPath[CyclonesPath.Length] = null;
            }
            else NewCyclonesPath[0] = null;

            CyclonesPath = NewCyclonesPath;

            ShowData();

        }

        private void button3_Click(object sender, EventArgs e)
        {// удалить циклон
            if (selectedOne != -1)
            {
                rings.RemoveAt(selectedOne);


                List<double[]>[] newCycl = new List<double[]>[CyclonesPath.Length - 1];

                int w = 0;

                for (int i = 0; i < CyclonesPath.Length; i++)
                {
                    List<double[]> tmp1 = new List<double[]>();
                    for (int q = 0; q < CyclonesPath[i].Count; q++)
                    {
                        double[] tmp3 = new double[2];
                        tmp3[0] = CyclonesPath[i][q][0];
                        tmp3[1] = CyclonesPath[i][q][1];

                        tmp1.Add(tmp3);
                    }
                    if (i != selectedOne) 
                    {
                        newCycl[w] = tmp1; 
                        w++;
                    } 
                }
                CyclonesPath = newCycl;

                    ShowData();
            }
        }

        // это кнопка создает траекторию для циклона
        private void button4_Click_1(object sender, EventArgs e)
        {
            if ((selectedOne != -1) && (PATHEDIT == 0))
            {
                // есть что редактировать.
                PATHEDIT = 1;
                this.BackColor = Color.Red;
                // вошли в режим редактирования пути. знаем для какого циклона.

                // очистим предыдущий путь и начнем заново.
                // первая и последняя точки в пути - центр циклона

               

                CyclonesPath[selectedOne] = new List<double[]>();
                CyclonesPath[selectedOne].Add(new double[2] { rings[selectedOne].X,rings[selectedOne].Y,});
                CyclonesPath[selectedOne].Add(new double[2] { rings[selectedOne].X,rings[selectedOne].Y,});

                // теперь для этого циклона - траектория из двух точек - из начала в начало
                ShowData();
            } else
            if (PATHEDIT == 1) 
            {
                // закончили редактировать путь для циклона
                PATHEDIT = 0; 
                

                this.BackColor = Control.DefaultBackColor;
                ShowData();
            }
                
        }

     



    }
}
