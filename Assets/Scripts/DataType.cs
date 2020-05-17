using System;
using System.Collections.Generic;

namespace JaniMJ.DataManager.Settings
{
    //TODO Cloud
    [Serializable]
    public class DataWrap
    {
        public List<DataType> DataTypesList;
    }
    [Serializable]
    public class DataType
    {
        public string Class;
        public string FileName;
        public string Path;
        public string Extension;
        //public string WebLocation;
        public DataHash Hash;
        //public DataLocation Location;
        public LocalLocation LocalLoc;
        public DataEncryption Encryption;
        public DataModel Model;
        public bool JsonPrettyPrint;
    }

    [Serializable]
    public enum LocalLocation
    {
        PersistentDataPath,
        ResourcesPath,
        CustomPath
    }

    /*[Serializable]
    public enum DataLocation
    {
        Local,
        Cloud
    }*/

    [Serializable]
    public enum DataHash
    {
        None,
        Sha256,
        Sha512,
        MD5
    }

    [Serializable]
    public enum DataEncryption
    {
        None,
        XOR,
        //Aes256
    }

    [Serializable]
    public enum DataModel
    {
        Json,
        Binary,
        XML
    }
}

