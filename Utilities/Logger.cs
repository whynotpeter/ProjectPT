using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Utilities
{
    public static class Logger
    {
        private static string path = Directory.GetParent(Application.ExecutablePath) + "\\log\\";
        private static string FileName = path + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".log";

        public static void Log(string message, params object[] pO)
        {
            if (pO != null)
                message = String.Format(message, pO);
            message = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": \t" + message;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            StreamWriter file = new StreamWriter(FileName, true, Encoding.UTF8);
            file.WriteLine(message);
            file.Close();
        }

        public static void StackTrace(Exception exception, string message = "", bool inner = false)
        {
            string header;
            if (!inner)
                header = "--------- EXCEPTION ---------\r\n" + message;
            else
                header = "++++++++ INNER EXCETION ++++++\r\n" + message;
            string content = exception.Message + "\r\n" + exception.GetType() + "\r\n" + exception.StackTrace;
            string footer;
            if (!inner)
                footer = "=============================";
            else
                footer = "#############################";
            StreamWriter file = new StreamWriter(FileName, true);
            file.WriteLine(header);
            file.WriteLine(content);
            if (exception.InnerException != null)
                StackTrace(exception.InnerException, inner: true);
            file.WriteLine(footer);
            file.Close();
        }
    }
}
