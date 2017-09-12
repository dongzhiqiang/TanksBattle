package com.engine.common.protocol.proxy;

import java.io.EOFException;
import java.io.IOException;

import org.apache.mina.core.buffer.IoBuffer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.protocol.Context;
import com.engine.common.protocol.exception.MalformedVarintException;

public abstract class AbstractProxy<T> implements Proxy<T> {

	protected Logger log = LoggerFactory.getLogger(getClass());
	
	// 1111 0000 
	public static byte TYPE_MASK = (byte) 0xF0;

	// 0000 0111
	public static byte SIGNAL_MASK = (byte) 0x07;
	
	// 1000 0000
	public static final int FLAG_0X80 = 0x80;

	// 0000 1000
	public static final int FLAG_0X08 = 0x08;

	/**
	 * Types
	 * 
	 * @return #### 0000
	 */
	public static byte getFlagTypes(byte flag) {
		byte code = (byte) (flag & TYPE_MASK);
		if(code == 0) {
			return flag;
		}
		return code;
	}
	
	/**
	 * Signal
	 * 
	 * @return 0000 0###
	 */
	public static byte getFlagSignal(byte flag) {
		byte signal = (byte) (flag & SIGNAL_MASK);
		return signal;
	}
	
	public static int readVarInt32(IoBuffer in, byte tag) throws IOException {
		// 1### #### (128 - (byte)0x80)
		if((tag & FLAG_0X80) == 0) {
			return tag & 0x7F;
		}
		
		int signal = tag & SIGNAL_MASK;
		if(in.remaining() < signal) {
			throw new EOFException();
		}
		
		if(signal > 4 || signal < 0) {
			throw new MalformedVarintException(4, signal);
		}
		
		int result = 0;
		for(int i = 8*(signal-1); i>=0; i-=8) {
			byte b = in.get();
			result |= (b & 0xFF) << i;
		}
		return result;
	}
	
	public static void putVarInt32(IoBuffer out, int value) throws IOException {
		if(value < 0) {
			// 不能 < 0
			throw new MalformedVarintException(value);
		}
		
		// 1### #### (128 - (byte)0x80)
		if(value < FLAG_0X80) {
			byte b = (byte) value;
			out.put(b);
		} else if(value <= Integer.MAX_VALUE) {
			// VarInt32
			if((value >>> 24) > 0) {
				byte b = (byte) (FLAG_0X80 | 4);
				out.put(b);
				// 
				byte b1 = (byte)(value >>> 24 & 0xFF);
				byte b2 = (byte)(value >>> 16 & 0xFF);
				byte b3 = (byte)(value >>> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				out.put(b1);
				out.put(b2);
				out.put(b3);
				out.put(b4);
			} else if((value >>> 16) > 0) {
				byte b = (byte) (FLAG_0X80 | 3);
				out.put(b);
				//
				byte b2 = (byte)(value >>> 16 & 0xFF);
				byte b3 = (byte)(value >>> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				out.put(b2);
				out.put(b3);
				out.put(b4);
			} else if((value >>> 8) > 0) {
				byte b = (byte) (FLAG_0X80 | 2);
				out.put(b);
				// 
				byte b3 = (byte)(value >>> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				out.put(b3);
				out.put(b4);
			} else {
				byte b = (byte) (FLAG_0X80 | 1);
				out.put(b);
				//
				byte b4 = (byte)(value & 0xFF);
				out.put(b4);
			}
		} else {
			// 不支持
			throw new IOException(new IllegalArgumentException("VarInt值超过范围"));
		}
	}
	
	public static long readVarInt64(IoBuffer in, byte tag) throws IOException {
		// 1### #### (128 - (byte)0x80)
		if((tag & FLAG_0X80) == 0) {
			return tag & 0x7F;
		}
		
		int signal = tag & SIGNAL_MASK;
		if(in.remaining() < signal) {
			throw new EOFException();
		}
		
		if(signal > 7 || signal < 0) {
			throw new MalformedVarintException(8, signal);
		}

		long result = 0;
		for(int i = 8*(signal-1); i>=0; i-=8) {
			byte b = in.get();
			result |= (long)(b & 0xFF) << i;
		}
		return result;
	}
	
	public static void putVarInt64(IoBuffer out, long value) throws IOException {
		if(value < 0) {
			// 不能 < 0
			throw new MalformedVarintException(value);
		}
		
		// 1### #### (128 - (byte)0x80)
		if(value < FLAG_0X80) {
			byte b = (byte) value;
			out.put(b);
		} else if(value <= Integer.MAX_VALUE) {
			// VarInt32
			if((value >>> 24) > 0) {
				byte b = (byte) (FLAG_0X80 | 4);
				out.put(b);
				// 
				byte b1 = (byte)(value >>> 24 & 0xFF);
				byte b2 = (byte)(value >>> 16 & 0xFF);
				byte b3 = (byte)(value >>> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				out.put(b1);
				out.put(b2);
				out.put(b3);
				out.put(b4);
			} else if((value >>> 16) > 0) {
				byte b = (byte) (FLAG_0X80 | 3);
				out.put(b);
				//
				byte b2 = (byte)(value >>> 16 & 0xFF);
				byte b3 = (byte)(value >>> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				out.put(b2);
				out.put(b3);
				out.put(b4);
			} else if((value >>> 8) > 0) {
				byte b = (byte) (FLAG_0X80 | 2);
				out.put(b);
				// 
				byte b3 = (byte)(value >>> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				out.put(b3);
				out.put(b4);
			} else {
				byte b = (byte) (FLAG_0X80 | 1);
				out.put(b);
				//
				byte b4 = (byte)(value & 0xFF);
				out.put(b4);
			}
		} else if(value <= 0x00FFFFFFFFFFFFFFL) {
			// VarInt64
			if((value >>> 48) > 0) {
				byte b = (byte) (FLAG_0X80 | 7);
				out.put(b);
				// 
				byte b1 = (byte)(value >>> 48 & 0xFF);
				byte b2 = (byte)(value >>> 40 & 0xFF);
				byte b3 = (byte)(value >>> 32 & 0xFF);
				byte b4 = (byte)(value >>> 24 & 0xFF);
				byte b5 = (byte)(value >>> 16 & 0xFF);
				byte b6 = (byte)(value >>> 8 & 0xFF);
				byte b7 = (byte)(value & 0xFF);
				out.put(b1);
				out.put(b2);
				out.put(b3);
				out.put(b4);
				out.put(b5);
				out.put(b6);
				out.put(b7);
			} else if((value >>> 40) > 0) {
				byte b = (byte) (FLAG_0X80 | 6);
				out.put(b);
				//
				byte b2 = (byte)(value >>> 40 & 0xFF);
				byte b3 = (byte)(value >>> 32 & 0xFF);
				byte b4 = (byte)(value >>> 24 & 0xFF);
				byte b5 = (byte)(value >>> 16 & 0xFF);
				byte b6 = (byte)(value >>> 8 & 0xFF);
				byte b7 = (byte)(value & 0xFF);
				out.put(b2);
				out.put(b3);
				out.put(b4);
				out.put(b5);
				out.put(b6);
				out.put(b7);
			} else if((value >>> 32) > 0) {
				byte b = (byte) (FLAG_0X80 | 5);
				out.put(b);
				// 
				byte b3 = (byte)(value >>> 32 & 0xFF);
				byte b4 = (byte)(value >>> 24 & 0xFF);
				byte b5 = (byte)(value >>> 16 & 0xFF);
				byte b6 = (byte)(value >>> 8 & 0xFF);
				byte b7 = (byte)(value & 0xFF);
				out.put(b3);
				out.put(b4);
				out.put(b5);
				out.put(b6);
				out.put(b7);
			} else {
				byte b = (byte) (FLAG_0X80 | 4);
				out.put(b);
				// 
				byte b4 = (byte)(value >>> 24 & 0xFF);
				byte b5 = (byte)(value >>> 16 & 0xFF);
				byte b6 = (byte)(value >>> 8 & 0xFF);
				byte b7 = (byte)(value & 0xFF);
				out.put(b4);
				out.put(b5);
				out.put(b6);
				out.put(b7);
			}
		} else {
			// 不支持
			throw new IOException(new IllegalArgumentException("VarInt值超过范围"));
		}
	}
	
	/* (non-Javadoc)
	 * @see com.engine.common.codec.proxy.Proxy#getValue(com.engine.common.codec.CodecContext, byte)
	 */
	public abstract T getValue(Context ctx, byte flag) throws IOException;

	/* (non-Javadoc)
	 * @see com.engine.common.codec.proxy.Proxy#setValue(com.engine.common.codec.CodecContext, T)
	 */
	public abstract void setValue(Context ctx, T value) throws IOException;
}
