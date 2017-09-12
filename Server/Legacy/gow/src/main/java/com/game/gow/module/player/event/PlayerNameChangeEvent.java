package com.game.gow.module.player.event;

import com.engine.common.event.Event;
import com.game.gow.module.role.event.RoleChangeEvent;

/**
 * 玩家改名事件
 * 
 * @author wenkin
 */
public class PlayerNameChangeEvent implements RoleChangeEvent {
	
	public static final String NAME = "player:changeName";

	/** 玩家操作标示 */
	private long owner;
	/** 称号集合 */
	private String playerName;

	// Getter and Setter ...

	@Override
	public String getName() {
		return NAME;
	}

	@Override
	public long getOwner() {
		return owner;
	}

	public String getPlayerName() {
		return playerName;
	}

//	@Override
//	public Hero getHero() {
//		throw new IllegalStateException("不允许使用该方法");
//	}

	// Static method's

	public static Event<PlayerNameChangeEvent> valueOf(long owner, String playerName) {
		PlayerNameChangeEvent event = new PlayerNameChangeEvent();
		event.owner = owner;
		event.playerName = playerName;
		return Event.valueOf(NAME, event);
	}

}
