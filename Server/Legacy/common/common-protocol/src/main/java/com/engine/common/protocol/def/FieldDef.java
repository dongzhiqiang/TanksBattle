package com.engine.common.protocol.def;

import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.Map;

import org.apache.commons.lang3.builder.CompareToBuilder;

/**
 * 属性定义
 * 
 */
public class FieldDef implements Comparable<FieldDef> {

	private int code;
	private String name;
	private Class<?> type;

	private Method getter;
	private Method setter;

	private Field field;

	public static FieldDef valueOf(int code, String name, Class<?> type, Method fastGet, Method fastSet) {
		FieldDef e = new FieldDef();
		e.name = name;
		e.type = type;
		e.code = code;

		e.getter = fastGet;
		e.setter = fastSet;

		if (e.getter != null) {
			e.getter.setAccessible(true);
		}
		if (e.setter != null) {
			e.setter.setAccessible(true);
		}
		return e;
	}

	public static FieldDef valueOf(int code, String name, Class<?> type, Field field) {
		FieldDef e = new FieldDef();
		e.name = name;
		e.type = type;
		e.code = code;
		e.field = field;
		if (e.field != null) {
			e.field.setAccessible(true);
		}
		return e;
	}

	public int getCode() {
		return code;
	}

	public String getName() {
		return name;
	}

	public Class<?> getType() {
		return type;
	}

	public boolean isReadonly() {
		if (type != null && setter == null && field == null) {
			return true;
		}
		return false;
	}

	@SuppressWarnings("rawtypes")
	Object getValue(Object instance) throws Exception {
		if (type == null && instance instanceof Map) {
			return ((Map) instance).get(name);
		}

		Object v;
		if (getter != null) {
			v = getter.invoke(instance);
		} else if (field != null) {
			v = field.get(instance);
		} else {
			v = null;
		}
		return v;
	}

	@SuppressWarnings({ "unchecked", "rawtypes" })
	void setValue(Object instance, Object value) throws Exception {
		if (type == null && instance instanceof Map) {
			((Map) instance).put(name, value);
			return;
		}

		if (isReadonly()) {
			return;
		}

		if (setter != null) {
			setter.invoke(instance, value);
		} else if (field != null) {
			field.set(instance, value);
		}
	}

	@Override
	public String toString() {
		return "FieldDef [" + code + ", " + name + "]";
	}

	@Override
	public int compareTo(FieldDef o) {
		return new CompareToBuilder().append(this.code, o.code).append(this.name, o.name).toComparison();
	}

}
