﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GameEngineEditor.utilities
{
    public static class Serializer
    {
        public static void toFile<T>(T inst, string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(fs, inst);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to serialize {inst} to {path}");
                throw;
            }
        }

        public static T fromFile<T>(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    T inst = (T)serializer.ReadObject(fs);
                    return inst;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to deserialize {path}");
                throw;
            }
        }
    }
}
