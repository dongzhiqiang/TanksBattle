package com.game.gow.module.item.facade;

import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.anno.SocketPush;
import com.engine.common.socket.core.Command;

import static com.game.gow.module.item.facade.ItemModule.*;

@SocketPush
@SocketModule(MODULE)
public interface ItemPush {
	/** 道具改变 */
	public static final Command ENFORCE_LOGOUT = Command.valueOf(PUSH_ITEM_MODIFY, MODULES);

	/**
	 * 道具改变 
	 */
	@SocketCommand(PUSH_ITEM_MODIFY)
	void itemModify();
}
