using UnityEngine;
using UnityEditor;
using System.Collections;

public class GrabEditor2 : EditorWindow
{
    public GrabCxt2 grabCxt;
    
    public Transform m_monster;
    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.GRAB_EDITOR2, OnOpen);
    }
    static void OnOpen(object param1, object param2, object param3)
    {
        string title = (string)param1;
        GrabCxt2 cfg = (GrabCxt2)param2;
        Transform source = (Transform)param3;
        ShowWindow(title, cfg, source);
    }

    public static void ShowWindow(string title, GrabCxt2 cfg, Transform source)
    {
        GrabEditor2 instance = (GrabEditor2)EditorWindow.GetWindow(typeof(GrabEditor2), true);
        instance.grabCxt = cfg;
        instance.minSize = new Vector2(200f, 100.0f);
        instance.titleContent = new GUIContent(title);
    }

    private void OnGUI()
    {
        if (grabCxt != null)
        {
            grabCxt.type = (enRelativeType)EditorGUILayout.Popup("相对移动类型", (int)grabCxt.type, GrabCxt2.RelativeTypeName);
            m_monster = (Transform)EditorGUILayout.ObjectField("怪物", m_monster, typeof(Transform), true);

            if (GUILayout.Button("保存位置"))
            {
                save();
            }
        }
    }

    void save()
    {
        if (grabCxt == null)
            return;

        Transform camera = CameraMgr.instance.Tran;
        Transform changeTran = m_monster;
        Transform relativeTran = RoleMgr.instance.Hero.transform;
        if (grabCxt.type == enRelativeType.monster)
        {
            changeTran = RoleMgr.instance.Hero.transform;
            relativeTran = m_monster;
        }

        Vector3 cameraDir = camera.position - relativeTran.position;

        //从主角正前方 旋转到相机方向的角度
        Quaternion quater = Quaternion.FromToRotation(relativeTran.forward, cameraDir);
        grabCxt.Camera_euler = quater.eulerAngles;

        //主角方向转到相机正前方的角度
        Vector3 heroDir = relativeTran.position - camera.position;
        Quaternion delta = Quaternion.FromToRotation(heroDir, camera.forward);
        grabCxt.Role_Camera_euler = delta.eulerAngles;

        //相机和角色距离
        grabCxt.Camera_distance = Vector3.Distance(relativeTran.position, camera.position);

        //两角色间的距离
        grabCxt.Role_distance = Vector3.Distance(relativeTran.position, m_monster.position);

        //保存相机在角色哪一侧
        cameraDir.y = 0;
        Vector3 heroForward = relativeTran.forward;
        heroForward.y = 0;
        Vector3 aaa = Vector3.Cross(cameraDir, heroForward);
        if (aaa.y > 0)
            grabCxt.isLeft = true;
        if (aaa.y < 0)
            grabCxt.isLeft = false;
    }
}
