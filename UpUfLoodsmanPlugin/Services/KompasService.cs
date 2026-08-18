using Kompas6API5;
using Kompas6Constants;
using KompasAPI7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thread = System.Threading.Thread;

namespace UpUfLoodsmanPlugin.Services
{
    public class KompasService
    {

        KompasObject kompasObjectAPI5;
        IApplication KompasObjectAPI7;
        IKompasDocument doc2D;
        public KompasObject ConnectToKompasApp()
        {
            
            try
            {
                kompasObjectAPI5 = (KompasObject)Marshal.GetActiveObject("KOMPAS.Application.5");

                return kompasObjectAPI5;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public KompasObject StartKompas()
        {

            Type t = Type.GetTypeFromProgID("KOMPAS.Application.5");
            kompasObjectAPI5 = (KompasObject)Activator.CreateInstance(t);

            return kompasObjectAPI5;
        }

        public async Task<bool> ConnectOrStartKompas()
        {
            
            KompasObject kompasObjectAPI5 = ConnectToKompasApp();

            if (kompasObjectAPI5 == null)
            {
                kompasObjectAPI5 = StartKompas();
            }

            if (kompasObjectAPI5 != null)
            {
                kompasObjectAPI5.Visible = true;
                return true;
            }
            return false;
        }

        public KompasObject TestMethode()
        {
            return kompasObjectAPI5;
        }

        public async Task<bool> OpenDocument(string Path)
        {            
            KompasObjectAPI7 = kompasObjectAPI5.ksGetApplication7();

            KompasObjectAPI7.HideMessage = ksHideMessageEnum.ksHideMessageYes;
            bool docVisible = true;
            doc2D = KompasObjectAPI7.Documents.Open(Path, docVisible, false);
            if (doc2D != null)
            {
                return true;
            }
            return false;
        }

    }
}
