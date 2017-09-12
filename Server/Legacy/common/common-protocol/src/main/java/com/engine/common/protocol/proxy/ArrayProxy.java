package com.engine.common.protocol.proxy;

import java.io.EOFException;
import java.io.IOException;
import java.lang.reflect.Array;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.UnknowSignalException;
import com.engine.common.protocol.exception.WrongTypeException;

public class ArrayProxy extends AbstractProxy<Object> {

	@Override
	public Object getValue(Context ctx, byte flag) throws IOException {
		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.ARRAY) {
			throw new WrongTypeException(Types.ARRAY, type);
		}

		byte signal = getFlagSignal(flag);
		if (signal == 0x00) {
			// #### 0000
			byte tag = in.get();
			int len = readVarInt32(in, tag);
			if (in.remaining() < len) {
				throw new EOFException();
			}
			Object[] result = new Object[len];
			// 加入引用表
			ctx.putObjectRef(result);
			for (int i = 0; i < len; i++) {
				byte fValue = in.get();
				Object value = ctx.getValue(fValue);
				result[i] = value;
			}
			return result;
		} else if (signal == 0x01) {
			// #### 0001
			byte tag = in.get();
			int ref = readVarInt32(in, tag);
			Object[] result = (Object[]) ctx.getObjectRef(ref);
			return result;
		}
		throw new UnknowSignalException(type, signal);
	}

	@Override
	public void setValue(Context ctx, Object value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.ARRAY;
		int ref = ctx.getObjectRef(value);
		if (ref > 0) {
			// #### 0001
			flag |= 0x01;
			out.put(flag);
			putVarInt32(out, ref);
		} else {
			// 加入引用表
			ctx.putObjectRef(value);

			// #### 0000
			out.put(flag);
			int len = Array.getLength(value);
			putVarInt32(out, len);
			for (int i = 0; i < len; i++) {
				Object obj = Array.get(value, i);
				ctx.setValue(obj);
			}
		}
	}

}
