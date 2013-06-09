using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MeteoServer.Objects;
using MeteoServer.Components.DataBaseAccess;


namespace MeteoServer.Components.FileManagement
{
    public class FileManager
    {

        public FileTree GetFiles(IUserID user) // дай мне список файлов. я - user
        {
            IDataBaseAccess db = Fabric.GetDataBaseAccess();
            FileListRequest t = new FileListRequest();
            t.SetUser(user);
            IObjects send = t;
            FileListRequest answer = (FileListRequest)db.SEND(ref send);
            if (answer == null) return null;
            return answer.answer;
        }
        public string[] GetThisFile(IUserID user,string path) // дай мне вот этот файл. путь я взял из предоставленного дерева файлов
        {
            FileGetRequest req = new FileGetRequest();
            req.ReqPath = path;
            req.SetUser = user;
            IDataBaseAccess db= Fabric.GetDataBaseAccess(); 

            IObjects send = req;
            req = (FileGetRequest)db.SEND(ref send);

            if (req.FileBuffer != null)
                return req.FileBuffer;
            else return null;
        }

        public void SaveThisFile(IUserID user, string path, string[] buffer)
        { 
            // сохранить файл

            FileSaveRequest t = new FileSaveRequest();
            t.FileBuffer = buffer;
            t.ReqPath = path;
            t.SetUser = user;

            IObjects send = t;
            IDataBaseAccess db = Fabric.GetDataBaseAccess(); 
            db.SEND(ref send);

        }

        public void CreateNewDirectory(IUserID user,string newDir)
        {
            CreateDirectoryRequest t = new CreateDirectoryRequest();
            t.directory = newDir;
            t.SetUser = user;

            IObjects send = t;
            IDataBaseAccess db = Fabric.GetDataBaseAccess();
            db.SEND(ref send);
        }

        public void CreateNewFile(IUserID user, string newFile,string[] rights)
        {
            CreateFileRequest t = new CreateFileRequest();
            t.newfile = newFile;
            t.SetUser = user;
            t.newrights = rights;

            IObjects send = t;
            IDataBaseAccess db = Fabric.GetDataBaseAccess();
            db.SEND(ref send);
        }

        public void Delete(IUserID user, string path)
        {
            DeleteFileRequest t = new DeleteFileRequest();
            t.deletefile = path;
            t.SetUser = user;

            IObjects send = t;
            IDataBaseAccess db = Fabric.GetDataBaseAccess();
            db.SEND(ref send);
        }

    }






    class FileListRequest:IObjects
    { // этот класс обозначает запрос на получение списка доступных директорий и файлов


        private FileTree requestedFileList; // тут список файлов
        public FileTree answer { set { requestedFileList = value; } get { return requestedFileList; } }


        private IUserID user;
        

        public void SetUser(IUserID who)
        {
            user = who;
        }

        private string ReqID = "FileListRequest";

        public IObjID ID
        {
            get { return new StringID(ReqID); }
        }

        public IUserID User
        {
            get { return user; }
        }
    }

    class FileGetRequest:IObjects
    {

        private IUserID user;
        public IUserID SetUser { set { user = value; } get { return user; } }

        private string ReqID = "FileGetRequest";
        public IObjID ID
        {
            get { return new StringID(ReqID); }
        }

        public IUserID User
        {
            get { return user; }
        }

        private string path;
        public string ReqPath { set { path = value; } get { return path; } }

        private string[] fileBuf; // буффер для записи содержимого файла

        public string[] FileBuffer { get { return fileBuf; } set { fileBuf = value; } }

    }

    class FileSaveRequest : IObjects
    {
        private IUserID user;
        public IUserID SetUser { set { user = value; } get { return user; } }

        private string ReqID = "FileSaveRequest";
        public IObjID ID
        {
            get { return new StringID(ReqID); }
        }

        public IUserID User
        {
            get { return user; }
        }

        private string path;
        public string ReqPath { set { path = value; } get { return path; } }

        private string[] fileBuf; // буффер для записи содержимого файла

        public string[] FileBuffer { get { return fileBuf; } set { fileBuf = value; } }
    }

    class CreateDirectoryRequest : IObjects
    {
        private IUserID user;
        public IUserID SetUser { set { user = value; } get { return user; } }

        private string ReqID = "CreateDirectoryRequest";
        public IObjID ID
        {
            get { return new StringID(ReqID); }
        }

        public IUserID User
        {
            get { return user; }
        }

        private string dir;// что нужно создать
        public string directory { get { return dir; } set { dir = value; } }

        

    }


    class CreateFileRequest : IObjects
    {
        private IUserID user;
        public IUserID SetUser { set { user = value; } get { return user; } }

        private string ReqID = "CreateFileRequest";
        public IObjID ID
        {
            get { return new StringID(ReqID); }
        }

        public IUserID User
        {
            get { return user; }
        }

        private string file;// что нужно создать
        public string newfile { get { return file; } set { file = value; } }

        private string[] rights;
        public string[] newrights { get { return rights; } set { rights = value; } }

    }

    class DeleteFileRequest : IObjects
    {
        private IUserID user;
        public IUserID SetUser { set { user = value; } get { return user; } }

        private string ReqID = "DeleteFileRequest";
        public IObjID ID
        {
            get { return new StringID(ReqID); }
        }

        public IUserID User
        {
            get { return user; }
        }

        private string file;// что нужно удалить
        public string deletefile { get { return file; } set { file = value; } }

    }

    class FileAccessChangeRequest
    { 
    
        // этот запрос на редактирование правил доступа к файлам (изменение атрибутов доступа)
        
    }



    public class FileTree
    { 
        // этот класс хранит в себе "снимок" доступной файловой системы. пути и имена файлов

        private Directorys root; // верхняя папка (все дерево доступной файловой системы)
        public Directorys FileList { set { root = value; } get { return root; } }


    }

    public class Directorys
    {// это "лист" в дереве файловой структуры
     
      public  string path; // путь к директории
      public  List<Directorys> subdirectorys; // список поддиректорий (тоже такие же узлы)
      public  List<string> files; // список файлов в этой директории
      public List<string[]> filesRights;// список в котором на первом месте имя файла а потом группы которым доступен этот файл
        
    }


}
