using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.gameProject.utilities
{
    public class FileHelper
    {
        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // The file is not locked; it can be accessed.
                    return false;
                }
            }
            catch (IOException)
            {
                // The file is locked by another process.
                return true;
            }
        }
    }
}
