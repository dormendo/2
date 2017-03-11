using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestsForEfficiency
{
	// This test app compares various methods of implementing file I/O code to determine which is the most efficient.
  // It also provides the capability to unit test the WinFileIO class, which provides very efficient I/O functions
  // to read and write data to and from files.
  //
  // Written by Robert G. Bryan in Feb, 2011.
  //
  public partial class MainForm : Form
	{
    // This clss handles the user interface with the form.
    public WinFileIOUnitTests WFIOUT;   // Object used to unit test the WinFileIO class functions.
		public FileEfficientTests EffTests; // This object is used to determine the most efficient I/O functions.
    //
		public MainForm()
		{
			InitializeComponent();
      // Pass this form object to the FileEfficientTests and WinFileIOUnitTests objects so that they can
      // update the status directly.
			EffTests = new FileEfficientTests(this);
      WFIOUT = new WinFileIOUnitTests(this);
		}

		public void DisplayMsg(String Msg)
		{
			// This function displays a message to the status window.
			StatusMsg.AppendText(Msg + "\r\n");
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// This event is called when the form has been selected to close.
      // Clean up the unmanaged objects.
			EffTests.Dispose();
      WFIOUT.Dispose();
		}

    private void btnRunAllFileTests_Click(object sender, EventArgs e)
    {
      EffTests.TestReadingFiles();
      EffTests.TestWritingFiles();
    }

    private void btnReadFile_Click(object sender, EventArgs e)
		{
			EffTests.TestReadingFiles();
		}

		private void btnWriteFile_Click(object sender, EventArgs e)
		{
			EffTests.TestWritingFiles();
		}

    private void btnWinFileIOUnitTests_Click(object sender, EventArgs e)
    {
      WFIOUT.RunAllUnitTests();
    }
	}
}