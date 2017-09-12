package com.engine.common.ramcache.orm;

/**
 * 分页参数
 * 
 */
public class Paging {
	
	/**
	 * 构造方法
	 * @param page 页码(从1开始)
	 * @param size 页容量(从1开始)
	 * @return 不会返回null
	 * @throws IllegalArgumentException 参数非法时抛出
	 */
	public static Paging valueOf(int page, int size) {
		return new Paging(page, size);
	}

	/** 页码(从1开始) */
	private final int page;
	/** 页容量(从1开始) */
	private final int size;
	
	/**
	 * 构造方法
	 * @param page 页码(从1开始)
	 * @param size 页容量(从1开始)
	 * @throws IllegalArgumentException 参数非法时抛出
	 */
	public Paging(int page, int size) {
		if (page <= 0 || size <= 0) {
			throw new IllegalArgumentException("页码或页容量必须是大于或等于1的正整数");
		}
		this.page = page;
		this.size = size;
	}
	
	/**
	 * 获取第一条记录的位置
	 * @return
	 */
	public int getFirst() {
		return size * page - size;
	}
	
	/**
	 * 获取最后一条记录的位置
	 * @return
	 */
	public int getLast() {
		return size * page;
	}
	
	// Getter and Setter ...

	/**
	 * 获取页码
	 * @return
	 */
	public int getPage() {
		return page;
	}

	/**
	 * 获取页容量
	 * @return
	 */
	public int getSize() {
		return size;
	}

}
