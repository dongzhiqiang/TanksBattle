package com.engine.common.socket.core;

import static com.engine.common.socket.core.MessageConstant.*;
import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.apache.mina.core.buffer.IoBuffer;
import org.junit.Test;

import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Header;
import com.engine.common.socket.core.Message;
import com.engine.common.socket.utils.ChecksumUtils;

/**
 * {@link Message}的单元测试
 * 
 */
public class MessageTest {
	
	/** 长度:32 格式:16 STATE:1 SN:1 SESSION:2 MODULE:1,2,4 COMMAND:8 */
//	private final byte[] headerArray = {0, 0, 0, 32, 16, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 8, 1, 2, 4 };
	/** 长度:28 格式:16 STATE:1 SN:1 csn:2 MODULE:1,2,4 COMMAND:8 */
	private final byte[] headerArray = {0, 0, 0, 28, 16, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 8, 1, 2, 4 };
	/** 假设的 body */
	private final byte[] body = {127, -128, 127, -128,127, -128, 127, -128};
	/** 假设的 attachment */
	private final byte[] attachment = {1, -1, 1, -1};
	/** 假设的信息 */
	private byte[] messageArray;
	
	public MessageTest() {
		IoBuffer buffer = IoBuffer.allocate(headerArray.length + 4 + body.length + attachment.length);
		buffer.put(headerArray);
		buffer.putInt(body.length + 4);
		buffer.put(body);
		buffer.put(attachment);
		messageArray = buffer.array();
	}
	
	/** 头信息转换测试 */
	@Test
	public void test_header() {
		Header header = Header.valueOf(headerArray);
		assertThat(header.getFormat(), is((byte) 16));
		assertThat(header.isResponse(), is(true));
		assertThat(header.getSn(), is(1L));
//		assertThat(header.getSession(), is(2L));
		assertThat(header.getCsn(), is(2));
		Command command = header.getCommand();
		assertThat(command.getModule().getId(), is(1));
		assertThat(command.getModule().getNext().getId(), is(2));
		assertThat(command.getModule().getNext().getNext().getId(), is(4));
		assertThat(command.getCommand(), is(8));
		
		byte[] bytes = header.toBytes();
		assertThat(bytes.length, is(headerArray.length));
		for (int i = 0; i < headerArray.length; i++) {
			assertThat(headerArray[i], is(bytes[i]));
		}
	}
	
	/** {@link Message}对象转换测试 */
	@Test
	public void test_message() {
		Message message = Message.valueOf(messageArray);
		assertThat(message.getState(), is(STATE_RESPONSE));
		assertThat(message.getFormat(), is((byte) 16));
		assertThat(message.getSn(), is(1L));
//		assertThat(message.getSession(), is(2L));
		assertThat(message.getCsn(), is(2));
		byte[] bytes = message.getBody();
		assertThat(bytes.length, is(body.length));
		for (int i = 0; i < bytes.length; i++) {
			assertThat(body[i], is(bytes[i]));
		}
		bytes = message.getAttachment();
		assertThat(bytes.length, is(attachment.length));
		for (int i = 0; i < bytes.length; i++) {
			assertThat(attachment[i], is(bytes[i]));
		}
	
		bytes = message.toBytes();
		assertThat(bytes.length, is(messageArray.length));
		for (int i = 0; i < messageArray.length; i++) {
			assertThat(messageArray[i], is(bytes[i]));
		}
	}
	
	/**message加密、解密测试*/
	@Test
	public void test_encry_message(){
		Message message_encry = Message.valueOf(messageArray);
		message_encry.encrypt();
		byte[] bytes = message_encry.toBytes();
		
		Message message_decry = Message.valueOf(bytes);
		message_decry.decrypt();
		
		assertThat(message_decry.getState(), is(message_encry.getState()));
		assertThat(message_decry.getFormat(), is(message_encry.getFormat()));
		assertThat(message_decry.getSn(), is(message_encry.getSn()));
		assertThat(message_decry.getCsn(), is(message_encry.getCsn()));
		
		byte[] bytes_de = message_decry.getBody();
		
		assertThat(bytes_de.length, is(body.length));
		for (int i = 0; i < bytes_de.length; i++) {
			assertThat(body[i], is(bytes_de[i]));
		}
		
		byte[] bytes_att = message_decry.getAttachment();
		assertThat(bytes_att.length, is(attachment.length));
		for (int i = 0; i < bytes_att.length; i++) {
			assertThat(attachment[i], is(bytes_att[i]));
		}
	
		byte[] bytes_total = message_decry.toBytes();
		assertThat(bytes_total.length, is(messageArray.length));
		for (int i = 0; i < messageArray.length; i++) {
			assertThat(messageArray[i], is(bytes_total[i]));
		}
		
		//校验码测试
		int hashcode_en= ChecksumUtils.checksum(bytes);
		message_decry.encrypt();
	    int  hashcode_de=ChecksumUtils.checksum(message_decry.toBytes());
	    assertThat(hashcode_de, is(hashcode_en));
		
		
	}

}
