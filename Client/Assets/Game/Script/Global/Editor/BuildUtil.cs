#region Header
/**
 * 名称：BuildUtil
 
 * 日期：2015.12.4
 * 描述：一些和编译相关的工具
 **/
#endregion
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Callbacks;

public class BuildUtil 
{
    [MenuItem("Tool/打包/single")]
    public static void SetSingle()
    {
        PlayerSettings.productName = "GOWSingle";
        PlayerSettings.bundleIdentifier = "com.Empty.GOWSingle";
        GameObject go =GameObject.Find("Main");
        if(go ==null)return;
        go.GetComponent<Main>().isSingle =true;
    }

    [MenuItem("Tool/打包/normal")]
    public static void SetNormal()
    {
        PlayerSettings.productName = "GOW";
        PlayerSettings.bundleIdentifier = "com.Empty.GOW";
        GameObject go = GameObject.Find("Main");
        if (go == null) return;
        go.GetComponent<Main>().isSingle = false;
    }

    [MenuItem("Tool/打包/设置需要加载的场景")]
    public static void SetBuildSettingsScene()
    {
        
        //EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
        List<EditorBuildSettingsScene> ebss = new List<EditorBuildSettingsScene>();

        //ebss.Add(new EditorBuildSettingsScene("Assets/Game/main.unity", true));
        //int i = Application.dataPath.Length-6;
        //string[] files = System.IO.Directory.GetFiles(Application.dataPath + "/Scene/levels", "*.unity", System.IO.SearchOption.AllDirectories);
        //foreach(string f in files){
        //    if(f.IndexOf("_G.unity")!=-1)
        //        continue;
        //    ebss.Add(new EditorBuildSettingsScene(f.Substring(i), true));
        //}

        foreach (var s in GetScenes())
        {
            ebss.Add(new EditorBuildSettingsScene(s, true));
        }
        
        EditorBuildSettings.scenes = ebss.ToArray();
    }

    public static List<string> GetScenes()
    {
        List<string> l = new List<string>();
        l.Add("Assets/Game/main.unity");

        if(RoomCfg.mAllRoomList.Count==0)
            RoomCfg.Init();
        foreach (var c in RoomCfg.mAllRoomList)
        {
            foreach (var sceneId in c.sceneId)
            {
                if (string.IsNullOrEmpty(sceneId))
                    continue;
                var s = "Assets/Scene/levels/" + sceneId + ".unity";
                if (l.Contains(s))
                    continue;
                l.Add(s);
            }
        }
        return l;
    }
    [MenuItem("Tool/打包/打包")]
    public static void Build()
    {
        string path = Build(null);
        if (!string.IsNullOrEmpty(path))
            Application.OpenURL(path);
    }

    //批处理调用的打包
    public static void BuildByCommonline()
    {
        //外部传进来的参数
        string[] ps = new string[0];
        string[] para = Environment.GetCommandLineArgs();
        int args_start = System.Array.FindLastIndex<string>(para, (string f) => { return f == "-args" ? true : false; });
        if (args_start != -1)
        {
            List<string> args = new List<string>();
            for (int i = args_start + 1; i < para.Length; ++i)
            {
                if (para[i].StartsWith("-"))//自己的参数不带-前缀
                    break;

                args.Add(para[i]);
            }
            ps =args.ToArray();
        }

        Build(ps.Length == 0 ? null : ps[0]);
        EditorApplication.Exit(0);
    }

    static string Build(string locationPathName)
    {
        try
        {
            //设置需要加载的场景
            //List<string> scenes = new List<string>();
            //scenes.Add("Assets/Game/main.unity");
            //int i = Application.dataPath.Length - 6;
            //string[] files = System.IO.Directory.GetFiles(Application.dataPath + "/Scene/levels", "*.unity", System.IO.SearchOption.AllDirectories);
            //foreach (string f in files)
            //{
            //    scenes.Add(f.Substring(i));
            //}
            List<string> scenes = GetScenes();


           // PlayerSettings.bundleIdentifier

            //如果没有指定路径或者路径指定在了asset下，那么放在和asset同级
            string defaultPath =Application.dataPath.Substring(0,Application.dataPath.Length-7);
            if(string.IsNullOrEmpty(locationPathName)||locationPathName.IndexOf(defaultPath) != -1)
                locationPathName =defaultPath +"/bin";
            locationPathName = locationPathName.Replace("\\", "/");

            //如果是文件，那么拿文件名当文件夹名
            if(!string.IsNullOrEmpty(Path.GetExtension(locationPathName)))
            {
                locationPathName = locationPathName.Substring(0,locationPathName.Length - Path.GetExtension(locationPathName).Length);
            }
            
            //删掉老的文件夹，创建一个新的
            if (Directory.Exists(locationPathName))
                Directory.Delete(locationPathName, true);
            Directory.CreateDirectory(locationPathName);

#if UNITY_ANDROID
            string fileName = locationPathName + "/GOW.apk";
            BuildTarget buildTarget = BuildTarget.Android;
#endif

#if UNITY_IPHONE
             string fileName = locationPathName;//ios上选择的是路径
             BuildTarget buildTarget = BuildTarget.iOS;
#endif

#if UNITY_STANDALONE_WIN
            string fileName = locationPathName + "/GOW.exe";
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
#endif
        
            string errorMsg = BuildPipeline.BuildPlayer(scenes.ToArray(), fileName, buildTarget, BuildOptions.None);
            if (!string.IsNullOrEmpty(errorMsg))//|| !System.IO.File.Exists(fileName)
            {
                Debug.LogError("errorMsg:" + errorMsg);
            }
            return locationPathName;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    
        return null;
    }
    

    #region pc打包ipa的时候要做的特殊操作
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
#if UNITY_IPHONE
        BuildIpa(pathToBuiltProject);
#endif        
    }

    static void FileCopy(string src, string dst)
    {
        if (File.Exists(dst))
            File.Delete(dst);

        File.Copy(src, dst);
    }
    static public void BuildIpa(string pathToBuiltProject)
    {
#if UNITY_EDITOR_WIN


        System.Diagnostics.Process p = new System.Diagnostics.Process();
        FileCopy(Application.dataPath + "/../iOS/BuildIOS.bat", pathToBuiltProject + "/BuildIOS.bat");
        FileCopy(Application.dataPath + "/../iOS/iOS.mobileprovision", pathToBuiltProject + "/iOS.mobileprovision");
        FileCopy(Application.dataPath + "/../iOS/test.bat", pathToBuiltProject + "/test.bat");
        p.StartInfo.FileName = pathToBuiltProject + "/BuildIOS.bat";

        string ipaname = PlayerSettings.iPhoneBundleIdentifier.Substring(PlayerSettings.iPhoneBundleIdentifier.LastIndexOf('.') + 1);

        string copyname = pathToBuiltProject.Substring(pathToBuiltProject.LastIndexOf('/') + 1);
        if (copyname.ToLower().EndsWith(".ipa"))
            copyname = copyname.Substring(0, copyname.Length - 4);

        string currentpath = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(pathToBuiltProject);

        // 参数1 xcode路径
        // 参数2 打包出来的ipa名
        string cmd = string.Format("\"{0}\" \"{1}\"", pathToBuiltProject, ipaname);
        p.StartInfo.Arguments = cmd;
        p.Start();
        p.WaitForExit();
        p.Close();
        Directory.SetCurrentDirectory(currentpath);

        if (File.Exists(pathToBuiltProject + "/Packages/" + ipaname + ".ipa"))
        {
            File.Delete(pathToBuiltProject + "/BuildIOS.bat");
            File.Delete(pathToBuiltProject + "/iphone.mobileprovision");
            File.Delete(pathToBuiltProject + "/test.bat");

            if (File.Exists(pathToBuiltProject + "/../" + copyname + ".ipa"))
                File.Delete(pathToBuiltProject + "/../" + copyname + ".ipa");

            File.Copy(pathToBuiltProject + "/Packages/" + ipaname + ".ipa", pathToBuiltProject + "/../" + copyname + ".ipa");
        }
#else
        Debug.LogError("平台不对，不能打包");
#endif
    }
    
    #endregion


}
