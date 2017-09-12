package com.game.gow.module.account.service;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.engine.common.ramcache.aop.AutoLocked;
import com.engine.common.ramcache.aop.IsLocked;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.game.gow.module.account.exception.AccountException;
import com.game.gow.module.account.exception.AccountExceptionCode;
import com.game.gow.module.account.manager.Account;
import com.game.gow.module.account.manager.AccountManager;
import com.game.gow.module.account.manager.DeviceInf;
import com.game.gow.module.account.model.Account2Player;
import com.game.gow.module.account.model.AccountVo;
import com.game.gow.module.account.model.LoginVo;
import com.game.gow.module.item.model.ItemVo;
import com.game.gow.module.item.service.ItemService;
import com.game.gow.module.role.service.PetService;
import com.game.gow.module.player.manager.Player;
import com.game.gow.module.player.service.PlayerService;
import com.game.gow.module.role.model.RoleVo;
import com.game.gow.module.system.SystemConfig;

/**
 * 账号服务
 * 
 * @author wenkin
 */
@Service
public class AccountService {

	private static final Logger logger = LoggerFactory.getLogger(AccountService.class);
	
	@Autowired
    private AccountManager accountManager;
	@Autowired
	private PlayerService playerService;
	@Autowired
	private SystemConfig systemConfig;
	@Autowired
	private ItemService itemService;
	@Autowired
	private PetService petService;
	
	/**
	 * 加载账号对象
	 * @param id
	 * @return
	 */
	public Account load(long id){
		return accountManager.load(id);
	}
	
	/**
	 * 获取指定名称的账号(账号有对应的角色时才返回)
	 * @param name 账号名
	 * @return 不存或未完成创建时返回 null
	 */
	public Account loadByName(String name) {
		Account account = accountManager.loadByName(name);
		if (account == null) {
			return null;
		}
		return account;
	}
	
    /**
     * 创建账号与角色
     * @param accountName
     * @param userId
     * @param oldUid
     * @param clientVer
     * @param token
     * @param sp
     * @param loginAddr
     * @param channel
     * @param deviceInf
     * @return
     */
	public Account2Player create(String accountName, String channelId, String userId, short serverId, String clientVer, DeviceInf deviceInf){
		Account account = this.loadByName(accountName);
		if(account==null){
			account= accountManager.create(accountName, channelId, userId, serverId, clientVer, deviceInf);
		}
		final long accountId=account.getId();
		Player player=playerService.load(accountId);
		if(player!=null){
			throw new AccountException(AccountExceptionCode.ACCOUNT_ALREADY_EXISTS, "名为[" + accountName + "]的账号已经存在");
		}
	    try{
			player=playerService.createPlayer(accountId, accountName);
		    //进行角色初始化相关数据
			return Account2Player.valueOf(account, player);		
		} catch (UniqueFieldException e) {
			throw new AccountException(AccountExceptionCode.PLAYER_ALREADY_EXISTS, "名为[" + "@"+accountId + "]的角色已经存在");
		} catch (Exception e) {
			logger.error("初始化奖励内容错误", e);
			throw new AccountException(AccountExceptionCode.INIT_REWARD_ERROR, e);
		}	
	}
	
	/**
	 * 获取账号的登录信息
	 * @param account
	 * @param player
	 * @return
	 */
	public LoginVo getLoginInfo(Account account, Player player) {
		AccountVo accountVo = AccountVo.valueOf(account);
		RoleVo roleVo = RoleVo.valueOf(player);
		List<ItemVo> items = itemService.getItemList(account.getId());
		roleVo.setItems(items);
		List<RoleVo> pets = petService.getPetList(account.getId());
		roleVo.setPets(pets);
		return LoginVo.valueOf(accountVo, roleVo);
	}
	
	/**
	 * 执行登录
	 * @param account
	 */
	@AutoLocked
	public void login(@IsLocked Account account,DeviceInf deviceInf){
		accountManager.login(account,deviceInf);
	}
	
	
	

}
