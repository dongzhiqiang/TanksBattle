using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelAreaGizmos : UILevelArea
{


    #region Fields
    public GameObject dirObject;
    public GameObject dirArrow;
    public StateHandle dirBtn;
    public ImageEx aniImage;
    public SimplePool arrowPool;
    public SimplePool friendBloodPool;
    public SimplePool targetPool;
    bool isSelect = false;
    List<GameObject> bloodList = new List<GameObject>();
    List<GameObject> targetList = new List<GameObject>();
    int ob;
    int ob2;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.gizmos; } }
    public override bool IsOpenOnStart { get { return false; } }

    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        dirBtn.AddClick(OnClickGo);
        aniImage.gameObject.SetActive(false);
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, OnRoleEnter);
        ob2 = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.FLAG_CHANGE, OnFlagChange);
        if (reopen)
        {
            foreach (GameObject go in bloodList)
            {
                UILevelBloodRadar radar = go.GetComponent<UILevelBloodRadar>();
                if (radar.m_role != null && radar.m_role.State == Role.enState.alive)
                    go.gameObject.SetActive(true);
            }

            foreach(GameObject go in targetList)
            {
                UILevelTargetRadar radar = go.GetComponent<UILevelTargetRadar>();
                if (radar.m_role != null && radar.m_role.State == Role.enState.alive)
                    go.gameObject.SetActive(true);
            }
        }
    }

    protected override void OnUpdateArea()
    {

    }

    public void LateUpdate()
    {
        if (this.m_parent == null || !this.m_parent.IsOpen)
            return;
        if (SceneMgr.instance.IsDirShow)
        {
            dirObject.gameObject.SetActive(true);
            UpdateGoGuide();
        }
        else
        {
            dirObject.gameObject.SetActive(false);
        }
    }

    //关闭
    protected override void OnCloseArea()
    {
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
        if (ob2 != EventMgr.Invalid_Id) { EventMgr.Remove(ob2); ob2 = EventMgr.Invalid_Id; }
        List<GameObject> arrowList = arrowPool.GetUnsing();
        foreach (GameObject go in arrowList)
            go.gameObject.SetActive(false);

        bloodList.Clear();
        bloodList = friendBloodPool.GetUnsing();
        foreach (GameObject go in bloodList)
            go.gameObject.SetActive(false);

        targetList.Clear();
        targetList = targetPool.GetUnsing();
        foreach (GameObject go in targetList)
            go.gameObject.SetActive(false);
    }

    protected override void OnRoleBorn()
    {

    }

    void OnClickGo()
    {
        int openLevel = ConfigValue.GetInt("openGuajiLevelId");
        int curLevelId = int.Parse(Role.LevelsPart.CurLevelId);
        if (curLevelId <= openLevel)
            return;

        if (Role == null || Role.State != Role.enState.alive) return;
        if (!Role.AIPart.IsPlaying)
            Role.AIPart.Play(AIPart.HeroAI);
        else
            Role.AIPart.Stop();
    }

    void OnFlagChange(object p, object p2, object p3, EventObserver ob)
    {
        string flag = (string)p;
        if (flag != GlobalConst.FLAG_SHOW_FRIENDBLOOD && flag != GlobalConst.FLAG_SHOW_TARGET)
            return;

        Role role = ob.GetParent<Role>();
        if (role.GetFlag(GlobalConst.FLAG_SHOW_FRIENDBLOOD) > 0)
        {
            Role hero = RoleMgr.instance.Hero;
            if (role.GetCamp() == hero.GetCamp())     //友军不显示
            {
                GameObject go = friendBloodPool.Get();
                UILevelBloodRadar rader = go.GetComponent<UILevelBloodRadar>();
                rader.SetData(role);
                return;
            }
            return;
        }
        else if(role.GetFlag(GlobalConst.FLAG_SHOW_TARGET) > 0)
        {
            GameObject go = targetPool.Get();
            UILevelTargetRadar rader = go.GetComponent<UILevelTargetRadar>();
            rader.SetData(role);
            return;
        }
        
    }
    void OnRoleEnter(object param)
    {
        Role role = param as Role;
        if (role != null && role.Id != this.Role.Id)
        {
            Role hero = RoleMgr.instance.Hero;
            if (role.GetCamp() == hero.GetCamp())     //友军不显示
                return;

            //宝箱和陷阱类型不显示
            if (role.Cfg.roleType == enRoleType.box || role.Cfg.roleType == enRoleType.trap)
                return;

            GameObject go = arrowPool.Get();
            UILevelRadar rader = go.GetComponent<UILevelRadar>();
            rader.SetData(role);
        }
    }


    #endregion

    #region Private Methods
    void UpdateGoGuide()
    {
        if (this.Role == null || Role.State != global::Role.enState.alive)
            return;

        if (Role.AIPart.IsPlaying)
            aniImage.gameObject.SetActive(true);
        else
            aniImage.gameObject.SetActive(false);

        RectTransform areaRT = this.GetComponent<RectTransform>();
        RectTransform dirRT = dirObject.gameObject.GetComponent<RectTransform>();

        Camera caUI = UIMgr.instance.UICamera;

        Camera ca = CameraMgr.instance.CurCamera;

        Vector3 rolePos = this.Role.TranPart.GetRoot() + Vector3.up;
        Vector2 heroPos2D;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            areaRT,
            ca.WorldToScreenPoint(rolePos),
            caUI, out heroPos2D))
        {
            dirRT.anchoredPosition = heroPos2D;
        }

        Vector3 dic = SceneMgr.instance.CurFindPos - rolePos;
        Quaternion qua = Quaternion.LookRotation(dic.normalized);
        float angles = qua.eulerAngles.y;

        if (ca != null)
        {
            float carmaAngles = ca.transform.eulerAngles.y;

            Vector3 localEulerAngles = dirObject.transform.localEulerAngles;
            localEulerAngles.z = 180 - angles + carmaAngles;
            dirObject.transform.localEulerAngles = localEulerAngles;

            if (dirBtn.gameObject.transform.parent == dirObject.transform)
            {
                localEulerAngles = dirBtn.transform.localEulerAngles;
                localEulerAngles.z = -(180 - angles + carmaAngles);

                dirBtn.gameObject.transform.localEulerAngles = localEulerAngles;
            }
            else
            {
                dirBtn.gameObject.transform.localEulerAngles = Vector3.zero;
            }
        }
    }

    #endregion

}
