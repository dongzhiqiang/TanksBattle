using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[NetModule(MODULE.MODULE_ROLE)]
public class RoleHandler
{
    [NetHandler(MODULE_ROLE.PUSH_SYNC_PROP)]
    public void RoleSyncPropVo(RoleSyncPropVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role.GetString(enProp.guid) != info.guid)
            role = role.PetsPart.GetPet(info.guid);
        if (role == null)
            return;

        PropPart part = role.PropPart;
        part.SyncProps(info);
    }

    public void RequestHeroInfo(int heroId)
    {
        RequestHeroInfoVo req = new RequestHeroInfoVo();
        req.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_ROLE, MODULE_ROLE.CMD_REQ_HERO_INFO, req);
    }

    [NetHandler(MODULE_ROLE.CMD_REQ_HERO_INFO)]
    public void OnRequestHeroInfo(FullRoleInfoVo vo)
    {
        //UIMgr.instance.Open<UIHeroInfo>(vo);
        UIMgr.instance.Open<UIHeroInfo2>(vo);
    }

    public void RequestPetInfo(int heroId, string petGuid)
    {
        var req = new RequestPetInfoVo();
        req.heroId = heroId;
        req.guid = petGuid;
        NetMgr.instance.Send(MODULE.MODULE_ROLE, MODULE_ROLE.CMD_REQ_PET_INFO, req);
    }

    [NetHandler(MODULE_ROLE.CMD_REQ_PET_INFO)]
    public void OnRequestPetInfo(FullRoleInfoVo vo)
    {
        UIMgr.instance.Open<UIPetInfo>(vo);
    }
}