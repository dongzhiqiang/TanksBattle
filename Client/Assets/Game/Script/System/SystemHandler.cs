using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[NetModule(MODULE.MODULE_SYSTEM)]
public class SystemHandler
{

    [NetHandler(MODULE_SYSTEM.PUSH_ADD_OR_UPDATE_SYSTEM)]
    public void OnAddOrUpdateSystem(AddOrUpdateSystemVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.SystemsPart.AddOrUpdateSystem(info.system);
    }

    [NetHandler(MODULE_SYSTEM.PUSH_ADD_OR_UPDATE_SYSTEMS)]
    public void OnAddOrUpdateSystem(AddOrUpdateSystemsVo info)
    {
        Role role = RoleMgr.instance.Hero;
        foreach(SystemVo system in info.systems)
        {
            role.SystemsPart.AddOrUpdateSystem(system);
        }
        
    }


    public void SendSetTeachData(string key, int val)
    {
        SetTeachDataVo req = new SetTeachDataVo();
        req.key = key;
        req.val = val;
        NetMgr.instance.Send(MODULE.MODULE_SYSTEM, MODULE_SYSTEM.CMD_SET_TEACH_DATA, req);
    }

    [NetHandler(MODULE_SYSTEM.CMD_SET_TEACH_DATA)]
    public void OnSetTeachData()
    {
    }

    [NetHandler(MODULE_SYSTEM.PUSH_SET_TEACH_DATA)]
    public void OnPushSetTeachData(PushSetTeachDataVo data)
    {
        var hero = RoleMgr.instance.Hero;
        var teaches = hero.SystemsPart.Teaches;
        if (teaches != null)
            teaches[data.key] = data.val;
    }

    [NetHandler(MODULE_SYSTEM.PUSH_CLEAR_TEACH_DATA)]
    public void OnClearTeachData()
    {
        var hero = RoleMgr.instance.Hero;
        var teaches = hero.SystemsPart.Teaches;
        if (teaches != null)
            teaches.Clear();
    }
}