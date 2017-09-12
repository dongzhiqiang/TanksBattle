package com.engine.common.protocol.proxy;

import java.io.IOException;
import java.math.BigDecimal;
import java.math.BigInteger;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicLong;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.Types;
import com.engine.common.protocol.exception.UnknowSignalException;
import com.engine.common.protocol.exception.WrongTypeException;

public class NumberProxy  extends AbstractProxy<Number>{
	
	public static byte INT32 = 0x01;
	public static byte INT64 = 0x02;
	public static byte FLOAT = 0x03;
	public static byte DOUBLE = 0x04;
	
	@Override
	public Number getValue(Context ctx, byte flag) throws IOException{
		IoBuffer in = ctx.getBuffer();
		byte type = getFlagTypes(flag);
		if (type != Types.NUMBER) {
			throw new WrongTypeException(Types.NUMBER, type);
		}

		// 0000 #000
		boolean nevigate = ((flag & FLAG_0X08) != 0); 
		
		// 0000 0###
		byte signal = getFlagSignal(flag);
		if(signal == INT32) {
			byte tag = in.get();
			int value = readVarInt32(in, tag);
			return nevigate ? -value : value;
		} else if(signal == INT64) {
			byte tag = in.get();
			long value = readVarInt64(in, tag);
			return nevigate ? -value : value;
		} else if(signal == FLOAT) {
			float value = in.getFloat();
			return value;
		} else if(signal == DOUBLE) {
			double value = in.getDouble();
			return value;
		}		
		throw new UnknowSignalException(type, signal);
	}

	@Override
	public void setValue(Context ctx, Number value) throws IOException {
		IoBuffer out = ctx.getBuffer();
		byte flag = Types.NUMBER;
		if(value instanceof Integer 
				|| value instanceof Short 
				|| value instanceof Byte
				|| value instanceof AtomicInteger) {
			int v = value.intValue();
			if(v < 0) {
				flag |= FLAG_0X08 | INT32;
				v = -v;
			} else {
				flag |= INT32;
			}
			out.put(flag);
			putVarInt32(out, v);
		} else if(value instanceof Long 
				|| value instanceof AtomicLong 
				|| value instanceof BigInteger) {
			long v = value.longValue();
			if(v < 0) {
				flag |= FLAG_0X08 | INT64;
				v = -v;
			} else {
				flag |= INT64;
			}
			out.put(flag);
			putVarInt64(out, v);
		} else if(value instanceof Float) {
			float v = value.floatValue();
			flag |= FLOAT;
			out.put(flag);
			out.putFloat(v);
		} else if(value instanceof Double 
				|| value instanceof BigDecimal) {
			double v = value.doubleValue();
			flag |= DOUBLE;
			out.put(flag);
			out.putDouble(v);
		} 
	}
	
}
