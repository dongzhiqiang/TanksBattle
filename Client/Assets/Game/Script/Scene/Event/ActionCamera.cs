using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionCamera : SceneAction {

    public static bool IsActionNow = false;
    public static CameraHandle cameraHandle = null;

    public ActionCfg_Camera mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        cameraHandle = null;
        IsActionNow = false;
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_Camera;
    }

    public override void OnAction()
    {
        SceneCfg.CameraCfg cfg = null;
        List<SceneCfg.CameraCfg> cfgList = SceneMgr.instance.SceneData.mCameraList;
        for(int i = 0; i < cfgList.Count; i++)
        {
            if (cfgList[i].cameraFlag == mActionCfg.cameraFlag)
            {
                cfg = cfgList[i];
                break;
            }
        }

        if (cfg != null)
        {
            Room.instance.StartCoroutine(CoStartCamera(cfg.cameraList));
        }
    }

    public static IEnumerator CoStartCamera(List<SceneCfg.CameraItem> cameraList)
    {
        IsActionNow = true;
        if (RoleMgr.instance.Hero != null)
            RoleMgr.instance.Hero.CanMove = false;
        for (int i = 0; i < cameraList.Count; i++)
        {
            float startTime = TimeMgr.instance.realTime;
            SceneCfg.CameraItem info = cameraList[i];
            if (!string.IsNullOrEmpty(info.roleId))
            {
                List<Role> roleList = LevelMgr.instance.CurLevel.GetRoleById(info.roleId);
                if (roleList != null && roleList.Count > 0)
                {
                    Role role = roleList[0];
                    if (role != null && role.State == Role.enState.alive)
                    {
                        Transform t = role.transform;
                        startTime = TimeMgr.instance.realTime;
                        cameraHandle = CameraMgr.instance.Still(t.position, t.forward, info.offset, info.moveTime, info.stayTime, info.fov, info.horizontalAngle, info.verticalAngle, info.distance, info.overDuration);
                        cameraHandle.m_info.durationType = CameraInfo.enDurationType.overWhenOverlay;
                    }
                }
            }
            else if(!string.IsNullOrEmpty(info.groupId) && !string.IsNullOrEmpty(info.pointId))
            {
                SceneCfg.RefPointCfg cfg = null;
                List<SceneCfg.RefGroupCfg> groupList = SceneMgr.instance.SceneData.mRefGroupList;

                for (int j = 0; j < groupList.Count; j++)
                {
                    if (groupList[j].groupFlag == info.groupId)
                    {
                        SceneCfg.RefGroupCfg group = groupList[j];
                        foreach (SceneCfg.RefPointCfg p in group.Points)
                        {
                            if (p.pointFlag == info.pointId)
                                cfg = p;
                        }
                    }

                }

                if (cfg != null)
                {
                    startTime = TimeMgr.instance.realTime;
                    cameraHandle = CameraMgr.instance.Still(cfg.pos, cfg.dir, info.offset, info.moveTime, info.stayTime, info.fov, info.horizontalAngle, info.verticalAngle, info.distance, info.overDuration);
                    cameraHandle.m_info.durationType = CameraInfo.enDurationType.overWhenOverlay;
                }
            }

            if (cameraHandle != null)
            {
                float allTime = info.moveTime + info.stayTime;
                while ((TimeMgr.instance.realTime - startTime) < allTime)
                    yield return 0;
            }
        }
        if (RoleMgr.instance.Hero != null)
            RoleMgr.instance.Hero.CanMove = true;
        IsActionNow = false;
        yield return 0;
    }
}
