using UnityEngine;
using System.Collections;
using System.IO;

namespace Dreamteck
{
    public static class Utils
    {
        public static Vector3 ProjectOnLine(Vector3 fromPoint, Vector3 toPoint, Vector3 project)
        {
            Vector3 projectedPoint = Vector3.Project((project - fromPoint), (toPoint - fromPoint)) + fromPoint;
            Vector3 dir = toPoint - fromPoint;
            Vector3 projectedDir = projectedPoint - fromPoint;
            float dot = Vector3.Dot(projectedDir, dir);
            if(dot > 0f)
            {
                if(projectedDir.sqrMagnitude <= dir.sqrMagnitude) return projectedPoint;
                else return toPoint;
            } else return fromPoint;
        }

        public static string FindFolder(string dir, string folderPattern)
        {
            if (folderPattern.StartsWith("/")) folderPattern = folderPattern.Substring(1);
            if (!dir.EndsWith("/")) dir += "/";
            if (folderPattern == "") return "";
            string[] folders = folderPattern.Split('/');
            if (folders.Length == 0) return "";
            string foundDir = "";
            try
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(d);
                    if (dirInfo.Name == folders[0])
                    {
                        if(folders.Length == 1)  return d;
                        foundDir = FindFolder(d, string.Join("/", folders, 1, folders.Length-1));
                        if (foundDir != "") return foundDir;
                    } else
                    {
                        foundDir = FindFolder(d, folderPattern);
                        if (foundDir != "") return foundDir;
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Debug.LogError(excpt.Message);
                return "";
            }
            return "";
        }

        public static Texture2D LoadTexture(string dreamteckPath, string textureFileName)
        {
            string path = Application.dataPath + "/Dreamteck/"+dreamteckPath;
            if (!Directory.Exists(path))
            {
                path = FindFolder(Application.dataPath, "Dreamteck/"+dreamteckPath);
                if (!Directory.Exists(path)) return null;
            }
            if (!File.Exists(path + "/" + textureFileName)) return null;
            byte[] bytes = File.ReadAllBytes(path + "/" + textureFileName);
            Texture2D result = new Texture2D(1, 1);
            result.name = textureFileName;
            result.LoadImage(bytes);
            return result;
        }

        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path)) return null;
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D result = new Texture2D(1, 1);
            FileInfo finfo = new FileInfo(path);
            result.name = finfo.Name;
            result.LoadImage(bytes);
            return result;
        }
    }
}