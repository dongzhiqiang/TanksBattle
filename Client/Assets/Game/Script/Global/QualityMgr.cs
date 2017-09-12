#region Header
/**
 * 名称：品质管理器
 
 * 日期：2016.8.10
 * 描述：管理显示品质和性能
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using System.Text.RegularExpressions;

public class QualityMgr : Singleton<QualityMgr>
{
    public enum enQuality
    {
        low,
        normal,
        high,
        custom,
    }

    public enum enFrameRate {
        low,//30帧
        high,//60帧
    }


    public const string BGM_Key = "BGM_Key";
    public const string Sound_Key = "Sound_Key";
    public const string Frame_Rate_Key = "Frame_Rate_Key";
    public const string Quality_Key = "Quality_Key";
    public const string Fog_Effect_Key = "Fog_Effect_Key";
    public const string Bloom_Effect_Key = "Bloom_Effect_Key";
    public const string Shadow_Effect_Key = "Shadow_Effect_Key";
    public const string Scene_Effect_Key = "Scene_Effect_Key";
    public const string Ani_Effect_Key = "Ani_Effect_Key";
    public const string Role_Effect_Key = "Role_Effect_Key";

    public bool roleAniFx = true;
    public bool roleSpecular = true;

    //修改下分辨率以提升性能
    public int originalHeight;
    public int originalWidth;


   

    public void Init()
    {
        originalHeight = Screen.height;
        originalWidth = Screen.width;
        if (!PlayerPrefs.HasKey(Quality_Key))
            MatchQuality();

        CheckFrameRate();
        roleAniFx = PlayerPrefs.GetInt(Ani_Effect_Key, 0) != 0;
        roleSpecular = PlayerPrefs.GetInt(Role_Effect_Key, 0) != 0;

      
    }

    public void OnChangeLevel()
    {
        CheckBgmVolumn();
        CheckSoundVolumn();

        CheckFogEffect();
        CheckBloomEffect();
        CheckShadowEffect();
        CheckSceneEffect();
        CheckAniEffect();
        CheckRoleEffect();
    }
    #region 品质
    //自动匹配品质和帧率
    public void MatchQuality()
    {
        bool isFrameRateHigh = false;
        if(Application.platform == RuntimePlatform.Android)
        {
            SetQuality(GetAndroidQuality());
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)//苹果设备不能获取cpu频率，这里通过GPU型号和内存判断
        {
            Regex reg = new Regex("Apple A(\\d+) GPU", RegexOptions.RightToLeft);
            var match = reg.Match(SystemInfo.graphicsDeviceName);
            int version = 0;
            if (match!=null&&match.Success&& match.Groups.Count>=1&&int.TryParse(match.Groups[1].ToString(),out version))
            {
                SetQuality(version >= 8 ? enQuality.high : enQuality.normal);
                isFrameRateHigh = version >= 9;//苹果A9以上才设置高帧率,安卓机一般发热比较严重
            }
            else if(SystemInfo.systemMemorySize > 1700)//2g内存的苹果应该性能比较好
                SetQuality(enQuality.high);
            else if (SystemInfo.systemMemorySize!=0 &&SystemInfo.systemMemorySize < 600)//512内存的苹果应该性能比较差
                SetQuality(enQuality.low);
            else
                SetQuality(enQuality.normal);
        }
        else
        {
            SetQuality(enQuality.high);
            isFrameRateHigh = true;
        }
            
        
        
        SetFrameRate(isFrameRateHigh ? enFrameRate.high: enFrameRate.low);
    }

    public enQuality GetAndroidQuality()
    {
        if (SystemInfo.processorFrequency > 2050 && SystemInfo.systemMemorySize > 2700)//2.1GHz,3G内存就算高配
            return enQuality.high;
        else if (SystemInfo.processorFrequency != 0 && SystemInfo.processorFrequency < 1250)//1.2GHz，就算低配
            return enQuality.low;
        else
            return enQuality.normal;
    }

    public void SetQuality(enQuality q)
    {
        if (PlayerPrefs.HasKey(Quality_Key) && GetQuality() == q)
            return;

        PlayerPrefs.SetInt(Quality_Key, (int)q);
        if(q == enQuality.low){
            SetFogEffect(false);
            SetBloomEffect(false);
            SetShadowEffect(false);
            SetSceneEffect(false);
            SetAniEffect(false);
            SetRoleffect(false);
        }
        else if (q == enQuality.normal)
        {
            SetFogEffect(false);
            SetBloomEffect(false);
            SetShadowEffect(true);
            SetSceneEffect(true);
            SetAniEffect(true);
            SetRoleffect(true);
        }
        else if (q == enQuality.high)
        {
            SetFogEffect(true);
            SetBloomEffect(true);
            SetShadowEffect(true);
            SetSceneEffect(true);
            SetAniEffect(true);
            SetRoleffect(true);
        }
        else //自定义
        {
            SetFogEffect(true);
            SetBloomEffect(true);
            SetShadowEffect(true);
            SetSceneEffect(true);
            SetAniEffect(true);
            SetRoleffect(true);
        }
        

    }

    public enQuality GetQuality()
    {
        return (enQuality)PlayerPrefs.GetInt(Quality_Key, (int)enQuality.normal);
    }
    #endregion
    #region 声音
    public void SetBgmVolumn(float v )
    {
        PlayerPrefs.SetFloat(BGM_Key, Mathf.Clamp01(v));
        CheckBgmVolumn();
    }
    public float GetBgmVolumn()
    {
        return PlayerPrefs.GetFloat(BGM_Key, 1f);
    }

    public void CheckBgmVolumn()
    {
        float v = GetBgmVolumn();
        if (v != SoundMgr.instance.bgmVol)
            SoundMgr.instance.SetBgmVolumn(v);       
    }
    public void SetSoundVolumn(float v)
    {
        PlayerPrefs.SetFloat(Sound_Key, Mathf.Clamp01(v));
        CheckSoundVolumn();
    }
    public float GetSoundVolumn()
    {
        return PlayerPrefs.GetFloat(Sound_Key, 1f);
    }

    public void CheckSoundVolumn()
    {
        float v = GetSoundVolumn();
        if (v != SoundMgr.instance.soundVol)
            SoundMgr.instance.SetSoundVolumn(v);
    }
    #endregion


    #region 帧率
    public void SetFrameRate(enFrameRate frameRate)
    {
        PlayerPrefs.SetInt(Frame_Rate_Key, (int)frameRate);
        
        CheckFrameRate();
    }
    public enFrameRate GetFrameRate()
    {
        return (enFrameRate)PlayerPrefs.GetInt(Frame_Rate_Key, (int)enFrameRate.high);
    }

    public void CheckFrameRate()
    {
        int frameRate = GetFrameRate() == enFrameRate.high ? 60 : 30;
        if (Application.targetFrameRate != frameRate)
            Application.targetFrameRate = frameRate;
    }
    #endregion
    
    #region 雾效
    public void SetFogEffect(bool open)
    {
        PlayerPrefs.SetInt(Fog_Effect_Key, open?1:0);
        CheckFogEffect();
    }
    public bool GetFogEffect()
    {
        return PlayerPrefs.GetInt(Fog_Effect_Key, 0)!=0;
    }

    public void CheckFogEffect()
    {
        bool open = GetFogEffect();
        if (RenderSettings.fog != open)
            RenderSettings.fog = open;
    }
    #endregion

    #region bloom
    public void SetBloomEffect(bool open)
    {
        PlayerPrefs.SetInt(Bloom_Effect_Key, open ? 1 : 0);
        CheckBloomEffect();
    }
    public bool GetBloomEffect()
    {
        return PlayerPrefs.GetInt(Bloom_Effect_Key, 0) != 0;
    }

    public void CheckBloomEffect()
    {
#if UNITY_ANDROID&&!UNITY_EDITOR
        if (GetBloomEffect())
        {
            //很多安卓机GPU都不是太好，这里在开了屏幕后期的情况下调小下分辨率可以显著提升性能，注意如果没有开屏幕后期对性能影响不大
            var q = GetAndroidQuality();
            if (q == enQuality.low && Screen.height > 640)//低配分辨率不大于640
                SetResolution(640);
            if (q == enQuality.normal && Screen.height > 720)//中配分辨率不大于720
                SetResolution(720);
            if (q == enQuality.high && Screen.height > 1080)//高配分辨率不大于1080
                SetResolution(1080);
        }

#endif

        Camera ca = CameraMgr.instance != null ? CameraMgr.instance.CurCamera : null;
        if (ca == null)
            return;
        CheckBloom(ca);
    }

    public void CheckBloom(Camera ca)
    {
        BloomOptimized bloom = ca.GetComponent<BloomOptimized>();
        if (bloom == null)
            return;
        bool open = GetBloomEffect();

        if (bloom.enabled != open)
            bloom.enabled = open;
        

    
    }
    #endregion

    #region 角色阴影
    public void SetShadowEffect(bool open)
    {
        PlayerPrefs.SetInt(Shadow_Effect_Key, open ? 1 : 0);
        CheckShadowEffect();
    }
    public bool GetShadowEffect()
    {
        return PlayerPrefs.GetInt(Shadow_Effect_Key, 0) != 0;
    }

    public void CheckShadowEffect()
    {
        bool open = GetShadowEffect();

        if (DynamicShadowMgr.instance.UseDynamicShadow != open)
            DynamicShadowMgr.instance.UseDynamicShadow = open;
    }
    #endregion

    #region 场景相关
    public void SetSceneEffect(bool open)
    {
        PlayerPrefs.SetInt(Scene_Effect_Key, open ? 1 : 0);
        CheckSceneEffect();
    }
    public bool GetSceneEffect()
    {
        return PlayerPrefs.GetInt(Scene_Effect_Key, 0) != 0;
    }

    GameObject Sky;
    GameObject DynamicBuilding;
    GameObject IgnorableBuilding;
    GameObject Effect;
    public void CheckSceneEffect()
    {
        bool open = GetSceneEffect();
        Sky = Sky != null ? Sky: GameObject.Find("MapScene/[Sky]");
        DynamicBuilding = DynamicBuilding != null ? DynamicBuilding : GameObject.Find("MapScene/[DynamicBuilding]");
        IgnorableBuilding = IgnorableBuilding != null ? IgnorableBuilding : GameObject.Find("MapScene/[IgnorableBuilding]");
        Effect = Effect != null ? Effect : GameObject.Find("MapScene/[Effect]");

        if (Sky != null) Sky.SetActive(open);
        if (DynamicBuilding != null) DynamicBuilding.SetActive(open);
        if (IgnorableBuilding != null) IgnorableBuilding.SetActive(open);
        if (Effect != null) Effect.SetActive(open);    
    }
    #endregion

    #region 动作特效相关
    public void SetAniEffect(bool open)
    {
        PlayerPrefs.SetInt(Ani_Effect_Key, open ? 1 : 0);
        roleAniFx = open;
        CheckAniEffect();
    }
    public bool GetAniEffect()
    {
        return roleAniFx;
    }
    
    public void CheckAniEffect()
    {
        //暂时什么都不用做    
    }
    #endregion

    #region 角色高光
    public void SetRoleffect(bool open)
    {
        PlayerPrefs.SetInt(Role_Effect_Key, open ? 1 : 0);
        roleSpecular = open;
        CheckRoleEffect();
    }
    public bool GetRoleEffect()
    {
        return roleSpecular;
    }

    
    public void CheckRoleEffect()
    {
        GameObject UnStatic = GameObject.Find("MapScene/[UnStatic]");
        if(UnStatic!= null)
        {
            var rs = UnStatic.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var r in rs)
                CheckMaterial(r);
        }
        
        foreach(var role in RoleMgr.instance.Roles)
        {
            role.RenderPart.CheckMainMaterialQuality();
        }
    }

    Dictionary<Material, Material> specularToDiffuse = new Dictionary<Material, Material>();
    Dictionary<Material, Material> diffuseToSpecular = new Dictionary<Material, Material>();
    Shader specularShader;
    Shader superShader;
    Shader diffuseShader;
    public bool CheckMaterial(Renderer r)
    {
        if (r==null || r.sharedMaterials.Length > 1)
            return false;

        if (specularShader == null )
        {
            superShader = Shader.Find("Custom/SuperShader");
            specularShader = Shader.Find("Mobile/Bumped Specular");
            diffuseShader = Shader.Find("Mobile/Diffuse");
        }

        var oldMat = r.sharedMaterial;
        if ((oldMat == null)||
            (roleSpecular && oldMat.shader != diffuseShader) ||
            (!roleSpecular && (oldMat.shader != specularShader&& oldMat.shader != superShader)))
            return false;

        Material newMat = null;
        if (roleSpecular)
        {
            if (!diffuseToSpecular.TryGetValue(oldMat, out newMat))//找不到就返回把，角色原先就没有高光的
                return false;
        }
        else
        {
            if (!specularToDiffuse.TryGetValue(oldMat, out newMat))
            {
                newMat = new Material(oldMat);
                newMat.shader = diffuseShader;
                specularToDiffuse[oldMat] = newMat;
                diffuseToSpecular[newMat] = oldMat;
            } 
        }

        r.sharedMaterial = newMat;
        return true;
    }

    //把相机bloom和角色高光去掉
    public void CheckGameObject(GameObject go)
    {
        var cameras = go.GetComponentsInChildren<Camera>(true);
        foreach (var ca in cameras)
            CheckBloom(ca);

        var rs = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var r in rs)
            CheckMaterial(r);

    }
    #endregion

    #region 分辨率
   


    public void SetResolution(int h)
    {
        float height = h;
        float width = originalWidth * height / originalHeight;
        Screen.SetResolution((int)width, (int)height, true);
    }
    #endregion
}
