using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
public enum enTextureColor
{
    black,
    blue,
    brown,
    green,
    lightBlue,
    lightRed,
    orange,
    pink,
    purple,
    red
}
public class EditorUtil  {
    

    static  Dictionary<string,Texture2D> s_previews = new Dictionary<string,Texture2D>();
    

    static GUIStyle s_buttonStyle;
    
    public static GUIStyle Button
    {
        get
        {
            if (s_buttonStyle == null)
            {
                s_buttonStyle = new GUIStyle(GUI.skin.button);
                s_buttonStyle.margin = new RectOffset(4, 4, 2, 2);//边缘，只影响layout的控件
                s_buttonStyle.padding = new RectOffset(6, 6, 2, 2);//内部边缘

            }
            return s_buttonStyle;
        }

    }
    private static GUIStyle s_labelWordWrap;
    public static GUIStyle LabelWordWrap
    {
        get
        {
            if (s_labelWordWrap == null)
            {
#if UNITY_EDITOR
                s_labelWordWrap = new GUIStyle(EditorStyles.label);
#else
                s_labelWordWrap = new GUIStyle(GUI.skin.label);
#endif
                s_labelWordWrap.wordWrap = true;
            }
            return s_labelWordWrap;
        }
    }

    private static GUIStyle s_textAreaWordWrap;
    public static GUIStyle TextAreaWordWrap
    {
        get
        {

            if (s_textAreaWordWrap == null)
            {
#if UNITY_EDITOR
                s_textAreaWordWrap = new GUIStyle(EditorStyles.textArea);
#else
                s_labelWordWrap = new GUIStyle(GUI.skin.textArea);
#endif
                s_textAreaWordWrap.wordWrap = true;
            }
            return s_textAreaWordWrap;
        }
    }
    static GUIStyle s_tipButton;
    public static GUIStyle TipButton
    {
        get
        {
            if (s_tipButton == null)
            {
                GUIStyle s = new GUIStyle(GUI.skin.button); 
                s.margin = new RectOffset(0, 0, 0, 0);//边缘，只影响layout的控件
                s.padding = new RectOffset(1, 1, 1, 1);//内部边缘
#if UNITY_EDITOR
                Texture2D t1 = LoadTexture2D("infoButton_normal");
                Texture2D t2 = LoadTexture2D("infoButton_over");
                if(t1!= null && t2!=null)
                {
                    s.normal.background = t1;
                    s.active.background = t2;
                    s.hover.background = t1;
                    s.focused.background = t1;
                    s.onNormal.background = t2;
                    s.onActive.background = t1;
                    s.onHover.background = t2;
                    s.onFocused.background = t2;
                    s.fixedHeight = 16;
                    s.fixedWidth= 16;
                }
                
#endif
                s.normal.textColor = Color.white;
                s.active.textColor = Color.white;
                s.hover.textColor = Color.white;
                s.focused.textColor = Color.white;
                s_tipButton = s;
            }
            return s_tipButton;
        }

    }

    static GUIStyle s_ingoreButton;
    public static GUIStyle IngoreButton
    {
        get
        {
            if (s_ingoreButton == null)
            {
                GUIStyle s = new GUIStyle(GUI.skin.button);
                s.margin = new RectOffset(0, 0, 0, 0);//边缘，只影响layout的控件
                s.padding = new RectOffset(1, 1, 1, 1);//内部边缘
#if UNITY_EDITOR
                Texture2D t1 = (Texture2D)EditorGUIUtility.IconContent("ViewToolOrbit On").image;
                Texture2D t2 = (Texture2D)EditorGUIUtility.IconContent("ViewToolOrbit").image;
                if (t1 != null && t2 != null)
                {
                    s.normal.background = t1;
                    s.active.background = t2;
                    s.hover.background = t1;
                    s.focused.background = t1;
                    s.onNormal.background = t2;
                    s.onActive.background = t1;
                    s.onHover.background = t2;
                    s.onFocused.background = t2;
                    s.fixedHeight = t1.height;
                    s.fixedWidth = t1.width;
                }

#endif
                s.normal.textColor = Color.white;
                s.active.textColor = Color.white;
                s.hover.textColor = Color.white;
                s.focused.textColor = Color.white;
                s_ingoreButton = s;
            }
            return s_ingoreButton;
        }

    }

    static GUIStyle s_middleLabelStyle;

    public static GUIStyle MiddleLabel
    {
        get
        {
            if (s_middleLabelStyle == null)
            {
                s_middleLabelStyle = new GUIStyle();
                try
                {
                    s_middleLabelStyle = new GUIStyle(GUI.skin.label);
                }
                catch(Exception e){

                }
                s_middleLabelStyle.alignment = TextAnchor.MiddleCenter;
                s_middleLabelStyle.fontSize = 15;
                s_middleLabelStyle.normal.textColor = Color.white;
                s_middleLabelStyle.active.textColor = Color.white;
                s_middleLabelStyle.hover.textColor = Color.white;
                s_middleLabelStyle.focused.textColor = Color.white;

            }
            return s_middleLabelStyle;
        }

    }
    static public void SetDirty(UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        if (obj)
        {
            //if (obj is Component) Debuger.Log(NGUITools.GetHierarchy((obj as Component).gameObject), obj);
            //else if (obj is GameObject) Debuger.Log(NGUITools.GetHierarchy(obj as GameObject), obj);
            //else Debuger.Log("Hmm... " + obj.GetType(), obj);
            UnityEditor.EditorUtility.SetDirty(obj);
        }
#endif
    }

#if UNITY_EDITOR
    static public List<T> LoadAssetsAtPath<T>(string path) where T : UnityEngine.Object
    {
        //如果不是绝对路劲，那么加上去
        string fullPath = Application.dataPath + "/" + path+"/";
        List<T> l = new List<T>();
        int startIndex = fullPath.Length + 1;
        if (!System.IO.Directory.Exists(fullPath))
        {
            Debuger.LogError("找不到文件夹:{0}", path);
            return l;
        }

        string[] files = System.IO.Directory.GetFiles(fullPath, "*.*", System.IO.SearchOption.AllDirectories);
        List<string> resList = new List<string>();
        string tmp;
        string s;
        for (int i = 0; i < files.Length; ++i)
        {
            s = files[i].Replace('\\', '/').Substring(startIndex);
            tmp = s.ToLower();
            s = "Assets/" + path + "/" + s;

            if (tmp.EndsWith(".meta") ||
                tmp.Contains("__copy__/") ||
                tmp.Contains(".svn/") ||
                tmp.EndsWith(".dll"))
                continue;

            T t = AssetDatabase.LoadAssetAtPath<T>(s);
            if (t != null)
                l.Add(t);
        }

        return l;
    }

    static public void RegisterUndo(string name, params UnityEngine.Object[] objects)
    {

        if (objects != null && objects.Length > 0&& objects[0]!=null)
        {
            UnityEditor.Undo.RecordObjects(objects, name);

            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null) continue;
                UnityEditor.EditorUtility.SetDirty(obj);
            }
        }

    }

    public static Texture2D LoadTexture2D(string textureName)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format("Assets/Common/Textures/{0}.png", textureName)) as Texture2D;
    }

    public static GUIContent GUITexContent(string textureName,string tooltip=null)
    {
        if (string.IsNullOrEmpty(textureName))
            return new GUIContent();
        else if(string.IsNullOrEmpty(tooltip))
            return new GUIContent(LoadTexture2D(textureName));
        else
            return new GUIContent(LoadTexture2D(textureName), tooltip);
    }

    static Dictionary<Color, GUIStyle> s_boxStyles = new Dictionary<Color, GUIStyle>();
    public static GUIStyle BoxStyle(Color c)
    {
        if (s_boxStyles.ContainsKey(c))
            return s_boxStyles[c];
        else
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            texture2D.SetPixel(1, 1, c);
            texture2D.hideFlags = (HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable);
            texture2D.Apply();
            GUIStyle s = new GUIStyle(GUI.skin.box);
            s.normal.background = texture2D;
            s.active.background = texture2D;
            s.hover.background = texture2D;
            s.focused.background = texture2D;
            s.normal.textColor = Color.white;
            s.active.textColor = Color.white;
            s.hover.textColor = Color.white;
            s.focused.textColor = Color.white;
            s_boxStyles[c] = s;
            return s;
        }
    }

    static Dictionary<int, GUIStyle> s_texColorStyles = new Dictionary<int, GUIStyle>();

    //圆角矩形
    public static GUIStyle TextureColorStyle(enTextureColor c,bool isSel)
    {
        int k = isSel ? (int)c + 100 : (int)c;
        if (s_texColorStyles.ContainsKey(k))
            return s_texColorStyles[k];
        else
        {
            Texture2D tex = LoadTexture2D(string.Format("Corner/{0}{1}", c.ToString() ,isSel?"Sel":""));
            GUIStyle s = new GUIStyle(GUI.skin.box);
            s.normal.background = tex;
            s.active.background = tex;
            s.hover.background = tex;
            s.focused.background = tex;
            s.normal.textColor = Color.white;
            s.active.textColor = Color.white;
            s.hover.textColor = Color.white;
            s.focused.textColor = Color.white;
            s.border = new RectOffset(15,15,15,15);
            s_texColorStyles[k] = s;
            return s;
        }

    }
    

    static Dictionary<Color, GUIStyle> s_areaStyles = new Dictionary<Color, GUIStyle>();
    public static GUIStyle AreaStyle(Color c)
    {
        if (s_areaStyles.ContainsKey(c))
            return s_areaStyles[c];
        else
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            texture2D.SetPixel(1, 1, c);
            texture2D.hideFlags = (HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable);
            texture2D.Apply();
            GUIStyle s = new GUIStyle(GUI.skin.box);
            s.normal.background = texture2D;
            s.active.background = texture2D;
            s.hover.background = texture2D;
            s.focused.background = texture2D;
            s.normal.textColor = Color.white;
            s.active.textColor = Color.white;
            s.hover.textColor = Color.white;
            s.focused.textColor = Color.white;
            s.border = new RectOffset(8,8,8,8);
            s.margin = new RectOffset(0, 0, 4, 6);
            s.padding = new RectOffset(2, 2, 2, 2);
            s.overflow = new RectOffset(1, 1, 0, 1);
            s_areaStyles[c] = s;
            return s;
        }
    }
        

    //一般是用来做九宫格
    public static GUIStyle TextureStyle(string tex,RectOffset overflow)
    {
        return TextureStyle(AssetDatabase.LoadAssetAtPath<Texture2D>(tex), overflow);
    }

    //一般是用来做九宫格
    public static GUIStyle TextureStyle(Texture2D texture, RectOffset overflow)
    {
        return new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(10, 10, 10, 10),
            overflow = overflow,
            normal =
                {
                    background = texture,
                    textColor = Color.white
                },
            active =
                {
                    background = texture,
                    textColor = Color.white
                },
            hover =
                {
                    background = texture,
                    textColor = Color.white
                },
            focused =
                {
                    background = texture,
                    textColor = Color.white
                },
            stretchHeight = true,
            stretchWidth = true
        };
    }
   
    
    public static void DrawPolyLine(Color color,int width, params Vector2[] points)
    {
        //宽度要限制下不然太耗性能
        if (width > 20)
            width = 20;

        int beginOffset = -(width - 1) / 2 -1 ;
        Vector2 offset = new Vector2(beginOffset, beginOffset);
        Vector3[] vs = new Vector3[points.Length];
        for (int i = 0; i < points.Length; ++i)
            vs[i] = points[i]+ offset;

        Color c = Handles.color;
        Handles.color = color;
        Vector3 offset3d = new Vector3(1, 1, 0);
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0;j < points.Length; ++j)
                vs[j] = vs[j] + offset3d;
            Handles.DrawPolyLine(vs);
        }

        
        
        Handles.color = c;
        //这种绘制方式没有裁剪
        //GL.PushMatrix();
        //HandleUtility.handleMaterial.SetPass(0);// DrawGL.Mat.SetPass(0);
        //GL.Begin(GL.LINES);
        //GL.Color(color);

        //for (int i = 0; i < width; ++i)
        //{
        //    if (i == 0)
        //        InternalPolyLine(points, Vector3.zero);
        //    else
        //    {
        //        int sign = i % 2 == 1 ? 1 : -1;
        //        int offset = (i + 1) / 2;
        //        Vector2 offsetHorizontal = new Vector2(sign*offset,0);
        //        Vector2 offsetVertical = new Vector2(0, sign * offset);
        //        InternalPolyLine(points, offsetHorizontal);
        //        InternalPolyLine(points, offsetVertical);
        //    }
        //}

        //GL.End();
        //GL.PopMatrix();
    }
    static void InternalPolyLine(Vector2[] points,Vector2 offset)
    {
        for (int j = 1; j < points.Length; ++j)
        {
            InternalLine(points[j]+ offset, points[j - 1]+ offset);
        }
    }

    public static void DrawGrid(Color color, float gridSize, Vector2 offset, Rect r)
    {
        DrawGL.Mat.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(color);
        float num = r.x + offset.x;
        if (offset.x < 0f)
        {
            num += gridSize;
        }
        for (float num2 = num; num2 < r.x + r.width; num2 += gridSize)
        {
            InternalLine(new Vector2(num2, r.y), new Vector2(num2, r.y + r.height));
        }
        float num3 = r.y + offset.y;
        if (offset.y < 0f)
        {
            num3 += gridSize;
        }
        for (float num4 = num3; num4 < r.y + r.height; num4 += gridSize)
        {
            InternalLine(new Vector2(r.x, num4), new Vector2(r.x + r.width, num4));
        }
        GL.End();
        GL.PopMatrix();
    }

    static void InternalLine(Vector2 p1, Vector2 p2)
    {
        GL.Vertex(p1);
        GL.Vertex(p2);
    }

    public static Texture2D GetModTexture(string name){
        if(string.IsNullOrEmpty(name))
            return Texture2D.whiteTexture;
        
        Texture2D tex = s_previews.Get(name);
        if (tex != null)
            return tex;

        
        GameObject prefab=  AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FBX/Resources/"+name+".prefab");
        if(prefab == null){
            s_previews[name] =Texture2D.whiteTexture;
            return Texture2D.whiteTexture;
        }

        
        tex = AssetPreview.GetAssetPreview(prefab);
        if (AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()))
        {
            return Texture2D.whiteTexture;
        }

       // tex = GetMidTex(tex);//先裁剪下
        s_previews[name] = tex;
        return tex;
    }

    public static Texture2D GetMidTex(Texture2D tex)
    {
        Debuger.Log("预览图大小 {0}x{1}", tex.width, tex.height);
        Texture2D mid = new Texture2D(tex.width / 2, tex.height / 2, TextureFormat.ARGB32, false);
        mid.SetPixels(tex.GetPixels(tex.width / 4, tex.height / 4, tex.width / 2, tex.height / 2));
        
        return mid;
    }

    //在代码编辑器里打开
    public static void OpenScriptEditorByObj(object obj)
    {
        OpenScriptEditor(obj.GetType());
    }
    public static void OpenScriptEditor(System.Type t)
    {
        MonoScript[] array = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null && array[i].GetClass() != null && array[i].GetClass().Equals(t))
            {
                AssetDatabase.OpenAsset(array[i]);
                return;
            }
        }
    }

    //选中一个对象所属的类文件
    public static void SelectScript(object obj)
    {
        System.Type t = obj.GetType();
        MonoScript[] array = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null && array[i].GetClass() != null && array[i].GetClass().Equals(t))
            {
                Selection.activeObject = array[i];
                return;
            }
        }
    }

    public static List<string> GetSelectPaths()
    {
        List<string> paths = new List<string>();
        foreach (string o in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(o);
            if (string.IsNullOrEmpty(path))
                continue;

            // 是个文件
            if (System.IO.File.Exists(path))
            {
                paths.Add(path);
            }
            else
            {
                path = path.Substring(7);//把asset/去掉
                paths.AddRange(Util.GetAllFileList(Application.dataPath + "/" + path, "Assets/" + path + "/"));
            }
        }
        return paths;
    }

    public static void GetSelectAsset<T>( ref List<T> asset) where T : UnityEngine.Object
    {
        List<string> ls =GetSelectPaths();
        foreach (string assetPath in ls)
        {
            T t = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (t == null)
                continue;
            asset.Add(t);
        }
    }

    public static void GetAssetAtPath<T>(string path,ref List<T> asset) where T : UnityEngine.Object
    {
        List<string> files = Util.GetAllFileList(Application.dataPath + "/" + path, "Assets/" + path + "/");
        foreach (string assetPath in files)
        {
            T t = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (t == null)
                continue;
            asset.Add(t);
        }
    }

    //选中场景中有这个材质的游戏对象
    public static void SelectMaterial(Material mat)
    {
        List<Renderer> rs = new List<Renderer>();
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            rs.AddRange(go.GetComponentsInChildren<Renderer>(true));
        }
        
        List<UnityEngine.Object> os = new List<UnityEngine.Object>();
        
        foreach (var r in rs)
        {
            
            Material[] ms = r.sharedMaterials;
            if (ms == null || ms.Length == 0)
                continue;

            foreach (Material m in ms)
            {
                if (mat == m && os.IndexOf(r.gameObject) == -1)
                {
                    os.Add(r.gameObject);
                    break;
                }
                    
            }
        }

        Debuger.Log("场景中找到{0}个有{1}材质的游戏对象", os.Count,mat);
        if (os.Count > 0)
            Selection.objects = os.ToArray();
    }



    public static Vector3 GetViewScenePosition()
    {
        UnityEditor.SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;
        if (sceneView == null)
            sceneView = (UnityEditor.SceneView)(UnityEditor.SceneView.sceneViews.Count == 0 ? null : UnityEditor.SceneView.sceneViews[0]);

        if (sceneView == null || sceneView.camera == null)
            return Vector3.zero;

        Ray ray = sceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 10000f))
            return hitInfo.point;

        return ray.GetPoint(50f);
    }
    static public bool DrawHeader(string text, string key = null, float space = 0)
    {

        key = string.IsNullOrEmpty(key) ? text : key;

        bool state = UnityEditor.EditorPrefs.GetBool(key, true);

        GUILayout.Space(3f);
        if (!state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        if (space != 0)
            GUILayout.Space(space);
        UnityEditor.EditorGUI.BeginChangeCheck();


        text = "<b><size=11>" + text + "</size></b>";
        if (state) text = "\u25BC " + text;
        else text = "\u25BA " + text;
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;


        if (UnityEditor.EditorGUI.EndChangeCheck()) UnityEditor.EditorPrefs.SetBool(key, state);

        GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!state) GUILayout.Space(3f);
        return state;

    }

    static public void DrawHeaderBtn(string text, string btnText, out bool isShow, out bool isClick, string key = null)
    {
        isShow = true;
        isClick = false;

        key = string.IsNullOrEmpty(key) ? text : key;
        isShow = UnityEditor.EditorPrefs.GetBool(key, true);
        isClick = false;

        GUILayout.Space(3f);
        if (!isShow) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();

        UnityEditor.EditorGUI.BeginChangeCheck();
        text = "<b><size=11>" + text + "</size></b>";
        if (isShow) text = "\u25BC " + text;
        else text = "\u25BA " + text;
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) isShow = !isShow;
        if (UnityEditor.EditorGUI.EndChangeCheck())
            UnityEditor.EditorPrefs.SetBool(key, isShow);

        if (GUILayout.Button(btnText, UnityEditor.EditorStyles.miniButtonRight, GUILayout.Width(45)))//UnityEditor.EditorStyles.miniButtonRight
            isClick = true;

        GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!isShow) GUILayout.Space(3f);

    }


    //public static void Field(GUIContent label, object field, GUIStyle style, params GUILayoutOption[] options)
    //{


    //}

    public static object Field( object obj, params GUILayoutOption[] options)
    {
        Type type = obj.GetType();
        if (type == typeof(int))
            return EditorGUILayout.IntField((int)obj, options);
        if (type == typeof(float))
            return EditorGUILayout.FloatField((float)obj, options);
        if (type == typeof(bool))
            return EditorGUILayout.Toggle((bool)obj, options);
        if (type == typeof(string))
            return EditorGUILayout.TextField((string)obj, options);
        if (type == typeof(Vector2))
            return EditorGUILayout.Vector2Field(GUIContent.none,(Vector2)obj, options);
        if (type == typeof(Vector3))
            return EditorGUILayout.Vector3Field(GUIContent.none, (Vector3)obj, options);
        Debuger.LogError("未知的类型：{0}", type);
        return obj;
    }

    public static object Field(GUIContent label, object obj, params GUILayoutOption[] options)
    {
        Type type = obj.GetType();
        if (type == typeof(int))
            return EditorGUILayout.IntField(label, (int)obj, options);
        if (type == typeof(float))
            return EditorGUILayout.FloatField(label, (float)obj, options);
        if (type == typeof(bool))
            return EditorGUILayout.Toggle(label, (bool)obj,  options);
        if (type == typeof(string))
            return EditorGUILayout.TextField(label, (string)obj, options);
        if (type == typeof(Vector2))
            return EditorGUILayout.Vector2Field(label, (Vector2)obj, options);
        if (type == typeof(Vector3))
            return EditorGUILayout.Vector3Field(label, (Vector3)obj, options);
        Debuger.LogError("未知的类型：{0}", type);
        return obj;
    }
    public static object Field(GUIContent label, object obj, GUIStyle style, params GUILayoutOption[] options)
    {
        Type type = obj.GetType();
        if (type == typeof(int))
            return EditorGUILayout.IntField(label,(int)obj,style,options);
        if (type == typeof(float))
            return EditorGUILayout.FloatField(label, (float)obj, style, options);
        if (type == typeof(bool))
            return EditorGUILayout.Toggle(label, (bool)obj, style, options);
        if (type == typeof(string))
            return EditorGUILayout.TextField(label, (string)obj, style, options);
        if (type == typeof(Vector2))
            return EditorGUILayout.Vector2Field(label, (Vector2)obj,  options);
        if (type == typeof(Vector3))
            return EditorGUILayout.Vector3Field(label, (Vector3)obj, options);
        Debuger.LogError("未知的类型：{0}",type);
        return obj;
    }
    public static void SelectScenePos(Vector3 position)
    {
        UnityEngine.Object[] intialFocus = Selection.objects;
        GameObject tempFocusView = new GameObject("Temp Focus View");
        tempFocusView.transform.position = position;
        try
        {
            Selection.objects = new UnityEngine.Object[] { tempFocusView };
            SceneView.lastActiveSceneView.FrameSelected();
            Selection.objects = intialFocus;
        }
        catch (NullReferenceException)
        {
            //do nothing
        }
        UnityEngine.Object.DestroyImmediate(tempFocusView);
    }
    public static void ApplyPrefab(GameObject go)
    {
        GameObject gameObject2 = PrefabUtility.FindValidUploadPrefabInstanceRoot(go);
        if (gameObject2 == null)
            return;
        var prefabParent = PrefabUtility.GetPrefabParent(gameObject2);
        if (prefabParent == null)
            return;
        EditorUtility.SetDirty(PrefabUtility.ReplacePrefab(gameObject2, prefabParent, ReplacePrefabOptions.ConnectToPrefab));
    }
#endif
}
