using System;
using System.Windows.Forms;
using WebView2CustomTitleBarApp; 

namespace WebView2CustomTitleBarApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); 
        }
    }
}
