using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;

namespace GBNAPI
{
    public static class Crypt
    {
        /// <summary>
        /// Шифрует строку value по ключу key. На выходе зашифрованная строка, которую можно сохранять в исходниках
        /// </summary>
        /// <param name="value">Входная строка</param>
        /// <param name="key">Ключ шифрования. Рекомендуется использовать bundle</param>
        /// <returns></returns>
        public static string GBNEncrypt(string value, string _key)
        {
            if (!string.IsNullOrEmpty(_key))
            {
                //Debug.Log(string.Format("Вход Encrypt value {0} key {1}", value, _key));
                byte[] bundleKey = UTF8Encoding.UTF8.GetBytes(_key);
                byte[] inputValue = UTF8Encoding.UTF8.GetBytes(value);

                MD5CryptoServiceProvider hashMD5Provider = new MD5CryptoServiceProvider();
                TripleDESCryptoServiceProvider trDesProvider = new TripleDESCryptoServiceProvider();
                trDesProvider.Key = hashMD5Provider.ComputeHash(bundleKey);
                trDesProvider.Mode = CipherMode.ECB;

                ICryptoTransform cTransform = trDesProvider.CreateEncryptor();
                byte[] resultByte = cTransform.TransformFinalBlock(inputValue, 0, inputValue.Length);
                trDesProvider.Clear();
                return System.Convert.ToBase64String(resultByte, 0, resultByte.Length);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Расшифровывает строку value по ключу key. На выходе расшифрованная строка, которую можно передавать методам. Расшифруется только то, что зашифрованно классов Crypt
        /// </summary>
        /// <param name="value">Входная зашифрованная строка</param>
        /// <param name="key">Ключ шифрования. Рекомендуется использовать bundle. Должен совпадать с ключом, который использовался при шифровании</param>
        /// <returns></returns>
        public static string GBNDecrypt(string value, string _key)
        {
            //Debug.Log("<color=red>!!! Decrypt value " + value + "</color>");
            if (!string.IsNullOrEmpty(_key) && !string.IsNullOrEmpty(value))
            {
                byte[] bundleKey = UTF8Encoding.UTF8.GetBytes(_key);
                byte[] inputValueEnc = System.Convert.FromBase64String(value);

                MD5CryptoServiceProvider hashMD5Provider = new MD5CryptoServiceProvider();
                TripleDESCryptoServiceProvider trDesProvider = new TripleDESCryptoServiceProvider();
                trDesProvider.Key = hashMD5Provider.ComputeHash(bundleKey);
                trDesProvider.Mode = CipherMode.ECB;

                ICryptoTransform cTransform = trDesProvider.CreateDecryptor();
                byte[] resultByte = cTransform.TransformFinalBlock(inputValueEnc, 0, inputValueEnc.Length);
                trDesProvider.Clear();
                return UTF8Encoding.UTF8.GetString(resultByte);
            }
            else
            {
                return null;
            }
        }

      
        public static string AESEncrypt(string data, string key)
        {
            if (!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(key))
            {
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] keyHash = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(key));
                byte[] keyHash128 = new byte[16];
                Array.Copy(keyHash, keyHash128, 16);

                Aes encryptor = Aes.Create();

                encryptor.Mode = CipherMode.ECB;
                encryptor.Key = keyHash128;

                MemoryStream memoryStream = new MemoryStream();
                ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

                byte[] plainBytes = UTF8Encoding.UTF8.GetBytes(data);
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();

                string cipherText = System.Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

                return cipherText;
            }
            else
            {
                return null;
            }
        }
        public static string AESDecrypt(string data, string key)
        {
            if (!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(key))
            {
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] keyHash = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(key));
                byte[] keyHash128 = new byte[16];
                Array.Copy(keyHash, keyHash128, 16);

                Aes encryptor = Aes.Create();

                encryptor.Mode = CipherMode.ECB;
                encryptor.Key = keyHash128;

                MemoryStream memoryStream = new MemoryStream();

                ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

                string plainText = String.Empty;

                try
                {
                    // Convert the ciphertext string into a byte array
                    byte[] cipherBytes = Convert.FromBase64String(data);

                    // Decrypt the input ciphertext string
                    cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                    // Complete the decryption process
                    cryptoStream.FlushFinalBlock();

                    // Convert the decrypted data from a MemoryStream to a byte array
                    byte[] plainBytes = memoryStream.ToArray();

                    // Convert the encrypted byte array to a base64 encoded string
                    plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
                }
                finally
                {
                    // Close both the MemoryStream and the CryptoStream
                    memoryStream.Close();
                    cryptoStream.Close();
                }

                // Return the encrypted data as a string
                return plainText;

            }
            else
            {
                return null;
            }
        }
    }
}
