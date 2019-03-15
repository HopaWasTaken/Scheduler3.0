using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Scheduler3._0
{
    class Class1
    {
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public Class1 () {

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 1, 0);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (MainWindow.ListOfActivities.Count > 0)
            {
                int time = (DateTime.Now.Hour * 100) + DateTime.Now.Minute;
                //Console.WriteLine(time);
                foreach (MainWindow.Activity activity in MainWindow.ListOfActivities)
                {
                    Console.WriteLine(MainWindow.times[activity.StartTime]);
                    if (time == MainWindow.times[activity.StartTime])
                    {
                        NotifyIcon nIcon = new NotifyIcon();
                        nIcon.Visible = true;
                        nIcon.Icon = SystemIcons.Information;
                        nIcon.BalloonTipTitle = "Change tasks";
                        nIcon.BalloonTipText = activity.Name.ToString();
                        nIcon.ShowBalloonTip(5000);
                    }
                }

            }
        }
    }
}
