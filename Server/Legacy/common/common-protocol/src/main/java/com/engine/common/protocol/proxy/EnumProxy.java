package com.engine.common.protocol.proxy;

import java.io.IOException;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.def.EnumDef;
import com.engine.common.protocol.exception.UnknowEnumDefException;
import com.engine.common.protocol.exception.WrongTypeException;

public class EnumProxy extends AbstractProxy<Object> {

	@Override
	public Object getValue(Context ctx, byte flag) throws IOException {
		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.ENUM) {
			throw new WrongTypeException(Types.ENUM, type);
		}

		// byte signal = getFlagSignal(flag);
		// if (signal == 0x00) {
		// #### 0000
		// 枚举类型
		byte tagType = in.get();
//		int enumType = readVarInt32(in, tagType);
		int enumType=ctx.getValue(tagType, Integer.class);
		EnumDef def = ctx.getEnumDef(enumType);
		if (def == null) {
			if (log.isWarnEnabled()) {
				log.warn("枚举定义[{}]不存在", enumType);
			}
			throw new UnknowEnumDefException(enumType);
		}
		// 枚举值
		byte tagValue = in.get();
		int ordinal = readVarInt32(in, tagValue);
		return def.getValue(ordinal);
	}

	@Override
	public void setValue(Context ctx, Object obj) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.ENUM;
		
		if(!(obj instanceof Enum)) {
			if (log.isWarnEnabled()) {
				log.warn("对象[{}]不是枚举", obj);
			}
			return;
		}
		
		Enum<?> value = (Enum<?>) obj;
		@SuppressWarnings("unchecked")
		Class<? extends Enum<?>> fclz = (Class<? extends Enum<?>>) value.getClass();
		EnumDef def = ctx.getEnumDef(fclz);
		if(def == null) {
			// throw new UnknowTypeDefException(fclz);
			if (log.isWarnEnabled()) {
				log.warn("枚举定义[{}]不存在", fclz);
			}
			String name = value.name();
			ctx.setValue(name);
			return;
		}
		
		// #### 0000
		out.put(flag);
		
		int code = def.getCode();
		int ordinal = value.ordinal();
//		putVarInt32(out, code);
		ctx.setValue(code);
		putVarInt32(out, ordinal);
	}

}
