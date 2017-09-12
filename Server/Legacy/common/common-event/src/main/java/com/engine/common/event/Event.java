package com.engine.common.event;

/**
 * 事件对象
 * @param <T> 事件体类型
 */
public class Event<T> {

	/** 事件名 */
	private String name;

	/** 事件体 */
	private T body;
	
	public static <T> Event<T> valueOf(String name, T body) {
		return new Event<T>(name, body);
	}

	/**
	 * 构造方法
	 * @param name 事件名
	 */
	public Event(String name) {
		this.name = name;
	}

	/**
	 * 构造方法
	 * @param name 事件名
	 * @param body 事件体
	 */
	public Event(String name, T body) {
		this.name = name;
		this.body = body;
	}

	/**
	 * 获取 事件名
	 * @return
	 */
	public String getName() {
		return name;
	}

	/**
	 * 获取 事件体
	 * @return
	 */
	public T getBody() {
		return body;
	}

	/**
	 * 设置 事件体
	 * @param body 事件体
	 */
	public void setBody(T body) {
		this.body = body;
	}

}
