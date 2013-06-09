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
        {// возвращает площадь пересечения


            return 0;
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
