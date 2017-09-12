package com.game.gow.module.item.event;

import com.engine.common.event.Event;
import com.game.gow.module.account.event.IdentityEvent;
import com.game.gow.module.item.model.ItemModifyVo;

public class ItemModifyEvent implements IdentityEvent {

	/** 事件名 */
	public static final String NAME = "item:itemModify";
	
	/** 用户标识 */
	private long id;
	
	/** 修改道具数据*/
	private ItemModifyVo itemModifyVo;
	
	public long getId() {
		return id;
	}

	public void setId(long id) {
		this.id = id;
	}

	public ItemModifyVo getItemModifyVo() {
		return itemModifyVo;
	}

	public void setItemModifyVo(ItemModifyVo itemModifyVo) {
		this.itemModifyVo = itemModifyVo;
	}

	@Override
	public String getName() {
		return NAME;
	}

	@Override
	public long getOwner() {
		return 	id;
	}
	
	/** 构造方法 */
	public static Event<ItemModifyEvent> valueOf(long id, ItemModifyVo itemModifyVo) {
		ItemModifyEvent body = new ItemModifyEvent();
		body.id = id;
		body.itemModifyVo = itemModifyVo;
		return Event.valueOf(NAME, body);
	}

}
