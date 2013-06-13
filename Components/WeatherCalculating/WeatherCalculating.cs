using System;
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
        public void GetWeather(IUserID user, string map, string weather, double time)
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

            // прочитаем карту сначала

            for (int i = 1; i < mapBuffer.Length; i++)
            {

                string[] lines1 = mapBuffer[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                double X = Convert.ToDouble(lines1[0]);
                double Y = Convert.ToDouble(lines1[1]);
                double R = Convert.ToDouble(lines1[2]);
                double VALUE = Convert.ToDouble(lines1[3]);

                Land tmp = new Land(X, Y, R, VALUE);

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

                    Cyclone tmp = new Cyclone(X, Y, R, VALUE);

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




            //  return workFrames;
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
            
            F1 = 2 * Math.Acos((a.R * a.R - b.R * b.R + D * D) / (2 * a.R * D));
            F2 = 2 * Math.Acos((b.R * b.R - a.R * a.R + D * D) / (2 * b.R * D));

            double S1, S2;

            S1=(a.R*a.R*(F1-Math.Sin(F1)))/2;
            S2 = (b.R * b.R * (F2 - Math.Sin(F2))) / 2;

            return S1+S2;
        }

        private int time;

        private List<Land> Land;// список элементов карты
        private List<Cyclone> Weather;// список элементов погоды

        public void AddLand(Land s) { if (Land == null) Land = new List<Land>(); Land.Add(s); time = 0; }
        public void AddWeather(Cyclone s) { if (Weather == null) Weather = new List<Cyclone>(); Weather.Add(s);}

        public void AddWeatherPath(int i, List<double[]> Path) 
        {
            Cyclone t = (Cyclone)Weather[i];
            t.Path = Path;
        }

        private bool XYonSegment(double pointX, double pointY, double X1, double Y1, double X2, double Y2)
        {
            // принадлежит ли точка отрезку

            double a=(Y1-Y2)/(X1-X2);
            double b=((Y1+Y2)-a*(X1+X2))/2;

            if ((pointY == a * pointX + b) && (pointX > X1) && (pointX < X2)) return true;
            else return false;
        }

        private void MakeWeather(int cyclone)
        { 
            // учесть погоду от вклада этого циклона на такт вперед
            // так как пересечений с землей может быть много. вычислим для каждого  - потом сложим.


            // все пересечения рассмотрим и обменяемся теплом
            for (int i = 0; i < Land.Count; i++)
            { 
                double S = IntersectionArea((IMapObject)Weather[cyclone],(IMapObject)Land[i]);
                if (S!=0)
                {
                    // значит пересекаются
                 
   
                }
            }

        }

        public WeatherCadr CalculateTact()
        {
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
                // проверим не находимся ли мы сейчас в начале какогонить отрезка
                for (int p = 0; p < Weather[i].Path.Count; p++)
                {
                    if ((Weather[i].Path[p][0] == Weather[i].X) && (Weather[i].Path[p][1] == Weather[i].Y))
                    { 
                        // значит в начале этого отрезка
                        // сдвинем циклон на 1/24 часть этого отрезка

                        double newX = Weather[i].Path[p][0]+(Weather[i].Path[p + 1][0] - Weather[i].Path[p][0]) / 24;
                        double newY = Weather[i].Path[p][1] + (Weather[i].Path[p + 1][1] - Weather[i].Path[p][1]) / 24;

                        Weather[i].X = newX;
                        Weather[i].Y = newY;
                        
                        // сдвинули этот циклон

                        // учтем изменение погоды
                        MakeWeather(i);
                        edit = true;
                        break;
                    }
                }

                if (!edit)
                { 
                    // значит этот циклон не в начале отрезка.
                    //  нужно найти на каком отрезке и сдвинуть еще

                }


            }

                return null;
        }

    }

    public interface IMapObject
    {
       double X { get; set;}
       double Y { get; set;}
       double R { get; set;}
    }

    public abstract class AMapObject:IMapObject
    { 
        // этот класс содержит общую информацию для каждого объекта на карте.
        private double x, y, r, value; // характеристики объекта
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public double R { get { return r; } set { r = value; } }
        public double V { get { return value; } set { this.value = value; } }

        public double S { get { return Math.PI * r * r; } }// площадь
	
    }

    public class Land : AMapObject
   {// либо гора либо низина
       public Land(double x, double y, double r, double v)
       {
           X = x; Y = y; R = r; V = v;
       }
   }

    public class Cyclone : AMapObject
   {// циклон
       public Cyclone(double x, double y, double r, double v)
       {
           X = x; Y = y; R = r; V = v; 
           
       }
       private List<double[]> CyclonesPath; // по точкам - движение циклона. будем считать, что один отрезок в день двигается.
       public List<double[]> Path { get { return CyclonesPath; } set { CyclonesPath = value; } }
   }

    public class WeatherCadr
    { // Этот класс содержит в себе данные о погоде.
      // по идее у нас циклоны и горы не всегда круглые, поэтому тут будет два хранилища. одно для объектов земли, другой - для погоды.
        

        private List<IMapObject> MyLand;// список элементов карты
        private List<IMapObject> MyWeather;// список элементов погоды
        // их заполним в при расчете погды.
        // наружу и отдадим их тогда. 

        public WeatherCadr(List<IMapObject> Land,List<IMapObject> Weather)
        {
            MyLand=Land; MyWeather=Weather;
        }

        public List<IMapObject> Land{get{return MyLand;}}
        public List<IMapObject> Weather{get{return MyWeather;}}

    }



}
