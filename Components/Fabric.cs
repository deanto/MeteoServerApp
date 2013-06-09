using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MeteoServer.Components.DataBaseAccess;
using MeteoServer.Components.UserManagement;
using MeteoServer.Components.FileManagement;

namespace MeteoServer.Components
{
    public static class Fabric
    {

       static public IDataBaseAccess GetDataBaseAccess() { return new DataBase(); }
       static public IUserManagement GetUserManagement() 
       {
           UserManagement.UserManagement t = new UserManagement.UserManagement();
           t.DB = GetDataBaseAccess();
           return t; 
       }
       static public FileManager GetFileManager() {return new FileManager(); }

        // так все компоненты должен отдавать
    }

}
