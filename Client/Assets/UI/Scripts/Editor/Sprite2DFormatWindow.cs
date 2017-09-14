
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class Sprite2DFormatWindow : EditorWindow
{
    public static void ShowWnd()
    {
        Sprite2DFormatWindow wnd = EditorWindow.GetWindow<Sprite2DFormatWindow>();
        wnd.minSize = new Vector2(300.0f, 200.0f);
        wnd.titleContent = new GUIContent("ui图片格式设置");
        wnd.autoRepaintOnSceneChange = true;
        wnd.m_texs.Clear();
        wnd.m_atlasName = "";
        wnd.m_prefix = "";

        //找到所有选中的图片
        TextureImporter textureImporter;
        //foreach (UnityEngine.Object o in Selection.objects)
        //{
        //    string path =AssetDatabase.GetAssetPath(o);
        foreach (string o in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(o);
            if (string.IsNullOrEmpty(path))
                continue;
            if (File.Exists(path))
            {
                // 是个文件
                textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                wnd.Add(textureImporter);
            }
            else
            {
                path = path.Substring(7);//把asset/去掉
                List<string> files= Util.GetAllFileList(Application.dataPath + "/" + path, "Assets/"+path + "/");
                foreach (string assetPath in files)
                {
                    textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    wnd.Add(textureImporter);
                }
            }
        }
    }

    public List<TextureImporter> m_texs=new List<TextureImporter>();
    public string m_atlasName ="";//图集名字
    public string m_prefix = "";//前缀



    Vector2 selScroll = Vector2.zero;
    //绘制窗口时调用
    void OnGUI()
    {
        EditorGUIUtility.labelWidth = 80f;
        EditorGUILayout.LabelField("所属图集", m_atlasName);
        EditorGUILayout.LabelField("前缀", m_prefix);

        //要设置成的图集的名字
        using (new AutoBeginHorizontal())
        {
            EditorGUILayout.PrefixLabel("选中的图片");
            using (AutoBeginScrollView a = new AutoBeginScrollView(selScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                selScroll = a.Scroll;
                foreach(TextureImporter tex in m_texs){
                    string path =AssetDatabase.GetAssetPath(tex);
                    if(string.IsNullOrEmpty(path))
                        continue;
                    EditorGUILayout.LabelField(path);
                }
                
            }
        }
        if (GUILayout.Button("设置", GUILayout.Height(50)))
        {
            Set();
        }
    }

    public void Add(TextureImporter tex)
    {
        if (tex == null)
            return;

        //if (string.IsNullOrEmpty(m_atlasName) && !string.IsNullOrEmpty(tex.spritePackingTag))
        //    m_atlasName = tex.spritePackingTag;
        if (string.IsNullOrEmpty(m_atlasName))
        {
            string[] ss = tex.assetPath.Split('/');
            if (ss[ss.Length - 3] == "Atlas")
                m_atlasName = ss[ss.Length - 2];
            else
                m_atlasName = ss[ss.Length - 3];

            m_prefix = ss[ss.Length - 2];
        }

        if (m_texs.Contains(tex))
            return;
        m_texs.Add(tex);
    }

    void Set()
    {
        int i=0;
        
        foreach (TextureImporter tex in m_texs)
        {
            CheckRename(tex, m_prefix);
            if (string.IsNullOrEmpty(m_atlasName))
                tex.textureType = TextureImporterType.Sprite;
            else
                tex.textureType = TextureImporterType.Default;
            tex.npotScale = TextureImporterNPOTScale.None;
            tex.lightmap = false;
            tex.normalmap = false;
            tex.spriteImportMode = SpriteImportMode.Single;
            tex.spritePackingTag = m_atlasName;
            tex.borderMipmap = false;
            tex.linearTexture = false;
            tex.alphaIsTransparency = true;
            tex.grayscaleToAlpha = false;
            tex.isReadable = false;
            tex.mipmapEnabled = false;
            tex.generateMipsInLinearSpace = false;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 1;
            
            //if (string.IsNullOrEmpty(m_atlasName) && IsNpot(tex))
            //{//单张图而且不是2的n次方的话，自动压缩
            //    SetFormat(tex, "Standalone", TextureImporterFormat.AutomaticCompressed);
            //    SetFormat(tex, "Android", TextureImporterFormat.AutomaticCompressed);
            //    SetFormat(tex, "iPhone", TextureImporterFormat.AutomaticCompressed);
            //}
            //else
            //{
            //SetFormat(tex, "", TextureImporterFormat.ARGB32);
            //tex.textureFormat = TextureImporterFormat.ARGB32;
            //SetFormat(tex,  "Standalone", TextureImporterFormat.ARGB32);
            tex.ClearPlatformTextureSettings("Standalone");
                
            SetFormat(tex, "Android", TextureImporterFormat.ETC2_RGBA8);
            SetFormat(tex, "iPhone", TextureImporterFormat.PVRTC_RGBA4);
            EditorUtil.SetDirty(tex);
            //}
            ++i;
            EditorUtility.DisplayProgressBar("Loading", string.Format("正在修改格式和重命名，{0}/{1}", i, m_texs.Count), ((float)i / m_texs.Count)*0.9f);
        }

        //根据格式重新导入下资源
        foreach (TextureImporter t in m_texs)
            AssetDatabase.WriteImportSettingsIfDirty(t.assetPath);
        try
        {
            AssetDatabase.StartAssetEditing();
            i = 0;
            foreach (TextureImporter t in m_texs)
            {
                EditorUtility.DisplayProgressBar("Loading", string.Format("正在重新导出资源:第{0}个,共{1}个", i, m_texs.Count), 0.1f + (0.9f * i) / m_texs.Count);
                AssetDatabase.WriteImportSettingsIfDirty(t.assetPath);
                ++i;
            }

        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        //检查是不是要重新打包
        EditorUtility.DisplayProgressBar("Loading", string.Format("检查打包中"), 0.9f);
        EditorUtility.ClearProgressBar();
        UnityEditor.Sprites.Packer.kDefaultPolicy = "DefaultPackerPolicy";//TightPackerPolicy DefaultPackerPolicy
#if UNITY_ANDROID
        UnityEditor.Sprites.Packer.RebuildAtlasCacheIfNeeded(BuildTarget.Android,true);
#endif

#if UNITY_IPHONE
        UnityEditor.Sprites.Packer.RebuildAtlasCacheIfNeeded(BuildTarget.iOS,true);
#endif

#if UNITY_STANDALONE_WIN
        UnityEditor.Sprites.Packer.RebuildAtlasCacheIfNeeded(BuildTarget.StandaloneWindows,true);
#endif

        //打包进UI资源管理器中
        List<string> path = new List<string>();
        foreach (TextureImporter tex in m_texs)
        {
            //Debuger.Log("资源路径"+tex.assetPath);
            path.Add(tex.assetPath);
        }
        UIResMgr.PackByPath(path);
    }

    

    static bool IsNpot(TextureImporter ti)
    {
        Texture2D t2d = AssetDatabase.LoadAssetAtPath(ti.assetPath, typeof(Texture2D)) as Texture2D;
        if (t2d.width != t2d.height )
            return true;

        return false;
    }

    static bool SetFormat(TextureImporter textureImporter, string platform, TextureImporterFormat format)
    {

        int maxTextureSize;
        TextureImporterFormat textureFormat;
        textureImporter.GetPlatformTextureSettings(platform, out maxTextureSize, out textureFormat);
        if (textureFormat != format)
        {
            textureImporter.SetPlatformTextureSettings(platform, maxTextureSize, format);
            return true;
        }

        return false;
    }

    //根据图集名重命名一个图片
    static void CheckRename(TextureImporter tex, string prefix)
    {
        if( string.IsNullOrEmpty(prefix))
            return;
       
       string path = tex.assetPath;
       int begin = path.LastIndexOf("/");
       int end = path.LastIndexOf(".");
       if(begin==-1 || end==-1 ||end -1== begin)
       {
           Debuger.LogError("重命名图集图片出错:" + path);
           return;
       }
       //Debuger.Log(path.Substring(begin + 1, end - begin-1));
       List<string> ss = new List<string>(path.Substring(begin + 1, end - begin-1).Split('_'));
       
       //添加"ui_atlasName"
       if(ss.Count<=0)
            ss.Insert(0,"ui");
       else
       {
            bool isFind = ss[0].IndexOf("ui", StringComparison.OrdinalIgnoreCase)>=0&& ss[0].Length==2;
            if(isFind)
                ss[0] = "ui";
            else
                ss.Insert(0, "ui");
       }

       if (ss.Count <= 1)
           ss.Insert(0, prefix);
       else
       {
           bool isFind = ss[1].IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0 && ss[1].Length == prefix.Length;
           if (isFind)
               ss[1] = prefix;
           else
               ss.Insert(1, prefix);
       }


       AssetDatabase.RenameAsset(tex.assetPath, string.Join("_", ss.ToArray()));
    }
   

}