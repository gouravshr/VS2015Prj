using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.IO;
using System.Configuration;

namespace iSql.Notification.Mesenger
{
    /// <summary>
    /// Needs a simple way to notify the DBA team when new dba manual valdiation requests being submitted recently, and they can click and login to the main site 
    /// to move the workflow forward, either accetp or reject the request.  
    /// Ideally this can be implemented as WebSockets for pushing model and efficiency (and may have extra layers upon WebSockets). As of now, we temporarily implemented as pull request. 
    /// Based on test, it should work with general default proxy of IE.
    /// </summary>
    public partial class NotificationMessager : Form
    {
        public NotificationMessager()
        {
            InitializeComponent();
            // no need to show the ugly form which is just a container, just minimize it.
            //this.WindowState = FormWindowState.Minimized;
            //this.SizeChanged += NotificationMessager_SizeChanged;
            this.Visible = false;
            this.notifyIcon.Visible = true;
            this.notifyIcon.BalloonTipClicked += notifyIcon_BalloonTipClicked;

            // we don't care about left or right click at this point
            this.notifyIcon.MouseUp += notifyIcon_MouseUp;

            timer.Interval = 60000;
            timer.Tick += timer_Tick;
            timer.Enabled = true;
            timer.Start();

            // all URLs are configurable now,  which makes local and stging tests easier.
            _dbaSrvUrl = ConfigurationManager.AppSettings["DbaNotifyURL"];   
            _sssUrl = ConfigurationManager.AppSettings["SssHome"];   
           
        }

        protected string _dbaSrvUrl;
        protected string _sssUrl;

        protected DateTime _lastNoticeTime = DateTime.MinValue;

        #region override methods
        protected override void SetVisibleCore(bool value)
        {
            if (!IsHandleCreated)
            {
                CreateHandle();
            }
            //we want to hide it anyway, the WindowState is not enough
            value = false;
            base.SetVisibleCore(value);
        }

        #endregion 

        #region helpers

        protected string URLWithTimeStamp
        {
            get
            {
                return _dbaSrvUrl + "?since=" + _lastNoticeTime;
            }
        }
        #endregion 

        #region timer and web fetch
        async void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timer.Stop();

                HttpWebRequest req = (HttpWebRequest)  HttpWebRequest.Create( URLWithTimeStamp );

                // we probably don't want to keep-alive at this point 
                req.KeepAlive = false;

                var ms = new MemoryStream();
                
                // we know it is HttpWebResposne here 
                using (HttpWebResponse rsp = (HttpWebResponse) await req.GetResponseAsync())
                {
                    // only care when we succeed, ignore all excepitons and errors for now
                    //TODO: logging
                    if (rsp.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = rsp.GetResponseStream())
                        {
                            await stream.CopyToAsync(ms);
                        }
                    }

                    string msg = Encoding.UTF8.GetString(ms.ToArray());

                    // remember the latest update timestamp
                    _lastNoticeTime = DateTime.Now;

                    Notify("Update:", msg);
                }

            }
            catch (Exception ex)
            {
                //TODO: logging errors but not going to further process it
            }
            finally
            {
                timer.Start();
            }
        }

        #endregion 

        /*
        void NotificationMessager_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.notifyIcon.Visible = true;
                this.notifyIcon.BalloonTipClicked += notifyIcon_BalloonTipClicked;
                Thread.Sleep(1000);
                notifyIcon.ShowBalloonTip(2000, "DBA notificaiton", "A new manual request is ready to review.", ToolTipIcon.Info);
            }
        }
*/
        public void Notify(string title, string message, int miniSeconds=2000, ToolTipIcon iconStyle = ToolTipIcon.Info)
        {
            notifyIcon.ShowBalloonTip(miniSeconds, title, message, iconStyle);
        }


        #region balloon tip 
        void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            ProcessStartInfo pinfo = new ProcessStartInfo(_sssUrl);
            Process.Start(pinfo);
        }

        #endregion 

        #region context menu
        
        void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            //NOTE:  it is quite tricky to show context menu.
            // e.Location is always (0,0), so it works but always place the menu in the wrong place.
            //      contextMenuStrip.Show( e.Location);
            // And we can not pass notifyIcon as a Control container, it is from different base class and won't even compile.
            //      contextMenuStrip.Show( notifyIcon, e.Location);
            // So have to go with reflection........................ sucks.
            // Just another bad design from MS.
            MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(notifyIcon, null);
        }
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This applicaiton is written to help DBAs to get short and quick notification from Self Service SQL application.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        #endregion 
    }
}
