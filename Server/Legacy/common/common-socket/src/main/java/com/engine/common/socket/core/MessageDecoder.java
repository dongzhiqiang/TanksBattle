package com.engine.common.socket.core;

import static com.engine.common.socket.core.MessageConstant.*;
import org.apache.mina.core.buffer.IoBuffer;
import org.apache.mina.core.session.AttributeKey;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.CumulativeProtocolDecoder;
import org.apache.mina.filter.codec.ProtocolDecoderOutput;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.socket.exception.DecodeException;
import com.engine.common.socket.other.IpUtils;
import com.engine.common.socket.utils.ChecksumUtils;

/**
 * 通信消息({@link Message})解码器
 * 
 */
public class MessageDecoder extends CumulativeProtocolDecoder {

	private static final Logger logger = LoggerFactory.getLogger(MessageDecoder.class);

	private static final AttributeKey CONTEXT = new AttributeKey(MessageDecoder.class, "context");

	/** 数据包状态 */
	public static enum State {
		/** 未开始接收数据 */
		NOT_STARTED,
		/** 数据接收中 */
		RECEIVING;
	}

	/** 消息解析上下文 */
	public static class Context {

		private int length;
		private State state = State.NOT_STARTED;

		public State getState() {
			return state;
		}

		public void setState(State state) {
			this.state = state;
		}

		public int getLength() {
			return length;
		}

		public void setLength(int length) {
			this.length = length;
		}

		/** 重置上下文状态 */
		public void reset() {
			this.length = 0;
			this.state = State.NOT_STARTED;
		}

	}

	@Override
	protected boolean doDecode(IoSession session, IoBuffer in, ProtocolDecoderOutput out) throws Exception {
		Context ctx = getContext(session);
		while (in.hasRemaining()) {
			int length;
			switch (ctx.getState()) {
			case NOT_STARTED:
				// 检查包头数据是否足够
				while (true) {
					if (in.remaining() < PACKAGE_LENGTH) {
						return false;
					}
					in.mark();
					if (in.getInt() == PACKAGE_INDETIFIER) {
						// 已检测到数据头
						break;
					} else {
						// 只略过一个字节
						in.reset();
						in.get();
					}

				}
				// 获取数据包长度
				length = in.getInt();
				if (length <= 4) { // 加上校验码长度至少4Byte
					logger.error("无效的数据包长度[{}]", length);
					return true;
				}
				ctx.setLength(length);
				ctx.setState(State.RECEIVING);
				// 这里没有 break 是正确的
				//$FALL-THROUGH$
			case RECEIVING:
				// 检查包数据是否足够
				length = ctx.getLength();
				if (in.remaining() < length) {
					return false;
				}

				// 真正包数据长度减去校验码4Byte
				length -= 4;
				try {
					// 提前当前包的数据
					byte[] data = new byte[length];
					in.get(data);
					// 将数据转为消息对象
					Message message = Message.valueOf(data);
					//将信息体进行解密
					message.decrypt();
					if (logger.isDebugEnabled()) {
						if (message.hasState(MessageConstant.STATE_COMPRESS)) {
							logger.debug("解码数据,会话:[{}] 头信息:[{}] 信息体:[二进制]", session.getId(), message.toString());
						} else {
							logger.debug("解码数据,会话:[{}] 头信息:[{}] 信息体:[{}]",
									new Object[] { session.getId(), message.toString(), new String(message.getBody()) });
						}
					}

					// 校验码比较
					int checksum = in.getInt();
					int hashcode = ChecksumUtils.checksum(data);
					if (checksum != hashcode) {
						// 校验码错误, 忽略消息
						// message.clearBody().changeToErrorResponse(ResponseConstants.PARAMETER_EXCEPTION);;
						// session.write(message);
						// if (logger.isDebugEnabled()) {
						// logger.debug("校验错误,会话:[{}] 头信息:[{}] 信息体:[{}]",
						// new Object[] { session.getId(), message.toString(), new String(message.getBody()) });
						// }
						FormattingTuple msg = MessageFormatter.format("校验码[{}]错误需要[{}]", checksum, hashcode);
						if (logger.isInfoEnabled()) {
							logger.info(msg.getMessage());
						}
						throw new DecodeException(msg.getMessage());
					} else {
						out.write(message);
					}
				} catch (Exception ex) {
					// 解码错误, 断开连接
					String ip = IpUtils.getIp(session);
					session.close(true);
					logger.error("断开连接[{}], 解码错误{} - {}", new Object[] { ip, ex.getClass(), ex.getMessage() });
					if (logger.isWarnEnabled()) {
						logger.warn("解码错误", ex);
					}
				} finally {
					// 重置上下文
					ctx.reset();
				}
				break;
			default:
				logger.error("无法处理的上下文状态[{}]", ctx.getState());
				break;
			}
		}
		return false;
	}

	/** 获取解码上下文 */
	private Context getContext(IoSession session) {
		Context context = (Context) session.getAttribute(CONTEXT);
		if (context == null) {
			context = new Context();
			session.setAttribute(CONTEXT, context);
		}
		return context;
	}
}
