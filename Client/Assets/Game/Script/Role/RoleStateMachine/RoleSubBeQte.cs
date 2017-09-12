using UnityEngine;
using System.Collections;

public class BeQteCxt : IBehitCxt
{
    public override enBehit Type { get { return enBehit.beQte; } }
    public GrabCxt2 grabCxt;
    public Skill parentSkill;
    int grabRoleId;
    Role grabRole;

    public Role GrabRole
    {
        get { return grabRole == null || grabRole.IsDestroy(grabRoleId) || grabRole.State != Role.enState.alive ? null : grabRole; }
        set
        {
            if (value == null || value.IsInPool || value.State != Role.enState.alive)
            {
                Debuger.LogError("逻辑错误，抓取上下文的抓取者为空或者已经死亡");
                grabRole = null;
                grabRoleId = -1;
                return;
            }
            grabRole = value;
            grabRoleId = grabRole.Id;

        }
    }
}


public class RoleSubBeQte : RoleBehitSubState
{
    #region Fields
    bool m_isPlaying = false;
    #endregion

    #region Properties
    public override enBehit Type { get { return enBehit.beQte; } }
    public BeQteCxt MyCxt { get { return (BeQteCxt)Cxt; } }
    public GrabCxt2 GrabCxt { get { return Cxt == null ? null : MyCxt.grabCxt; } }
    #endregion

    #region Frame
    public RoleSubBeQte(RoleStateBehit parent, enBehit stateTo) : base(parent, stateTo) { }


    public override bool CanParentLeave()
    {
        return !m_isPlaying;
    }
    public override IBehitCxt CanStateTo(IBehitCxt cxt)
    {
        if (BigQte.CurQte != null && BigQte.CurQte.IsPlaying)
            return cxt;
        else
            return null;
    }
    public override void Leave()
    {
        ClearTranCxt();
    }

    public override void Enter()
    {
        Do();
    }

    public override void Do()
    {
        Transform cameraTran = CameraMgr.instance.Tran;
        if (cameraTran == null)
            return;
        Transform relativeTran = null;
        TranPart tranPart = null;
        Role rotateRole = Parent;
        if (GrabCxt.type == enRelativeType.hero)
        {
            relativeTran = MyCxt.GrabRole.transform;
            tranPart = Parent.TranPart;
            rotateRole = MyCxt.GrabRole;
        }
        else if (GrabCxt.type == enRelativeType.monster)
        {
            relativeTran = Parent.transform;
            tranPart = MyCxt.GrabRole.TranPart;
            rotateRole = Parent;
        }

        //设置相对不变的角色方向
        Vector3 dir1 = cameraTran.position - relativeTran.position;
        dir1.y = 0;

        Vector3 dir2 = relativeTran.forward;
        dir2.y = 0;

        Vector3 crossValue = Vector3.Cross(dir1, dir2);
        Vector3 rotateForward = rotateRole.transform.forward;
        if (GrabCxt.isLeft && crossValue.y < 0)
        {
            rotateRole.TranPart.SetDir(-rotateForward);
        }
        else if (!GrabCxt.isLeft && crossValue.y > 0)
        {
            rotateRole.TranPart.SetDir(-rotateForward);
        }

        //设置变动的角色位置和朝向
        tranPart.SetPos(relativeTran.transform.position + relativeTran.forward * GrabCxt.Role_distance);
        tranPart.SetDir(-relativeTran.forward);

        m_isPlaying = true;

        ////设置相机位置
        //Vector3 dir = relativeTran.forward;
        ////m_camera.position = m_hero.position + Quaternion.Euler(Camera_euler) * dir * Camera_distance;
        //BigQte.mQteCamera.transform.position = relativeTran.position + (relativeTran.rotation * Quaternion.Euler(grabCxt.Camera_euler)) * Vector3.forward * grabCxt.Camera_distance;

        ////设置相机方向
        //Vector3 heroDir = relativeTran.position - BigQte.mQteCamera.transform.position;
        //BigQte.mQteCamera.transform.forward = Quaternion.Euler(grabCxt.Role_Camera_euler) * heroDir;

        //相机位置和方向和主角相同
        //BigQte.mQteCamera.transform.position = RoleMgr.instance.Hero.transform.position;
        //BigQte.mQteCamera.transform.rotation = RoleMgr.instance.Hero.transform.rotation;
        return;
    }
    public override void Update()
    {
        //抓取者死亡的判断
        if (MyCxt.GrabRole == null)
            BigQte.CurQte.Stop();
        if (CheckLeave())
            return;
    }

    #endregion

    #region Private Methods
    void ClearTranCxt()
    {
    }
    bool CheckLeave()
    {
        if (BigQte.CurQte == null)
        {
            Parent.RSM.GotoState(enRoleState.free);
            return true;
        }

        if (BigQte.CurQte.IsPlaying)
            return false;

        //成功销毁
        if (BigQte.CurQte.IsWin)
        {
            Parent.DeadPart.Handle(true, true);
            MyCxt.GrabRole.CombatPart.Stop();
            m_isPlaying = false;
            return true;
        }

        //GroundCxt cxt = IdTypePool<GroundCxt>.Get();
        //cxt.duration = 0.5f;
        //m_stateBehit.GotoState(cxt, false, true);
        Parent.RSM.CheckFree();
        MyCxt.GrabRole.CombatPart.Stop();
        m_isPlaying = false;
        return true;
        
       
    }
    #endregion


}
