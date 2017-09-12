package com.game.gow.module.account.facade;

import java.io.UnsupportedEncodingException;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang3.StringUtils;
import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import com.engine.common.event.Event;
import com.engine.common.event.EventBus;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.core.Attribute;
import com.engine.common.socket.core.Message;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.filter.ManagementFilter;
import com.engine.common.socket.filter.session.SessionEventCause;
import com.engine.common.socket.filter.session.SessionManager;
import com.engine.common.utils.ManagedException;
import com.engine.common.utils.json.JsonUtils;
import com.engine.common.utils.model.Result;
import com.game.gow.module.account.event.LoginCompleteEvent;
import com.game.gow.module.account.event.LoginEvent;
import com.game.gow.module.account.event.LogoutEvent;
import com.game.gow.module.account.manager.Account;
import com.game.gow.module.account.manager.DeviceInf;
import com.game.gow.module.account.model.Account2Player;
import com.game.gow.module.account.model.AccountRequest;
import com.game.gow.module.account.model.AccountState;
import com.game.gow.module.account.model.LoginVo;
import com.game.gow.module.account.model.ReLoginVo;
import com.game.gow.module.account.service.AccountService;
import com.game.gow.module.equip.service.EquipService;
import com.game.gow.module.player.manager.Player;
import com.game.gow.module.player.service.PlayerService;
import com.game.gow.module.role.service.PetService;
import com.game.gow.module.system.SystemConfig;
import com.game.gow.module.system.service.SystemService;
import com.game.gow.utils.HttpUtil;


@Component
public class AccountFacadeImpl implements AccountFacade {
	
	private static final Logger logger=LoggerFactory.getLogger(AccountFacadeImpl.class);
	
	@Autowired
	private  SessionManager sessionManager;
	@Autowired
	private EventBus eventBus;
	@Autowired
	private SystemConfig systemConfig;
	@Autowired
	private AccountService accountService;
	@Autowired
	private PlayerService playerService;
	@Autowired
	private EquipService equipService;
	@Autowired
	private PetService petService;
	@Autowired
	private SystemService systemService;
	
	/** 管理后台标识属性 */
	private static final Attribute<String> ATT_MANAGEMENT = new Attribute<String>(ManagementFilter.MANAGEMENT);
	
	@Override
	public Result<LoginVo> login(IoSession session, AccountRequest accountRequest ) {
		
		try{
			if (!chkLoginToken(accountRequest.getChannelId(), accountRequest.getUserId(), accountRequest.getToken())) {
				return Result.ERROR(AccountResult.LOGIN_KEY_ILLEGAL);
			}
			//验证服务器Id(区号)
			short serverId = (short)accountRequest.getServerId();
		   if (!systemConfig.getServers().contains(serverId)) {
				return Result.ERROR(AccountResult.INVAILD_ACCOUNT_NAME);
		   }
		   String accountName = Account.getAccountName(accountRequest.getChannelId(), accountRequest.getUserId(), serverId);
		   Account account=accountService.loadByName(accountName);
		   DeviceInf deviceInf = DeviceInf.valueOf(accountRequest.getDeviceModel(), accountRequest.getOsName(), accountRequest.getRoot(), accountRequest.getMacAddr(),
				   accountRequest.getNetwork(), accountRequest.getScreenWidth(), accountRequest.getScreenHeight());
		   if(account==null){
			   //未绑定账号,创建账号
				if (!systemConfig.isRegistable()) {
					return Result.ERROR(AccountResult.UNREGISTABLE);
				}
				Account2Player a2p=accountService.create(accountName, accountRequest.getChannelId(), accountRequest.getUserId(), serverId, accountRequest.getClientVer(), deviceInf);
				account=a2p.getAccount();
		   }
		   if (account.getState() == AccountState.BLOCK) {
			   if (StringUtils.isBlank(ATT_MANAGEMENT.getValue(session))) {
				   // 不是管理后台
				   return Result.ERROR(AccountResult.ACCOUNT_IS_BLOCK);
				}
			}
			if (account.getState() == AccountState.CLEAN) {
				return Result.ERROR(AccountResult.ACCOUNT_IS_CLEAN);
			}   
           
           long id = account.getId();
           Player player = playerService.load(id);
           if(player==null){
        	   return Result.ERROR(AccountResult.ACCOUNT_NOT_FOUND);
           }
           if(!equipService.verifyEquipList(player.getEquips())) // 版本更新后可考虑移除
           {
        	   player.setEquips(equipService.getInitEquips("kratos"));
           }
           // !临时 测试宠物
           petService.createTestPets(id);
           
           
   		   IoSession prev = sessionManager.getSession(id);
   		   if (prev != null) {
   			   Event<LogoutEvent> event = LogoutEvent.valueOf(id, SessionEventCause.KICK, prev);
   			   // 玩家处于在线状态
   			   Request<Void> request = Request.valueOf(AccountPush.ENFORCE_LOGOUT, null);
   			   sessionManager.send(request, prev);
   			   // 踢原在线玩家下线(负数的原因标识使通信模块不会触发事件通知)
   			   sessionManager.kick(-SessionEventCause.ENFORCE_LOGOUT, id);
   			   // 以同步方式发出登出事件，迫使状态恢复到下线状态
   			   eventBus.syncPost(event);
   		   }

   		   // 设置会话身份
   		   sessionManager.bind(session, id);
   		   accountService.login(account,deviceInf);
   		   // 发出用户登录事件，通知其他模块完成登录处理
   		   eventBus.post(LoginEvent.valueOf(id, session));
   		   //在全局服更新角色信息
   		   updateRoleInfo(account, player);
   		   //返回的登录信息
   		   LoginVo result=accountService.getLoginInfo(account, player);
   		   return Result.SUCCESS(result);
		}catch(ManagedException e){
			logger.error("玩家登陆时发生错误", e);
			return Result.ERROR(e.getCode());
		}catch(Exception e){
			logger.error("玩家登陆时发生错误", e);
			return Result.ERROR(AccountResult.UNKNOWN_ERROR);
		}
	}
	
	@Override
	public int setNameAndPro(long accountId, String name) {
		if (!validPlayerName(name)) {
			return AccountResult.PLAYER_NAME_ILLEGAL;
		}
		Player player = playerService.load(accountId);
		if (player == null) {
			return AccountResult.ACCOUNT_NOT_FOUND;
		}
		// 已改过名
		if (!player.getName().startsWith("@")) {
			return AccountResult.PLAYER_NAME_ILLEGAL;
		}
		//已经设置过职业

		try {
			boolean result = playerService.setNameAndPro(player, name);
			if (!result) {
				return AccountResult.PROFESSION_CHOOSE_ERROR;
			}
			return AccountResult.SUCCESS;
		} catch (UniqueFieldException ex) {
			return AccountResult.PLAYER_ALREADY_EXISTS;
		} catch (RuntimeException e) {
			logger.error("玩家[{}]选择职业时发生未知错误", accountId, e);
			return AccountResult.UNKNOWN_ERROR;
		}
	}
	
	@Override
	public int loginComplete(long accountId) {
		Player player = playerService.load(accountId);
		eventBus.post(LoginCompleteEvent.valueOf(player));
		return AccountResult.SUCCESS;
	}
	
	@Override
	public int rename(long accountId, String name) {
		if (!validPlayerName(name)) {
			return AccountResult.PLAYER_NAME_ILLEGAL;
		}
		Player player = playerService.load(accountId);
		if (player == null) {
			return AccountResult.ACCOUNT_NOT_FOUND;
		}
		// 已改过名
		if (!player.getName().startsWith("@")) {
			return AccountResult.PLAYER_NAME_ILLEGAL;
		}
		try{
			boolean result=playerService.rename(player, name);
			if(!result){
				return AccountResult.PLAYER_NAME_ILLEGAL;
			}
			return AccountResult.SUCCESS;
		}catch(UniqueFieldException ex){
			
		}catch (RuntimeException e) {
			logger.error("玩家[{}]改名时发生未知错误", accountId, e);
			return AccountResult.UNKNOWN_ERROR;
		}
		return 0;
	}
	
	@Override
	public Result<ReLoginVo> relogin(IoSession session, @InBody String channelId, @InBody String userId, @InBody String token, @InBody int serverId, @InBody int csn) {
		if (!chkLoginToken(channelId, userId, token)) {
			return Result.ERROR(AccountResult.LOGIN_KEY_ILLEGAL);
		}
		String accountName = Account.getAccountName(channelId, userId, (short)serverId); 
		Account account = accountService.loadByName(accountName);
		if (account == null) {
			return Result.ERROR(AccountResult.ACCOUNT_NOT_FOUND);
		}

		// 开始断线重连处理
		long id = account.getId();
		IoSession prev = sessionManager.getSession(id);
		if (prev == null) {
			return Result.ERROR(AccountResult.RELOGIN_FAIL);
		}

		// 不相同SESSION
		if (prev != session) {
			if (prev.isConnected()) {
				// 玩家处于在线状态
				Request<Void> request = Request.valueOf(AccountPush.ENFORCE_LOGOUT, null);
				sessionManager.send(request, prev);
				// 踢原在线玩家下线(负数的原因标识使通信模块不会触发事件通知)
				sessionManager.kick(-SessionEventCause.ENFORCE_LOGOUT, id);
			}

			// 复制旧的会话属性(由于复制了旧会话的身份处理完成标记，因此新会话不会触发IDENTIFIED会话事件)
			sessionManager.replace(prev, session);
		}
		List<Message> messages=sessionManager.getStoreMessage(id, csn, session);
        return Result.SUCCESS(ReLoginVo.valueOf(id, messages));
	}
	
	@Override
	public boolean checkAccount(String account) {
		if(accountService.loadByName(account)!=null){
			return true;
		}
		return false;
	}
	
	// 内部方法
	/** 检查登录密匙是否合法(true:合法,false:不合法) */
	private boolean chkLoginToken(String channelId, String userId, String token) {
		return systemService.getGlobalServerManager().checkLogin(channelId, userId, token);
	}

	/** 全局服更新角色信息 */
	private boolean updateRoleInfo(Account account, Player player) {
		return systemService.getGlobalServerManager().updateRoleInfo(account.getChannelId(), account.getUserId(), player.getName(), player.getLevel(), player.getId(), account.getServerId());
	}

	/**
	 * 检查角色名是否合法
	 * @param name 被检查的角色名
	 * @return true:合法,false:不合法
	 */
	private boolean validPlayerName(String name) {
		if (name.startsWith("@")) {
			return false;
		}
		try {
			byte[] bytes = name.getBytes("GB2312");
			if (bytes.length < 4 || bytes.length > 12) {
				return false;
			}
		} catch (UnsupportedEncodingException e) {
		}
		return true;
	}
}
