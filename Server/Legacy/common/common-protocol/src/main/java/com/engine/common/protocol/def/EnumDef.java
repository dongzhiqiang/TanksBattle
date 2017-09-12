package com.engine.common.protocol.def;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.Arrays;

import org.apache.commons.lang3.builder.CompareToBuilder;
import org.apache.mina.core.buffer.IoBuffer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * 枚举定义
 * 
 */
public class EnumDef implements Comparable<EnumDef> {

	private static Logger log = LoggerFactory.getLogger(EnumDef.class);

	private int code;
	private Class<?> type;
	private String[] names;
	private Enum<?>[] values;

	public static EnumDef valueOf(int code, Class<?> type) {
		EnumDef e = new EnumDef();
		e.code = code;
		e.type = type;

		if (type == null) {
			return e;
		}

		@SuppressWarnings("unchecked")
		Class<? extends Enum<?>> enumClz = (Class<? extends Enum<?>>) type;
		Enum<?>[] values = enumClz.getEnumConstants();
		e.values = values;
		e.names = new String[values.length];
		for (int i = 0; i < values.length; i++) {
			e.names[i] = values[i].name();
		}
		return e;
	}

	public static EnumDef valueOf(int code, String[] names) {
		EnumDef e = new EnumDef();
		e.code = code;
		e.names = names;
		return e;
	}

	public static EnumDef valueOf(ByteBuffer buf) throws IOException {
		// 类型, 类标识, (枚举类名长度, 枚举类名字节), 枚举数值数量, (名字长度, 名字字节)....
		short code = buf.getShort();
		short nLen = buf.getShort();
		byte[] nBytes = new byte[nLen];
		buf.get(nBytes);
		String clzName = new String(nBytes);

		int len = buf.getShort();
		String[] names = new String[len];
//		Arrays.sort(names);放在这个位置貌似有问题,wenkin 修改放到下面了
		for (int i = 0; i < len; i++) {
			int n = buf.getShort();
			byte[] a = new byte[n];
			buf.get(a);
			names[i] = new String(a);
		}
		Arrays.sort(names);//放到这个位置，没有问题了
		Class<?> clz;
		try {
			clz = Class.forName(clzName);
			return EnumDef.valueOf(code, clz);
		} catch (ClassNotFoundException e) {
			// 类型不存在
			// throw new IOException(e);
			log.warn("枚举[{}]不存在, 当作String处理");
			clz = null;
		}
		return EnumDef.valueOf(code, names);
	}

	public void describe(IoBuffer buf) {
		// 类型, 类标识, (枚举类名长度, 枚举类名字节), 枚举数值数量, (名字长度, 名字字节)....
		int code = this.getCode();
		Class<?> enumClz = this.getType();
		String enumName = enumClz.getName();
		byte[] bytes = enumName.getBytes();
		buf.put((byte) 0x00);
		buf.putShort((short) code);
		buf.putShort((short) bytes.length);
		buf.put(bytes);

		String[] names = this.getNames();
		buf.putShort((short) values.length);
		for (String name : names) {
			byte[] nameBytes = name.getBytes();
			buf.putShort((short) nameBytes.length);
			buf.put(nameBytes);
		}
	}

	public int getCode() {
		return code;
	}

	public Class<?> getType() {
		return type;
	}

	protected Enum<?>[] getValues() {
		return values;
	}

	public String[] getNames() {
		return names;
	}

	public Object getValue(int ordinal) {
		if (type != null) {
			return values[ordinal];
		}
		return names[ordinal];
	}

	@Override
	public int compareTo(EnumDef o) {
		return new CompareToBuilder().append(this.code, o.code).append(this.type, o.type).toComparison();
	}

}
