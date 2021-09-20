using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class SecurityUtil
{
    public static string Md5File(string filePath)
    {
        return Md5File(filePath, 0);
    }

    public static string Md5File(string filePath, int size)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                string result = BitConverter.ToString(hash).Replace("-", "").ToLower();
                if (size > 0)
                {
                    return result.Substring(0, size);
                }
                else
                {
                    return result;
                }
            }
        }
    }

    public static string Md5(byte[] data)
    {
        return Md5(data, 0);
    }

    public static string Md5(byte[] data, int size)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(data);
            string result = BitConverter.ToString(hash).Replace("-", "").ToLower();
            if (size > 0)
            {
                return result.Substring(0, size);
            }
            else
            {
                return result;
            }
        }
    }

    public static byte[] Xor(byte[] data, string key)
    {
        byte[] newData = new byte[data.Length];
        Array.Copy(data, newData, data.Length);
        int length = newData.Length;
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
        int keyLength = keyArray.Length;
        int i = 0;
        int k = 0;
        while (i < length)
        {
            newData[i] ^= keyArray[k];
            i++;
            k++;
            if (k >= keyLength)
            {
                k = 0;
            }
        }
        return newData;
    }

    public static void XorFile(string srcFilePath, string dstFilePath, string key)
    {
        byte[] data = File.ReadAllBytes(srcFilePath);
        data = Xor(data, key);
        File.WriteAllBytes(dstFilePath, data);
    }
}
