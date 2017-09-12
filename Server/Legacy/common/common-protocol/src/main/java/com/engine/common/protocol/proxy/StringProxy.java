package com.engine.common.protocol.proxy;

import java.io.EOFException;
import java.io.IOException;
import java.util.concurrent.TimeUnit;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.UnknowSignalException;
import com.engine.common.protocol.exception.WrongTypeException;
import com.engine.common.protocol.utils.QuickLZUtils;

public class StringProxy extends AbstractProxy<String> {

	public static String CHARSET = "UTF-8";

	/**
	 * 是否对自动压缩字符串
	 */
	private boolean autoCompress = false;
	private int autoSize = 64;
	
	public boolean isAutoCompress() {
		return autoCompress;
	}
	public void setAutoCompress(boolean autoCompress) {
		this.autoCompress = autoCompress;
	}
	
	public int getAutoSize() {
		return autoSize;
	}
	public void setAutoSize(int autoSize) {
		this.autoSize = autoSize;
	}

	public String getValue(Context ctx, byte flag) throws IOException {
		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.STRING) {
			throw new WrongTypeException(Types.STRING, type);
		}
			
		byte signal = getFlagSignal(flag);
		if(signal == 0x00) {
			// #### 0000 
			byte tag = in.get();
			int len = readVarInt32(in, tag);
			if (in.remaining() < len) {
				throw new EOFException();
			}
			byte[] buf = new byte[len];
			in.get(buf);
			String result = new String(buf, CHARSET);
			// 添加到字符串表
			ctx.putStringRef(result);
			return result;
		} else if(signal == 0x01) {
			// #### 0001 
			byte tag = in.get();
			int ref = readVarInt32(in, tag);
			String result = ctx.getStringRef(ref);
			return result;
		} else if(signal == 0x02) {
			// #### 0010
			byte tag = in.get();
			int len = readVarInt32(in, tag);
			if (in.remaining() < len) {
				throw new EOFException();
			}
			byte[] buf = new byte[len];
			in.get(buf);
			// 压缩的字符串
			byte[] unzip = QuickLZUtils.unzip(buf, 30, TimeUnit.SECONDS);
			String result = new String(unzip, CHARSET);
			// 添加到字符串表
			ctx.putStringRef(result);
			return result;
		}
		throw new UnknowSignalException(type, signal);
	}

	public void setValue(Context ctx, String value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.STRING;
		int ref = ctx.getStringRef(value);
		if (ref > 0) {
			// #### 0001
			flag |= 0x01;
			out.put(flag);
			putVarInt32(out, ref);
		} else {
			// 加入引用表
			ctx.putStringRef(value);

			byte[] bytes = value.getBytes(CHARSET);
			if(isAutoCompress() && bytes.length > getAutoSize()) {
				flag |= 0x02;	
				bytes = QuickLZUtils.zip(bytes);
			}
			out.put(flag);
			
			int len = bytes.length;
			putVarInt32(out, len);
			out.put(bytes);
		}
	}

}
