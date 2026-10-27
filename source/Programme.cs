using System;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Misterword")]
[assembly: System.Reflection.AssemblyProduct("Misterword")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]

namespace Misterword
{
    static class Programme
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Fenetre());
        }
    }
}
