package com.game.gow.module.common.event;

import java.util.Date;

import com.engine.common.event.Event;
import com.game.gow.module.account.event.IdentityEvent;

/**
 * 开服事件
 *
 */
public class StartServerEvent implements IdentityEvent {
	public static final String EVENT_NAME = "start:server";

	/** 开服时间 */
	private Date startDate;

	@Override
	public String getName() {
		return EVENT_NAME;
	}

	@Override
	public long getOwner() {
		return 0;
	}

	public Date getStartDate() {
		return startDate;
	}

	// Static method's

	public static Event<StartServerEvent> valueOf(Date startDate) {
		StartServerEvent event = new StartServerEvent();
		event.startDate = startDate;
		return Event.valueOf(EVENT_NAME, event);
	}

}
