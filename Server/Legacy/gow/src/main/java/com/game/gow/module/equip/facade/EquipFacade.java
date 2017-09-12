package com.game.gow.module.equip.facade;

import static com.engine.common.socket.filter.session.SessionManager.IDENTITY;
import static com.game.gow.module.equip.facade.EquipModule.*;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.utils.model.Result;
import com.game.gow.module.equip.model.ChangeWeaponResultVo;
import com.game.gow.module.equip.model.EquipVo;

/**
 * 装备服务门面
 */
@SocketModule(MODULE)
public interface EquipFacade {
	/**
	 * 升级装备
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	@SocketCommand(COMMAND_UPGRADE)
	Result<EquipVo> upgrade(@InSession(IDENTITY) long accountId,@InBody long roleId,@InBody int equipPosIndex);
	
	/**
	 * 升品装备
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	@SocketCommand(COMMAND_ADVANCE)
	Result<EquipVo> advance(@InSession(IDENTITY) long accountId,@InBody long roleId,@InBody int equipPosIndex);
	
	/**
	 * 觉醒装备
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	@SocketCommand(COMMAND_ROUSE)
	Result<EquipVo> rouse(@InSession(IDENTITY) long accountId,@InBody long roleId,@InBody int equipPosIndex);
	
	/**
	 * 穿上装备
	 * @param accountId
	 * @param equipId 装备唯一ID
	 * @param roleId 装备角色id
	 * @param posIndex 装备孔
	 * @return 装备孔
	 */
	@SocketCommand(COMMAND_CHANGE_WEAPON)
	Result<ChangeWeaponResultVo> changeWeapon(@InSession(IDENTITY) long accountId,@InBody long roleId, @InBody int posIndex);
	
	/**
	 * 一键升级
	 * @param accountId
	 * @param roleId 角色id
	 * @param equipPosIndex 装备位
	 * @return {@link EquipVo}
	 */
	@SocketCommand(COMMAND_UPGRADE_ONCE)
	Result<EquipVo> upgradeOnce(@InSession(IDENTITY) long accountId,@InBody long roleId,@InBody int equipPosIndex);
}
