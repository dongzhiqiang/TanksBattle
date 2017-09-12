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
 * 账号登录事件处理器
 * 
 * @author wenkin
 */
@Component
public class LoginEventReceiver extends AbstractReceiver<LoginEvent> {
	//改日志记录临时用
	private static final Logger logger=LoggerFactory.getLogger(LoginEventReceiver.class);
	
	@Autowired
	private AccountService accountService;
	
	@Autowired
	private PlayerService playerService;
//	
//	@Autowired
//	private CurrencyService currencyService;
//	
//	@Static("LogRecord")
//	private RunningLogger logger;
	
	@Override
	public String[] getEventNames() {
		return new String[]{LoginEvent.NAME};
	}

	@Override
	public void doEvent(LoginEvent event) {
		long accountId = event.getOwner();
		
		
		final Account account = this.accountService.load(accountId);
		final Player player = this.playerService.load(accountId);
//		final Wallet wallet = this.currencyService.getWallet(accountId);
		final IdInfo idInfo = new IdInfo(accountId);
//		final LogRecord log = new LogRecord(accountId, account.getName(), player.getName(), 
//				0, player.getLevel(), wallet.getGold(), wallet.getGift(), IpUtils.getIp(event.getSession()), 
//				account.getContinuousDays(), account.getTotalDays(), account.getCreatedOn().getTime(), account.getChannel(), System.currentTimeMillis());
//		logger.log(idInfo.getServer(), null, log);
		logger.info("账号:"+account.getId()+" 玩家:"+player.getId()+"登录到服务器:"+idInfo.getServer());
		
	}

}
