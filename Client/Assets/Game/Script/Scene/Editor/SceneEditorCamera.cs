using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class SceneEditorCamera : SceneEditorBase
{
    public string name = "";
    public SceneCfg.CameraCfg mCameraCfg;
    public bool bOpen { get; set; }
    public bool bSceneView { get; set; }
    public string cameraFlag { get { return mCameraCfg.cameraFlag; } set { mCameraCfg.cameraFlag = value; } }


    public override void Init()
    {
        mCameraCfg = new SceneCfg.CameraCfg();
    }

    public void Init(SceneCfg.CameraCfg cameraInfo)
    {
        bOpen = true;
        mCameraCfg = cameraInfo;
    }

    public void OnInspectorGUI()
    {
        List<SceneCfg.CameraItem> cameras = mCameraCfg.cameraList;

        Color tmp = GUI.color;

        for (int i = 0; i < cameras.Count; i++)
        {
            if ((i + 1) % 2 == 0)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;

            SceneCfg.CameraItem item = cameras[i];
            using (new AutoBeginHorizontal())
            {
                item.groupId = EditorGUILayout.TextField("刷新组id", item.groupId);
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    List<SceneCfg.RefGroupCfg> groupList = SceneMgr.instance.SceneData.mRefGroupList;
                    for (int j = 0; j < groupList.Count; j++)
                    {
                        int type = j;
                        selectMenu.AddItem(new GUIContent(groupList[j].groupFlag), false, () =>
                        {
                            item.groupId = groupList[type].groupFlag;
                        });
                    }
                    selectMenu.ShowAsContext();
                }

                if (string.IsNullOrEmpty(item.groupId))
                    item.pointId = "";
            }

            using (new AutoBeginHorizontal())
            {
                item.pointId = EditorGUILayout.TextField("刷新点id", item.pointId);
                if (!string.IsNullOrEmpty(item.groupId))
                {
                    if (GUILayout.Button("选择"))
                    {
                        List<SceneCfg.RefGroupCfg> groupList = SceneMgr.instance.SceneData.mRefGroupList;
                        List<SceneCfg.RefPointCfg> Points = new List<SceneCfg.RefPointCfg>();
                        foreach (SceneCfg.RefGroupCfg refCfg in groupList)
                        {
                            if (refCfg.groupFlag == item.groupId)
                                Points = refCfg.Points;
                        }
                        if (Points != null && Points.Count > 0)
                        {
                            GenericMenu selectMenu = new GenericMenu();
                            for (int j = 0; j < Points.Count; j++)
                            {
                                int type = j;
                                selectMenu.AddItem(new GUIContent(Points[j].pointFlag), false, () =>
                                {
                                    item.pointId = Points[type].pointFlag;
                                });
                            }
                            selectMenu.ShowAsContext();
                        }
                    }
                }

            }

            using (new AutoBeginHorizontal())
            {
                item.roleId = EditorGUILayout.TextField("角色id", item.roleId);
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int j = 0; j < RoleCfg.RoleNames.Length; j++)
                    {
                        int type = j;
                        selectMenu.AddItem(new GUIContent(RoleCfg.RoleNames[j]), false, () =>
                        {
                            item.roleId = RoleCfg.RoleIds[type];
                        });
                    }
                    selectMenu.ShowAsContext();
                }

            }

            item.offset = EditorGUILayout.Vector3Field("偏移", item.offset);
            item.verticalAngle = EditorGUILayout.FloatField("高度角", item.verticalAngle);
            item.horizontalAngle = EditorGUILayout.FloatField("水平角", item.horizontalAngle);
            item.fov = EditorGUILayout.FloatField("视野", item.fov);
            item.distance = EditorGUILayout.FloatField("距离", item.distance);
            item.moveTime = EditorGUILayout.FloatField("移动时间", item.moveTime);
            item.stayTime = EditorGUILayout.FloatField("停留时间", item.stayTime);
            item.overDuration = EditorGUILayout.FloatField("返回时间", item.overDuration);
            //item.accSpeed = EditorGUILayout.FloatField("加速度", item.accSpeed);
            //item.decSpeed = EditorGUILayout.FloatField("减速度", item.decSpeed);
            //item.maxSpeed = EditorGUILayout.FloatField("最大速度", item.maxSpeed);
            if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
            {
                if (EditorUtility.DisplayDialog("删除转向", "确定删除转向？", "确定", "取消"))
                {
                    cameras.Remove(item);
                    return;
                }
            }
            GUI.color = Color.red;
            Separator();
            GUI.color = tmp;
        }

        GUI.color = tmp;
        if (GUILayout.Button("添加转向"))
        {
            SceneCfg.CameraItem item = new SceneCfg.CameraItem();
            mCameraCfg.cameraList.Add(item);
        }
    }

    public void OnSceneGUI()
    {

    }

    public override string Save()
    {
        string error = "";
        base.Save();
        return error;
    }

}
