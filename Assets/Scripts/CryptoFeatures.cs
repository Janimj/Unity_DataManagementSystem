using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using JaniMJ.DataManager.Settings;
using UnityEditor.UI;

namespace JaniMJ.DataManager
{
    public static class CryptoFeatures
    {
        public static string Hash(string toHash, DataHash hashType)
        {
            byte[] input = Encoding.ASCII.GetBytes(toHash);
            byte[] hash = new byte[0];
            StringBuilder sb = new StringBuilder();
            if (hashType == DataHash.MD5)
            {
                MD5 md = MD5.Create();
                hash = md.ComputeHash(input);
            }
            else if (hashType == DataHash.Sha256)
            {
                SHA256 sha = SHA256.Create();
                hash = sha.ComputeHash(input);
            }
            else
            {
                SHA512 sha = SHA512.Create();
                hash = sha.ComputeHash(input);
            }
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static bool CompareHash(string hashToCompare, string data, DataHash hashType)
        {
            return hashToCompare.Equals(Hash(data, hashType));
        }

        public static string SimpleXor(string toProcess, int key)
        {
            StringBuilder sbOut = new StringBuilder(toProcess.Length);
            //StringBuilder sbIn = new StringBuilder(toProcess);

            char c;
            for (int i = 0; i < toProcess.Length; i++)
            {
                c = toProcess[i];
                c = (char) (c ^ key);
                sbOut.Append(c);
            }

            return sbOut.ToString();
        }

        public static int StringToInt(string toConvert)
        {
            int temp = 0;
            try
            {
                temp = int.Parse(toConvert);
            }
            catch (Exception)
            {
                for (int i = 0; i < toConvert.Length; i++)
                {
                    temp += toConvert[i];
                }
            }
            return temp;
        }
    }
}
