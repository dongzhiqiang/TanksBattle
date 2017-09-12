package com.game.gow.module.account.event;

import com.engine.common.event.Event;
import com.game.gow.module.player.manager.Player;

/**
 * 登录完成事件体
 * 其它模块若在登录完成后需要进行相关处理，可以通过创建事件接收者处理
 * 
 * @author wenkin 
 */
public class LoginCompleteEvent implements IdentityEvent {

	/** 事件名 */
	public static final String NAME = "common:loginComplete";

	/** 登录的用户标识 */
	private Player player;

	// Getter and Setter ...

	@Override
	public String getName() {
		return NAME;
	}

	@Override
	public long getOwner() {
		return 0;
	}

	public Player getPlayer() {
		return player;
	}

	// Static Method's ...

	/** 构造方法 */
	public static Event<LoginCompleteEvent> valueOf(Player player) {
		LoginCompleteEvent body = new LoginCompleteEvent();
		body.player = player;
		return Event.valueOf(NAME, body);
	}

}
