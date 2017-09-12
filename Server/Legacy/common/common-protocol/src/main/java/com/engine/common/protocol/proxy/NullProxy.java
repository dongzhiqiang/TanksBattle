package com.engine.common.protocol.proxy;

import java.io.IOException;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;

public class NullProxy  extends AbstractProxy<Object>{
	
	@Override
	public Object getValue(Context ctx, byte flag) throws IOException{
		// 0000 0001 (1 - 0x01)
		return null;
	}
	
	@Override
	public void setValue(Context ctx, Object value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.NULL;
		out.put((byte)flag);
	}
}
