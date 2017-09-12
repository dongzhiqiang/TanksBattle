package com.engine.common.protocol.proxy;

import java.io.IOException;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.def.FieldDef;
import com.engine.common.protocol.def.TypeDef;
import com.engine.common.protocol.exception.ObjectProxyException;
import com.engine.common.protocol.exception.UnknowSignalException;
import com.engine.common.protocol.exception.UnknowTypeDefException;
import com.engine.common.protocol.exception.WrongTypeException;

public class ObjectProxy extends AbstractProxy<Object> {

	@Override
	public Object getValue(Context ctx, byte flag) throws IOException {
		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.OBJECT) {
			throw new WrongTypeException(Types.OBJECT, type);
		}
			
		byte signal = getFlagSignal(flag);
		if(signal == 0x00) {
			// #### 0000
//			byte tag = in.get();
//			int rawType = readVarInt32(in, tag);
			byte tag=in.get();
			int rawType=ctx.getValue(tag, Integer.class);
			
			if (rawType == 0) {
				// TODO 未定义的类型
				throw new UnknowTypeDefException(rawType);
			}

			// 对象解析
			TypeDef def = ctx.getTypeDef(rawType);
			if(def == null) {// || def.getCode() < 0) {
				if (log.isWarnEnabled()) {
					log.warn("传输对象类型定义[{}]不存在", type);
				}
				throw new UnknowTypeDefException(rawType);
			}
			
			List<FieldDef> fields = def.getFields();
			Object obj;
			try {
				// 对象赋值
				obj = def.newInstance();
			} catch (Exception e) {
				throw new ObjectProxyException(e);
			}
			// 加入引用表
			ctx.putObjectRef(obj);
			// 字段数量, 最大255
			byte len = in.get();
			for (int i = 0; i < len; i++) {
				byte fValue = in.get();
				FieldDef fieldDef = fields.get(i);
				Class<?> clz = fieldDef.getType();
				Object value = ctx.getValue(fValue, clz);
				if(value == null) {
					continue;
				}
				// 字段赋值
				try {
					def.setValue(obj, i, value);
				} catch (Exception e) {
					throw new ObjectProxyException(e);
				}
			}
			return obj;
		} else if(signal == 0x01) {
			// #### 0001 
			byte tag = in.get();
			int ref = readVarInt32(in, tag);
			Object result = ctx.getObjectRef(ref);
			return result;
		}
		throw new UnknowSignalException(type, signal);
	}

	@Override
	public void setValue(Context ctx, Object value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.OBJECT;
		int ref = ctx.getObjectRef(value);
		if (ref > 0) {
			// #### 0001
			flag |= 0x01;
			out.put(flag);
			putVarInt32(out, ref);
		} else {
			Class<? extends Object> type = value.getClass();
			TypeDef def = ctx.getTypeDef(type);
			if (def == null) {// || def.getCode() < 0) {
				if (log.isInfoEnabled()) {
					log.info("传输对象类型定义[{}]不存在", type);
				}
				// 类型定义不存在
				// throw new UnknowTypeDefException(type);
				TypeDef mappedDef = ctx.getMappedDef(type);
				List<FieldDef> fields = mappedDef.getFields();
				int size = fields.size();
				Map<Object, Object> map = new HashMap<Object, Object>(size);
				for (int i = 0; i < size; i++) {
					FieldDef fieldDef = fields.get(i);
					String k = fieldDef.getName();
					Object o;
					try {
						o = mappedDef.getValue(value, i);
						if(o != null) {
							map.put(k, o);							
						}
					} catch (Exception e) {
						log.error("对象[{}]属性[{}]赋值异常",
								new Object[] { mappedDef.getClass(), i, e });
						throw new ObjectProxyException(e);
					}
				}
				ctx.setValue(map);				
			} else {
				// 加入引用表
				ctx.putObjectRef(value);

				// #### 0000
				out.put(flag);
				
				int code = def.getCode();//对象标识用hashCode表示后，可能为负数
				ctx.setValue(code);
//				putVarInt32(out, code);

				// 字段数量, 最大255
				List<FieldDef> fields = def.getFields();
				int size = fields.size();
				out.put((byte) size);
				// 遍历属性
				for (int i = 0; i < size; i++) {
					Object o;
					try {
						o = def.getValue(value, i);
						ctx.setValue(o);
					} catch (Exception e) {
						log.error("对象[{}]属性[{}]赋值异常",
								new Object[] { def.getClass(), i, e });
						throw new ObjectProxyException(e);
					}
				}
			}
		}
	}
}
