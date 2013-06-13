using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using MeteoServer.Components.FileManagement;

using MeteoServer.Objects;

using System.IO;

namespace MeteoServer.Components.DataBaseAccess
{
    public interface IDataBaseAccess
    {
        IObjects SEND(ref IObjects obj);
    }


    class DataBase : IDataBaseAccess
    {

        private HandlersList HandlerList;

        public DataBase()
        {
           
            HandlerList = new HandlersList();
            HandlerList.AddHandler(new GetUserHadler());
            HandlerList.AddHandler(new OtherUserRequestHandler());
            HandlerList.AddHandler(new LogOffHandler());
            HandlerList.AddHandler(new FileListRequestHandler());
            HandlerList.AddHandler(new FileGetRequestHandler());
            HandlerList.AddHandler(new FileSaveRequestHandler());
            HandlerList.AddHandler(new CreateDirectoryRequestHandler());
            HandlerList.AddHandler(new CreateFileRequestHandler());
            HandlerList.AddHandler(new DeleteFileRequestHandler());
        }

        public IObjects SEND(ref IObjects obj)
        {

            IObjects answer=null;
            // определимся ваще есть ли такой пользователь

            WatchingUsers t = new WatchingUsers();


            bool open = false;


            if ((obj.User == null) && ((obj.ID.ToString().Equals("GetUser")||(obj.ID.ToString().Equals("LogOff"))))) open=true;
                if (obj.User!=null)
                    if (t.RetunUserRights(obj.User.GetValue) != null) open = true;


                if (open)
            {
                bool found = false;
                int n = 0;
                while (n < HandlerList.count && !found)
                {
                    if (HandlerList.GetList[n].RelatedRequest.Equals(obj.ID.ToString()))
                    {
                        found = true;

                        answer = HandlerList.GetList[n].Get(obj);

                    }
                    n++;
                }

            }

            return answer;

        }

        
    }

    
}

interface IRequestHandler // обработчик запроса
{
    string RelatedRequest { get; } // по этому полю будем понимать что он может обработать такой запрос. тоже что и у запроса строка.
    IObjects Get(IObjects request); // обрабатывает запрос и возвращает объект
}


abstract class UserRequestHandler : IRequestHandler
{
    protected string handler;

    public string RelatedRequest
    {
        get { return handler; }
    }

    public abstract IObjects Get(IObjects request);
   

    protected UserList ReadUsers()
    {

        UserList list = null;

        string[] lines = System.IO.File.ReadAllLines(@"DATA_BASE\Users.txt");


        for (int i = 0; i < lines.Length; i++)
        {
            string[] tmp = lines[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

            if (list == null)
                list = new UserList(lines.Length, tmp.Length);

            for (int j = 0; j < list.R; j++) 
                list[i, j] = tmp[j];

        }


        return list;
    }
    protected void WrightUsers(UserList s)
    {
        string[] lines = new string[s.N];

        for (int i = 0; i < s.N; i++)
        {
            string tmp = "";
            for (int j = 0; j < s.R; j++) if (j != s.R - 1) tmp += s[i, j] + " "; else tmp += s[i, j];
            lines[i] = tmp;
        }

        System.IO.File.WriteAllLines(@"DATA_BASE\Users.txt", lines);

    }

    
}

class OtherUserRequestHandler : UserRequestHandler
{

    public OtherUserRequestHandler() { handler = "UserRequest";}

    public override IObjects Get(IObjects request)
    {

            UserRequest a;
            a = (UserRequest)request;

            UserList list = ReadUsers();
            list.changed = 0;
            WatchingUsers s = new WatchingUsers();
            string[] tmp = s.RetunUserRights(request.User.GetValue);

            if (!tmp[0].Equals("yes")) 
                list = null;
            a.execute(ref list); 

        if (list!=null)
            if (list.changed == 1)
            {
                WrightUsers(list);
                // записали пользователей
                // надо обновить страничку presence

                WatchingUsers t = new WatchingUsers();

                t.UpdatePresenceAfterUpdatingUsersTable(list);

            }


        

            return request;
    }
}

class GetUserHadler : UserRequestHandler
{
    

    public GetUserHadler() { handler = "GetUser"; }
    

    public override IObjects Get(IObjects request)
    {
        GetUser tmp = (GetUser)request;

        UserList basa = ReadUsers();

        // получили базу пользователей. первая часть с атрибутами. вторая с залогиненными в сети пользователями с их id шниками
        // с индекса i начинаются пользователи сейчас залогиненные

        // дальше начинается блок 

        // найдем пользователя, создадим ему id и вернем. а id запишем в базу
        // тока уникальный


        WatchingUsers t = new WatchingUsers();

        int answer = t.AddPresence(tmp.log, tmp.pass);
        if (answer == -1) return null; else

        tmp.SetReqestedUser(new UserID(answer));

        return tmp;

    }
}

class HandlersList
{
    private List<IRequestHandler> list;// тут хранятся все обработчики

    public List<IRequestHandler> GetList { get { return list; } }
    public int count { get { return list.Count; } }

    public void AddHandler(IRequestHandler handler)
    {
        if (list==null) list = new List<IRequestHandler>();

        list.Add(handler);
    }
}

class WatchingUsers
{
    // этот класс отвечает за работу с пользователями на стороне IDataBaseAccess
    // может вернуть список пользователей, список зарегистрированных пользователей
    // может записать эти списки обратно в файлы

    // также, может ответить на вопрос что позволено пользователю с таким id
    
    // через этот класс будет происходить обработка запросов. по id узнаем у него все про пользователя и решаем разрешено ли то или иное действие или нет

    private string UsersDataBase = @"DATA_BASE\Users.txt";

    private UserList GetUsers()
    { 
        UserList list = null;

        string[] lines = System.IO.File.ReadAllLines(UsersDataBase);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] tmp = lines[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

            if (list == null)
                list = new UserList(lines.Length, tmp.Length);

            for (int j = 0; j < list.R; j++)
                list[i, j] = tmp[j];
        }
        return list;

     }
    private void PutUsers(UserList s)
    {
        string[] lines = new string[s.N];

        for (int i = 0; i < s.N; i++)
        {
            string tmp = "";
            for (int j = 0; j < s.R; j++) if (j != s.R - 1) tmp += s[i, j] + " "; else tmp += s[i, j];
            lines[i] = tmp;
        }

        System.IO.File.WriteAllLines(UsersDataBase, lines);

    }

    public UserList users { get { return GetUsers(); } set { PutUsers(value); } }

    private string UserPresenceDataBase = @"DATA_BASE\PresentUsers.txt";

    private string[] GetPresence() 
    {  // вот это вернет строчки из базы
        
        string[] lines = System.IO.File.ReadAllLines(UserPresenceDataBase);
        return lines;
         
    }
    private void SetPresence(string[] lines)
    {
        System.IO.File.WriteAllLines(UserPresenceDataBase, lines);
    }
    public int AddPresence(string login, string password)
    { 
        // посмотрим совпадают ли логин-пароль, потом создадим уникальный айпишник и вернем, при этом запишем значение для пользователя в табличку

        UserList udb = GetUsers();

        bool exist = false;
        for (int i = 0; i < udb.N; i++)
            if (udb[i, 0] == password && udb[i, 1] == login) exist = true;

        if (exist)
        {
            string[] lines = GetPresence();
            // сейчас тут строчки: на первом месте имя пользователя, потом через пробелы числовые значения идентификаторов

            List<int> exitsInt = new List<int>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] tmp = lines[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                // tmp[0] это имя. остальное это числа
                for (int j = 1; j < tmp.Length; j++) exitsInt.Add(Convert.ToInt32(tmp[j]));
            }

            // получили значт все числа которые забиты

            int newID = 0;
            bool ok = false;
            while (!ok)
            {

                bool find = false;
                for (int i = 0; i < exitsInt.Count; i++)
                    if (exitsInt[i] == newID) find = true;
                if (find) newID++; else ok = true;

            }


            // нашли. теперь запишем его туда 

            for (int i = 0; i < lines.Length; i++)
            {
                string[] tmp = lines[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                if (tmp[0] == login)
                {
                    string[] tmp2 = new string [tmp.Length + 1];
                    for (int q = 0; q < tmp.Length; q++) tmp2[q] = tmp[q];
                        tmp2[tmp.Length] = Convert.ToString(newID);
                    
                    string t = "";
                    for (int d=0;d<tmp2.Length-1;d++) t+=tmp2[d]+" ";
                    t+=tmp2[tmp2.Length-1];

                    lines[i] = t;
                }
            }

            SetPresence(lines);

                return newID;


        } else return -1; // типа неподходит пароль то
    }

    public string[] ReturnUserFileRights(int ID)
    {
        string[] all = RetunUserRights(ID);
        return all[1].Split(new[] { ',' });
    }

    public string[] RetunUserRights(int ID)
    {  // по номеру пользователя нужно вернуть строку с правами пользователя

        string[] allPresence = GetPresence();

        string userName = null; // это имя пользователя

        for (int i = 0; i < allPresence.Length; i++)
        {
            string[] tmp = allPresence[i].Split(new[] { ' ' });
            for (int j = 1; j < tmp.Length; j++)
                if (Convert.ToInt32(tmp[j]) == ID)
                    userName = tmp[0];
        }

        string[] allUsers = System.IO.File.ReadAllLines(UsersDataBase);

        for (int i = 0; i < allUsers.Length; i++)
        {
            string[] tmp = allUsers[i].Split(new[] { ' ' });

            if (tmp[1] == userName)
            {
                // вот она строка со всей информацией

                string[] tmp2 = new string[tmp.Length - 2];
                for (int y = 2; y < tmp.Length; y++)
                    tmp2[y - 2] = tmp[y];
                return tmp2;
            }
        }

        return null;

    }

    public void RemoveUser(int ID)
    {
        string[] pres = GetPresence();

        for (int i = 0; i < pres.Length; i++)
        {
            string[] tmp = pres[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            for (int j = 1; j < tmp.Length; j++) if (Convert.ToInt32(tmp[j]) == ID)
                {

                    string[] tmp2 = new string[tmp.Length -1];
                    for (int u = 0; u < j; u++) tmp2[u] = tmp[u];
                    for (int u = j + 1; u < tmp.Length; u++) tmp2[u] = tmp[u];

                    string t = "";
                    for (int d = 0; d < tmp2.Length - 1; d++) t += tmp2[d] + " ";
                    t += tmp2[tmp2.Length - 1];

                    pres[i] = t;

                }
        }

        SetPresence(pres);



    }

    public void UpdatePresenceAfterUpdatingUsersTable(UserList newTable)
    { 
    
            // 

        string[] currentPresence = GetPresence();

        // оставить те строчки которые есть в newTable и добавить новые только с именем которых тут нет

        string[] existing_names = new string[newTable.N];
        for (int i = 0; i < newTable.N; i++)
            existing_names[i] = newTable.GetName(i);

        string[] newPres = new string[newTable.N];

        bool exist=false;
        string[] tmp;
        for (int i = 0; i < newTable.N; i++)
        {
            exist=false;
            for (int j = 0; j < currentPresence.Length; j++)
            {
                tmp = currentPresence[j].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                if (tmp[0].Equals(newTable.GetName(i)))
                {
                    newPres[i]=currentPresence[j];
                    exist=true;
                }
            }
            if (!exist) newPres[i]=newTable.GetName(i);
        }

        SetPresence(newPres);

    
        
    }


}

class WatchingFileRights
{ 
    // этот класс отвечает за работу с файликом прав доступа.

    private string FileAccessRights = @"DATA_BASE/FilesRools.txt";

    public string[] AllowedUserList(string file)
    { // возвращает список групп, которым позволено работать с данным файлом

        string[] lines = System.IO.File.ReadAllLines(FileAccessRights);
        // считали все строчки

        for (int i = 0; i < lines.Length; i++)
        {
            string[] tmp = lines[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

            if (tmp[0] == file)
            {
                string[] answer = new string[tmp.Length - 1];
                for (int j = 0; j < answer.Length; j++)
                    answer[j] = tmp[j + 1];

                return answer;
            }

        }


            return null;
    }

    public void Add(string path, string[] userGroups)
    { 
        // добавляет права доступа к файлику. перезыписывает предыдущие!

        string[] lines = System.IO.File.ReadAllLines(FileAccessRights);
        // считали все строчки

        int pos = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] tmp = lines[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            if (tmp[0] == path)
                pos = i; 
               
        }

        if (pos == -1)
        {
            // значит такого нет пока

            string[] newLines = new string[lines.Length + 1];
            for (int i = 0; i < lines.Length; i++)
                newLines[i] = lines[i];

            string tmp = "";
            tmp += path;
            for (int i = 0; i < userGroups.Length; i++) tmp += " " + userGroups[i];
            newLines[lines.Length] = tmp;

            System.IO.File.WriteAllLines(FileAccessRights, newLines);

        }
        else
        {
            string tmp = "";
            tmp += path;
            for (int i = 0; i < userGroups.Length; i++) tmp += " " + userGroups[i];

            lines[pos] = tmp;
            System.IO.File.WriteAllLines(FileAccessRights, lines);
        }

    }

    public void Remove(string path)
    { 
        // удаляет такую строку
        string[] lines = System.IO.File.ReadAllLines(FileAccessRights);
        int pos = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] tmp = lines[i].Split(new[] { ' ' });
            if (tmp[0] == path) pos = i;
        }

        if (pos != 1)
        {
            if (lines.Length == 1) System.IO.File.WriteAllText(FileAccessRights, "");
            else
            {
                string[] newLines = new string[lines.Length];
                for (int i = 0; i < lines.Length - 1; i++) newLines[i] = lines[i];
                newLines[pos] = lines[lines.Length - 1];
                System.IO.File.WriteAllLines(FileAccessRights, newLines);
            }
        }
    }

    public bool FileAllowedForUser(IUserID user, string file)
    {
        
        WatchingUsers users = new WatchingUsers();

        string[] filegroups = AllowedUserList(file); // тут группы у файла

        string[] usergroups = users.ReturnUserFileRights(user.GetValue);//



        // если в группах файла есть какаянить группа из групп пользователя - вернуть что "можно"

        for (int i = 0; i < filegroups.Length; i++)
        {
            for (int j = 0; j < usergroups.Length; j++)
                if (filegroups[i].Equals(usergroups[j])) return true;
        }


        return false;

    }

}

class LogOffHandler : UserRequestHandler
{
    public LogOffHandler() { handler = "LogOff"; }

    public override IObjects Get(IObjects request)
    {

        LogOff tmp = (LogOff)request;

        WatchingUsers t = new WatchingUsers();
        t.RemoveUser(tmp.deleted.GetValue);

        return null;
    }
}

class FileListRequestHandler : IRequestHandler
{// обработчик для запроса на списко доступных файлов и директорий


    private string handler = "FileListRequest";
    private string Treeroot = @"DATA_BASE\Files"; // начальная папка

    //private WatchingFileRights files; // тут можно посмотреть для каждого файла какие у него группы
    //private  WatchingUsers users; // тут можно посмотреть права пользователей 4 параметр - через запятую группы

    private IUserID currentUser;

    public string RelatedRequest
    {
        get { return handler; }
    }
    


    public IObjects Get(IObjects request)
    {

        FileListRequest req = (FileListRequest)request;
        // получили запрос. там внутри есть пользователь и дерево нужно туда для него засунуть

        // вообще, для файлов установлена группа, которая может его смотреть. смотрим у пользователя какая категория есть.
        // если совпадает - можем ему показать этот файлик
        // если в директории нет файлов, которые может смотреть этот пользователь - и директорию ему не показываем


        //files = new WatchingFileRights();
        //users = new WatchingUsers();
        currentUser=request.User;
        
        // есть начальная директория. есть пользователь, есть набор прав для него, есть для каждого файла набор групп которым можно его читать
        FileTree answer = new FileTree();
        // сюда запишем дерево файлов
        answer.FileList = MakeTree(Treeroot);

        req.answer = answer;

        return req;
    }

    private Directorys MakeTree(string root)
    {
        // нужно заполнить дерево. (пока возвращает все.)

        Directorys curDir = new Directorys();
        curDir.path = root;
        curDir.files = new List<string>();

        curDir.filesRights = new List<string[]>();

        WatchingFileRights wfr = new WatchingFileRights();
  
        string[] curFiles =  Directory.GetFiles(root,"*txt");
        for (int i = 0; i < curFiles.Length; i++)
        {
            if (wfr.FileAllowedForUser(currentUser, curFiles[i]))
            curDir.files.Add(curFiles[i]);

            string[] tmp=wfr.AllowedUserList(curFiles[i]);


            if (tmp != null)
            {
                string[] tmp2 = new string[tmp.Length + 1];
                tmp2[0] = curFiles[i]; for (int a = 0; a < tmp.Length; a++) tmp2[a + 1] = tmp[a];

                // тут проверим доступен ли этот файл текущему пользователю
                if (wfr.FileAllowedForUser(currentUser, tmp2[0]))
                curDir.filesRights.Add(tmp2);
            }
        }

        string[] curDirs = Directory.GetDirectories(root);

        curDir.subdirectorys = new List<Directorys>();
        for (int i = 0; i < curDirs.Length; i++)
        { 
            curDir.subdirectorys.Add(MakeTree(curDirs[i]));
        }


        return curDir;

    }


}

class FileGetRequestHandler : IRequestHandler
{

    private string handler = "FileGetRequest";
    private string Treeroot = @"DATA_BASE\Files"; // начальная папка


    public string RelatedRequest
    {
        get { return handler; }
    }

    public IObjects Get(IObjects request)
    {
        FileGetRequest req = (FileGetRequest)request;

        req.FileBuffer = ReadFile(req.ReqPath);

        return req;

    }

    private string[] ReadFile(string path)
    {

        string[] lines = System.IO.File.ReadAllLines(path);
        return lines;
    }
}

class FileSaveRequestHandler : IRequestHandler
{
    private string handler = "FileSaveRequest";
    private string Treeroot = @"DATA_BASE\Files"; // начальная папка


    public string RelatedRequest
    {
        get { return handler; }
    }

    public IObjects Get(IObjects request)
    {
        FileSaveRequest t = (FileSaveRequest)request;

        // проверим есть ли права у этого пользователя на запись в эту папку.
        WatchingFileRights wfr = new WatchingFileRights();
        if (wfr.FileAllowedForUser(t.User,t.ReqPath))
        SaveFile(t.ReqPath, t.FileBuffer);

        return null;
    }

    private void SaveFile(string path,string[] buf)
    {
        System.IO.File.WriteAllLines(path, buf);
    }


}

class CreateDirectoryRequestHandler : IRequestHandler
{

    private string handler = "CreateDirectoryRequest";
    private string Treeroot = @"DATA_BASE\Files"; // начальная папка

    public string RelatedRequest
    {
        get { return handler; }
    }

    public IObjects Get(IObjects request)
    {
        CreateDirectoryRequest t = (CreateDirectoryRequest)request;
        CreateNewDirectory(t.directory);
        return t;
    }

    private void CreateNewDirectory(string dir)
    {
        if (!dir.Contains(Treeroot)) dir = Treeroot +"\\"+ dir; 

       Directory.CreateDirectory(dir);
    }
}
class CreateFileRequestHandler : IRequestHandler
{

    private string handler = "CreateFileRequest";
    private string Treeroot = @"DATA_BASE\Files"; // начальная папка

    public string RelatedRequest
    {
        get { return handler; }
    }

    public IObjects Get(IObjects request)
    {
        CreateFileRequest t = (CreateFileRequest)request;
        CreateNewFile(t.newfile);
        // создали файл. нужо добавить права в файл прав
        WatchingFileRights wfr = new WatchingFileRights();
        wfr.Add(t.newfile, t.newrights);
        
        return t;
    }

    private void CreateNewFile(string file)
    {
        // файл создадим тут.
        if (!System.IO.File.Exists(file))
        System.IO.File.Create(file);
       
    }
}

class DeleteFileRequestHandler : IRequestHandler
{

    private string handler = "DeleteFileRequest";
    private string Treeroot = @"DATA_BASE\Files"; // начальная папка

    public string RelatedRequest
    {
        get { return handler; }
    }

    public IObjects Get(IObjects request)
    {
        DeleteFileRequest t = (DeleteFileRequest)request;
        if (t.deletefile.Contains(".txt"))
        {
            WatchingFileRights wfr = new WatchingFileRights();

            if (wfr.FileAllowedForUser(t.User, t.deletefile))
            DeleteFile(t.deletefile);
        }
        else DeleteDirectory(t.deletefile);
        

        return t;
    }

    private void DeleteFile(string file)
    {
        System.IO.File.Delete(file);
        // вместе с удалением файла. нужно удалить права доступа
        WatchingFileRights wfr = new WatchingFileRights();
        wfr.Remove(file);
    }

    private void DeleteDirectory(string file)
    {
        // удаляем директорию только если там нет файлов!
        if(Directory.GetFiles(file).Length==0)
        Directory.Delete(file, true);
    }
}


