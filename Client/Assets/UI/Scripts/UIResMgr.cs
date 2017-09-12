using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIPanelCfg
{
    public List<string> panelNames = new List<string>();//保存着所有界面的预制体
}

public class UIResMgr : MonoBehaviour {
    static bool s_notAllowInstance=false;
    static UIResMgr s_instance;
    //public List<GameObject> m_bases = new List<GameObject>();//保存着所有界面的预制体
    public List<Sprite> m_sprites = new List<Sprite>();//保存着所有图片
    
    
    

    Dictionary<string, Sprite> m_spriteDict  = new Dictionary<string,Sprite>();
    public Dictionary<string, Sprite> SpriteDict { get{
        if (m_spriteDict.Count == 0)
        {
            CalcSpriteDict();
        }
        return m_spriteDict;
    }}

    public static UIResMgr instance {
        get
        {
            if(s_notAllowInstance)
            {
                Debuger.LogError("正常游戏流程不应该访问UIResMgr");
                return null;
            }
            if (s_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("UIResMgr");
                s_instance= prefab.GetComponent<UIResMgr>();
            }

            return s_instance;
        }
    }

    public static bool NotAllowInstance
    {
        get { return s_notAllowInstance; }
        set
        {
            s_notAllowInstance = value;
            if (s_notAllowInstance)
                s_instance = null;
        }
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    

    //注意改变m_sprites后要调用一次这个函数再取SpriteDict
    public void CalcSpriteDict()
    {
        m_spriteDict.Clear();
        foreach (var sprite in m_sprites)
        {
            if (sprite == null)
                continue;
            m_spriteDict[sprite.name] = sprite;
        }
    }

    public Sprite GetSprite(string spriteName)
    {
        Sprite sprite=null;
        if(SpriteDict.TryGetValue(spriteName,out sprite))
            return sprite;

        Debuger.LogError(string.Format("找不到精灵:{0}",spriteName));
        return null;
        
    }

    public static void PackByPath(List<string> paths)
    {
#if UNITY_EDITOR
        List<Sprite> sprites = new List<Sprite>();
        foreach(var path in paths){
            Sprite s =UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(s == null)
            {
                Debuger.LogError(string.Format("没有找到精灵{0}",path));
                continue;
            }
            sprites.Add(s);
        }
        Pack(sprites);
#endif
    }

    public static void Pack(List<Sprite> sprites)
    {
#if UNITY_EDITOR
        
        UIResMgr uiResMgr = instance;
        HashSet<Sprite> set = new HashSet<Sprite>();

        int removeCount = 0;
        foreach (var sprite in uiResMgr.m_sprites)
        {
            if (sprite == null)
            {
                ++removeCount;
                continue;
            }
            set.Add(sprite);
        }
        Debuger.Log(string.Format("有{0}个已经被删除的sprite", removeCount));

        int addCount = 0;
        foreach(var sprite in sprites ){
            if (set.Add(sprite))
                ++addCount;
        }
        Debuger.Log(string.Format("增加了{0}个sprite", addCount));

        uiResMgr.m_sprites.Clear();
        foreach (Sprite sprite in set)
        {
            uiResMgr.m_sprites.Add(sprite);
        }
        UIResMgr.instance.CalcSpriteDict();
        UnityEditor.EditorUtility.SetDirty(uiResMgr);
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
        
#endif

    }

    //[ContextMenu("转成json")]
    //void ConvertToJson()
    //{

    //    var ps = new UIPanelCfg();
    //    foreach (var prefab in m_bases)
    //    {
    //        ps.panelNames.Add(prefab.name);
    //    }

    //    Util.SaveJsonFile("UIPanelCfg", ps);


    //}

}
