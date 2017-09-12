using UnityEngine;
using System.Collections;

public class UIHeroMenuItem : MonoBehaviour {

    public UIHeroMenu.MenuType type;
    System.Func<int, bool> callFunc;
    int menuId;
    public void Init(RoleMenuConfig cfg, System.Func<int, bool> onClick)
    {
        StateHandle m_state = this.GetComponent<StateHandle>();
        callFunc = onClick;
        menuId = cfg.menuId;
        m_state.AddClick(OnClickFunc);
        var txt = m_state.GetComponentInChildren<TextEx>();
        txt.text = cfg.menu;
        type = (UIHeroMenu.MenuType)cfg.menuId;
    }

    void OnClickFunc()
    {
        callFunc(menuId);
    }

   
}
