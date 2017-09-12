package com.engine.common.protocol;

import java.io.IOException;
import java.lang.reflect.Type;
import java.util.Collection;
import java.util.Date;
import java.util.Map;
import java.util.concurrent.atomic.AtomicBoolean;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.def.EnumDef;
import com.engine.common.protocol.def.TypeDef;
import com.engine.common.protocol.exception.WrongTypeException;
import com.engine.common.protocol.proxy.AbstractProxy;
import com.engine.common.protocol.proxy.Proxy;
import com.engine.common.protocol.utils.RefTable;

/**
 * 编解码上下文
 * 
 * 
 *
 */
public class Context {
	private RefTable<String> stringTable = new RefTable<String>();
	private RefTable<Object> objectTable = new RefTable<Object>();
	private IoBuffer buffer;
	private Transfer root;
	
	Context(IoBuffer buffer, Transfer root) {
		this.buffer = buffer;
		this.root = root;
	}
	
	public IoBuffer getBuffer() {
		return buffer;
	}

	public TypeDef getTypeDef(int def) {
		return root.getTypeDef(def);
	}
	
	public TypeDef getTypeDef(Class<?> def) {
		return root.getTypeDef(def);
	}
	
	public EnumDef getEnumDef(int def) {
		return root.getEnumDef(def);
	}
	
	public EnumDef getEnumDef(Class<? extends Enum<?>> def) {
		return root.getEnumDef(def);
	}
	
	public TypeDef getMappedDef(Class<?> type) {
		return root.getMappedDef(type, true);
	}
	
	public String getStringRef(int ref) {
		return stringTable.get(ref);
	}
	
	public int getStringRef(String value) {
		return stringTable.get(value);
	}
	
	public int putStringRef(String value) {
		int code = stringTable.incrementAndGet();
		stringTable.put(code, value);
		return code;
	}
	
	public Object getObjectRef(int ref) {
		return objectTable.get(ref);
	}
	
	public int getObjectRef(Object value) {
		return objectTable.get(value);
	}
	
	public int putObjectRef(Object value) {
		int code = objectTable.incrementAndGet();
		objectTable.put(code, value);
		return code;
	}
	
	
	public Object getValue(byte flag) throws IOException {
		byte type = AbstractProxy.getFlagTypes(flag);
		Proxy<?> proxy = root.getProxy(type);
		if(proxy != null) {
			return proxy.getValue(this, flag);
		}
		throw new WrongTypeException(type);
	}
	
	public <T> T getValue(byte flag, Type clz) throws IOException {
		Object value = this.getValue(flag);
		return root.convert(value, clz);
	}	
	
	public void setValue(Object value) throws IOException {
		byte type;
		// 类型
		if(value == null) {
			type = Types.NULL;
		} else if(value instanceof Number) {
			type = Types.NUMBER;
		} else if(value instanceof String) {
			type = Types.STRING;
		} else if(value instanceof Boolean 
				|| value instanceof AtomicBoolean) {
			type = Types.BOOLEAN;
		} else if(value instanceof Enum<?>) {
			type = Types.ENUM;
		} else if(value instanceof Date) {
			type =Types.DATE_TIME;
		} else if(value instanceof Map) {
			type = Types.MAP;
		} else if(value instanceof byte[]) {
			type = Types.BYTE_ARRAY;
		} else if(value.getClass().isArray()) {
			type = Types.ARRAY;
		} else if(value instanceof Collection) {
			type = Types.COLLECTION;
		} else {
			type = Types.OBJECT;
		}
		
		@SuppressWarnings("unchecked")
		Proxy<Object> proxy = (Proxy<Object>) root.getProxy(type);
		if(proxy != null) {
			proxy.setValue(this, value);
		} else {
			throw new WrongTypeException(type);
		}
	}
}
