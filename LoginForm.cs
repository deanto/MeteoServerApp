using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using MeteoServer.Components.DataBaseAccess;
using MeteoServer.Components.UserManagement;

using MeteoServer.Components;

using MeteoServer.Objects;

namespace MeteoServer
{
    public partial class LoginForm : Form
    {
        private string login, password;
        public IUserID answer;

        public LoginForm()
        {
            InitializeComponent();
            
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetUser tmp=null;

                login = textBox1.Text.ToString();
                password = textBox2.Text.ToString();

                IDataBaseAccess db = Fabric.GetDataBaseAccess();


                IObjects req = new GetUser(login, password);
                tmp = (GetUser)db.SEND(ref req);


             //   if (tmp == null) MessageBox.Show("Пара логин / пароль неверна.\nПроверьве правильность указанных данных.");

                if (tmp == null) answer = null;
                else
                {
                    answer = tmp.Answer;
                    this.Close();
                }
            


        }

        
    }
}
