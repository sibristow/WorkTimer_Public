using System;
using System.IO;
using System.Text;

namespace WorkTimer4
{
    internal static class ExceptionLogger
    {
        public static string LogException(Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var file = string.Format("{0}-report-{1}.log", AssemblyInfo.ProductName, DateTime.Now.ToString("yyyyMMMdd_hhmmss"));
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WorkTimer", file);

            var e = exception;
            var sb = new StringBuilder();
            while (e!= null)
            {
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                sb.AppendLine();

                e = e.InnerException;
            }

            File.WriteAllText(logPath, sb.ToString());

            return logPath;
        }
    }
}
