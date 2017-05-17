using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iSql.Notification.Mesenger
{
    static class Notifier
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new NotificationMessager();
            form.WindowState = FormWindowState.Minimized;
            form.Notify("Greeting", "The Self Service SQL notification center is up and running.");
            Application.Run( form );
        }
    }
}
