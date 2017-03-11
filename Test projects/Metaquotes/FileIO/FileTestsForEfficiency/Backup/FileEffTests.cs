using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using Win32FileIO;

namespace TestsForEfficiency
{
  public partial class FileEfficientTests : IDisposable
  {
    // This class is used to benchmark different I/O methods in order to determine which one is the
    // most efficient.
    //
    // Written by Robert G. Bryan in Feb, 2011.
    //
    private MainForm TheForm;					  // Form where output msgs are to be displayed.
    private static DateTime StartTime;  // Time when a test begins.
    private static DateTime EndTime;    // Time when a test ends.
    private static TimeSpan TimeToComplete;  // Total time it took to complete a test.
    private bool FilesInit;             // Flag used to identify whether the files have been initialized.
    private String DataPath;            // The folder location where all the data files are located.
    private String[] FileNames;         // Contains the file names used in the file I/O tests.
    private String[] FileNamesW;        // Contains the file names used in the write file I/O tests.
    private String[] FileDesc = new String[3] { "< 1MB", "10MB", "50MB" };  // Desc of files.
    private char[] CharBuf;             // Buffer used to read in unicode data from files in the file I/O tests.
    private static byte[] ByteBuf;      // Buffer used to read in byte data from files in the file I/O tests.
    private static byte[] ByteBufVer;   // Buffer used to verify that the TestFileStreamRead7 ran correctly.
    private const int BufSize = 60000000;  // Size of file I/O buffers.
    private const int BufSizeM1M = BufSize - 1000000; // The max amount of data read in at any one time.
    private const int AsyncBufSize = 131072;  // The block size used for the asynch tests.
    private const int BlockSize = 65536;  // Size of block used to read/write files.
    private static FileStream AsyncFS;  // The file stream used for the asynch tests.
    private static ManualResetEvent WaitReadAsync; // Event flag used to notify when an asynch test has completed.
    private static ManualResetEvent WaitSignal;		 // Event flag used to notify when an asynch test has completed.
    private static int BufIndex;				// Used in the async tests to calculate where to place the next block of data.
    private static int BufCount;        // Counts the bytes read in TestFileStreamRead7.
    private static UTF8Encoding UTF8;		// Object is required to convert between byte[] & String.
    private const byte CR = 13;         // End of line marker, used to parse a text file.
    private const int ReadCount = 100;  // # of times to perform the long read files tests.
    private const int WriteCount = 10;  // # of times to perform the long write files tests.
    private int[] BytesInFiles = new int[3];	// Holds the number of bytes that are in each file.
    private String[] DataLines;					// Holds the data read in from the .6834MB data file.
    private String[] DataLines10;				// Holds the data read in from the  10 MB data file.
    private String[] DataLines50;				// Holds the data read in from the  50 MB data file.
    private ArrayList StringContents;	  // Holds the refernces to each of the above arrays.
    private WinFileIO WFIO;             // The object that implements the windows readfile and writefile system functions.
    //
    public FileEfficientTests(MainForm theForm)
    {
      TheForm = theForm;
      FilesInit = false;
      FileNames = new String[3];
      FileNamesW = new String[3];
      CharBuf = new char[BufSizeM1M];
      ByteBuf = new byte[BufSizeM1M];
      ByteBufVer = new byte[BufSizeM1M];
      WFIO = new WinFileIO(ByteBuf);
      WaitReadAsync = new ManualResetEvent(false);
      WaitSignal = new ManualResetEvent(true);
      UTF8 = new UTF8Encoding();
      StringContents = new ArrayList(3);
    }

    protected void Dispose(bool disposing)
	  {
      // This function should be called in order to clean everything up.  Usually the Close method should be
      // called to close the file and the 
      WFIO.Dispose();
	  }

	  public void Dispose()
	  {
      // This method should be called to clean everything up.
	    Dispose(true);
	    // Tell the GC not to finalize since clean up has already been done.
	    GC.SuppressFinalize(this);
	  }

    ~FileEfficientTests()
	  {
      // Destructor gets called by the garbage collector if the user did not call Dispose.
	    Dispose(false);
	  }
    public void RunAllTests()
    {
      TestReadingFiles();
      TestWritingFiles();
    }

    public void GetDataPath()
    {
      // This function gets the path where the data is located for the file I/O tests and loads the
      // FileNames array with the file names used in the test.
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
      FileNames[0] = DataPath + "All Customers Orders Order Details.txt";
      FileNames[1] = DataPath + "10 MB.txt";
      FileNames[2] = DataPath + "50 MB.txt";
      FileNamesW[0] = DataPath + "All Customers Orders Order Details Write.txt";
      FileNamesW[1] = DataPath + "10 MB Write.txt";
      FileNamesW[2] = DataPath + "50 MB Write.txt";
    }

    public void ReadDataFiles()
    {
      // This function reads in the test data files that are required for the file I/O write tests.
      String S;
      int NumLines, NumLines10, NumLines50;
      if (!FilesInit)
      {
        try
        {
          if (!File.Exists(FileNames[0]))
          {
            S = "ReadDataFiles: First data file (All Customers Orders Order Details.txt) not found.  Check the install.";
            DispUIMsg(S);
            return;
          }
          // Read all the lines in the test files to get some data to test with.
          DataLines = File.ReadAllLines(FileNames[0], Encoding.UTF8);
          DataLines10 = File.ReadAllLines(FileNames[1], Encoding.UTF8);
          DataLines50 = File.ReadAllLines(FileNames[2], Encoding.UTF8);
          NumLines = DataLines.GetLength(0);
          NumLines10 = DataLines10.GetLength(0);
          NumLines50 = DataLines50.GetLength(0);
          StringContents.Add(DataLines);
          StringContents.Add(DataLines10);
          StringContents.Add(DataLines50);
          FilesInit = true;
        }
        catch (System.Exception ex)
        {
          S = "ReadDataFiles: threw an exception of type " + ex.GetType().ToString();
          DispUIMsg(S);
          DispUIMsg(ex.Message);
          DataPath = "";
        }
      }
    }

    public void TestReadingFiles()
    {
      // This function tests out several ways of reading in files for file sizes of < 1MB, 10MB, and 50MB.
      GetDataPath();
      if (DataPath == "")
        return;
      ReadDataFiles();
      if (!FilesInit)
        return;
      DispUIMsg("Running the read file tests:");
      TestReadAllLines();
      TestReadAllText();
      TestReadAllBytes();
      DispUIMsg("");
      TestBinaryReader1();
      DispUIMsg("");
      TestStreamReader1();
      TestStreamReader2();
      TestStreamReader3();
      TestStreamReader4();
      TestStreamReader5();
      TestStreamReader6();
      TestStreamReader7();
      DispUIMsg("");
      TestFileStreamRead1();
      TestFileStreamRead2();
      TestFileStreamRead2A();
      TestFileStreamRead3();
      TestFileStreamRead4();
      TestFileStreamRead5();
      TestFileStreamRead6();
      TestFileStreamRead7();
      TestFileStreamRead8();
      DispUIMsg("");
      TestReadFileWinAPI1();
      TestReadFileWinAPI2();
      TestReadFileWinAPI3();
      DispUIMsg("");
      TestBinaryReader1NoOpenClose();
      TestStreamReader2NoOpenClose();
      TestFileStreamRead1NoOpenClose();
      TestReadFileWinAPINoOpenClose();
      DispUIMsg("Read file tests have completed.\r\n");
    }

    public void TestWritingFiles()
    {
      // This function tests out several ways of writing out files for file sizes of < 1MB, 10MB, and 50MB.
      GetDataPath();
      if (DataPath == "")
        return;
      ReadDataFiles();
      if (!FilesInit)
        return;
      DispUIMsg("Running the write file Tests:");
      TestWriteAllLines();
      TestWriteAllText();
      TestWriteAllBytes();
      DispUIMsg("");
      TestBinaryWriter1();
      DispUIMsg("");
      TestStreamWriter1();
      DispUIMsg("");
      TestFileStreamWrite1();
      TestFileStreamWrite2();
      TestFileStreamWrite3();
      DispUIMsg("");
      TestWriteFileWinAPI1();
      TestWriteFileWinAPI2();
      DispUIMsg("Write file tests have completed.\r\n");
    }

    private void TestReadAllLines()
    {
      // This function tests out how quickly the File.ReadAllLines method returns all the lines in a file
      // using the ReadAllLines function.
      String[] AllLines;
      String S;
      int NumLines, FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          AllLines = File.ReadAllLines(FileNames[FileLoop], System.Text.Encoding.UTF8);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with File.ReadAllLines				= " + TimeToComplete.ToString();
          DispUIMsg(S);
          NumLines = AllLines.GetLength(0);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestReadAllLines: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestReadAllText()
    {
      // This function tests out how quickly the File.ReadAllLines method returns all the lines in a file
      // using the ReadAllText function.
      String AllLines;
      String S;
      int NumBytes, FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          AllLines = File.ReadAllText(FileNames[FileLoop], System.Text.Encoding.UTF8);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with File.ReadAllText				= " + TimeToComplete.ToString();
          DispUIMsg(S);
          NumBytes = AllLines.Length;
        }
      }
      catch (System.Exception ex)
      {
        S = "TestReadAllText: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestReadAllBytes()
    {
      // This function tests out how quickly the File.ReadAllLines method returns all the lines in a file
      // using the ReadAllBytes function.
      String S;
      int NumBytes, FileLoop;
      byte[] BB;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          BB = File.ReadAllBytes(FileNames[FileLoop]);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with File.ReadAllBytes				= " + TimeToComplete.ToString();
          DispUIMsg(S);
          NumBytes = BB.Length;
        }
      }
      catch (System.Exception ex)
      {
        S = "TestReadAllBytes: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestBinaryReader1()
    {
      // This function tests reading data from a file with the BinaryReader class.  This function uses a char buf.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          BinaryReader BRFile = new BinaryReader(File.Open(FileNames[FileLoop], FileMode.Open));
          NumChars = BRFile.Read(CharBuf, 0, BufSizeM1M);
          BRFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with BinaryReader.Read				= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestBinaryReader1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader1()
    {
      // This function tests reading data from a file with the StreamReader class.  The Read function is tested here.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamReader SRFile = new StreamReader(FileNames[FileLoop]);
          NumChars = SRFile.Read(CharBuf, 0, BufSizeM1M);
          SRFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with StreamReader1.Read			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader2()
    {
      // This function tests reading data from a file with the StreamReader class.  The Read function is tested here.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamReader SRFile = new StreamReader(FileNames[FileLoop], System.Text.Encoding.UTF8, false, BlockSize);
          NumChars = SRFile.Read(CharBuf, 0, BufSizeM1M);
          SRFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with StreamReader2.Read(large buf)		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader2: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader3()
    {
      // This function tests reading data from a file with the StreamReader class.  The ReadBlock function is tested here.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamReader SRFile = new StreamReader(FileNames[FileLoop], System.Text.Encoding.UTF8, false, BlockSize);
          NumChars = SRFile.ReadBlock(CharBuf, 0, BufSizeM1M);
          SRFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with StreamReader3.ReadBlock			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader3: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader4()
    {
      // This function tests reading data from a file with the StreamReader class.  The ReadToEnd function is tested here.
      String S, AllChars;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamReader SRFile = new StreamReader(FileNames[FileLoop], System.Text.Encoding.UTF8, false, BlockSize);
          AllChars = SRFile.ReadToEnd();
          SRFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with StreamReader4.ReadToEnd			= " + TimeToComplete.ToString();
          NumChars = AllChars.Length;
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader4: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader5()
    {
      // This function tests reading data from a file with the StreamReader class.  The Read function is tested here
      // using a smaller buffer size and reading into memory just BlockSize bytes each time until the entire file is
      // read into memory.
      String S;
      int FileLoop, BlockLoop = 0, NumChars = 0;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamReader SRFile = new StreamReader(FileNames[FileLoop], System.Text.Encoding.UTF8, false, BlockSize);
          while (SRFile.Peek() >= 0)
          {
            // Documentation says to use less than the buffer size for best performance, but not how much less.
            NumChars += SRFile.Read(CharBuf, 0, 64000);
            BlockLoop++;
          }
          SRFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with mult StreamReader5.Read			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader5: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader6()
    {
      // This function tests reading data from a file with the StreamReader class.  The ReadLine function is tested here
      // using the ReadLine function.
      String S, Line;
      int FileLoop, LineCount = 0;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamReader SRFile = new StreamReader(FileNames[FileLoop], System.Text.Encoding.UTF8, false, BlockSize);
          while (SRFile.Peek() >= 0)
          {
            // Documentation says to use less than the buffer size for best performance, but not how much less.
            Line = SRFile.ReadLine();
            LineCount++;
          }
          SRFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with StreamReader6.ReadLine			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader6: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader7()
    {
      // This function tests reading data from a file with the StreamReader class.  The Read function is tested here.
      // The difference beween this function and TestStreamReader2 which uses the exact same code for reading the file in
      // is that this function parses the bufffer into individual lines so that a comparision can be made between
      // this method and TestStreamReader6, which does the parsing for us to determine which method is the most
      // efficient.
      String S, Line;
      int FileLoop, NumChars, NextIndex, LastIndex, LineCount, Len;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamReader SRFile = new StreamReader(FileNames[FileLoop], System.Text.Encoding.UTF8, false, BlockSize);
          NumChars = SRFile.Read(CharBuf, 0, BufSizeM1M);
          SRFile.Close();
          LastIndex = LineCount = 0;
          // Parse each line in the buffer by looking for the <CR> char at the end of each line.
          for (;;)
          {
            NextIndex = Array.IndexOf(CharBuf, '\r', LastIndex);
            if (NextIndex == -1)
              break;
            Len = NextIndex - LastIndex;
            Line = new String(CharBuf, LastIndex, Len);
            LastIndex = NextIndex + 2;
            LineCount++;
          }
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with StreamReader7.Read parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader7: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamRead1()
    {
      // This function tests reading data from a file with the FileStream class.  The Read function is tested here.
      // This first test reads in the entire file using the Sequential Scan option in the constructor.  No parsing of
      // lines is done here.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          FileStream FSFile = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, BlockSize, FileOptions.SequentialScan);
          NumChars = FSFile.Read(ByteBuf, 0, BufSizeM1M);
          FSFile.Close();
          FSFile.Dispose();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream1.Read no parsing  		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamRead2()
    {
      // This function tests reading data from a file with the FileStream class.  The Read function is tested here.
      // This test reads in the entire file using the Sequential Scan option in the constructor.  The lines are
      // parsed into strings.
      String S, Line;
      int FileLoop, NumChars, NextIndex, LastIndex, LineCount, Len;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          FileStream FSFile = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, BlockSize, FileOptions.SequentialScan);
          NumChars = FSFile.Read(ByteBuf, 0, BufSizeM1M);
          FSFile.Close();
          FSFile.Dispose();
          LastIndex = LineCount = 0;
          // Parse each line in the buffer by looking for the <CR> char at the end of each line.
          for (;;)
          {
            NextIndex = Array.IndexOf(ByteBuf, CR, LastIndex);
            if (NextIndex == -1)
              break;
            Len = NextIndex - LastIndex;
            Line = UTF8.GetString(ByteBuf, LastIndex, Len);
            LastIndex = NextIndex + 2;
            LineCount++;
          }
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream2.Read parsing			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead2: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamRead2A()
    {
      // This function tests reading data from a file with the FileStream class.  The Read function is tested here.
      // This test reads in the file using the same buffer size as TestFileStreamRead5 so that a true comparison can be
      // made when parsing the file into strings using multiple reads.
      String S, Line;
      int FileLoop, NumChars, NextIndex, LastIndex, LineCount, Len;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          FileStream FSFile = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, AsyncBufSize, FileOptions.SequentialScan);
          NumChars = 0;
          for (;;)
          {
            Len = FSFile.Read(ByteBuf, NumChars, AsyncBufSize);
            NumChars += Len;
            if (Len < AsyncBufSize)
              break;
          }
          FSFile.Close();
          FSFile.Dispose();
          LastIndex = LineCount = 0;
          // Parse each line in the buffer by looking for the <CR> char at the end of each line.
          for (;;)
          {
            NextIndex = Array.IndexOf(ByteBuf, CR, LastIndex);
            if (NextIndex == -1)
              break;
            Len = NextIndex - LastIndex;
            Line = UTF8.GetString(ByteBuf, LastIndex, Len);
            LastIndex = NextIndex + 2;
            LineCount++;
          }
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with multiFileStream2A.Read parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead2A: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamRead3()
    {
      // This function tests reading data from a file with the FileStream class.  The Read function is tested here.
      // This test reads in the entire file using the RandomAccess option in the constructor.  No parsing is done here.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          FileStream FSFile = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, BlockSize, FileOptions.RandomAccess);
          NumChars = FSFile.Read(ByteBuf, 0, BufSizeM1M);
          FSFile.Close();
          FSFile.Dispose();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream3.Read(Rand) no parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead3: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamRead4()
    {
      // This function tests reading data from a file with the FileStream class.  The async BeginRead function is tested here.
      // This test reads in each file asynchronously reading AsyncBufSize bytes at a time.  No parsing is done.
      String S;
      int FileLoop;
      bool TimedOut;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback4);
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          WaitReadAsync.Reset();
          StartTime = DateTime.Now;
          AsyncFS = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, AsyncBufSize, FileOptions.Asynchronous);
          BufIndex = 0;
          IAsyncResult aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, 0);
          // The code should wait until the rest of the file is read in via EndReadCallback4.  a 15 sec timeout is used
          // in case there is a problem.
          TimedOut = WaitReadAsync.WaitOne(15000, false);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream4.BeginRead no parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead4: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private static void EndReadCallback4(IAsyncResult asyncResult)
    {
      // This is the async call back function used by BeginRead.  The data is returned via the EndRead function.
      // This function implements a lock that prevents more than 1 read from being processed at the same time.
      // The idea is to execute the next read and then process the data while reading in the next block.
      // In this test, there is no processing of data in order to compare with other tests that do the same thing.
      int NumChars;
      IAsyncResult aResult;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback4);
      lock (WaitReadAsync)
      {
        NumChars = AsyncFS.EndRead(asyncResult);
        if (NumChars > 0)
        {
          if (NumChars == AsyncBufSize)
          {
            BufIndex += NumChars;
            aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, 0);
          }
        }
        if (NumChars < AsyncBufSize)
        {
          AsyncFS.Close();
          AsyncFS.Dispose();
          WaitReadAsync.Set();
        }
      }
    }

    private void TestFileStreamRead5()
    {
      // This function tests reading data from a file with the FileStream class.  The async BeginRead function is tested
      // here. This test reads in each file asynchronously reading AsyncBufSize bytes at a time.  The file is parsed while
      // the next block is read.
      String S;
      int FileLoop;
      bool TimedOut;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback5);
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        ByteBuf[AsyncBufSize] = CR;
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          WaitReadAsync.Reset();
          StartTime = DateTime.Now;
          AsyncFS = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, AsyncBufSize, FileOptions.Asynchronous);
          BufIndex = 0;
          IAsyncResult aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, 0);
          TimedOut = WaitReadAsync.WaitOne(30000, false);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream5.BeginRead parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead5: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private static void EndReadCallback5(IAsyncResult asyncResult)
    {
      // This is the async call back function used by BeginRead.  The data is returned via the EndRead function.
      // This function implements a lock that prevents more than 1 read from being processed at the same time.
      // The idea is to execute the next read and then process the data while reading in the next block.
      // In this test, there is parsing of data in order to compare with other tests that do the same thing.
      int NumChars, NextIndex, Len, LastIndex, LineCount;
      String Line;
      IAsyncResult aResult;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback5);
      lock (WaitReadAsync)
      {
        NumChars = AsyncFS.EndRead(asyncResult);
        if (NumChars > 0)
        {
          if (NumChars == AsyncBufSize)
          {
            LastIndex = BufIndex;
            BufIndex += NumChars;
            aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, 0);
            LineCount = 0;
            // Parse each line in the buffer by looking for the <CR> char at the end of each line.
            for (;;)
            {
              NextIndex = Array.IndexOf(ByteBuf, CR, LastIndex);
              if ((NextIndex == -1) || (NextIndex >= BufIndex))
                break;
              Len = NextIndex - LastIndex;
              Line = UTF8.GetString(ByteBuf, LastIndex, Len);
              LastIndex = NextIndex + 2;
              LineCount++;
            }
          }
        }
        if (NumChars < AsyncBufSize)
        {
          AsyncFS.Close();
          AsyncFS.Dispose();
          WaitReadAsync.Set();
        }
      }
    }

    private void TestFileStreamRead6()
    {
      // This test is the same as TestFileStreamRead5, with the only difference being that the callback function uses
      // a ManualResetEvent object for controlling thread access instead of a lock statement.
      // The async BeginRead function is tested here. This test reads in each file asynchronously reading
      // AsyncBufSize bytes at a time.  The file is parsed while the next block is read.
      String S;
      int FileLoop;
      bool TimedOut;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback6);
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          WaitReadAsync.Reset();
          StartTime = DateTime.Now;
          AsyncFS = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, AsyncBufSize, FileOptions.Asynchronous);
          BufIndex = 0;
          IAsyncResult aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, 0);
          TimedOut = WaitReadAsync.WaitOne(300000, false);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream6.BeginRead parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead6: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private static void EndReadCallback6(IAsyncResult asyncResult)
    {
      // This is the async call back function used by BeginRead.  The data is returned via the EndRead function.
      // This function implements a lock that prevents more than 1 read from being processed at the same time.
      // The idea is to execute the next read and then process the data while reading in the next block.
      // In this test, there is parsing of data in order to compare with other tests that do the same thing.
      int NumChars, NextIndex, Len, LastIndex, LineCount;
      String Line;
      IAsyncResult aResult;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback6);
      NumChars = AsyncFS.EndRead(asyncResult);
      WaitSignal.WaitOne();
      WaitSignal.Reset();
      if (NumChars > 0)
      {
        if (NumChars == AsyncBufSize)
        {
          LastIndex = BufIndex;
          BufIndex += NumChars;
          aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, 0);
          LineCount = 0;
          // Parse each line in the buffer by looking for the <CR> char at the end of each line.
          for (;;)
          {
            NextIndex = Array.IndexOf(ByteBuf, CR, LastIndex);
            if ((NextIndex == -1) || (NextIndex >= BufIndex))
              break;
            Len = NextIndex - LastIndex;
            Line = UTF8.GetString(ByteBuf, LastIndex, Len);
            LastIndex = NextIndex + 2;
            LineCount++;
          }
        }
      }
      if (NumChars < AsyncBufSize)
      {
        AsyncFS.Close();
        AsyncFS.Dispose();
        WaitReadAsync.Set();
      }
      // Allow the next thread to go through.
      WaitSignal.Set();
    }

    private void TestFileStreamRead7()
    {
      // This test removes the lock at the top of the callback function, which means that multiple threads will
      // execute simultaneously which should in theory yield better performance.  The last argument of BeginRead
      // specifies the index to the buffer where data should be processed.
      // The async BeginRead function is tested here. This test reads in each file asynchronously reading
      // AsyncBufSize bytes at a time.  No parsing is done in this test.
      String S;
      int FileLoop, NumChars;
      bool TimedOut;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback7);
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          // Read in the same file using the sync method of FileStream to determine if this test ran correctly.
          FileStream FSFile = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, BlockSize, FileOptions.RandomAccess);
          NumChars = FSFile.Read(ByteBufVer, 0, BufSizeM1M);
          FSFile.Close();
          FSFile.Dispose();
          WaitReadAsync.Reset();
          StartTime = DateTime.Now;
          AsyncFS = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, AsyncBufSize, FileOptions.Asynchronous);
          BufIndex = BufCount = 0;
          IAsyncResult aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, BufIndex);
          TimedOut = WaitReadAsync.WaitOne(300000, false);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream7.BeginRead  	  	                = " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead7: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private static void EndReadCallback7(IAsyncResult asyncResult)
    {
      // This is the async call back function used by BeginRead.  The data is returned via the EndRead function.
      // Unlike the other tests, this callback function allows multiple threads to be processed simultaneously.
      // 
      // The idea is to execute the next read and then process the data while reading in the next block.
      // In this test, there is parsing of data in order to compare with other tests that do the same thing.
      int NumChars, Index, NextIndex, FileByteCount;
      // int Loop;
      IAsyncResult aResult;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback7);
      NumChars = AsyncFS.EndRead(asyncResult);
      FileByteCount = Interlocked.Add(ref BufCount, NumChars);
      Index = (int)asyncResult.AsyncState;
      if (NumChars > 0)
      {
        if (NumChars == AsyncBufSize)
        {
          NextIndex = Index + AsyncBufSize;
          aResult = AsyncFS.BeginRead(ByteBuf, NextIndex, AsyncBufSize, CallBack, NextIndex);
        }
      }
      // By using a stack variable instead of a class variable (that is used by all the threads)
      // a lock statement does not have to be used to single thread here.
      if (FileByteCount >= AsyncFS.Length)
      {
        /* This code is currently commented out since it would interfere with the timing of this test.
        // Its primary purpose is to show that locks are not required to correctly read a file into memory
        // using an asynch method that uses multiple threads.
        //
        // Verify that the file read in matches the verification buffer.
        if (FileByteCount != AsyncFS.Length)
          BufCount = 0; // Set a breakpoint on this line to detect an error.
        for (Loop = 0; Loop < FileByteCount; Loop++)
        {
          if (ByteBuf[Loop] != ByteBufVer[Loop])
            BufCount = 0; // Set a breakpoint on this line to detect an error.
        }
        */
        AsyncFS.Close();
        AsyncFS.Dispose();
        WaitReadAsync.Set();
      }
    }

    private void TestFileStreamRead8()
    {
      // This test removes the lock at the top of the callback function, which means that multiple threads will
      // execute simultaneously which should in theory yield better performance.  The last argument of BeginRead
      // specifies the index to the buffer where data should be placed.  When EndRead is called in the callback
      // function, it obtains this value in order to calculate the next index where data will be placed.
      // The async BeginRead function is tested here. This test reads in each file asynchronously reading
      // AsyncBufSize bytes at a time.  The file is parsed while the next block is read.
      String S;
      int FileLoop;
      bool TimedOut;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback8);
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          WaitReadAsync.Reset();
          StartTime = DateTime.Now;
          AsyncFS = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, AsyncBufSize, FileOptions.Asynchronous);
          BufIndex = BufCount = 0;
          IAsyncResult aResult = AsyncFS.BeginRead(ByteBuf, BufIndex, AsyncBufSize, CallBack, BufIndex);
          TimedOut = WaitReadAsync.WaitOne(300000, false);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream8.BeginRead parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead8: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private static void EndReadCallback8(IAsyncResult asyncResult)
    {
      // This is the async call back function used by BeginRead.  The idea is to execute the next read and then
      // process the data while reading in the next block.  In this test, there is parsing of data in order to
      // compare with other tests that do the same thing.  The data is returned via the EndRead function.
      // Unlike the other tests, this callback function allows multiple threads to be processed simultaneously.
      // The reason it can be done like this without any problems is because the index to the buffer is passed by
      // BeginRead in the last argument.
      // The only place where a lock is required is when calculating BufCount.  The Interlocked.Add method is used
      // to obtain the index to place data at on the next read.
      // To verify that this test works as intended, uncomment the code before the AsyncFS.Close() statement and
      // set a breakpoint on each error condition test.
      int NumChars, Index, NextIndex, Len, LastIndex, LineCount, FileByteCount;
      // int Loop;
      String Line;
      IAsyncResult aResult;
      AsyncCallback CallBack = new AsyncCallback(EndReadCallback8);
      NumChars = AsyncFS.EndRead(asyncResult);
      FileByteCount = Interlocked.Add(ref BufCount, NumChars);
      Index = (int)asyncResult.AsyncState;
      if (NumChars > 0)
      {
        if (NumChars == AsyncBufSize)
        {
          NextIndex = Index + AsyncBufSize;
          aResult = AsyncFS.BeginRead(ByteBuf, NextIndex, AsyncBufSize, CallBack, NextIndex);
        }
        LineCount = 0;
        LastIndex = Index;
        // Parse each line in the buffer by looking for the <CR> char at the end of each line.
        for (;;)
        {
          NextIndex = Array.IndexOf(ByteBuf, CR, LastIndex);
          if ((NextIndex == -1) || (NextIndex >= Index + AsyncBufSize))
            break;
          Len = NextIndex - LastIndex;
          Line = UTF8.GetString(ByteBuf, LastIndex, Len);
          LastIndex = NextIndex + 2;
          LineCount++;
        }
      }
      // By using a stack variable instead of a class variable (that is used by all the threads)
      // a lock statement does not have to be used to single thread here.
      if (FileByteCount >= AsyncFS.Length)
      {
        /* This code is currently commented out since it would interfere with the timing of this test.
        // Its primary purpose is to show that locks are not required to correctly read a file into memory
        // using an asynch method that uses multiple threads.
        //
        // Verify that the file read in matches the verification buffer.
        if (FileByteCount != AsyncFS.Length)
          BufCount = 0; // Set a breakpoint on this line to detect an error.
        for (Loop = 0; Loop < FileByteCount; Loop++)
        {
          if (ByteBuf[Loop] != ByteBufVer[Loop])
            BufCount = 0; // Set a breakpoint on this line to detect an error.
        }
        */
        AsyncFS.Close();
        AsyncFS.Dispose();
        WaitReadAsync.Set();
      }
    }

    private void TestReadFileWinAPI1()
    {
      // This function tests out the Windows API function ReadFile.  This function tests the ReadFile function
      // by reading in the entire file with one call.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          WFIO.OpenForReading(FileNames[FileLoop]);
          WFIO.Read(BufSizeM1M);
          WFIO.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with WFIO1.Read No Parsing			= "
            + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestReadFileWinAPI1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestReadFileWinAPI2()
    {
      // This function tests out the Windows API function ReadFile.  This test tests out the ReadUntilEOF method
      // in the WinFileIO class.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          WFIO.OpenForReading(FileNames[FileLoop]);
          WFIO.ReadUntilEOF();
          WFIO.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with WFIO2.ReadUntilEOF No Parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestReadFileWinAPI2: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestReadFileWinAPI3()
    {
      // This function tests out the Windows API function ReadFile.  This test tests out the ReadUntilEOF method
      // in the WinFileIO class.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          WFIO.OpenForReading(FileNames[FileLoop]);
          WFIO.ReadBlocks(BufSizeM1M);
          WFIO.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with WFIO3.ReadBlocks API No Parsing		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestReadFileWinAPI3: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestBinaryReader1NoOpenClose()
    {
      // This function tests reading data from a file with the BinaryReader class.
      // This function is exactly the same as TestBinaryReader1, except it measures the time taken after
      // opening the file and before closing it.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          BinaryReader BRFile = new BinaryReader(File.Open(FileNames[FileLoop], FileMode.Open));
          StartTime = DateTime.Now;
          NumChars = BRFile.Read(CharBuf, 0, BufSizeM1M);
          EndTime = DateTime.Now;
          BRFile.Close();
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with BinaryReader.Read				= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestBinaryReader1NoOpenClose: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamReader2NoOpenClose()
    {
      // This function tests reading data from a file with the StreamReader class.
      // This function is exactly the same as TestStreamReader2, except it measures the time taken after
      // opening the file and before closing it.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StreamReader SRFile = new StreamReader(FileNames[FileLoop], System.Text.Encoding.UTF8, false, BlockSize);
          StartTime = DateTime.Now;
          NumChars = SRFile.Read(CharBuf, 0, BufSizeM1M);
          EndTime = DateTime.Now;
          SRFile.Close();
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with StreamReader2.Read(large buf)		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader2NoOpenClose: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamRead1NoOpenClose()
    {
      // This function tests reading data from a file with the FileStream class.
      // This function is exactly the same as TestFileStreamRead1, except it measures the time taken after
      // opening the file and before closing it.
      String S;
      int FileLoop, NumChars;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          FileStream FSFile = new FileStream(FileNames[FileLoop], FileMode.Open, FileAccess.Read,
            FileShare.None, BlockSize, FileOptions.SequentialScan);
          StartTime = DateTime.Now;
          NumChars = FSFile.Read(ByteBuf, 0, BufSizeM1M);
          EndTime = DateTime.Now;
          FSFile.Close();
          FSFile.Dispose();
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with FileStream1.Read no parsing  		= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamRead1NoOpenClose: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestReadFileWinAPINoOpenClose()
    {
      // This function is similar to TestReadFileWinAPI, except that it times only the reading of the file.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          WFIO.OpenForReading(FileNames[FileLoop]);
          StartTime = DateTime.Now;
          WFIO.Read(BufSizeM1M);
          EndTime = DateTime.Now;
          WFIO.Close();
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time reading " + FileDesc[FileLoop] + " with WFIO.Read No Open/Close			= " +
            TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestReadFileWinAPINoOpenClose: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestWriteAllLines()
    {
      // This function tests out how quickly the File.ReadAllLines method returns all the lines in a file.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to write out the .683MB, 10MB, and 50MB files.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          File.WriteAllLines(FileNamesW[FileLoop], (String[])StringContents[FileLoop], Encoding.UTF8);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with File.WriteAllLines				= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestWriteAllLines: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestWriteAllText()
    {
      // This function tests out how quickly the File.ReadAllLines method returns all the lines in a file.
      String S, AllLines;
      StringBuilder SB = new StringBuilder();
      String[] DataLines;
      int FileLoop, Loop;
      try
      {
        // Get how fast it takes to write out the .683MB, 10MB, and 50MB files.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          AllLines = "";
          DataLines = (String[])StringContents[FileLoop];
          for (Loop = 0; Loop < DataLines.Length; Loop++)
          {
            SB.Append(DataLines[Loop]);
          }
          AllLines = SB.ToString();
          StartTime = DateTime.Now;
          File.WriteAllText(FileNamesW[FileLoop], AllLines, Encoding.UTF8);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with File.TestWriteAllText				= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestWriteAllText: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestWriteAllBytes()
    {
      // This function tests out how quickly the File.ReadAllLines method returns all the lines in a file.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to write out the .683MB, 10MB, and 50MB files.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          File.WriteAllBytes(FileNamesW[FileLoop], ByteBuf);
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with File.WriteAllBytes				= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestWriteAllLines: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestBinaryWriter1()
    {
      // This function tests writing data from a file with the BinaryReader class.  This function uses a char buf.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to write out the .683MB, 10MB, and 50MB files to disk.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          BinaryWriter BWFile = new BinaryWriter(File.Open(FileNamesW[FileLoop], FileMode.Create));
          BWFile.Write(ByteBuf, 0, BytesInFiles[FileLoop]);
          BWFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with BinaryWriter.Write				= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestBinaryWriter1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestStreamWriter1()
    {
      // This function tests writing data to a file with the StreamReader class.  The write function is tested here.
      String S;
      int FileLoop;
      try
      {
        // Read the 3 test files into memory to get the number of bytes in each file and so that this test has
        // something to write out.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          WFIO.OpenForReading(FileNames[FileLoop]);
          BytesInFiles[FileLoop] = WFIO.ReadUntilEOF();
          WFIO.Close();
        }
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          StreamWriter SWFile = new StreamWriter(FileNamesW[FileLoop]);
          SWFile.Write(CharBuf, 0, BytesInFiles[FileLoop]);
          SWFile.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with StreamWriter1.Write				= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestStreamReader1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamWrite1()
    {
      // This function tests writing data to a file with the FileStream class.  The write function is tested here.
      // This first test writes out the file with the FileOptions set to none.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          FileStream FSFile = new FileStream(FileNamesW[FileLoop], FileMode.Create, FileAccess.Write,
            FileShare.None, BlockSize, FileOptions.None);
          FSFile.Write(ByteBuf, 0, BytesInFiles[FileLoop]);
          FSFile.Close();
          FSFile.Dispose();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with FileStream1.Write no parsing  			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamWrite1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamWrite2()
    {
      // This function tests writing data to a file with the FileStream class.  The write function is tested here.
      // This test writes out the file with the FileOptions set to WriteThrough.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          FileStream FSFile = new FileStream(FileNamesW[FileLoop], FileMode.Create, FileAccess.Write,
            FileShare.None, BlockSize, FileOptions.WriteThrough);
          FSFile.Write(ByteBuf, 0, BytesInFiles[FileLoop]);
          FSFile.Close();
          FSFile.Dispose();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with FileStream2.Write no parsing  			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamWrite2: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestFileStreamWrite3()
    {
      // This function tests writing data to a file with the FileStream class.  The write function is tested here.
      // This test involves writing out the file in 65536 byte chunks .vs. writing it out all at once.
      String S;
      int FileLoop, BytesToWrite, BufIndex;
      try
      {
        // Get how fast it takes to read in the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          FileStream FSFile = new FileStream(FileNamesW[FileLoop], FileMode.Create, FileAccess.Write,
            FileShare.None, BlockSize, FileOptions.WriteThrough);
          BufIndex = 0;
          do
          {
            BytesToWrite = Math.Min(BlockSize, BytesInFiles[FileLoop] - BufIndex);
            FSFile.Write(ByteBuf, BufIndex, BytesToWrite);
            BufIndex += BytesToWrite;
          } while (BufIndex < BytesInFiles[FileLoop]);
          FSFile.Close();
          FSFile.Dispose();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with FileStream3.Write no parsing  			= " + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestFileStreamWrite3: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestWriteFileWinAPI1()
    {
      // This function tests out the Windows API function WriteFile.  This function tests the WriteFile function
      // by writing out the entire file with one call.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to write ou the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          WFIO.OpenForWriting(FileNames[FileLoop]);
          WFIO.Write(BytesInFiles[FileLoop]);
          WFIO.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with WFIO1.Write API No Parsing		                = "
            + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestWriteFileWinAPI1: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void TestWriteFileWinAPI2()
    {
      // This function tests out the Windows API function WriteFile.  This function tests the WriteFile function
      // by writing out the file in blocks.
      String S;
      int FileLoop;
      try
      {
        // Get how fast it takes to write ou the .683MB, 10MB, and 50MB files into memory.
        for (FileLoop = 0; FileLoop < 3; FileLoop++)
        {
          StartTime = DateTime.Now;
          WFIO.OpenForWriting(FileNames[FileLoop]);
          WFIO.WriteBlocks(BytesInFiles[FileLoop]);
          WFIO.Close();
          EndTime = DateTime.Now;
          TimeToComplete = EndTime.Subtract(StartTime);
          S = "   Total time writing " + FileDesc[FileLoop] + " with WFIO2.WriteBlocks API No Parsing		= "
            + TimeToComplete.ToString();
          DispUIMsg(S);
        }
      }
      catch (System.Exception ex)
      {
        S = "TestWriteFileWinAPI2: threw an exception of type " + ex.GetType().ToString();
        DispUIMsg(S);
        DispUIMsg(ex.Message);
      }
    }

    private void DispUIMsg(String Msg)
    {
      TheForm.DisplayMsg(Msg);
    }
  }
}
