using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MeteoServer.Components.FileEditor;

using MeteoServer.Objects;

using MeteoServer.Components.FileManagement;

namespace MeteoServer.Components.FileManagement
{
    public partial class FileManagerView : Form
    {
        private IUserID current;

        public IUserID setUser { set { current = value; } }
        FileManager fm;

        FileTree files;

        List<string> tree;// сюда будет добавлено построчно вся инфа о доступной файловой структуре


        public FileManagerView()
        {
            InitializeComponent();

        }

        private void FileManagerView_Load(object sender, EventArgs e)
        {
           
            ShowFileTree();

          
        }


        private void ShowFileTree()
        {
            fm = Fabric.GetFileManager();
            files = fm.GetFiles(current);

            tree = new List<string>();
            FillTreeList(files.FileList);

            string show = "";
            for (int i = 0; i < tree.Count; i++)
                show += tree[i] + "\n";

            richTextBox1.Text = show;
            // это мы отобразили список файлов и директорий.
            // отобразим теперь для них группы пользователей в соседнее окошечко

            string[] showlines = show.Split(new[] { '\n' });

            string shourights = "";
            for (int i = 0; i < tree.Count; i++)
            { 
                // пройдем по всем строчкам, возьмем имена файлов - к ним найдем права и отобразим.
                 GetRights(showlines[i], files.FileList);
                 string[] rights = rightss;

                if (rights == null)
                    shourights += "\n";
                else
                {
                    string tmp = ""; for (int a = 0; a < rights.Length; a++) tmp += " " + rights[a];
                    shourights += tmp + "\n";
                }


            }

            richTextBox2.Text = shourights;
        }

        private string[] rightss;

        private void GetRights(string file,Directorys dir)
        {
           

            for (int i = 0; i < dir.filesRights.Count;i++ )
            {
                if (dir.filesRights[i][0].Equals(file))
                {
                    string[] tmp = new string[dir.filesRights[i].Length - 1];
                    for (int q = 1; q < dir.filesRights[i].Length; q++) tmp[q - 1] = dir.filesRights[i][q];
                    if (tmp.Length != 0)
                    {
                        rightss = new string[tmp.Length];
                        rightss = tmp;
                    }
                }
            }

            for (int e = 0; e < dir.subdirectorys.Count; e++)
                GetRights(file, dir.subdirectorys[e]);


            


        }

        private void FillTreeList(Directorys listtree)
        {
            // заполнить List<string> tree из  FileTree files;

            for (int i = 0; i < listtree.subdirectorys.Count; i++)
                tree.Add(listtree.subdirectorys[i].path);
            

            for (int i = 0; i < listtree.files.Count; i++)
                tree.Add(listtree.files[i]);
            

            for (int i = 0; i < listtree.subdirectorys.Count; i++)
                FillTreeList(listtree.subdirectorys[i]);

        }

        private void richTextBox1_Click(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string path = richTextBox1.SelectedText;
            string correctpath = "";
            for (int i = 0; i < path.Length-1; i++) correctpath += path[i];

            string[] file = fm.GetThisFile(current,correctpath);


            EditFiles editor = new EditFiles();
            editor.setUser = current;
            editor.Buffer = file;

           

                editor.WinFormsEdit();



            file = editor.Buffer;// теперь новое значение

            fm.SaveThisFile(current, correctpath, file);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            fm.CreateNewDirectory(current, textBox1.Text.ToString());
            ShowFileTree();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                string[] rights = textBox4.Text.Split(new[] { ' ' });
                fm.CreateNewFile(current, textBox2.Text.ToString(), rights);
                ShowFileTree();
            }
            else MessageBox.Show("не указаны права!");
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string path = richTextBox1.SelectedText;
            string correctpath = "";
            for (int i = 0; i < path.Length - 1; i++) correctpath += path[i];

            fm.Delete(current, correctpath);
            ShowFileTree();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

            fm.CreateNewDirectory(current, textBox1.Text);
            ShowFileTree();
        }
        

    }
}
