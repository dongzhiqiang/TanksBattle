package com.game.gow.module.item.event;

import org.apache.mina.core.session.IoSession;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.event.AbstractReceiver;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.filter.session.SessionManager;
import com.engine.common.utils.model.Result;
import com.game.gow.module.item.facade.ItemPush;
import com.game.gow.module.item.model.ItemModifyVo;

/**
 * 道具改变事件处理器
 */
@Component
public class ItemModifyEventReceiver extends AbstractReceiver<ItemModifyEvent> {
	
	@Autowired
	private  SessionManager sessionManager;
	
	@Override
	public String[] getEventNames() {
		return new String[]{ItemModifyEvent.NAME};
	}
	
	@Override
	public void doEvent(ItemModifyEvent event) {
		IoSession session = sessionManager.getSession(event.getId());
		if( session != null && session.isConnected() )
		{
			Request<Result<ItemModifyVo>> request = Request.valueOf(ItemPush.ENFORCE_LOGOUT, Result.SUCCESS(event.getItemModifyVo()));
			sessionManager.send(request, session);
		}
	}
}
