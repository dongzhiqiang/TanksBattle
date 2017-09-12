package com.engine.common.socket.forward;

import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

import javax.annotation.PostConstruct;

import org.apache.mina.core.service.IoHandler;
import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Message;
import com.engine.common.socket.handler.CommandRegister;
import com.engine.common.socket.server.ServerHandler;

/**
 * 可支持消息转发的{@link IoHandler}
 * TODO 未完成
 * 
 */
public class CommunicateHandler extends ServerHandler {

	public CommunicateHandler(CommandRegister register) {
		super(register);
	}

	private Set<ForwardClient> clients = new HashSet<ForwardClient>();
	
	/** 初始化方法 */
	@PostConstruct
	protected void initialize() {
		for (ForwardConfig config : getConfigs()) {
			ForwardClient client = config.buildClient(this);
			client.start();
			clients.add(client);
		}
	}

	private Map<Command, ForwardClient> destinations = new ConcurrentHashMap<Command, ForwardClient>();
	
	@Override
	public void messageReceived(IoSession session, Object in) throws Exception {
		Message message = (Message) in;
		Command command = message.getCommand();

		ForwardClient client = destinations.get(command);
		if (client == null) {
			super.messageReceived(session, in);
			return;
		}

		// 转发处理
		client.forward(session, message);
	}

	private List<ForwardConfig> configs;
	
	// Getter and Setter ...

	public List<ForwardConfig> getConfigs() {
		return configs;
	}

	public void setConfigs(List<ForwardConfig> configs) {
		this.configs = configs;
	}

}
