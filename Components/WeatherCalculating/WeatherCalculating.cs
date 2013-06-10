using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using MeteoServer.Components;
using MeteoServer.Components.UserManagement;
using MeteoServer.Objects;

namespace MeteoServer.Components
{

    class WeatherCalculating
    {// как на ютубе порциями загружается. так и у нас будет)
        private int frames; // на сколько кадров вперед идет расчет


        // компонента отвечающая за расчет погоды
        public List<Graphics> GetWeather(IUserID user, string map,string weather, double time)
        { 
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


            

            return workFrames;
        }
    }

    class WeatherCalculator
    { 
        // этот класс реализует алгоритм рассчета погоды

        private double IntersectionArea(IMapObject a, IMapObject b)
        {// возвращает площадь пересечения окружностей

            double D;// расстояние между центрами окружностей

            D = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

            double F1, F2;
            
            F1 = 2 * Math.Acos((a.R * a.R - b.R * b.R + D * D) / (2 * a.R * D));
            F2 = 2 * Math.Acos((b.R * b.R - a.R * a.R + D * D) / (2 * b.R * D));

            double S1, S2;

            S1=(a.R*a.R*(F1-Math.Sin(F1)))/2;
            S2 = (b.R * b.R * (F2 - Math.Sin(F2))) / 2;

            return S1+S2;
        }

        private List<IMapObject> Land;// список элементов карты
        private List<IMapObject> Weather;// список элементов погоды

        private int time;

        public void AddLand(IMapObject s) { if (Land == null) Land = new List<IMapObject>(); Land.Add(s); time = 0; }
        public void AddWather(IMapObject s) { if (Weather == null) Weather = new List<IMapObject>(); Weather.Add(s);}

        private void CalculateTact()
        { /*// взять текущее расположение погоды на карте и рассчитать такт вперед
            // у нас есть положение. круги могут быть уже перекрыты другими.
            // текущее время у нас есть time 
            // нужно увеличить круги. потом посмотреть все перекрытия и рассчитать количество энергии которое нужно перераспределить

            // сначала пройдем все пересечения и посмотрим сколько каждый должен отдать (соответственно другой получит)
            // так как одновременно должно происходить смешивание. то у нас могут быть области, которые одновременно и получают и отдают...
            
           * 

            */
        }

    }

    interface IMapObject
    {
       double X { get; set;}
       double Y { get; set;}
       double R { get; set;}
    }

   abstract class AMapObject:IMapObject
    { 
        // этот класс содержит общую информацию для каждого объекта на карте.
        private double x, y, r, value; // характеристики объекта
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public double R { get { return r; } set { r = value; } }
        public double V { get { return value; } set { this.value = value; } }

        public double S { get { return Math.PI * r * r; } }// площадь
	
    }

   class Land : AMapObject
   {// либо гора либо низина
       public Land(double x, double y, double r, double v)
       {
           X = x; Y = y; R = r; V = v;
       }
   }

   class Cyclone : AMapObject
   {// циклон
       public Cyclone(double x, double y, double r, double v)
       {
           X = x; Y = y; R = r; V = v; 
       }
   }





}
