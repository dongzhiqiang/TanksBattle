using UnityEngine;
using System.Collections;

public class Grab2EventCfg : SkillEventCfg
{

    public GrabCxt2 grabCxt = new GrabCxt2();
    
    public override enSkillEventType Type { get { return enSkillEventType.grab2; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {

        switch (col)
        {
            case 0: if (h(ref r, "抓取编辑器2", COL_WIDTH * 4)) onTip("抓取编辑器2，编辑大qte开始时主角和怪物和相机的位置"); return false;

            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {

        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 4;
                    if (GUI.Button(r, "打开"))
                    {
                        RoleModel model = tran == null ? null : tran.GetComponent<RoleModel>();
                        Role role = model == null ? null : model.Parent;

                        EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.GRAB_EDITOR2, "抓取编辑器2", grabCxt, role.transform);
                    }

                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif
    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        BeQteCxt cxt = IdTypePool<BeQteCxt>.Get();
        cxt.grabCxt = grabCxt;
        cxt.GrabRole = source;
        
        //如果目标隐藏，强制显示出来不然位置计算会有问题
        if (!target.IsShow)
            target.Show(true);

        if (target.RSM.GotoState(enRoleState.beHit, cxt, false, true))
        {
            BigQte.CurQte.PlayOrPause(target);
            return true;
        }
        else
            return false;
    }

}
