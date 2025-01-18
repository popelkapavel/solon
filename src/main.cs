using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace solon {
    static class main {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arg){            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new fMain(arg));
        }
    }
}
