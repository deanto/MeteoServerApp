using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        public string WEATHER { set{weather=value;}}

        public IUserID user;
        public IUserID USER { set { user = value; } }

        List<WeatherCadr> initialvideo; // ролик в начале.

        public WinFormWeatherDisplay()
        {
            InitializeComponent();
        }

        private void WinFormWeatherDisplay_Load(object sender, EventArgs e)
        {
           initialvideo = wc.GetWeatherFromBegin(user, map, weather);
        }





    }
}
