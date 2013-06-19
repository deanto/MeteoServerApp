using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MeteoServer.Components.WeatherCalculating;
using MeteoServer.Objects;

namespace MeteoServer.Components.WeatherDisplay
{
   public abstract class ADisplayer
    { 
        // абстрактный класс отображения.
        // кто захочет сделать свое отображение - это wpf и winform - отнаследуются от этого класса и будут иметь уже внутри экземпляр компоненты рассчета погоды
        // и дальше сами работают.

       protected WeatherCalculating.WeatherCalculating wc;

        public ADisplayer()
        {
            wc = new WeatherCalculating.WeatherCalculating();
            
        }

    }


    //тут напишем класс, для отображения погоды на winform 
    // для pwf будет в другом проекте реализовано

    class winfomfDisplayer : ADisplayer
    {
        public winfomfDisplayer() : base() { }

        public void ShowWeather(IUserID user, string map, string weather)
        {// отобразить карту и погоду

            WinFormWeatherDisplay wfwd = new WinFormWeatherDisplay();// окошечко создаем, передаем ему компоненту рассчета погоды и запускаем
            wfwd.WC = wc;
            wfwd.WEATHER = weather;
            wfwd.MAP = map;
            wfwd.USER = user;
            wfwd.ShowDialog();

        }

    }


}
