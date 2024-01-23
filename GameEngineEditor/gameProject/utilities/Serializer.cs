using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GameEngineEditor.gameProject.utilities
{
    public static class Serializer
    {
        public static void toFile<T>(T inst, string path)
        {
            try
            {
                var fs = new FileStream(path, FileMode.Create);
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(fs, inst);
            }
            catch(Exception ex) 
            { 
                Debug.WriteLine(ex.Message);
            }
        }

        public static T fromFile<T>(string path)
        {
            try
            {
                var fs = new FileStream(path, FileMode.Open);
                var serializer = new DataContractSerializer(typeof(T));
                T inst = (T)serializer.ReadObject(fs);
                return inst;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return default(T);
            }
        }
    }
}
