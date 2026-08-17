using Kompas6API5;
using KompasAPI7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UpUfLoodsmanPlugin.Services
{
    public class KompasService
    {


        public KompasObject ConnectToKompasApp()
        {
            KompasObject kompasObjectAPI5;
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
            KompasObject kompasObjectAPI5 = (KompasObject)Activator.CreateInstance(t);

            return kompasObjectAPI5;
        }

        public KompasObject ConnectOrStartKompas()
        {
            KompasObject kompasObjectAPI5 = ConnectOrStartKompas();

            if (kompasObjectAPI5 == null)
            {
                kompasObjectAPI5 = StartKompas();
            }

            return kompasObjectAPI5;
        }

    }
}
