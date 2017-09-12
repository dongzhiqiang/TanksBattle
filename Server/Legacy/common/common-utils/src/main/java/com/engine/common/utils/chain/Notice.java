package com.engine.common.utils.chain;

/**
 * 处理通知对象
 * 
 * 
 */
public class Notice<T> {

	/** 处理任务名 */
	private final String name;
	/** 处理任务内容 */
	private final T content;
	/** 执行到第几步 */
	private int step;
	/** 当前是进入还是返回 */
	private Way way;
	/** 是否被打断 */
	private boolean interrupt;

	public static <T> Notice<T> valueOf(String name, T content) {
		return new Notice<T>(name, content);
	}
	
	public Notice(String name, T content) {
		this.name = name;
		this.content = content;
	}

	/**
	 * 获取事件名(不可变)
	 * @return
	 */
	public String getName() {
		return name;
	}

	/**
	 * 获取通知信息体(不可变，指引用不变)
	 * @return
	 */
	public T getContent() {
		return content;
	}

	/**
	 * 获取当前的处理步骤
	 * @return
	 */
	public int getStep() {
		return step;
	}

	/**
	 * 设置当前的处理步骤
	 * @param step
	 */
	void setStep(int step) {
		this.step = step;
	}

	/**
	 * 获取当前事件的方向
	 * @return {@link Way#IN} / {@link Way#OUT} 不会出现其他
	 */
	public Way getWay() {
		return way;
	}

	/**
	 * 设置当前事件的方向
	 * @param way
	 */
	void setWay(Way way) {
		this.way = way;
	}

	/**
	 * 判断通知的处理是否被打断
	 * @return
	 */
	public boolean isInterrupt() {
		return interrupt;
	}

	/**
	 * 设置通知处理是否被打断
	 * @param interrupt
	 */
	void setInterrupt(boolean interrupt) {
		this.interrupt = interrupt;
	}
}
