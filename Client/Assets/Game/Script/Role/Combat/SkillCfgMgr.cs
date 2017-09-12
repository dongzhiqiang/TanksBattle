#region Header
/**
 * 名称：技能索引配置
 
 * 日期：2015.12.8
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;



public class SkillCfgMgr
{
    
    static SkillCfgMgr s_instance;

    public static SkillCfgMgr instance
    {
        get{
            if(s_instance==null){
                s_instance  = Util.LoadJsonFile<SkillCfgMgr>("skill/SkillCfgMgr");
                if (s_instance ==null)
                {
                    s_instance = new SkillCfgMgr();
                }
            }
            return s_instance;
        }
    }

    //public int skillCounter=0;
    //public int eventGroupCounter = 0;
    //public int eventFrameCounter = 0;
    //public int eventCounter =0;
    public List<SkillEventGroupFile> eventGroups = new List<SkillEventGroupFile>();
    public List<FlyerFile> flyers = new List<FlyerFile>();

    public void Save()
    {
        Util.SaveJsonFile("skill/SkillCfgMgr",this);
        
    }
    
}
