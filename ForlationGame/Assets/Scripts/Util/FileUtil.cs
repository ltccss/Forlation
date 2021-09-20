using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileUtil
{
    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public static byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public static void WriteAllText(string path, string text)
    {
        File.WriteAllText(path, text);
    }

    public static void WriteAllBytes(string path, byte[] data)
    {
        File.WriteAllBytes(path, data);
    }

    public static bool IsFileExist(string path)
    {
        return File.Exists(path);
    }

    public static bool isDirectoryExist(string path)
    {
        return Directory.Exists(path);
    }

    public static void CreateDirectory(string dirPath)
    {
        Directory.CreateDirectory(dirPath);
    }

    public static void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public static void MoveFile(string oldPath, string newPath)
    {
        File.Move(oldPath, newPath);
    }

    public static void CopyFile(string oldPath, string newPath, bool overwrite)
    {
        File.Copy(oldPath, newPath, overwrite);
    }

    public static void DeleteDirectory(string path, bool recursive)
    {
        Directory.Delete(path, recursive);
    }

    public static void CopyDirectory(string srcPath, string dstPath)
    {
        if (!Directory.Exists(dstPath))
        {
            Directory.CreateDirectory(dstPath);
        }
        string[] files = Directory.GetFiles(srcPath);

        foreach (string file in files)
        {
            string pFilePath = Path.Combine(dstPath, Path.GetFileName(file));
            File.Copy(file, pFilePath, true);
        }

        string[] dirs = Directory.GetDirectories(srcPath);
        foreach (string dir in dirs)
        {
            CopyDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
        }
    }

    public static string[] GetFilesNoRecursive(string path)
    {
        return Directory.GetFiles(path);
    }

    public static string[] GetDirectoriesNoRecursive(string path)
    {
        return Directory.GetDirectories(path);
    }

    public static string[] GetFiles(string path)
    {
        List<string> fileList = new List<string>();
        _GetFiles(path, fileList);
        return fileList.ToArray();
    }

    private static void _GetFiles(string path, List<string> resultList)
    {
        DirectoryInfo folder = new DirectoryInfo(path);

        // 遍历文件
        foreach (FileInfo cFile in folder.GetFiles())
        {
            resultList.Add(cFile.FullName);
        }

        // 遍历文件夹
        foreach (DirectoryInfo cFolder in folder.GetDirectories())
        {
            _GetFiles(cFolder.FullName, resultList);
        }
    }
}
