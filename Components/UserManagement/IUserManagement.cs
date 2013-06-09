using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MeteoServer.Components.DataBaseAccess;
using MeteoServer.Objects;



namespace MeteoServer.Components.UserManagement
{
   public  interface IUserManagement
    {
        IUserID GetProfile(); // получить профайл для работы
        int UserAdd(IUserID user, string[] newuser); // добавить нового пользователя
        int ChangeUser(IUserID user, string[] changes); // поменять пользователя
        int DeleteUser(IUserID user, string delete); // удалить пользователя
        UserList WatchUsers(IUserID user); // посмотреть базу
        void LogOff(IUserID user);

        IDataBaseAccess DB { set; }

    }

   public class UserManagement : IUserManagement
    {
        // общую функциональность вынести отдельно

        private IDataBaseAccess db; // интерфейс базы данных с которым работаем
        public IDataBaseAccess DB { set { db = value; } }

        public IUserID GetProfile()
        {

            LoginForm login = new LoginForm();
            login.ShowDialog();

            return login.answer;
        }

        public int UserAdd(IUserID user, string[] newuser)
        {

            CreateNewUser req = new CreateNewUser(newuser);
            req.setUser = user;
            IObjects tmp = req;

                db.SEND(ref tmp);
                return 1;
            
        }

        public int ChangeUser(IUserID user, string[] changes)
        {

            ChangeUser req = new ChangeUser(changes);
            req.setUser = user;
                IObjects send = req;
                db.SEND(ref send);

                return 1;
            
        }

        public int DeleteUser(IUserID user, string delete)
        {

            
            DeleteUser req = new DeleteUser(delete);
            req.setUser = user;

            IObjects send = req;
                db.SEND(ref send);

                return 1;
            

        }

        public UserList WatchUsers(IUserID user)
        {

            WatchUsers req = new WatchUsers();
            req.setUser = user;
            IObjects s = (IObjects)req;
            WatchUsers t = (WatchUsers)db.SEND(ref s);
                if (t == null) return null;
                return t.myUserList;
            
        }

        public void LogOff(IUserID user)
        {
            Objects.LogOff req = new LogOff(user);
            IObjects s = (IObjects)req;
            db.SEND(ref s);
        }
    }


}
