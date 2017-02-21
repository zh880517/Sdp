using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser
{
    public class FileHelper
    {
        /// <summary>
        /// 扫描指定文件夹获取所有文件
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strType"></param>
        /// <returns></returns>
        public static List<string> GetAllFile(string strPath, string strType)
        {
            List<string> files = new List<string>();
            DirectoryInfo folder = new DirectoryInfo(strPath);
            FileInfo[] files2 = folder.GetFiles(strType, SearchOption.AllDirectories);
            for (int i = 0; i < files2.Length; i++)
            {
                FileInfo file = files2[i];
                files.Add(file.FullName);
            }
            return files;
        }

        /// <summary>
        /// 根据一个源文件路径获取目标相对路径文件的全路径
        /// </summary>
        /// <param name="strSourceFile"></param>
        /// <param name="strTargetFile"></param>
        /// <returns></returns>
        public static string GetTargetFileBySourceFile(string strSourceFile, string strTargetFile)
        {
            string strCurPath = Path.GetDirectoryName(strSourceFile);
            return Path.GetFullPath(Path.Combine(strCurPath, strTargetFile));
        }

        /// <summary>
        /// 计算相对路径
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="desPath"></param>
        /// <returns></returns>
        public static string CalRelativePatch(string srcPath, string desPath)
        {
            var path1Array = srcPath.Split('\\', '/');
            var path2Array = desPath.Split('\\', '/');
            if (Path.IsPathRooted(srcPath) || Path.IsPathRooted(desPath))
            {
                if (path1Array[0] != path2Array[0])
                    return desPath;
            }

            int s = path1Array.Length >= path2Array.Length ? path2Array.Length : path1Array.Length;
            //两个目录最底层的共用目录索引
            int closestRootIndex = -1;
            for (int i = 0; i < s; i++)
            {
                if (path1Array[i] == path2Array[i])
                {
                    closestRootIndex = i;
                }
                else
                {
                    break;
                }
            }
            //由path1计算 ‘../'部分
            string path1Depth = "";
            for (int i = 0; i < path1Array.Length; i++)
            {
                if (i > closestRootIndex + 1)
                {
                    path1Depth += "../";
                }
            }
            //由path2计算 ‘../'后面的目录
            string path2Depth = "";
            for (int i = closestRootIndex + 1; i < path2Array.Length; i++)
            {
                path2Depth += "/" + path2Array[i];
            }
            path2Depth = path2Depth.Substring(1);
            return path1Depth + path2Depth;
        }

        public static string TranPath(string path)
        {
            string outPath = Path.GetFullPath(path).Replace(Path.DirectorySeparatorChar, '/');
            if (outPath[outPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                outPath = outPath + "/";
            }
            return outPath;
        }
    }
}
