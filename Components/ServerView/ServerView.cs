using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MeteoServer.Components.UserManagement;
using MeteoServer.Objects;
using MeteoServer.Components.FileManagement;

namespace MeteoServer.Components.ServerView
{
    public partial class ServerView : Form
    {

        private IUserID current;

        

        public ServerView()
        {
            InitializeComponent();

            IUserManagement userManagement = Fabric.GetUserManagement();

            current = userManagement.GetProfile();
        

        }

        private void ServerView_Load(object sender, EventArgs e)
        {
            if (current == null) this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 t = new Form1();
            
            t.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            FileManagerView t = new FileManagerView();
            t.setUser = current;

            t.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            IUserManagement um = Fabric.GetUserManagement();
            um.LogOff(current);
        }
    }
}
