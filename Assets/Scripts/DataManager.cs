using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using JaniMJ.DataManager.Settings;

namespace JaniMJ.DataManager.Local
{
    public static class DataManager
    {
        public static DataWrap DataTypes;
        public static Action<DataType> OnHashMismatch;
        public static Action<DataType> OnHashMatch;
        public static Action<DataType> OnHashMissing;
        /// <summary>
        /// Initialize
        /// </summary>
        static DataManager()
        {
            if (DataTypes == null)
            {
                LoadDataTypes();
            }
        }

        /// <summary>
        /// Loads the Settings.json file that contains the settings and populates the DataTypes variable
        /// </summary>
        public static void LoadDataTypes()
        {
            try
            {
                string data = File.ReadAllText(Path.Combine(Application.dataPath + "/", "Settings.json"));
                DataTypes = JsonUtility.FromJson<DataWrap>(data);
            }
            catch (Exception e)
            {
                Debug.Log("Unable to load datatypes " + e);
                Debug.Log("Initializing empty wrapper");
                DataTypes = new DataWrap();
                DataTypes.DataTypesList = new List<DataType>();
            }
        }

        /// <summary>
        /// Saves the Settings.json file
        /// </summary>
        public static void SaveDataTypes()
        {
            try
            {
                string data = JsonUtility.ToJson(DataTypes);
                string path = Path.Combine(Application.dataPath + "/", "Settings.json");
                File.WriteAllText(path, data);
            }
            catch (Exception e)
            {
                Debug.Log("Unable to save datatypes " + e);
            }
        }


        /// <summary>
        /// Gets the asset name and return it as a string if its a valid type managed by the datamanager
        /// </summary>
        /// <typeparam name="T"> Object to get name of </typeparam>
        /// <returns> File name </returns>
        public static string GetAssetName<T>()
        {
            // Unity Resouces.Load wont work properly with file extensions so Filename is used without the extension
            var types = DataTypes.DataTypesList;
            var t = typeof(T).ToString();
            DataType dataType = TryGetType(typeof(T));
            return dataType != null ? dataType.FileName : "";
        }

        /// <summary>
        /// Get path of the asset if its a valid type managed by the datamanager
        /// </summary>
        /// <typeparam name="T"> Object to get path to </typeparam>
        /// <returns> Path to object </returns>
        public static string GetPath(DataType dataType)
        {
            string path = "";
            if (dataType == null) return path;
            if (dataType.LocalLoc == LocalLocation.CustomPath)
            {
                path = Path.Combine(dataType.Path + "/", dataType.FileName + dataType.Extension);
            }
            else if (dataType.LocalLoc == LocalLocation.PersistentDataPath)
            {
                path = Path.Combine(Application.persistentDataPath + "/", dataType.FileName + dataType.Extension);
            }
            else
            {
                path = Path.Combine(Application.dataPath + "/", "Resources/" + dataType.FileName + dataType.Extension);
            }
            return path;
        }


        //TODO Runtime loading
        /// <summary>
        /// Allows loading data in runtime of a build
        /// </summary>
        /// <typeparam name="T"> Data type to load </typeparam>
        /// <returns> Data loaded </returns>
        /*public static T LoadTextResources<T>()
        {
            try
            {
                var wrapper = JsonUtility.FromJson<T>((Resources.Load(GetAssetName<T>()) as TextAsset).ToString());
                Resources.UnloadUnusedAssets();
                return (T)Convert.ChangeType(wrapper, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }*/

        /// <summary>
        /// Loads data of given type and returns object of the type given
        /// </summary>
        /// <typeparam name="T"> Type to load </typeparam>
        /// <returns> Type object </returns>
        public static T LoadData<T>()
        {
            try
            {
                DataType dataType = TryGetType((T)Activator.CreateInstance(typeof(T)));

                if (dataType == null)
                {
                    throw new FileNotFoundException("Could not work with the data type");
                }

                string data = File.ReadAllText(GetPath(dataType));

                data = ValidateAndExtractHash(dataType, data);

                return DeSerialize<T>(dataType.Model, data);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load data: " + e);
                return default;
            }
        }

        /// <summary>
        /// Loads encrypted data of given type and returns object of the type given
        /// </summary>
        /// <typeparam name="T"> Type to load </typeparam>
        /// <param name="key"> Encryption key </param>
        /// <returns> Type object </returns>
        public static T LoadData<T>(string key)
        {
            try
            {
                DataType dataType = TryGetType((T)Activator.CreateInstance(typeof(T)));

                if (dataType == null)
                {
                    throw new FileNotFoundException("Could not work with the data type");
                }

                string data = File.ReadAllText(GetPath(dataType));
                if (dataType.Hash != DataHash.None)
                {
                    data = ValidateAndExtractHash(dataType, data, key);
                }
                else
                {
                    data = CryptoFeatures.SimpleXor(data, CryptoFeatures.StringToInt(key));
                }
                
                return DeSerialize<T>(dataType.Model, data);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load data: " + e);
                return default;
            }
        }

        /// <summary>
        /// Saves the data to given location with the given serialization type
        /// </summary>
        /// <typeparam name="T">Type of the object to save</typeparam>
        /// <param name="data">Object of type T to save</param>
        public static void SaveData<T>(T data)
        {
            try
            {
                DataType dataType = TryGetType(data);
                if (dataType == null)
                {
                    throw new FileNotFoundException("Data type could not be found.");
                }

                string path = GetPath(dataType);

                string dataString = Serialize(data, dataType);
                string hash = GetHash(dataType.Hash, dataString);
                if (hash != "") hash = "Hash:" + hash;
                File.WriteAllText(path, dataString + hash);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to save data: " + e);
            }
        }

        /// <summary>
        /// Save encrypted data with given key to given location with the given serialization type
        /// </summary>
        /// <typeparam name="T">Type of the object to save</typeparam>
        /// <param name="data">Object of type T to save</param>
        /// <param name="key">Key to encrypt the data with</param>
        public static void SaveData<T>(T data, string key)
        {
            try
            {
                DataType dataType = TryGetType(data);
                if (dataType == null)
                {
                    throw new FileNotFoundException("Data type could not be found.");
                }

                string dataString = Serialize(data, dataType);
                string hash = GetHash(dataType.Hash, dataString);
                dataString = CryptoFeatures.SimpleXor(dataString, CryptoFeatures.StringToInt(key));
                string path = GetPath(dataType);

                if (hash != "") hash = "Hash:" + hash;
                File.WriteAllText(path, dataString + hash);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to save data: " + e);
            }

        }

        /// <summary>
        /// Serializes the data according to the settings for the data type
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize</typeparam>
        /// <param name="data">Data to serialize</param>
        /// <param name="dataType">DataType object</param>
        /// <returns>Serialized data</returns>
        private static string Serialize<T>(T data, DataType dataType)
        {
            string dataString;
            if (dataType.Model == DataModel.Json)
            {
                dataString = JsonUtility.ToJson(data, dataType.JsonPrettyPrint);
            }
            else if (dataType.Model == DataModel.Binary)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, data);
                    dataString = Convert.ToBase64String(ms.ToArray());
                }
            }
            else
            {
                XmlSerializer xml = new XmlSerializer(data.GetType());
                using (StringWriter writer = new StringWriter())
                {
                    xml.Serialize(writer, data);
                    dataString = writer.ToString();
                }
            }

            return dataString;
        }
        /// <summary>
        /// Deserializes the data according to the settings for the data type
        /// </summary>
        /// <typeparam name="T">Type of the object to deserialize</typeparam>
        /// <param name="model">Data format of the data type</param>
        /// <param name="data">The data to deserialize</param>
        /// <returns>Deserialized data</returns>
        private static T DeSerialize<T>(DataModel model, string data)
        {
            switch (model)
            {
                default:
                case DataModel.Json:
                {
                    var jsonData = JsonUtility.FromJson<T>(data);
                    return (T) Convert.ChangeType(jsonData, typeof(T));
                }
                case DataModel.Binary:
                {
                    byte[] bytes = Convert.FromBase64String(data);
                    BinaryFormatter bf = new BinaryFormatter();
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        return (T) bf.Deserialize(ms);
                    }
                }
                case DataModel.XML:
                {
                    XmlSerializer xml = new XmlSerializer(typeof(T), new XmlRootAttribute(typeof(T).ToString()));
                    StringReader reader = new StringReader(data);
                    return (T) xml.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Gets a hash of given data with the given algorithm
        /// </summary>
        /// <param name="hashType">Algorithm to use</param>
        /// <param name="dataString">Data to hash</param>
        /// <returns>Hash for the data</returns>
        private static string GetHash(DataHash hashType, string dataString)
        {
            string hash = "";
            if (hashType != DataHash.None)
            {
                hash = CryptoFeatures.Hash(dataString, hashType);
            }

            return hash;
        }

        /// <summary>
        /// If the file contains a hash, will remove it from the data string and compare it to the data
        /// </summary>
        /// <param name="dataType"> Type of data </param>
        /// <param name="data"> Data string </param>
        /// <returns> Data string without hash </returns>
        private static string ValidateAndExtractHash(DataType dataType, string data)
        {
            if (dataType.Hash == DataHash.None) return data;
            if (!data.Contains("Hash:"))
            {
                OnHashMissing?.Invoke(dataType);
                return data;
            }

            if (data.Length - data.IndexOf("Hash:") < 10)
            {
                OnHashMissing?.Invoke(dataType);
                return data;
            }

            string hash = data.Substring(data.IndexOf("Hash:"));
            data = data.Replace("Hash:", "");
            hash = hash.Remove(0, 5);

            data = data.Replace(hash, "");

            if (CryptoFeatures.CompareHash(hash, data, dataType.Hash))
            {
                OnHashMatch?.Invoke(dataType);
            }
            else
            {
                OnHashMismatch?.Invoke(dataType);
            }
            return data;
        }

        /// <summary>
        /// If data consists of both hash and encryption, this will validate the hash and decrypt the data
        /// </summary>
        /// <param name="dataType">Type of data</param>
        /// <param name="data">The Encrypted data with hash</param>
        /// <param name="key">Key to decrypt the data with</param>
        /// <returns>Decrypted data string</returns>
        private static string ValidateAndExtractHash(DataType dataType, string data, string key)
        {
            if (dataType.Hash == DataHash.None) return data;
            if (!data.Contains("Hash:"))
            {
                OnHashMissing?.Invoke(dataType);
                return data;
            }

            if (data.Length - data.IndexOf("Hash:") < 10)
            {
                OnHashMissing?.Invoke(dataType);
                return data;
            }

            string hash = data.Substring(data.IndexOf("Hash:"));
            hash = hash.Remove(0, 5);
            data = data.Replace("Hash:" + hash, "");
            data = CryptoFeatures.SimpleXor(data, CryptoFeatures.StringToInt(key));
            if (CryptoFeatures.CompareHash(hash, data, dataType.Hash))
            {
                OnHashMatch?.Invoke(dataType);
            }
            else
            {
                OnHashMismatch?.Invoke(dataType);
            }
            return data;
        }

        /// <summary>
        /// Takes in a type and checks if it is a existing type within the datasystem
        /// </summary>
        /// <typeparam name="T">Type to get</typeparam>
        /// <param name="type">Object of to Type T</param>
        /// <returns>DataType object related to type T or null if no type was found</returns>
        public static DataType TryGetType<T>(T type)
        {
            var t = typeof(T).ToString();
            var types = DataTypes.DataTypesList;

            DataType ret = null;
            for (int i = 0; i < types.Count; i++)
            {
                if (t.Equals(types[i].Class))
                {
                    ret = types[i];
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// https://answers.unity.com/questions/206665/typegettypestring-does-not-work-in-unity.html
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public static Type GetType(string TypeName)
        {

            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = Type.GetType(TypeName);

            // If it worked, then we're done here
            if (type != null)
                return type;

            // If the TypeName is a full name, then we can try loading the defining assembly directly
            if (TypeName.Contains("."))
            {

                // Get the name of the assembly (Assumption is that we are using 
                // fully-qualified type names)
                var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

                // Attempt to load the indicated Assembly
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                    return null;

                // Ask that assembly to return the proper Type
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;

            }

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {

                // Load the referenced assembly
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    // See if that assembly defines the named type
                    type = assembly.GetType(TypeName);
                    if (type != null)
                        return type;
                }
            }

            // The type just couldn't be found...
            return null;

        }
    }

}

