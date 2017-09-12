package com.engine.common.socket.core;

import org.apache.mina.core.buffer.IoBuffer;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolEncoderAdapter;
import org.apache.mina.filter.codec.ProtocolEncoderOutput;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.socket.utils.ChecksumUtils;

/**
 * 通信消息({@link Message})编码器
 * 
 */
public class MessageEncoder extends ProtocolEncoderAdapter {
	
	private static final Logger logger = LoggerFactory.getLogger(MessageEncoder.class);

	@Override
	public void encode(IoSession session, Object in, ProtocolEncoderOutput out) throws Exception {
		if (in == null) {
			return;
		}
		
		if (in instanceof Message) {
			writeMessage(in, out);
		} else if (in instanceof byte[]) {
			writeByteArray(in, out);
		}
		out.flush();
	}

	private void writeByteArray(Object in, ProtocolEncoderOutput out) {
		byte[] bytes = (byte[]) in;
		
		IoBuffer buffer = IoBuffer.allocate(bytes.length);
		buffer.setAutoExpand(true); 
		buffer.put(bytes);
		buffer.flip();
		out.write(buffer);
	}

	private void writeMessage(Object in, ProtocolEncoderOutput out) {
		Message message = (Message) in;
		//对message信息体进行加密
		message.encrypt();
		byte[] bytes = message.toBytes();
		if (logger.isDebugEnabled()) {
			logger.debug("编码:{}", message.toString());
		}
		IoBuffer buffer = IoBuffer.allocate((bytes == null ? 0 : bytes.length) + 12);
		buffer.putInt(MessageConstant.PACKAGE_INDETIFIER);
		buffer.putInt((bytes == null ? 0 : bytes.length) + 4);
		int hashcode;
		if (bytes != null) {
			buffer.put(bytes);
			hashcode = ChecksumUtils.checksum(bytes);
		} else {
			hashcode = 0;
		}
		buffer.putInt(hashcode);
		buffer.flip();
		/*
		if (logger.isDebugEnabled()) {
			logger.debug("详细返回内容:{}", Arrays.toString(buffer.array()));
		}
		*/
		out.write(buffer);
	}

}
