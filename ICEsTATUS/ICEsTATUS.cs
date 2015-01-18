using System;
using System.Windows.Forms;

namespace ICEsTATUS
{
    class ICEsTATUS
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new Systray());
        }
    }
}
