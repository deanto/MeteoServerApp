﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using MeteoServer.Components;
using MeteoServer.Components.UserManagement;
using MeteoServer.Objects;

namespace MeteoServer.Components.WeatherCalculating
{
    /* нужен универсальный формат, который можно отдать рисовальщику wpf и winForm и они отрисуют как им нужно
     * 
     * впринципе, формат погоды может быть не только круги. и объекты на земле.
     * но форматы у wpf и winform чтото не очень согласуются. 
     * 
     * 
     * 
    */


    public class WeatherCalculating
    {// как на ютубе порциями загружается. так и у нас будет)
        private int frames; // на сколько кадров вперед идет расчет

        // компонента отвечающая за расчет погоды
        public List<WeatherCadr> GetWeatherFromBegin(IUserID user, string map, string weather)
        {
            frames = 50; // порция кадров

            // дай мне графику(набор кадров вперед) для этой карты для этой погоды в это время
            // считается что можно указывать файлик погоды. типа например есть карта россии
            // для нее есть погода в январе, в феврале .... и соответственно файлики называются. загружаем и смотрим

            List<Graphics> workFrames = new List<Graphics>();
            // этот буффер и отдадим

            // загрузим данные
            FileManagement.FileManager fm = Fabric.GetFileManager();


            string[] mapBuffer = fm.GetThisFile(user, map); // буффер карты
            string[] weatherBuffer = fm.GetThisFile(user, weather);// буффер погоды
            //
            // прочитаем заполним структуры в  WeatherCalculator
            WeatherCalculator wcalc = new WeatherCalculator();
            wcalc.setTime = 0;

            // прочитаем карту сначала

            string[] lines = mapBuffer[0].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            wcalc.SetRect(Convert.ToDouble(lines[0]),Convert.ToDouble(lines[1]));
            // установили размеры карты


            for (int i = 1; i < mapBuffer.Length; i++)
            {

                string[] lines1 = mapBuffer[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                double X = Convert.ToDouble(lines1[0]);
                double Y = Convert.ToDouble(lines1[1]);
                double R = Convert.ToDouble(lines1[2]);
                double VALUE = Convert.ToDouble(lines1[3]);
                double COND = Convert.ToDouble(lines1[4]);

                Land tmp = new Land(X, Y, R, VALUE,COND);

                wcalc.AddLand(tmp);
            }

            // теперь прочитаем файл погоды.
            // добавим циклоны
            int pos = 0;
            for (int i = 1; i < weatherBuffer.Length; i++)
            {

                string[] lines1 = weatherBuffer[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                if (lines1.Length != 0) // это условие отбросит строчки после пустой(траектории там для циклонов, если это погода)
                {

                    double X = Convert.ToDouble(lines1[0]);
                    double Y = Convert.ToDouble(lines1[1]);
                    double R = Convert.ToDouble(lines1[2]);
                    double VALUE = Convert.ToDouble(lines1[3]);
                    double COND = Convert.ToDouble(lines1[4]);

                    Cyclone tmp = new Cyclone(X, Y, R, VALUE,COND);

                    wcalc.AddWeather(tmp);
                }
                else { pos = i; break; }

            }
            // к циклонам добавим траектории пути

            if (pos != 0)
            {

                pos++;

                for (int i = 0; i < weatherBuffer.Length - pos; i++)
                {

                    if (weatherBuffer[i + pos].Length != 0)
                    {
                        List<double[]> add = new List<double[]>();

                        string[] tmp = weatherBuffer[i + pos].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                        for (int t = 0; t < tmp.Length; t += 2)
                        {

                            add.Add(new double[2] { Convert.ToDouble(tmp[t]), Convert.ToDouble(tmp[t + 1]) });

                        }
                        wcalc.AddWeatherPath(i, add);
                    }

                }

            }

            // все структуры заполнены


            List<WeatherCadr> answer = new List<WeatherCadr>();
            for (int i = 0; i < frames; i++)
            {
                answer.Add(wcalc.CalculateTact());// рассчитали такт еще один и положили в ролик
            }

            return answer;

            //  return workFrames;
        }
        public List<WeatherCadr> GetWeatherFromCadr(WeatherCadr cadr,int time, IUserID user, string weather)
        {
            frames = 10; // порция кадров

            // нам нужно в память загрузить кадо

            WeatherCalculator wcalc = new WeatherCalculator();
            wcalc.setTime = time;
            // добавим землю
            for (int i = 0; i < cadr.Land.Count; i++)
            {
                Land t=(Land)cadr.Land[i];
                Land tmp = new Land(t.X, t.Y, t.R, t.V, t.C);
                wcalc.AddLand(tmp);
            }
            // добавим циклоны
            for (int i = 0; i < cadr.Weather.Count; i++)
            {
                Cyclone t = (Cyclone)cadr.Weather[i];
                Cyclone tmp = new Cyclone(t.X, t.Y, t.R, t.V, t.C);
                wcalc.AddWeather(tmp);
            }





            // добавим траектории циклонов
            FileManagement.FileManager fm = Fabric.GetFileManager();
            string[] weatherBuffer = fm.GetThisFile(user, weather);// буффер погоды

            string[] lines = weatherBuffer[0].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            wcalc.SetRect(Convert.ToDouble(lines[0]), Convert.ToDouble(lines[1]));


            int pos = 0;
            for (int i = 1; i < weatherBuffer.Length; i++)
            {
                string[] lines1 = weatherBuffer[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                if (lines1.Length == 0) // это условие отбросит строчки после пустой(траектории там для циклонов, если это погода)
                {
                    pos = i; break; 
                }
            }

            if (pos != 0)
            {
                pos++;

                for (int i = 0; i < weatherBuffer.Length - pos; i++)
                {

                    if (weatherBuffer[i + pos].Length != 0)
                    {
                        List<double[]> add = new List<double[]>();

                        string[] tmp = weatherBuffer[i + pos].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                        for (int t = 0; t < tmp.Length; t += 2)
                        {

                            add.Add(new double[2] { Convert.ToDouble(tmp[t]), Convert.ToDouble(tmp[t + 1]) });

                        }
                        wcalc.AddWeatherPath(i, add);
                    }

                }

            }

            // все заполнили


            List<WeatherCadr> answer = new List<WeatherCadr>();

            for (int i = 0; i < frames; i++)
            {
                answer.Add(wcalc.CalculateTact());// рассчитали такт еще один и положили в ролик
            }

            return answer;

        }
    }

    class WeatherCalculator
    { 
        // этот класс реализует алгоритм рассчета погоды
        // при рассчете погоды, все структуры изменяются. на шаг вперед по времени
        // один день рассчета - отрезок на траектории

        private double IntersectionArea(IMapObject a, IMapObject b)
        {
            // возвращает площадь пересечения окружностей

            double D;// расстояние между центрами окружностей

            
            D = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

            if (a.R + b.R <= D) return 0;

            double F1, F2;


            if (((a.R * a.R - b.R * b.R + D * D) / (2 * a.R * D)) > 1 || (b.R * b.R - a.R * a.R + D * D) / (2 * b.R * D) > 1)
            {
                double sS1 = a.R * a.R * 2 * Math.PI;
                double sS2 = b.R * b.R * 2 * Math.PI;

                if (sS1 > sS2) return sS2; else return sS1;
            }

            F1 = 2 * Math.Acos((a.R * a.R - b.R * b.R + D * D) / (2 * a.R * D));
            F2 = 2 * Math.Acos((b.R * b.R - a.R * a.R + D * D) / (2 * b.R * D));

            

            double S1, S2;

            S1=(a.R*a.R*(F1-Math.Sin(F1)))/2;
            S2 = (b.R * b.R * (F2 - Math.Sin(F2))) / 2;

            return S1+S2;
        }

        private int time;
        public int setTime { set { time = value; } }

        private double MapX,MapY;
        private List<Land> Land;// список элементов карты
        private List<Cyclone> Weather;// список элементов погоды

        public void SetRect(double X,double Y)
        {
            MapX=X;MapY=Y;
        }
        public void AddLand(Land s) { if (Land == null) Land = new List<Land>(); Land.Add(s); }
        public void AddWeather(Cyclone s) { if (Weather == null) Weather = new List<Cyclone>(); Weather.Add(s);}

        public void AddWeatherPath(int i, List<double[]> Path) 
        {
            Cyclone t = (Cyclone)Weather[i];
            t.Path = Path;
        }

        private bool XYonSegment(double pointX, double pointY, double X1, double Y1, double X2, double Y2)
        {
            // принадлежит ли точка отрезку
            // посчитаем следующим образом - найдем расстояние от нашей точки до каждой границы отрезка
            // если сумма расстояний почти равна длине отрезка - точка почти на отрезке)


            double d1 = Math.Sqrt((pointX - X1) * (pointX - X1) + (pointY - Y1) * (pointY - Y1));
            double d2 = Math.Sqrt((pointX - X2) * (pointX - X2) + (pointY - Y2) * (pointY - Y2));

            double d = Math.Sqrt((X1 - X2) * (X1 - X2) + (Y1 - Y2) * (Y1 - Y2));

            if (Math.Abs((d1 + d2) - d) < 1) // если разница меньше 1 то точка почти на отрезке
                return true; 

            return false;
        }

        private void MakeWeather(int cyclone)
        { 
            // учесть погоду от вклада этого циклона на такт вперед
            // так как пересечений с землей может быть много. вычислим для каждого  - потом сложим.

            // все пересечения рассмотрим и обменяемся теплом
            /*
             * сделаем так. заморозим температуру пока для циклона. 
             * в каждом пересечении запомним сколько мы отдали или получили энергии - местность нагреем или остудим.
             * потом все вместе сложим с замороженной температурой и положим как текущую.
             */
            double freez = Weather[cyclone].V; // запомнили
            double finalE = 0;// количество энергии суммарное от всех пересечений + или - 

            bool flag = false;

            for (int i = 0; i < Land.Count; i++)
            { 
                double S = IntersectionArea((IMapObject)Weather[cyclone],(IMapObject)Land[i]);
                if (S!=0)
                {
                    // значит пересекаются
                    flag = true;

                    double LandE = S * Land[i].V; // количество энергии у земли на этой площади
                    double WeathE = S * Weather[cyclone].V; // количество энергии у циклона на этой площади

                    double qs = (LandE + WeathE) / 2;// средняя

                    // тот у кого энергия была меньше установившейся средней - получит тепло - разницу между тем что было и средней
                    // тот у кого энергия была больше - отдаст энергию аналогично

                    // полученная/принятая энергия умножается на коэффициент теплопроводности (0-100) % и вот именно это количество и получается

                   
                        double r = qs - WeathE; 
                        r *= Weather[cyclone].C / 100; // это учли теплопроводность.
                        finalE += r; // если разница положительная - то нагрелся циклон

                        r = (qs - LandE) * Land[i].C / 100; // что получит или отдаст земля

                        // энергия это площадь умноженная на температуру.

                        double finalLand = r + Land[i].V*Land[i].S;  // вот сложили энергию
                        Land[i].V = finalLand / Land[i].S;// распределили по всей площади энергию

                }
            }

            if (flag)
            {
                double finalWeather = finalE + freez * Weather[cyclone].S;
                Weather[cyclone].V = finalWeather / Weather[cyclone].S; // распределили энергию по циклону.
            }


        }

        public WeatherCadr CalculateTact()
        {

            int cadrPerLine = 24;

            //рассчитываем кадр исходя их текущих данных и возвращаем копию текущего состоянияю

            /*
             * рассчет:
             * 
             * для каждого циклона:
             *  по текущему времени вычисляем точку на траектории, на которой находится циклон сейчас.
             *  двигаем циклон на шаг вперед.
             *      по всем пересечениям ЭТОГО циклона с МЕСТНОСТЬЮ - обмениваем теплом циклон и местность.
             */

            for (int i = 0; i < Weather.Count; i++)
            { 
                // time это такты времени на которые мы двигаемся в рамках этого объекта калькулятора
                // один отрезок траектории - это день. такт времени путь будет час. 24 часа в дне. 

                // сдвинуть в следующее положение нужно каждый циклон на 1/24 часть отрезка, на котором он сейчас находится.
                // на сами точки границ отрезков тоже проверяем.
                bool edit = false;
                // проверим не находимся ли мы сейчас внутри какогонить отрезка
                for (int p = 0; p < Weather[i].Path.Count-1; p++)
                {

                    // может мы в начале отрезка
                    //if ((Weather[i].Path[p][0] == Weather[i].X) && (Weather[i].Path[p][1] == Weather[i].Y))
                    if (Math.Abs(Weather[i].Path[p][0] - Weather[i].X) < 1 && Math.Abs(Weather[i].Path[p][1] - Weather[i].Y)<1)
                    { 
                        // значит в начале этого отрезка
                        // сдвинем циклон на 1/24 часть этого отрезка

                        Weather[i].X = Weather[i].Path[p][0];
                        Weather[i].Y = Weather[i].Path[p][1];

                        double newX = Weather[i].Path[p][0] + (Weather[i].Path[p + 1][0] - Weather[i].Path[p][0]) / cadrPerLine;
                        double newY = Weather[i].Path[p][1] + (Weather[i].Path[p + 1][1] - Weather[i].Path[p][1]) / cadrPerLine;

                        Weather[i].X = newX;
                        Weather[i].Y = newY;
                        
                        // сдвинули этот циклон

                        // учтем изменение погоды
                        MakeWeather(i);
                        edit = true;
                        break;
                    }
                    // может быть мы внутри этого отрезка

                    int t = p + 1;
                    if (t == Weather[i].Path.Count) t = 0;
                    if (edit) continue;
                    if (XYonSegment(Weather[i].X,Weather[i].Y,Weather[i].Path[p][0],Weather[i].Path[p][1], Weather[i].Path[t][0],Weather[i].Path[t][1]))
                    {
                        // значит внутри
                        // можно узнать направление по каждой оси куда двигаемся по отрезку. и прибавить в эту сторону 1/24 часть длины отрезка!

                        int x; if (Weather[i].Path[p][0] < Weather[i].Path[p + 1][0]) x = 1; else x = -1;
                        int y; if (Weather[i].Path[p][1] < Weather[i].Path[p + 1][1]) y = 1; else y = -1;

                        double newX = Math.Abs(Weather[i].Path[p][0] - Weather[i].Path[p + 1][0]) * x / cadrPerLine + Weather[i].X;
                        double newY = Math.Abs(Weather[i].Path[p][1] - Weather[i].Path[p + 1][1]) * y / cadrPerLine + Weather[i].Y;

                        Weather[i].X = newX;
                        Weather[i].Y = newY;

                        MakeWeather(i);
                    }

                }

            }
            // сменили положение на один такт-час

            time++;

            WeatherCadr cadr = new WeatherCadr(Land, Weather,MapX,MapY);
            cadr.TIME = time;

                return cadr;
        }

        
    }

    public interface IMapObject
    {
       double X { get; set;}
       double Y { get; set;}
       double R { get; set;}
       double V { get; set; }
       
       
    }

    public abstract class AMapObject:IMapObject
    { 
        // этот класс содержит общую информацию для каждого объекта на карте.
        private double x, y, r, value,c; // характеристики объекта
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public double R { get { return r; } set { r = value; } }
        public double V { get { return value; } set { this.value = value; } }
        public double C { get { return c; } set { c = value; } }

        public double S { get { return Math.PI * r * r; } }// площадь
	
    }

    public class Land : AMapObject
   {// либо гора либо низина
       public Land(double x, double y, double r, double v,double c)
       {
           X = x; Y = y; R = r; V = v; C = c;
       }
   }

    public class Cyclone : AMapObject
   {// циклон
       public Cyclone(double x, double y, double r, double v,double c)
       {
           X = x; Y = y; R = r; V = v; C = c;
           
       }
       private List<double[]> CyclonesPath; // по точкам - движение циклона. будем считать, что один отрезок в день двигается.
       public List<double[]> Path { get { return CyclonesPath; } set { CyclonesPath = value; } }
   }

    public class WeatherCadr
    { // Этот класс содержит в себе данные о погоде.
      // по идее у нас циклоны и горы не всегда круглые, поэтому тут будет два хранилища. одно для объектов земли, другой - для погоды.

        private double X, Y; // размеры кадра
        public double height { get { return Y; }  }
        public double weight { get { return X; }  }

        private int time;
        public int TIME { get { return time; } set { time = value; } }

        private List<IMapObject> MyLand;// список элементов карты
        private List<IMapObject> MyWeather;// список элементов погоды
        // их заполним в при расчете погды.
        // наружу и отдадим их тогда. 

        public WeatherCadr(List<Land> Land,List<Cyclone> Weather,double x,double y)
        {// принимаем местность и погоду.
            // не по ссылке возьмем а скопируем

            X = x;
            Y = y;

            //MyLand=Land;
            MyLand = new List<IMapObject>();
            for (int i = 0; i < Land.Count; i++)
            { 
                IMapObject tmp = new Land(Land[i].X,Land[i].Y,Land[i].R,Land[i].V,Land[i].C);
                MyLand.Add(tmp);
            }
            
            
            
            
            //MyWeather=Weather;
            MyWeather = new List<IMapObject>();
            for (int i = 0; i < Weather.Count; i++)
            { 
                IMapObject tmp = new Cyclone(Weather[i].X,Weather[i].Y,Weather[i].R,Weather[i].V,Weather[i].C);
                MyWeather.Add(tmp);
            }

        }

        public List<IMapObject> Land{get{return MyLand;}}
        public List<IMapObject> Weather{get{return MyWeather;}}

    }



}
