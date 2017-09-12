package com.engine.common.protocol.proxy;

import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.UnknowSignalException;
import com.engine.common.protocol.exception.WrongTypeException;

public class MapProxy extends AbstractProxy<Map<Object, Object>> {

	@Override
	public Map<Object, Object> getValue(Context ctx, byte flag)
			throws IOException {
		// 非明确定义的对象类型，均当做MAP解析

		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.MAP) {
			throw new WrongTypeException(Types.MAP, type);
		}

		byte signal = getFlagSignal(flag);
		if (signal == 0x00) {
			// 对象解析
			try {
				// 对象赋值
				Map<Object, Object> result = new HashMap<Object, Object>();
				// 加入引用表
				ctx.putObjectRef(result);
				// 字段数量, 最大255
				int len = (byte) in.get();
				for (int i = 0; i < len; i++) {
					byte fKey = in.get();
					Object key = ctx.getValue(fKey);
					byte fValue = in.get();
					Object value = ctx.getValue(fValue);
					// 字段赋值
					result.put(key, value);
				}
				return result;
			} catch (Exception e) {
				throw new IOException(e);
			}
		} else if (signal == 0x01) {
			// #### 0001
			byte tag = in.get();
			int ref = readVarInt32(in, tag);
			@SuppressWarnings("unchecked")
			Map<Object, Object> result = (Map<Object, Object>) ctx
					.getObjectRef(ref);
			return result;
		}

		throw new UnknowSignalException(type, signal);
	}

	@Override
	public void setValue(Context ctx, Map<Object, Object> value)
			throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.MAP;
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
			Set<Entry<Object, Object>> entrySet = value.entrySet();
			// 字段数量, 最大255
			int size = entrySet.size();
			out.put((byte) size);
			int total = 0;
			for (Entry<Object, Object> e : entrySet) {
				ctx.setValue(e.getKey());
				ctx.setValue(e.getValue());
				total++;
				if (total > 255) {
					break;
				}
			}
		}
	}
}
