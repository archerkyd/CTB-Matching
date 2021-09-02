using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace BaslerGIGE_Test1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool IsRun;
            Mutex m = new Mutex(true, "PylonCamera", out IsRun);
            if (IsRun)
            {
#if DEBUG

                Environment.SetEnvironmentVariable("PYLON_GIGE_HEARTBEAT", "300000" /*ms*/);
#endif

                try
                {



                    PylonC.NET.Pylon.Initialize();

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Camera_Form());
                    m.ReleaseMutex();
                }
                catch (Exception es)
                {
                    PylonC.NET.Pylon.Terminate();
                    MessageBox.Show(es.Message);
                    throw;
                }
                PylonC.NET.Pylon.Terminate();
            }
            else
            {
                MessageBox.Show("Package program is already running!", "Error", MessageBoxButtons.OK);
                return;
            }

        }

    }
}

