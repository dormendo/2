using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SendTfsBackupsByFtp
{
    class Program
    {
        const int BUFFER_SIZE = 16 * 1024 * 1024;

        static int Main(string[] args)
        {
            if (args.Length != 4)
            {
                return 1;
            }

            string sourceFolder = args[0];
            string targetServer = args[1];
            string login = args[2];
            string password = args[3];

            List<string> sourceFilesToUpload;
            List<string> targetFilesToRemove;
            GetFilesToUploadAndRemove(sourceFolder, targetServer, login, password, out sourceFilesToUpload, out targetFilesToRemove);

            foreach (string fileName in sourceFilesToUpload)
            {
                UploadFile(targetServer, sourceFolder, login, password, fileName);
            }

            foreach (string fileName in targetFilesToRemove)
            {
                RemoveFile(targetServer, fileName, login, password);
            }

            return 0;
        }

        private static void GetFilesToUploadAndRemove(string sourceFolder, string targetServer, string login, string password,
                out List<string> sourceFilesToUpload, out List<string> targetFilesToRemove)
        {
            sourceFilesToUpload = new List<string>();
            targetFilesToRemove = new List<string>();

            List<string> sourceFiles = new List<string>(Directory.GetFiles(sourceFolder));
            for (int i = 0; i < sourceFiles.Count; i++)
            {
                sourceFiles[i] = Path.GetFileName(sourceFiles[i]);
            }

            string result = "";
            FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(targetServer);
            uploadRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            uploadRequest.Credentials = new NetworkCredential(login, password);
            using (FtpWebResponse response = (FtpWebResponse)uploadRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }

            string[] targetFilesData = result.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> targetFiles = new List<string>();

            foreach (string targetFileData in targetFilesData)
            {
                string[] tfParts = targetFileData.Split(" \t".ToCharArray(), 9, StringSplitOptions.RemoveEmptyEntries);
                string fileName = tfParts[8];
                if (!sourceFiles.Contains(fileName))
                {
                    targetFilesToRemove.Add(fileName);
                }
                else
                {
                    string strLength = tfParts[4];
                    int targetLength = int.Parse(strLength);
                    string sourceFilePath = Path.Combine(sourceFolder, fileName);
                    if (File.Exists(sourceFilePath))
                    {
                        FileInfo fi = new FileInfo(sourceFilePath);
                        if (fi.Length != targetLength)
                        {
                            sourceFilesToUpload.Add(fileName);
                        }
                    }
                }

                targetFiles.Add(fileName);
            }

            foreach (string sourceFile in sourceFiles)
            {
                if (!targetFiles.Contains(sourceFile))
                {
                    sourceFilesToUpload.Add(sourceFile);
                }
            }

        }

        private static void UploadFile(string targetServer, string sourceFolder, string login, string password, string fileName)
        {
            FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(Path.Combine(targetServer, fileName));
            uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
            uploadRequest.Credentials = new NetworkCredential(login, password);

            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesReadTotal = 0;
            int bytesRead;

            using (FileStream source = new FileStream(Path.Combine(sourceFolder, fileName),
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BUFFER_SIZE, FileOptions.SequentialScan))
            {
                uploadRequest.ContentLength = source.Length;

                using (Stream target = uploadRequest.GetRequestStream())
                {
                    while (source.Length > bytesReadTotal)
                    {
                        bytesRead = source.Read(buffer, 0, BUFFER_SIZE);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        target.Write(buffer, 0, bytesRead);
                        bytesReadTotal += bytesRead;
                    }
                }
            }

            FtpWebResponse uploadResponse = (FtpWebResponse)uploadRequest.GetResponse();
            uploadResponse.Close();
            Console.WriteLine("Upload: " + fileName);
        }

        private static void RemoveOldFiles(string targetServer, List<string> sourceFiles, string login, string password)
        {
            string strList = "";

            FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(targetServer);
            uploadRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            uploadRequest.Credentials = new NetworkCredential(login, password);
            using (FtpWebResponse response = (FtpWebResponse)uploadRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        strList = sr.ReadToEnd();
                    }
                }
            }

            string[] targetFiles = strList.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string targetFile in targetFiles)
            {
                if (!sourceFiles.Contains(targetFile))
                {
                    RemoveFile(targetServer, targetFile, login, password);
                }
            }
        }

        private static void RemoveFile(string targetServer, string fileName, string login, string password)
        {
            FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(Path.Combine(targetServer, fileName));
            uploadRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            uploadRequest.Credentials = new NetworkCredential(login, password);
            FtpWebResponse response = (FtpWebResponse)uploadRequest.GetResponse();
            response.Close();
            Console.WriteLine("Remove: " + fileName);
        }
    }
}
