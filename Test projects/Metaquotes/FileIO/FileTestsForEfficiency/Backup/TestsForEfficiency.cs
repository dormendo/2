using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestsForEfficiency
{
	static class TestsForEfficiency
	{
    // Written by Robert G. Bryan in Feb, 2011.
    //
    /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
