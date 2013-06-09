using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MeteoServer.Objects;


namespace MeteoServer.Components.FileEditor
{
    class EditFiles
    {

        private IUserID user;
        public IUserID setUser { set { user = value; } get { return user; } }

        private string[] fileBuffer;
        public string[] Buffer { get { return fileBuffer; } set { fileBuffer = value; } }


        public void WinFormsEdit()
        { // для серверной части

            // тут открываем окошечко. в нем рисуем буффер -меняем и отдаем обратно

            WinFormEditor e = new WinFormEditor();
            e.FILEBuf = fileBuffer;
            e.ShowDialog();

            fileBuffer = e.FILEBuf;

        }






    }
}
