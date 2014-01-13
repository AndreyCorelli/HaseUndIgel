using System.IO;

namespace HaseUndIgel.Util
{
    public static class ExecutablePath
    {
        private static string execPath;
        /// <summary>
        /// путь к исполняемому файлу без завершающего слэш
        /// </summary>
        public static string ExecPath
        {
            get
            {
                if (string.IsNullOrEmpty(execPath))
                {
                    var sm = System.Reflection.Assembly.GetEntryAssembly() ??
                             System.Reflection.Assembly.GetExecutingAssembly();
                    execPath = Path.GetDirectoryName(sm.Location);
                }
                return execPath;
            }
        }
    }
}
