package com.engine.common.protocol.proxy;

import java.io.IOException;
import java.util.concurrent.atomic.AtomicBoolean;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.UnknowSignalException;
import com.engine.common.protocol.exception.WrongTypeException;

public class BooleanProxy extends AbstractProxy<Object> {

	@Override
	public Object getValue(Context ctx, byte flag) throws IOException {
		// ByteBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.BOOLEAN) {
			throw new WrongTypeException(Types.BOOLEAN, type);
		}

		byte signal = getFlagSignal(flag);
		if (signal == 0x00) {
			return false;
		} else if (signal == 0x01) {
			return true;
		}
		throw new UnknowSignalException(type, signal);
	}

	@Override
	public void setValue(Context ctx, Object value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.BOOLEAN;
		if (value instanceof Boolean) {
			Boolean bool = (Boolean) value;
			if (bool) {
				// #### 0001
				flag |= 0x01;
				out.put(flag);
			} else {
				// #### 0000
				out.put(flag);
			}
		} else if (value instanceof AtomicBoolean) {
			AtomicBoolean bool = (AtomicBoolean) value;
			if (bool.get()) {
				// #### 0001
				flag |= 0x01;
				out.put(flag);
			} else {
				// #### 0000
				out.put(flag);
			}
		}
	}

}
