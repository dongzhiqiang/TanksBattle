package com.engine.common.ramcache.service;

/**
 * 索引值对象
 * 
 */
public class IndexValue {
	
	/**
	 * 构造方法
	 * @param name 索引名
	 * @param value 索引值
	 * @return
	 */
	public static IndexValue valueOf(String name, Object value) {
		if (name == null) {
			throw new IllegalArgumentException("索引名不能为null");
		}
		return new IndexValue(name, value);
	}

	private final String name;
	private final Object value;
	
	/** 构造方法 */
	private IndexValue(String name, Object value) {
		this.name = name;
		this.value = value;
	}

	/**
	 * 获取索引名
	 * @return
	 */
	public String getName() {
		return name;
	}

	/**
	 * 获取索引值
	 * @return
	 */
	public Object getValue() {
		return value;
	}
	
	/**
	 * 获取索引值
	 * @return
	 */
	@SuppressWarnings("unchecked")
	public <T> T getValue(Class<T> clz) {
		return (T) value;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((name == null) ? 0 : name.hashCode());
		result = prime * result + ((value == null) ? 0 : value.hashCode());
		return result;
	}
	
	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		IndexValue other = (IndexValue) obj;
		if (name == null) {
			if (other.name != null)
				return false;
		} else if (!name.equals(other.name))
			return false;
		if (value == null) {
			if (other.value != null)
				return false;
		} else if (!value.equals(other.value))
			return false;
		return true;
	}
	
}
