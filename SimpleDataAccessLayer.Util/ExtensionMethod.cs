using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleDataAccessLayer.Util
{
    public static class ExtensionMethod
    {
        public static void WriteLog(string text, string rootPath)
        {
            string _path = Path.Combine(rootPath, "Log");
            string _filePath = string.Format(@"{0}\{1}.txt", _path, DateTime.Now.ToString("dd_MM_yyyy"));

            DirectoryInfo diretorio = new DirectoryInfo(_path);
            if (!diretorio.Exists)
                diretorio.Create();

            while (true)
            {
                try
                {
                    StreamWriter _streamWriter = new StreamWriter(_filePath, true);
                    _streamWriter.WriteLine(string.Format("App : {0} - Horário : {1} ", System.AppDomain.CurrentDomain.FriendlyName, DateTime.Now.ToString("hh:mm:ss")));
                    _streamWriter.WriteLine(text);
                    _streamWriter.WriteLine("-------------------------------------------------------------------------------------------------------------------");
                    _streamWriter.Close();
                    break;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(4000);
                }
            }
        }

        public static void WriteLog(this Exception ex, string rootPath)
        {
            StringBuilder _text = new StringBuilder();

            _text.AppendLine(string.Format("Mensagem : {0}", ex.Message));
            _text.AppendLine(string.Format("StackTrace : {0}", ex.StackTrace));
            _text.AppendLine(string.Format("Source : {0}", ex.Source));
            WriteLog(_text.ToString(), rootPath);
        }

        public static bool FileInUse(this FileInfo fileInfo)
        {
            StreamWriter _streamWriter = null;

            try
            {
                fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                if (fileInfo.Exists)
                    _streamWriter = new StreamWriter(fileInfo.FullName, true);
                else
                    return false; 
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (_streamWriter != null)
                    _streamWriter.Close();
            }

            return false;
        }

        public static bool FileInUse(this string filePath)
        {
            return FileInUse(new FileInfo(filePath));
        }

        public static void ClearDirectory(string directoryPath)
        {
            System.IO.DirectoryInfo _directoryInfo = new DirectoryInfo(directoryPath);
            foreach (FileInfo _fileInfo in _directoryInfo.GetFiles())
            {
                _fileInfo.Delete();
            }
            foreach (DirectoryInfo _directory in _directoryInfo.GetDirectories())
            {
                _directory.Delete(true);
            }
        }

        public static string GetDescription(this object obj)
        {
            DescriptionAttribute _attribute = (DescriptionAttribute)obj.GetType().GetField(obj.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            return _attribute != null ? _attribute.Description : string.Empty;
        }

        public static Type GetFieldType(string textType)
        {
            Type _returnType;

            switch (textType.ToLower())
            {
                case "varchar":
                    _returnType = typeof(string);
                    break;
                case "int":
                    _returnType = typeof(int);
                    break;
                case "datetime":
                    _returnType = typeof(DateTime);
                    break;
                default:
                    _returnType = typeof(string);
                    break;

            }
            return _returnType;
        }

        public static void ConvertFileEncoding(String sourcePath, String destinyPath, Encoding sourceEncoding, Encoding destinyEncoding)
        {
            string _parent = Path.GetDirectoryName(Path.GetFullPath(destinyPath));
            if (!Directory.Exists(_parent))
                Directory.CreateDirectory(_parent);

            if (sourceEncoding == destinyEncoding)
            {
                File.Copy(sourcePath, destinyPath, true);
                return;
            }

            string _tempName = null;
            try
            {
                _tempName = Path.GetTempFileName();
                using (StreamReader _streamReader = new StreamReader(sourcePath, sourceEncoding, false))
                {
                    using (StreamWriter _streamWriter = new StreamWriter(_tempName, false, destinyEncoding))
                    {
                        int charsRead;
                        char[] buffer = new char[128 * 1024];
                        while ((charsRead = _streamReader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                        {
                            _streamWriter.Write(buffer, 0, charsRead);
                        }
                    }
                }
                File.Delete(destinyPath);
                File.Move(_tempName, destinyPath);
            }
            finally
            {
                File.Delete(_tempName);
            }
        }

        /// <summary>
        /// Converts a file from one encoding to another.
        /// </summary>
        /// <param name="sourcePath">the file to convert</param>
        /// <param name="sourceEncoding">the original file encoding</param>
        /// <param name="destEncoding">the encoding to which the contents should be converted</param>
        public static string ConvertFileEncoding(String sourcePath, Encoding sourceEncoding, Encoding destinyEncoding)
        {
            if (sourceEncoding == destinyEncoding)
            {
                return sourcePath;
            }
            string fileDest = Path.GetFileNameWithoutExtension(sourcePath);
            string pathDest = Path.GetDirectoryName(Path.GetFullPath(sourcePath));
            string pathFileDest = pathDest + "\\" + fileDest + "_enc_" + destinyEncoding.CodePage.ToString() + Path.GetExtension(sourcePath);
            // Convert the file.
            string _tempName = null;
            try
            {
                _tempName = Path.GetTempFileName();
                using (StreamReader _streamReader = new StreamReader(sourcePath, sourceEncoding, false))
                {
                    using (StreamWriter _streamWriter = new StreamWriter(_tempName, false, destinyEncoding))
                    {
                        int charsRead;
                        char[] buffer = new char[128 * 1024];
                        while ((charsRead = _streamReader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                        {
                            _streamWriter.Write(buffer, 0, charsRead);
                        }
                    }
                }
                File.Delete(pathFileDest);
                File.Move(_tempName, pathFileDest);
            }
            finally
            {
                File.Delete(_tempName);
            }
            return pathFileDest;
        }

        public static T ConvertBaseToSubType<T>(this object obj)
        {
            T _returnObject = (T)Activator.CreateInstance(typeof(T));

            _returnObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList().ForEach(a => 
            {
                if (obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(b => b.Name.Equals(a.Name)).Any())
                {
                    PropertyInfo _propertyInfo = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(b => b.Name.Equals(a.Name)).FirstOrDefault();
                     a.SetValue(_returnObject, _propertyInfo.GetValue(obj));
                }
            });


            return _returnObject;
        }
    }
}
