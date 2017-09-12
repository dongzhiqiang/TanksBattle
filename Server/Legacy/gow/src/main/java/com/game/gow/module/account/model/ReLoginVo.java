package com.game.gow.module.account.model;

import java.util.ArrayList;
import java.util.List;

import com.engine.common.protocol.annotation.Transable;
import com.engine.common.socket.core.Message;

/** 
 * 断线重连返回VO
 *
 * @author wenkin
 */

@Transable
public class ReLoginVo {
    
	private List<byte[]> messages;
	
	public static ReLoginVo valueOf(long owner, List<Message> messages)
	{
		ReLoginVo result = new ReLoginVo();
		result.setMessages(new ArrayList<byte[]>());
		for (Message message : messages) {
			result.messages.add(message.toBytes());
		}
		return result;
	}

	public List<byte[]> getMessages() {
		return messages;
	}

	public void setMessages(List<byte[]> messages) {
		this.messages = messages;
	}
}
