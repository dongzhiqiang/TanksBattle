package com.game.gow.module.account.event;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.event.AbstractReceiver;
import com.engine.common.utils.id.IdGenerator.IdInfo;
import com.game.gow.module.account.manager.Account;
import com.game.gow.module.account.service.AccountService;
import com.game.gow.module.player.manager.Player;
import com.game.gow.module.player.service.PlayerService;

/**
 * 账号登出事件处理器
 * 
 * @author wenkin
 */
@Component
public class LogoutEventReceiver extends AbstractReceiver<LogoutEvent> {
	
	//改日志记录临时用
	private static final Logger logger=LoggerFactory.getLogger(LogoutEventReceiver.class);
	
	@Autowired
	private AccountService accountService;
	
	@Autowired
	private PlayerService playerService;
	
	@Override
	public String[] getEventNames() {
		return new String[]{LogoutEvent.NAME};
	}

	@Override
	public void doEvent(LogoutEvent event) {
		// 登出
		long accountId = event.getOwner();
		final Account account =accountService.load(accountId);
		final Player player = playerService.load(accountId);
//		accountService.logout(account, player);
		final IdInfo idInfo = new IdInfo(accountId);
//		final LogRecord log = new LogRecord(accountId, account.getName(), player.getName(), 1, player.getLevel(),
//				wallet.getGold(), wallet.getGift(), event.getIp(), account.getContinuousDays(), account.getTotalDays(),
//				account.getCreatedOn().getTime(), account.getChannel(), System.currentTimeMillis());
		logger.info("账号:"+account.getId()+" 玩家:"+player.getId()+"登出服务器:"+idInfo.getServer());
	}

}
