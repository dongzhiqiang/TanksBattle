using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMainCityRoleItem : MonoBehaviour {
    public UIArtFont grade;
    public TextEx name;
    public StateGroup starGroup;
    public StateHandle btn;
    public Image bg;
    Role role;
    GameObject roleGo;

	public void Init(Role role,GameObject roleGo)
    {
        this.role = role;
        this.roleGo = roleGo;
        btn.AddClick(OnClick);
        if (role != null && roleGo != null)
        {
            gameObject.SetActive(true);
            if (role.GetInt(enProp.heroId) > 0)
            {
                grade.gameObject.SetActive(false);
                starGroup.gameObject.SetActive(false);
                bg.gameObject.SetActive(true);
                               
                /*ActivityPart part = role.ActivityPart;
                int score = part.GetInt(enActProp.arenaScore);
                int gradeNum = ArenaGradeCfg.GetGrade(score);                      

                grade.SetNum((gradeNum + 1).ToString());*/
                name.text = string.Format("Lv.{0} {1}", role.GetInt(enProp.level), role.GetString(enProp.name));
            }
            else
            {
                grade.gameObject.SetActive(false);
                starGroup.gameObject.SetActive(true);
                bg.gameObject.SetActive(false);
                int advLv = role.GetInt(enProp.advLv);

                PetAdvLvPropRateCfg cfg = PetAdvLvPropRateCfg.m_cfgs[advLv];

               string advName = GetQualityName(cfg.quality, cfg.qualityLevel);               

                name.text = string.Format("Lv.{0} {1}", role.GetInt(enProp.level), advName);

                starGroup.SetCount(role.GetInt(enProp.star));


            }
            Camera ca = CameraMgr.instance.CurCamera;
            Camera caUI = UIMgr.instance.UICameraHight;

            Transform title = roleGo.transform.Find("model/Title");
            Vector3 newPos = title.position;
            if (role.GetInt(enProp.heroId) > 0)           
                newPos = new Vector3(title.transform.position.x, title.transform.position.y - 1f, title.transform.position.z);
        
            if (ca != null && caUI != null)
            {
                Vector2 pos2D;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform.parent.GetComponent<RectTransform>(), ca.WorldToScreenPoint(newPos), caUI, out pos2D))
                {
                    GetComponent<RectTransform>().anchoredPosition = pos2D;
                }
                else
                    Debuger.LogError("主城角色信息计算不出2d位置");
            }
        }
        else
            gameObject.SetActive(false);
    }

    public void UpdatePos()
    {        
        if (roleGo != null)
        {
            Camera ca = CameraMgr.instance.CurCamera;
            Camera caUI = UIMgr.instance.UICameraHight;

            Transform title = roleGo.transform.Find("model/Title");
            Vector3 newPos = title.position;
            if (role.GetInt(enProp.heroId) > 0)
                newPos = new Vector3(title.transform.position.x, title.transform.position.y - 1f, title.transform.position.z);
            if (ca != null && caUI != null)
            {               
                Vector2 pos2D;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform.parent.GetComponent<RectTransform>(), ca.WorldToScreenPoint(newPos), caUI, out pos2D))
                {
                    this.GetComponent<RectTransform>().anchoredPosition = pos2D;
                }
                else
                    Debuger.LogError("主城角色信息计算不出2d位置");
            }
        }
        else
            return;
    }

    void OnClick()
    {
        if (role.GetInt(enProp.heroId)>0)
            UIMgr.instance.Open<UIEquip>();
        else
            UIMgr.instance.Open<UIChoosePet>();
    }

    string GetQualityName(int quality, int qualityLevel)
    {
        string name = role.GetString(enProp.name);
        string text = "";
        if (qualityLevel > 0)
        {
            text = text + "+" + qualityLevel;
        }
        QualityCfg qualityCfg = QualityCfg.m_cfgs[quality];
        string color = qualityCfg.color;
        if (color.StartsWith("#"))
            color = color.Replace("#", string.Empty);
        string colorText = string.Format("<color=#{0}>{1}{2}</color>", color, name, text);
        return colorText;
    }
}
