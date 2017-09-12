package com.game.gow.module.equip.facade;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.utils.model.Result;
import com.game.gow.module.equip.exception.EquipException;
import com.game.gow.module.equip.model.ChangeWeaponResultVo;
import com.game.gow.module.equip.model.EquipVo;
import com.game.gow.module.equip.service.EquipService;

@Component
public class EquipFacadeImpl implements EquipFacade {
	
	private static final Logger logger=LoggerFactory.getLogger(EquipFacadeImpl.class);
	
	@Autowired
	private EquipService equipService;

	@Override
	public Result<EquipVo> upgrade(long accountId, long roleId, int equipPosIndex) {
		try{
			EquipVo result=equipService.upgrade(accountId, roleId, equipPosIndex);
			return Result.SUCCESS(result);
		}catch (EquipException e) {
			logger.error("玩家[{}]升级装备时发生错误", accountId, e);
			return Result.ERROR(e.getCode());
		}catch (RuntimeException e) {
			logger.error("玩家[{}]升级装备时发生未知错误", accountId, e);
			return Result.ERROR(EquipResult.UNKNOWN_ERROR);
		}
	}

	@Override
	public Result<EquipVo> advance(long accountId, long roleId, int equipPosIndex) {
		try{
			EquipVo result=equipService.advance(accountId, roleId, equipPosIndex);
			return Result.SUCCESS(result);
		}catch (EquipException e) {
			logger.error("玩家[{}]升品装备时发生错误", accountId, e);
			return Result.ERROR(e.getCode());
		}catch (RuntimeException e) {
			logger.error("玩家[{}]升品装备时发生未知错误", accountId, e);
			return Result.ERROR(EquipResult.UNKNOWN_ERROR);
		}
	}

	@Override
	public Result<EquipVo> rouse(long accountId, long roleId, int equipPosIndex) {
		try{
			EquipVo result=equipService.rouse(accountId, roleId, equipPosIndex);
			return Result.SUCCESS(result);
		}catch (EquipException e) {
			logger.error("玩家[{}]觉醒装备时发生错误", accountId, e);
			return Result.ERROR(e.getCode());
		}catch (RuntimeException e) {
			logger.error("玩家[{}]觉醒装备时发生未知错误", accountId, e);
			return Result.ERROR(EquipResult.UNKNOWN_ERROR);
		}
	}

	@Override
	public Result<ChangeWeaponResultVo> changeWeapon(long accountId, long roleId, int posIndex) {
		try{
			ChangeWeaponResultVo result=equipService.changeWeapon(accountId, roleId, posIndex);
			return Result.SUCCESS(result);
		}catch (EquipException e) {
			logger.error("玩家[{}]穿上装备时发生错误", accountId, e);
			return Result.ERROR(e.getCode());
		}catch (RuntimeException e) {
			logger.error("玩家[{}]穿上装备时发生未知错误", accountId, e);
			return Result.ERROR(EquipResult.UNKNOWN_ERROR);
		}
	}
	
	@Override
	public Result<EquipVo> upgradeOnce(long accountId, long roleId, int equipPosIndex) {
		try{
			EquipVo result=equipService.upgradeOnce(accountId, roleId, equipPosIndex);
			return Result.SUCCESS(result);
		}catch (EquipException e) {
			logger.error("玩家[{}]一键升级装备时发生错误", accountId, e);
			return Result.ERROR(e.getCode());
		}catch (RuntimeException e) {
			logger.error("玩家[{}]一键升级装备时发生未知错误", accountId, e);
			return Result.ERROR(EquipResult.UNKNOWN_ERROR);
		}
	}
}
