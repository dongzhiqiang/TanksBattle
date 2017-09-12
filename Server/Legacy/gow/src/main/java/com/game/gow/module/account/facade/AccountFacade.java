package com.game.gow.module.account.facade;

import static com.engine.common.socket.filter.session.SessionManager.IDENTITY;
import static com.game.gow.module.account.facade.AccountModule.COMMAND_CHECK_ACCOUNT;
import static com.game.gow.module.account.facade.AccountModule.COMMAND_LOGIN;
import static com.game.gow.module.account.facade.AccountModule.COMMAND_LOGIN_COMPLETE;
import static com.game.gow.module.account.facade.AccountModule.COMMAND_RELOGIN;
import static com.game.gow.module.account.facade.AccountModule.COMMAND_RENAME;
import static com.game.gow.module.account.facade.AccountModule.COMMAND_CHOOSE_PROFESSION;
import static com.game.gow.module.account.facade.AccountModule.MODULE;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.anno.Sync;
import com.engine.common.utils.model.Result;
import com.game.gow.module.account.model.AccountRequest;
import com.game.gow.module.account.model.LoginVo;
import com.game.gow.module.account.model.ReLoginVo;


/**
 * 账号服务门面
 * 
 *@author wenkin
 */
@SocketModule(MODULE)
public interface AccountFacade {

    /**
     * 账号登录
     */
	@Sync("ACCOUNT")
	@SocketCommand(value = COMMAND_LOGIN)
	Result<LoginVo> login(IoSession session, AccountRequest accountRequest);

	/**
	 * 重登录(断线重连的登录方法)
	 * @param session 当前的通信会话
	 * @param channelId 渠道ID
	 * @param userId 用户ID
	 * @param token 登录验证令牌
	 * @param csn 客户端序列号
	 * @return {@link ReLoginVo}
	 */
	@Sync("ACCOUNT")
	@SocketCommand(COMMAND_RELOGIN)
	Result<ReLoginVo> relogin(IoSession session, @InBody String channelId, @InBody String userId, @InBody String token, @InBody int serverId, @InBody int csn);
	
	/**
	 * 检查账号是否存在
	 * @param account 账号名(包括服标识)
	 * @return
	 */
	@SocketCommand(COMMAND_CHECK_ACCOUNT)
	boolean checkAccount(@InBody String account);
	
	/**
	 * 创建角色名和职业
	 * @param name 角色名称
	 * @return 状态码{@link AccountResult}
	 */
	@Sync("ACCOUNT")
	@SocketCommand(COMMAND_CHOOSE_PROFESSION)
	int setNameAndPro(@InSession(IDENTITY) long accountId, @InBody String name);
    
	/**
	 * 修改角色名
	 * @param accountId
	 * @param name
	 * @return 状态码{@link AccountResult}
	 */
	@Sync("ACCOUNT")
	@SocketCommand(COMMAND_RENAME)
	int rename(@InSession(IDENTITY) long accountId,@InBody String name);

	/**
	 * 登录完成
	 * @return 状态码{@link AccountResult}
	 */
	@SocketCommand(COMMAND_LOGIN_COMPLETE)
	int loginComplete(@InSession(IDENTITY) long accountId);

}
