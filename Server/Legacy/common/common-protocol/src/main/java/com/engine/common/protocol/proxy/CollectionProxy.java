package com.engine.common.protocol.proxy;

import java.io.IOException;
import java.util.Arrays;
import java.util.Collection;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.WrongTypeException;

public class CollectionProxy extends AbstractProxy<Collection<?>> {

	@Override
	public Collection<?> getValue(Context ctx, byte flag) throws IOException {
		byte type = getFlagTypes(flag);
		if (type != Types.COLLECTION) {
			throw new WrongTypeException(Types.COLLECTION, type);
		}
		byte signal = getFlagSignal(flag);

		// 读取数组
		byte arrayFlag = (byte) (Types.ARRAY | signal);
		Object[] array = (Object[]) ctx.getValue(arrayFlag);
		return Arrays.asList(array);
	}

	@Override
	public void setValue(Context ctx, Collection<?> value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.COLLECTION;
		Object[] array = value.toArray();
		int ref = ctx.getObjectRef(array);
		if (ref > 0) {
			// #### 0001
			flag |= 0x01;
			out.put(flag);
			putVarInt32(out, ref);
		} else {
			// 加入引用表
			ctx.putObjectRef(array);

			// #### 0000
			out.put(flag);
			int len = array.length;
			putVarInt32(out, len);
			for (int i = 0; i < len; i++) {
				Object obj = array[i];
				ctx.setValue(obj);
			}
		}
	}
	
	

}
