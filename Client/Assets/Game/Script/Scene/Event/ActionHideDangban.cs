using UnityEngine;
using System.Collections;

public class ActionHideDangban : SceneAction
{
    public ActionCfg_HideDangban mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_HideDangban;
    }

    public override void OnAction()
    {
        GameObject go = SceneMgr.instance.GetDangban(mActionCfg.flagId);
        if (go == null)
        {
            Debug.LogError(string.Format("挡板{0}获取失败", mActionCfg.flagId));
            return;
        }

        Room.instance.StartCoroutine(HideEffect(go));

    }

    IEnumerator HideEffect(GameObject go)
    {
        Transform closeTran = go.transform.Find("DangBan");
        if (closeTran != null)
        {
            Animator ani = closeTran.GetComponent<Animator>();
            if (ani != null)
            {
                ani.enabled = false;
                ani.Play("DangBanEffect02", 0, 0);
                ani.enabled = true;
                ani.Update(0);

                yield return new WaitForSeconds(2.0f);
            }
            go.gameObject.SetActive(false);
        }
        else
            go.gameObject.SetActive(false);
    }
}
