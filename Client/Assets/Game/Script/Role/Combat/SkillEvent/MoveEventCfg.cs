#region Header
/**
 * 名称：卡帧事件
 
 * 日期：2015.1.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum enMoveEventMoveType
{
    forward,//主角前方
    backward,//主角反向
    look,//主角看向目标
    back,//目标看向主角
    joystick,//摇杆方向(最后)
    targetForward,//目标前方
    targetBackward,//目标反向
    left,//左移
    right,//右移
    joystickCur,//摇杆方向(实时)
    backEventGroupRoot
}

public enum enMoveEventDirType
{
    none,//无
    forward,//与移动方向一样
    backward,//与移动方向相反
    look,//主角看向目标
    back,//目标看向主角
}

public class MoveEventCfg : SkillEventCfg
{
    public static string[] MoveTypeName = new string[] { "释放者前方", "释放者反向", "释放者看向目标", "目标看向释放者", "摇杆方向(最后)", "目标前方", "目标反向", "左移", "右移", "摇杆方向(实时)", "目标看向飞出物" };
    public static string[] DirTypeName = new string[] { "无", "移动方向", "移动反向", "释放者看向目标", "目标看向释放者" };

    public enMoveEventMoveType moveType = enMoveEventMoveType.forward;//移动类型
    public enMoveEventDirType dirType = enMoveEventDirType.forward;//方向类型

    public bool self = false;//是自己移动，还是target移动
    public float distance =5;//前进距离
    public int frame = 5;//前进多少帧
    public bool avoid = false;//穿越小怪(但不能穿越大怪)，见战斗文档的碰撞层里的角色翻滚层设定
    public int touchOverFrame = -1;//在第几帧到移动完成之间检测是不是碰到敌人，碰到立即结束，如果填-1表明不用判断是不是碰到敌人
    public bool endIfSkillEnd = false;//技能结束则结束
    public string endSkill = "";//结束技能
    

    public override enSkillEventType Type { get { return enSkillEventType.move; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "自己", COL_WIDTH * 2)) onTip("是自己移动，还是目标移动"); return false;
            case 1: if (h(ref r, "移动类型", COL_WIDTH * 4)) onTip("只控制位置，不控制方向"); return false;
            case 2: if (h(ref r, "方向类型", COL_WIDTH * 4)) onTip("相对于位置变化，方向的类型"); return false;
            case 3: if (h(ref r, "前进距离", COL_WIDTH * 3)) onTip("单位米,内部的实现会转换成速度，速度=前进距离/持续帧数"); return false;
            case 4: if (h(ref r, "持续帧数", COL_WIDTH * 3)) onTip("多少帧之后不移动,填0的话只会修改方向，不会移动"); return false;
            case 5: if (h(ref r, "穿透", COL_WIDTH * 3)) onTip("穿越小怪(但不能穿越大怪)，见战斗文档的碰撞层里的角色翻滚层设定"); return false;
            case 6: if (h(ref r, "接触帧", COL_WIDTH * 3)) onTip("在第几帧到移动完成之间检测是不是碰到敌人，碰到立即结束，如果填-1表明不用判断是不是碰到敌人"); return false;
            case 7: if (h(ref r, "技能绑定", COL_WIDTH * 3)) onTip("技能结束的时候移动结束"); return false;
            case 8: if (h(ref r, "结束技能", COL_WIDTH * 7)) onTip("移动结束的时候释放一个技能,注意只有不被打断的情况下才会释放这个技能.注意这个技能的目标是当前事件的目标，如果当前事件的目标不是敌人则取技能目标，如果仍取不到则走自动朝向流程"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 2;
                    self = EditorGUI.Toggle(r, GUIContent.none, self);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 4;
                    int idx = (int)moveType;
                    int newIdx = EditorGUI.Popup(r, idx, MoveTypeName);
                    if (newIdx != idx && newIdx != -1)
                        moveType = (enMoveEventMoveType)newIdx;
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 4;
                    int idx = (int)dirType;
                    int newIdx = EditorGUI.Popup(r, idx, DirTypeName);
                    if (newIdx != idx && newIdx != -1)
                        dirType = (enMoveEventDirType)newIdx;
                    r.x += r.width;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 3;
                    distance = EditorGUI.FloatField(r, GUIContent.none, distance);
                    r.x += r.width;
                }; return false;
            case 4:
                {
                    r.width = COL_WIDTH * 3;
                    frame = EditorGUI.IntField(r, GUIContent.none, frame);
                    r.x += r.width;
                }; return false;
            case 5:
                {
                    r.width = COL_WIDTH * 3;
                    avoid = EditorGUI.Toggle(r, GUIContent.none, avoid);
                    r.x += r.width;
                }; return false;
            case 6:
                {
                    r.width = COL_WIDTH * 3;
                    touchOverFrame = EditorGUI.IntField(r, GUIContent.none, touchOverFrame);
                    r.x += r.width;
                }; return false;
            case 7:
                {
                    r.width = COL_WIDTH * 3;
                    endIfSkillEnd = EditorGUI.Toggle(r, GUIContent.none, endIfSkillEnd);
                    r.x += r.width;
                }; return false;
            case 8:
                {
                    r.width = COL_WIDTH * 7;
                    endSkill = EditorGUI.TextField(r, endSkill);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        RoleStateMachine rsm = target.RSM;
        //空中免疫
        if (rsm.IsAir)
            return false;
        
        
        if(frame < 0){
            Debuger.LogError("移动事件参数出错，持续帧数不能填0");
        }

        //一些检错
        if(source == target && (
            moveType == enMoveEventMoveType.look ||
            moveType == enMoveEventMoveType.back ||
            dirType == enMoveEventDirType.look ||
            dirType == enMoveEventDirType.back )){
            Debuger.LogError("移动事件参数出错，作用对象和释放者不能是同一个人:{0} {1} {2}",source.Cfg.id,moveType,dirType);
            return false;
        }
        Role r = self ? source : target;
        Role other = self ? target:source  ;

        //计算移动对应的一些参数
        TranPartCxt.enMove cxtMoveType= TranPartCxt.enMove.dir;
        Vector3 moveDir=  Vector3.zero;
        Pos moveTarget=null;
        
        if (moveType == enMoveEventMoveType.forward)
            moveDir = source.transform.forward;
        else if (moveType == enMoveEventMoveType.backward)
            moveDir = -source.transform.forward;
        else if (moveType == enMoveEventMoveType.look){
            cxtMoveType = self ? TranPartCxt.enMove.look: TranPartCxt.enMove.back;
            moveDir = target.transform.position - source.transform.position;
            moveTarget = IdTypePool<PosTran>.Get();
            ((PosTran)moveTarget).SetTran(self ?target.transform :source.transform);
        }
        else if (moveType == enMoveEventMoveType.back)
        {
            cxtMoveType = self ? TranPartCxt.enMove.back : TranPartCxt.enMove.look;
            moveDir = source.transform.position -target.transform.position;
            moveTarget = IdTypePool<PosTran>.Get();
            ((PosTran)moveTarget).SetTran(self ?target.transform:source.transform);
        }
        else if(moveType == enMoveEventMoveType.joystick){
            moveDir = source.MovePart.LastDir;
            if(moveDir == Vector3.zero)
                moveDir = source.transform.forward;
        }
        else if (moveType == enMoveEventMoveType.targetForward)
            moveDir = target.transform.forward;
        else if (moveType == enMoveEventMoveType.targetBackward)
            moveDir = -target.transform.forward;
        else if (moveType == enMoveEventMoveType.left)
        {
            Vector3 pos = other.transform.position;
            Vector3 posSelf = r.transform.position;
            Vector3 link = posSelf -pos;
            link.y = 0;
            float dis = link.magnitude;
            float perimeter = 2 * Mathf.PI * dis;
            if (dis == 0)
                moveDir = -r.transform.right;
            else if(distance > (perimeter / 4))
                moveDir = (-link).HorizontalLeft();
            else
            {
                float angle = 360f * (distance / perimeter); //转成左跳多少角度
                pos = pos + Quaternion.Euler(0, angle, 0) * link;//计算出圆周上左跳的终点
                moveDir = pos - posSelf;
            }

        }
        else if (moveType == enMoveEventMoveType.right)
        {
            Vector3 pos = other.transform.position;
            Vector3 posSelf = r.transform.position;
            Vector3 link = posSelf - pos;
            link.y = 0;
            float dis = link.magnitude;
            float perimeter = 2 * Mathf.PI * dis;
            if (dis == 0)
                moveDir = r.transform.right;
            else if (distance > (perimeter / 4))
                moveDir = (-link).HorizontalRight();
            else
            {
                float angle = 360f * (distance / perimeter); //转成左跳多少角度
                pos = pos + Quaternion.Euler(0, -angle, 0) * link;//计算出圆周上左跳的终点
                moveDir = pos - posSelf;
            }
        }
        else if (moveType == enMoveEventMoveType.joystickCur)
        {
            moveDir = source.MovePart.CurDir;
            if (moveDir == Vector3.zero)
                moveDir = source.transform.forward;
        }
        else if (moveType == enMoveEventMoveType.backEventGroupRoot)
        {
            cxtMoveType = self ? TranPartCxt.enMove.back : TranPartCxt.enMove.look;
            moveDir = eventFrame.EventGroup.Root.position- target.transform.position;
            moveTarget = IdTypePool<PosTran>.Get();
            ((PosTran)moveTarget).SetTran(eventFrame.EventGroup.Root);
        }
        else
        {
            Debuger.LogError("移动事件，未知的移动类型:{0}", moveType);
            return false;
        }
        moveDir.y =0;
        if(Vector3.zero ==moveDir)
        {
            if (moveTarget != null) moveTarget.Put();
            Debuger.Log("计算出来的移动方向为0:{0}", moveType);
            return false;
        }
        
        //计算方向
        Pos dirTarget = null;
        TranPartCxt.enDir cxtDirType;
        if (dirType == enMoveEventDirType.none)
            cxtDirType = TranPartCxt.enDir.none;
        else if (dirType == enMoveEventDirType.forward)
        {
            cxtDirType = TranPartCxt.enDir.forward;
            r.TranPart.SetDir(moveDir);//先设置一次方向
        }
        else if (dirType == enMoveEventDirType.backward)
        {
            cxtDirType = TranPartCxt.enDir.backward;
            r.TranPart.SetDir(-moveDir);//先设置一次方向
        }
        else if (dirType == enMoveEventDirType.look)
        {
            cxtDirType = self ? TranPartCxt.enDir.look : TranPartCxt.enDir.back;
            r.TranPart.SetDir(target.transform.position - source.transform.position);//先设置一次方向
            dirTarget = IdTypePool<PosTran>.Get();
            ((PosTran)dirTarget).SetTran(self ? target.transform : source.transform);
        }
        else if (dirType == enMoveEventDirType.back)
        {
            cxtDirType = self ? TranPartCxt.enDir.back : TranPartCxt.enDir.look;
            r.TranPart.SetDir(source.transform.position - target.transform.position);//先设置一次方向
            dirTarget = IdTypePool<PosTran>.Get();
            ((PosTran)dirTarget).SetTran(self ? target.transform : source.transform);
        }
        else
        {
            Debuger.LogError("移动事件，未知的移动类型:{0}", moveType);
            return false;
        }

        //只改方向的情况
        if (frame == 0 )
        {
            if(dirTarget != null)dirTarget.Put();
            if (moveTarget != null) moveTarget.Put();
            return true;
        }

        //计算速度
        float speed =distance/(frame*Util.One_Frame);

        TranPartCxt cxt = r.TranPart.AddCxt();
        if (cxt == null)
        {
            if (dirTarget != null) dirTarget.Put();
            if (moveTarget != null) moveTarget.Put();
            return false;
        }
        cxt.moveType = cxtMoveType;
        cxt.SetMoveDir(moveDir);
        cxt.moveTarget=moveTarget;
        cxt.speed = speed;
        cxt.dirType = cxtDirType;
        cxt.dirTarget = dirTarget;
        cxt.dirModelSmooth = false;
        cxt.duration =frame*Util.One_Frame;

        //可穿透
        if (avoid)
            cxt.SetColliderLayer(r, enGameLayer.heroAvoidCollider);

        //技能结束则结束
        if (endIfSkillEnd)
            eventFrame.EventGroup.AddBindObj(r, cxt, SkillEventGroupBindObject.enType.tranPartCxt);

        //碰到结束
        cxt.touchOverFrame = touchOverFrame;
        
        //结束技能
        if(!string.IsNullOrEmpty(endSkill))
        {
            Role endSkillTarget =RoleMgr.instance.IsEnemy(target, source)? target:null;
            if (endSkillTarget == null)
                endSkillTarget = eventFrame.EventGroup.Target;

            cxt.SetEndSkill(endSkill, endSkillTarget);
        }
            

        return true;
    }
}