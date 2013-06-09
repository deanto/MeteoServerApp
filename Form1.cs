using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using MeteoServer.Objects;
using MeteoServer.Components.DataBaseAccess;
using MeteoServer.Components.UserManagement;

using MeteoServer.Components;

namespace MeteoServer
{
    public partial class Form1 : Form
    {

        private IDataBaseAccess db;
        private IUserManagement userMANAGER;

        private IUserID current;// текущий пользователь


        public Form1()
        {
            InitializeComponent();
            db = Fabric.GetDataBaseAccess();
            userMANAGER = Fabric.GetUserManagement();
            userMANAGER.DB = db;

            current = userMANAGER.GetProfile();
            string tmp = "";
            if (current == null) tmp = "Error"; else

            tmp = current.GetValue.ToString();
            textBox1.Text = tmp;

            UpdateBase();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (current == null) this.Close();
        }

       private void UpdateBase()
       {
           UserList dbview= userMANAGER.WatchUsers(current);
           string view="";

           if (dbview!=null)
           {

           for (int i=0;i<dbview.N;i++)
           {
               string tmp = "";
               for (int y = 0; y < dbview.R; y++) if (y != dbview.R) tmp += dbview[i, y] + " "; else tmp += dbview[i, y];
                   view += tmp + "\n";
              
           }

           richTextBox1.Text=view;

           } else richTextBox1.Text = "NOT available!";
       }

       private void button1_Click(object sender, EventArgs e)
       {
           if (current != null) 
               userMANAGER.LogOff(current);

          current=userMANAGER.GetProfile();
          string tmp = "";
          tmp = current.GetValue.ToString();
          textBox1.Text = tmp;

          UpdateBase();
       }

       private void button2_Click(object sender, EventArgs e)
       {

           string[] tmp = new string[] { textBox3.Text.ToString(), textBox2.Text.ToString(), "def", "def", "def" };
           
           userMANAGER.UserAdd(current,tmp);

           UpdateBase();
       }

       private void button3_Click(object sender, EventArgs e)
       {

           userMANAGER.DeleteUser(current, textBox4.Text.ToString());
           UpdateBase();
       }

       private void button4_Click(object sender, EventArgs e)
       {
           userMANAGER.ChangeUser(current, textBox5.Text.ToString().Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
           UpdateBase();

       }

       private void textBox5_TextChanged(object sender, EventArgs e)
       {

       }

       private void button5_Click(object sender, EventArgs e)
       {

           userMANAGER.LogOff(current);
           UpdateBase();
       }





    }
}
