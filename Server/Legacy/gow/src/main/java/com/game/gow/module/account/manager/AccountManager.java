package com.game.gow.module.account.manager;

import java.util.Date;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.exception.UniqueFieldException;
import com.engine.common.ramcache.orm.Querier;
import com.engine.common.ramcache.service.EntityBuilder;
import com.engine.common.ramcache.service.EntityCacheService;
import com.engine.common.socket.filter.session.SessionManager;
import com.engine.common.utils.id.IdGenerator;
import com.engine.common.utils.id.MultiServerIdGenerator;
import com.game.gow.module.account.exception.AccountException;
import com.game.gow.module.account.exception.AccountExceptionCode;
import com.game.gow.module.account.model.AccountState;
import com.game.gow.module.system.MultiServerIdGeneratorHolder;

/**
 * 账号管理器
 * 
 * @author wenkin
 */
@Component
public class AccountManager {

	private Logger logger = LoggerFactory.getLogger(getClass());

	@Inject
	private EntityCacheService<Long, Account> accountCache;
	@Autowired
	private Querier querier;
	@Autowired
	private SessionManager sessionManager;
	@Autowired
	private MultiServerIdGeneratorHolder generatorHolder;
	private MultiServerIdGenerator idGenerator;

	/** 帐号名对应帐号标识的映射 */
	private ConcurrentMap<String, Long> name2Id = new ConcurrentHashMap<String, Long>();

	@PostConstruct
	protected void init() {
		generatorHolder.initialize(Account.class, Account.MAX_ID);
		idGenerator = generatorHolder.getGenerator(Account.class);

		List<Object[]> result = querier.list(Account.class, Object[].class, Account.NAME2ID);
		for (Object[] objs : result) {
			name2Id.put((String) objs[0], ((Number) objs[1]).longValue());
		}
		logger.warn("初始化玩家账号数量[{}]", result.size());
	}

	/**
	 * 创建账号
	 * @param name 账号名(包含服标识)
	 * @param channel 帐号来源渠道
	 */
	public Account create(final String accountName, final String channelId, final String userId, final short serverId, final String clientVer, final DeviceInf deviceInf) {
		Account result = this.loadByName(accountName);
		if (result != null) {
			if (IdGenerator.toServer(result.getId()) == serverId) {
				return result;
			} else {
				throw new AccountException(AccountExceptionCode.ACCOUNT_ALREADY_EXISTS, "名为[" + accountName + "]的账号已经存在");
			}
		}

		// 创建新账号实体
		final long id = idGenerator.getNext(serverId);
		if (logger.isDebugEnabled()) {
			logger.debug("生成账号[{}]ID[{}]所属服务器[{} => {}]", new Object[] { accountName, id, serverId, IdGenerator.toServer(id) });
		}
		// 再次检测账号是否存在
		Long oldId = name2Id.putIfAbsent(accountName, id);
		if (oldId != null && !oldId.equals(id)) {
			// 账号已被占用
			throw new AccountException(AccountExceptionCode.ACCOUNT_ALREADY_EXISTS, "名为[" + accountName + "]的账号已经存在");
		}

		try {
			result = accountCache.loadOrCreate(id, new EntityBuilder<Long, Account>() {
				public Account newInstance(Long id) {
					return Account.valueOf(id, accountName, channelId, userId, serverId, clientVer, deviceInf);
				}
			});
		} catch (UniqueFieldException e) {
			// 移除创建失败的玩家账号
			name2Id.remove(accountName, id);
			throw new AccountException(AccountExceptionCode.ACCOUNT_ALREADY_EXISTS, "名为[" + accountName + "]的账号已经存在");
		}
		return result;
	}

	/**
	 * 按账号名加载账号对象
	 * @param name 账号名(包含服标识)
	 * @return
	 */
	public Account loadByName(String name) {
		Long id = name2Id.get(name);
		if (id == null) {
			return null;
		}
		return load(id);
	}

	/**
	 * 按账号名转换ID
	 * @param name 账号名(包含服标识)
	 * @return
	 */
	public Long name2Id(String name) {
		Long id = name2Id.get(name);
		return id;
	}

	/**
	 * 按标识加载账号对象
	 * @param id 标识
	 * @return
	 */
	public Account load(long id) {
		return accountCache.load(id);
	}

	/**
	 * 执行登录时间统计
	 * @param account
	 * @return
	 */
	public void login(Account account,DeviceInf deviceInf) {
		if (account != null) {
			account.loginTimeCount(new Date());
			account.setDeviceInf(deviceInf);
			if (logger.isDebugEnabled()) {
				logger.debug("+++ 账号[{}]登入, 连续在线[{}]...", account.getName(), account.getContinuousDays());
			}
		}
	}

	/**
	 * 执行登出
	 * @param account
	 * @return
	 */
	public void logout(Account account) {
		if (account != null) {
			account.logout(new Date());
			if (logger.isDebugEnabled()) {
				logger.debug("--- 账号[{}]登出, 连续在线[{}]...", account.getName(), account.getContinuousDays());
			}
		}
	}

	/**
	 * 封禁用户
	 * @param account
	 * @param block
	 */
	public void block(Account account, boolean block, int cause) {
		if(AccountState.CLEAN.equals(account.getState())) {
			//被清理状态的玩家无法设置block
			return;
		}
		if (block) {
			account.setState(AccountState.BLOCK);
			sessionManager.kick(cause, account.getId());
		} else {
			account.setState(AccountState.NORMAL);
		}
	}

	/**
	 * 踢所有用户下线
	 */
	public void kickAll(int cause) {
		sessionManager.kickAll(cause);
	}

	/**
	 * 踢指定用户下线
	 * @param playerId
	 * @param cause
	 */
	public void kick(long playerId, int cause) {
		sessionManager.kick(cause, playerId);
	}

	/**
	 * 统计当前的全部帐号数量
	 * @return
	 */
	public long count() {
		return querier.unique(Account.class, Long.class, Account.COUNT);
	}

}
