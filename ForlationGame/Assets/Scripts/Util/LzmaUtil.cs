using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System;

public class LzmaUtil
{
    public static string DecompressFileToString(string filePath)
    {
        string inpath = filePath;

        try
        {
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            var inputFS = new MemoryStream(File.ReadAllBytes(inpath));
            MemoryStream outputFS = new MemoryStream();


            int propertiesSize = SevenZip.Compression.LZMA.Encoder.kPropSize;
            byte[] properties = new byte[propertiesSize];
            inputFS.Read(properties, 0, properties.Length);

            byte[] fileLengthBytes = new byte[8];
            inputFS.Read(fileLengthBytes, 0, 8);
            long fileLength = System.BitConverter.ToInt64(fileLengthBytes, 0);

            decoder.SetDecoderProperties(properties);
            decoder.Code(inputFS, outputFS, inputFS.Length, fileLength, null);
            outputFS.Flush();

            var r = System.Text.Encoding.UTF8.GetString(outputFS.ToArray());

            outputFS.Close();
            inputFS.Close();

            return r;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex.ToString());
        }
        return null;
    }

    public static byte[] DecompressBytes(byte[] rawData)
    {
        try
        {
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            var inputFS = new MemoryStream(rawData);
            //FileStream inputFS = new FileStream(inpath, FileMode.Open);
            //FileStream outputFS = new FileStream(outpath, FileMode.Create);
            MemoryStream outputFS = new MemoryStream();


            int propertiesSize = SevenZip.Compression.LZMA.Encoder.kPropSize;
            byte[] properties = new byte[propertiesSize];
            inputFS.Read(properties, 0, properties.Length);

            byte[] fileLengthBytes = new byte[8];
            inputFS.Read(fileLengthBytes, 0, 8);
            long fileLength = System.BitConverter.ToInt64(fileLengthBytes, 0);

            decoder.SetDecoderProperties(properties);
            decoder.Code(inputFS, outputFS, inputFS.Length, fileLength, null);
            outputFS.Flush();

            var data = outputFS.ToArray();

            outputFS.Close();
            inputFS.Close();

            return data;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex.ToString());
        }
        return null;
    }

    public static string DecompressBytesToString(byte[] rawData)
    {
        var data = DecompressBytes(rawData);
        try
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex.ToString());
        }
        return null;
    }

    public static void DecompressFileTest()
    {
        string testPath = Path.Combine(Application.dataPath, "Config/output");
        string inpath = Path.Combine(testPath, "cfg.ab");
        string outpath = Path.Combine(testPath, "cfgss.json");

        try
        {
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            FileStream inputFS = new FileStream(inpath, FileMode.Open);
            FileStream outputFS = new FileStream(outpath, FileMode.Create);

            

            int propertiesSize = SevenZip.Compression.LZMA.Encoder.kPropSize;
            byte[] properties = new byte[propertiesSize];
            inputFS.Read(properties, 0, properties.Length);

            byte[] fileLengthBytes = new byte[8];
            inputFS.Read(fileLengthBytes, 0, 8);
            long fileLength = System.BitConverter.ToInt64(fileLengthBytes, 0);

            decoder.SetDecoderProperties(properties);
            decoder.Code(inputFS, outputFS, inputFS.Length, fileLength, null);
            outputFS.Flush();
            outputFS.Close();
            inputFS.Close();
            Debug.Log("解压完毕");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public static void DecompressFileInMemoryTest()
    {
        string testPath = Path.Combine(Application.dataPath, "Config/output");
        string inpath = Path.Combine(testPath, "cfg.ab");
        string outpath = Path.Combine(testPath, "cfgss.json");

        try
        {
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            var inputFS = new MemoryStream(File.ReadAllBytes(inpath));
            //FileStream inputFS = new FileStream(inpath, FileMode.Open);
            //FileStream outputFS = new FileStream(outpath, FileMode.Create);
            MemoryStream outputFS = new MemoryStream();


            int propertiesSize = SevenZip.Compression.LZMA.Encoder.kPropSize;
            byte[] properties = new byte[propertiesSize];
            inputFS.Read(properties, 0, properties.Length);

            byte[] fileLengthBytes = new byte[8];
            inputFS.Read(fileLengthBytes, 0, 8);
            long fileLength = System.BitConverter.ToInt64(fileLengthBytes, 0);

            decoder.SetDecoderProperties(properties);
            decoder.Code(inputFS, outputFS, inputFS.Length, fileLength, null);
            outputFS.Flush();

            var r = System.Text.Encoding.UTF8.GetString(outputFS.ToArray());
            Debug.Log(">>>> " + r);

            outputFS.Close();
            inputFS.Close();
            Debug.Log("解压完毕");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
}
