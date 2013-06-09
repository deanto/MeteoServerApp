using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeteoServer.Objects
{

    /// <summary>
    /// у каждого объекта будет свой ID. у нас будет строка. 
    /// будет использоваться при распаковке объекта. узнаем кто он-и преобразовываем к тому типу что нужно.
    /// дальше зная что это такое работаем нормально с ним.
    /// </summary>
    public interface IObjID
    { 
        string ToString();
    }
    class StringID : IObjID
    {
        private string MyID;
        public StringID(string s) { MyID = s; }

        public override string ToString()
        {
            return MyID;
        }
    }



    /// <summary>
    /// вот общий для всех интерфейс объекта. 
    /// каждый объект может сказать кто он. дальше мы создаем объект такого класса и работаем уже с известным.
    /// </summary>
    public interface IObjects
    {
        IObjID ID { get; } // каждый объект может сказать ЧТО он такое. 
        IUserID User { get; } // какой user работает
        
    }




    /// <summary>
    ///  общий класс для всех запросов связанных с пользователями
    /// </summary>
    public abstract class UserRequest : IObjects
    {
        private string myID = "UserRequest";
        private IUserID user;

        public IUserID setUser { set { user = value; } }

        public IObjID ID
        {
            get { return new StringID(myID); }
        }

        public abstract void execute(ref UserList s);


        public IUserID User
        {
            get { return user; }
        }
    }

    class CreateNewUser : UserRequest
    {
        private string[] newuser;

        public CreateNewUser(string[] user) { newuser = user; }

        public override void execute(ref UserList s)
        {
            ///работаю с предоставленным s. чо хочу то и делаю. а s сформировала компонента доступа к бд. и функции она выполняет сохранения его выполняет бд
            ///    если у пользователся есть права - то и формируется полный список. если не все- то там пусто например.

            if (s != null)
            {

                bool flag = true;
                for (int i = 0; i < s.N; i++)
                    if (s.GetName(i).Equals(newuser[1]))
                    {
                        // если равны
                        flag = false;
                    }
                if (flag)
                {
                    // тогда добавляем пользователя

                    UserList my = new UserList(s.N + 1, s.R);
                    my.CopyBaseFrom(s);

                    for (int i = 0; i < my.R; i++) my[my.N - 1, i] = newuser[i];

                    s = my;

                    s.changed = 1;

                }
            } 
        
        }
    }

    
    class ChangeUser : UserRequest
    {
        private string[] newvalue;
        public ChangeUser(string[] changes)
        {
            newvalue = changes;
        }

        public override void execute(ref UserList s)
        {
            for (int i = 0; i < s.N; i++)
                if (s[i, 1] == newvalue[1])
                {
                    for (int j = 0; j < s.R; j++)
                        s[i, j] = newvalue[j];
                }

                s.changed = 1;
        }
    }

    class DeleteUser : UserRequest
    {
        private string delete;

        public DeleteUser(string name) { delete = name; }
        public override void execute(ref UserList s)
        {

            if (s != null)
            {
                bool exist = false;
                for (int i = 0; i < s.N; i++)
                    if (s[i, 1] == delete)
                    {
                        for (int j = 0; j < s.R; j++)
                            s[i, j] = s[s.N - 1, j];
                        exist = true;

                    }
                if (exist)
                {
                    s.changed = 1;

                    UserList my = new UserList(s.N - 1, s.R);
                    for (int i = 0; i < my.N; i++)
                        for (int j = 0; j < my.R; j++)
                            my[i, j] = s[i, j];

                    s = my;
                    s.changed = 1;
                }
            }
        }
    }

    class WatchUsers : UserRequest
    {
        public UserList myUserList;

        public override void execute(ref UserList s)
        {
            myUserList = s;
        }
    }


    class LogOff : IObjects
    {
        private string myID = "LogOff";

        public IObjID ID
        {
            get { return new StringID(myID); }
        }

        public IUserID User
        {
            get { return null; }
        }

        private IUserID deletethis;

        public LogOff(IUserID user)
        {
            deletethis = user;
        }

        public IUserID deleted { get { return deletethis; } }


    }


    class GetUser : IObjects // специальный запрос на получение идентификатора пользователя по логину и паролю
    {
        private string myID = "GetUser";

        private IUserID RequestedUserID=null;

        public IObjID ID
        {
            get { return new StringID(myID); }
        }

        public IUserID User
        {
            get { return null; }
        }

        private string mylogin, mypassword;
        public string log{get{return mylogin;}}
        public string pass{get{return mypassword;}}

        public GetUser(string login, string password)
        { 
            mylogin=login;
            mypassword=password;
        }

        public void SetReqestedUser(IUserID answer)
        {
            RequestedUserID = answer;
        }

        public IUserID Answer { get { return RequestedUserID; } }


    }


    /// <summary>
    ///  этот класс является представлением списка пользователей в системе.
    /// </summary>
    public class UserList
    {


        private string[,] myUsers;// просто двумерный массив строк. 
        private int count;
        private int rank;// размерность список свой знает


        public UserList(int N, int R) { count = N; rank = R; myUsers = new string[N, R]; }

        public int changed;// флаг устанавливается если база поменялась

        public int N { get { return count; } }
        public int R { get { return rank;  } }

        public string this[int index, int index2] { get { return myUsers[index, index2]; } set { myUsers[index, index2] = value; } }
 

        public void CopyBaseFrom(UserList s)
        { 
            for (int i=0;i<s.N;i++)
                for (int j = 0; j < s.R; j++)
                {
                    this.myUsers[i, j] = s.myUsers[i, j];
                }
        }



        public string GetName(int index) { return myUsers[index, 1]; }

    }


    class PresentUsers
    {
        private string[] users; // список пользователей сейчас зарегистрированных
        private string[] id; // тут для каждого пользователя строка где через пробелы хранятся числовые значения id-шников

        public PresentUsers(int count) { users = new string[count]; id = new string[count]; }

        public string GetUser(int index) { return users[index]; }
        public string GetID(int index) { return id[index]; }

        public void PutUser(string user) { }


    }




    /// <summary>
    ///  это идентификатор пользователя. присутствует во всех запросах и по нему можно понять что за пользователь работает
    /// </summary>
    /// 
   public interface IUserID
    {
        int GetValue { get; }
    }


    class UserID:IUserID
    {
        private int user;// числовой идентификатор пользователя
        
        // будет разрешено выполнить только  компоненте управление пользователями
        public UserID(int ID) {  user = ID;  }

        public int GetValue { get { return user; } }

    }





    /// <summary>
    ///  класс предоставляет оболочку для данных
    /// </summary>
  abstract class DataBuf
    {
        private string buf;// тут хранится информация файла
        private string attributes; // тут атрибуты файла отдельно
        private string link; // это путь к файлу(ну или какая либо ссылка на него)

        public abstract string GetBuf();  // объект сам решает как показать свой буффер
        public abstract string GetAttributes() ;
        public abstract string Link();

    }

  abstract class FileRequest : IObjects
  {
      private string myID = "FileRequest";
      private IUserID user;

      public IUserID setUser { set { user = value; } }

      public IObjID ID
      {
          get { return new StringID(myID); }
      }

      public abstract void RePack( string data );
      public abstract string Pack();


      public IUserID User
      {
          get { return user; }
      }
  }

  class DirectoryRequest : FileRequest
  {
      

      public override void RePack(string data)
      {
          throw new NotImplementedException();
      }

      public override string Pack()
      {
          throw new NotImplementedException();
      }
  }



}
