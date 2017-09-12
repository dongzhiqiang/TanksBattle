package com.engine.common.protocol.proxy;

import java.io.EOFException;
import java.io.IOException;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.WrongTypeException;

public class BytesProxy extends AbstractProxy<byte[]> {

	@Override
	public byte[] getValue(Context ctx, byte flag) throws IOException {
		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.BYTE_ARRAY) {
			throw new WrongTypeException(Types.BYTE_ARRAY, type);
		}

		// byte signal = getFlagSignal(flag);
		// if (signal == 0x00) {
		// #### 0000
		byte tag = in.get();
		int len = readVarInt32(in, tag);
		if (in.remaining() < len) {
			throw new EOFException();
		}
		byte[] result = new byte[len];
		in.get(result);
		return result;
		// }
		// throw new WrongTypeException();
	}

	@Override
	public void setValue(Context ctx, byte[] value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.BYTE_ARRAY;
		// #### 0000
		out.put(flag);
		int len = value.length;
		putVarInt32(out, len);
		out.put(value);
	}
}
