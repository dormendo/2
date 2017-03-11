using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Win32FileIO;

namespace TestsForEfficiency
{
  public partial class WinFileIOUnitTests
  {
    // This class tests out the WinFileIO class methods to make sure they are working properly.
    //
    // Written by Robert G. Bryan in Feb, 2011.
    //
    private MainForm TheForm;					  // Form where output msgs are to be displayed.
    private String TestFileName;        // Path & name of file to be tested.
    private String TestFileName2;       // Path & name of file used for the write tests.
    private WinFileIO WFIO;             // The object that implements the windows readfile and writefile system functions.
    private byte[] ByteBuf;             // Buffer used to read in data from the file.
    private byte[] ByteBufVer;          // Buffer used to verify that the read & write functions worked properly.
    private int VerBytesRead;           // Number of bytes read into the verification buffer.
    private const int BufSize = 1000000;  // Size of file I/O buffers.
    private const int BlockSize = 65536;  // Size of block used to read in the verification file.
    //
    public WinFileIOUnitTests(MainForm theForm)
    {
      TheForm = theForm;
      Init();
    }

    private void Init()
    {
      // This function initializes the WinFileIO object and reads in a file to be used for testing and verification.
      String DataPath;
      ByteBuf = new byte[BufSize];
      ByteBufVer = new byte[BufSize];
      WFIO = new WinFileIO(ByteBuf);
      DataPath = Application.StartupPath;
      int n = DataPath.LastIndexOf("bin\\");
      DataPath = DataPath.Substring(0, n);
      DataPath += "data\\";
      if (!Directory.Exists(DataPath))
      {
        String S = "GetDataPath: data Folder can't be found.  Should have been created on install.";
        DispUIMsg(S);
        DataPath = "";
      }
      // Read the test file into the verification buffer.
      TestFileName = DataPath + "All Customers Orders Order Details.txt";
      FileStream FSFile = new FileStream(TestFileName, FileMode.Open, FileAccess.Read,
        FileShare.None, BlockSize, FileOptions.SequentialScan);
      VerBytesRead = FSFile.Read(ByteBufVer, 0, BufSize);
      FSFile.Close();
      FSFile.Dispose();
      TestFileName2 = DataPath + "TestWriteFile.txt";
    }

    protected void Dispose(bool disposing)
	  {
      // This function frees up the unmanaged resources of this class.
      WFIO.Dispose();
	  }

	  public void Dispose()
	  {
      // This method should be called to clean everything up.
	    Dispose(true);
	    // Tell the GC not to finalize since clean up has already been done.
	    GC.SuppressFinalize(this);
	  }

    ~WinFileIOUnitTests()
	  {
      // Destructor gets called by the garbage collector if the user did not call Dispose.
	    Dispose(false);
	  }

    public void RunAllUnitTests()
    {
      // This function runs all of the unit tests in order to show that each of WinFileIO methods works correctly.
      DispUIMsg("Running the WinFileIO unit tests:");
      TestRead();
      TestReadUntilEOF();
      TestReadBlocks();
      TestWrite();
      TestWriteBlocks();
      DispUIMsg("WinFileIO unit tests have completed.\r\n");
    }

    private void TestRead()
    {
      // This function tests the Read function.
      int BytesRead, Loop;
      String S;
      try
      {
        WFIO.OpenForReading(TestFileName);
        BytesRead = WFIO.Read(BufSize);
        WFIO.Close();
        // Check to see if the data read in matches the verification buffer.
        if (BytesRead != VerBytesRead)
        {
          S = "  TestRead: test failed - bytes read != VerBytesRead";
          DispUIMsg(S);
          return;
        }
        // Compare the 2 arrays.  If there are any differences, then report an error.
        for (Loop = 0; Loop < BytesRead; Loop++)
        {
          if (ByteBuf[Loop] != ByteBufVer[Loop])
          {
            S = "  TestRead: test failed - the " + Loop.ToString() + " element does not match the verification buffer.";
            DispUIMsg(S);
            return;
          }
        }
        S = "  TestRead: Passed";
        DispUIMsg(S);
      }
      catch (System.Exception ex)
      {
        S = "  TestRead: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        S = "  " + ex.Message;
        DispUIMsg(S);
      }
    }

    private void TestReadUntilEOF()
    {
      // This function tests the ReadUntilEOF function.
      int BytesRead, Loop;
      String S;
      try
      {
        WFIO.OpenForReading(TestFileName);
        BytesRead = WFIO.ReadUntilEOF();
        WFIO.Close();
        // Check to see if the data read in matches the verification buffer.
        if (BytesRead != VerBytesRead)
        {
          S = "  TestReadUntilEOF: test failed - bytes read != VerBytesRead";
          DispUIMsg(S);
          return;
        }
        // Compare the 2 arrays.  If there are any differences, then report an error.
        for (Loop = 0; Loop < BytesRead; Loop++)
        {
          if (ByteBuf[Loop] != ByteBufVer[Loop])
          {
            S = "  TestReadUntilEOF: test failed - the " + Loop.ToString() + " element does not match the verification buffer.";
            DispUIMsg(S);
            return;
          }
        }
        S = "  TestReadUntilEOF: Passed";
        DispUIMsg(S);
      }
      catch (System.Exception ex)
      {
        S = "  TestReadUntilEOF: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        S = "  " + ex.Message;
        DispUIMsg(S);
      }
    }

    private void TestReadBlocks()
    {
      // This function tests the ReadBlocks function.
      int BytesRead, Loop;
      String S;
      try
      {
        WFIO.OpenForReading(TestFileName);
        BytesRead = WFIO.ReadBlocks(VerBytesRead);
        WFIO.Close();
        // Check to see if the data read in matches the verification buffer.
        if (BytesRead != VerBytesRead)
        {
          S = "  TestReadBlocks: test failed - bytes read != VerBytesRead";
          DispUIMsg(S);
          return;
        }
        // Compare the 2 arrays.  If there are any differences, then report an error.
        for (Loop = 0; Loop < BytesRead; Loop++)
        {
          if (ByteBuf[Loop] != ByteBufVer[Loop])
          {
            S = "  TestReadBlocks: test failed - the " + Loop.ToString() + " element does not match the verification buffer.";
            DispUIMsg(S);
            return;
          }
        }
        S = "  TestReadBlocks: Passed";
        DispUIMsg(S);
      }
      catch (System.Exception ex)
      {
        S = "  TestReadBlocks: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        S = "  " + ex.Message;
        DispUIMsg(S);
      }
    }

    private void TestWrite()
    {
      // This function tests the Write function.
      int BytesWritten, BytesRead, Loop;
      String S;
      try
      {
        // Open the file and output the buffer.
        WFIO.OpenForWriting(TestFileName2);
        BytesWritten = WFIO.Write(VerBytesRead);
        WFIO.Close();
        // Check to see if the data that was written out matches the verification buffer.
        if (BytesWritten != VerBytesRead)
        {
          S = "  TestWrite: test failed - BytesWritten != VerBytesRead";
          DispUIMsg(S);
          return;
        }
        // Read in the file that was just written out.
        WFIO.OpenForReading(TestFileName2);
        BytesRead = WFIO.ReadUntilEOF();
        WFIO.Close();
        // Check to see if the data read in matches the verification buffer.
        if (BytesRead != BytesWritten)
        {
          S = "  TestWrite: test failed - bytes read != bytes written";
          DispUIMsg(S);
          return;
        }
        // Compare the 2 arrays.  If there are any differences, then report an error.
        for (Loop = 0; Loop < BytesWritten; Loop++)
        {
          if (ByteBuf[Loop] != ByteBufVer[Loop])
          {
            S = "  TestWrite: test failed - the " + Loop.ToString() + " element does not match the verification buffer.";
            DispUIMsg(S);
            return;
          }
        }
        S = "  TestWrite: Passed";
        DispUIMsg(S);
      }
      catch (System.Exception ex)
      {
        S = "  TestWrite: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        S = "  " + ex.Message;
        DispUIMsg(S);
      }
    }

    private void TestWriteBlocks()
    {
      // This function tests the WriteBlocks function.
      int BytesWritten, BytesRead, Loop;
      String S;
      try
      {
        // Open the file and output the buffer.
        WFIO.OpenForWriting(TestFileName2);
        BytesWritten = WFIO.WriteBlocks(VerBytesRead);
        WFIO.Close();
        // Check to see if the data read in matches the verification buffer.
        if (BytesWritten != VerBytesRead)
        {
          S = "  TestWrite: test failed - BytesWritten != VerBytesRead";
          DispUIMsg(S);
          return;
        }
        // Read in the file that was just written out.
        WFIO.OpenForReading(TestFileName);
        BytesRead = WFIO.ReadUntilEOF();
        WFIO.Close();
        // Check to see if the data read in matches the verification buffer.
        if (BytesRead != BytesWritten)
        {
          S = "  TestWriteBlocks: test failed - bytes read != bytes written";
          DispUIMsg(S);
          return;
        }
        // Compare the 2 arrays.  If there are any differences, then report an error.
        for (Loop = 0; Loop < BytesWritten; Loop++)
        {
          if (ByteBuf[Loop] != ByteBufVer[Loop])
          {
            S = "  TestWriteBlocks: test failed - the " + Loop.ToString() +
              " element does not match the verification buffer.";
            DispUIMsg(S);
            return;
          }
        }
        S = "  TestWriteBlocks: Passed";
        DispUIMsg(S);
      }
      catch (System.Exception ex)
      {
        S = "  TestWriteBlocks: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        S = "  " + ex.Message;
        DispUIMsg(S);
      }
    }

    private void DispUIMsg(String Msg)
    {
      TheForm.DisplayMsg(Msg);
    }
  }
}
