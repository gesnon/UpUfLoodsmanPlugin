using Kompas6API5;
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

        public bool ConnectOrStartKompas()
        {
            Thread.Sleep(5000);
            return true;
            KompasObject kompasObjectAPI5 = ConnectToKompasApp();

            if (kompasObjectAPI5 == null)
            {
                kompasObjectAPI5 = StartKompas();
            }

            if (kompasObjectAPI5 != null)
            {
                return true;
            }
            return false;
        }

        public KompasObject TestMethode()
        {
            return kompasObjectAPI5;
        }

    }
}
