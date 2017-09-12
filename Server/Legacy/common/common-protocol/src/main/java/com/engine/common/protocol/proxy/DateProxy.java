package com.engine.common.protocol.proxy;

import java.io.IOException;
import java.util.Date;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.WrongTypeException;

public class DateProxy extends AbstractProxy<Date> {

	@Override
	public Date getValue(Context ctx, byte flag) throws IOException {
		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.DATE_TIME) {
			throw new WrongTypeException(Types.DATE_TIME, type);
		}

		// byte signal = getFlagSignal(flag);
		// if (signal == 0x00) {
		// #### 0000
		byte tag = in.get();
		long timestame = readVarInt64(in, tag);
		return new Date(timestame * 1000);
		// }
		// throw new WrongTypeException();
	}

	@Override
	public void setValue(Context ctx, Date value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.DATE_TIME;
		// #### 0000
		out.put(flag);
		long timestame = value.getTime() / 1000;
		if(timestame < 0) {
			// 时间为1970-1-1 0:00:00.000 时, 根据时区信息， 有机会 < 0, 导致出错, 所以要置 0
			timestame = 0;
		}
		putVarInt64(out, timestame);
	}
}
