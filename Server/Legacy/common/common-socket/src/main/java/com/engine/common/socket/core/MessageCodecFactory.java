package com.engine.common.socket.core;

import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolCodecFactory;
import org.apache.mina.filter.codec.ProtocolDecoder;
import org.apache.mina.filter.codec.ProtocolEncoder;

/**
 * 通信消息({@link Message})对象编码工厂
 * 
 */
public class MessageCodecFactory implements ProtocolCodecFactory {
	
	public static final String NAME = "codec";
	
	private MessageDecoder decoder = new MessageDecoder();
	private MessageEncoder encoder = new MessageEncoder();

	@Override
	public ProtocolEncoder getEncoder(IoSession session) throws Exception {
		return encoder;
	}

	@Override
	public ProtocolDecoder getDecoder(IoSession session) throws Exception {
		return decoder;
	}
}
