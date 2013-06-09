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

            

            return workFrames;
        }


        

    }



}
