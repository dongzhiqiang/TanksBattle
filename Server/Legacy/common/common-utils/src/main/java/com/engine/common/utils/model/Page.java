package com.engine.common.utils.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.SortedSet;

import org.codehaus.jackson.annotate.JsonIgnore;

/**
 * 分页对象，用于存储分页信息与页面内的数据
 * 
 */
public class Page<T> implements Iterable<T>, Serializable {

	private static final long serialVersionUID = 6141397074402785607L;

	/** 当前页第一条数据的位置,从0开始 */
	private long offset;
	/** 每页记录数 */
	private long size;
	/** 当前页中的数据集 */
	private Collection<T> result;
	/** 总记录数 */
	private long count;

	/**
	 * 构造方法，只构造空页
	 */
	public Page() {
		this(0, 0, 0, new ArrayList<T>());
	}

	/**
	 * 默认构造方法
	 * @param start 本页第一项数据的起始位置
	 * @param count 总记录数
	 * @param pageSize 每页容量
	 * @param result 本页包含的数据集
	 */
	public Page(long offset, long count, long pageSize, Collection<T> result) {
		if (offset < 0) {
			throw new IllegalArgumentException("当前页码必须大于1");
		}
		this.size = pageSize;
		this.offset = offset;
		this.count = count;
		this.result = result;
	}

	/**
	 * 获取 总记录数
	 */
	public long getCount() {
		return this.count;
	}

	/**
	 * 获取 总页数
	 */
	@JsonIgnore
	public long getPageCount() {
		if (count == 0) {
			return 1;
		}

		if (count % size == 0)
			return count / size;
		else
			return count / size + 1;
	}

	/**
	 * 获取第一页的页码
	 * @return
	 */
	@JsonIgnore
	public long getFirstPage() {
		if (count > 0) {
			return 1;
		}
		return 0;
	}

	/**
	 * 获得 最后一页的页码
	 * @return
	 */
	@JsonIgnore
	public long getLastPage() {
		return getPageCount();
	}

	/**
	 * 获取 每页记录数
	 */
	@JsonIgnore
	public long getPageSize() {
		return size;
	}

	/**
	 * 获取 当前页中的数据集
	 */
	public Collection<T> getResult() {
		return result;
	}

	@SuppressWarnings("unchecked")
	public T[] arrayResult() {
		return (T[]) result.toArray();
	}

	/**
	 * 获取 当前页中的数据集合大小
	 */
	@JsonIgnore
	public long getResultSize() {
		return this.result.size();
	}

	/**
	 * 取当前页码,页码总是从1开始
	 */
	@JsonIgnore
	public long getPageIndex() {
		if (size > 0) {
			return (offset / size) + 1;
		}
		return 1;
	}

	/**
	 * 是否有下一页
	 */
	@JsonIgnore
	public boolean isHasNextPage() {
		return this.getPageIndex() < this.getPageCount();
	}

	/**
	 * 获得下一页的页码
	 * @return
	 */
	@JsonIgnore
	public long getNextPage() {
		return getPageIndex() + 1;
	}

	/**
	 * 是否有上一页
	 */
	@JsonIgnore
	public boolean isHasPreviousPage() {
		return (this.getPageIndex() > 1);
	}

	/**
	 * 获得上一页的页码
	 * @return
	 */
	@JsonIgnore
	public long getPreviousPage() {
		return getPageIndex() - 1;
	}

	@Override
	public Iterator<T> iterator() {
		return result.iterator();
	}

	public void setResult(Collection<T> result) {
		this.result = result;
	}

	public void setCount(long count) {
		this.count = count;
	}

	public long getOffset() {
		return offset;
	}

	public void setOffset(long offset) {
		this.offset = offset;
	}

	public long getSize() {
		return size;
	}

	public void setSize(long size) {
		this.size = size;
	}

	// Static Method's ...

	/**
	 * 通过指定页的数据列表构建分页对象
	 * @param <T>
	 * @param part 页的数据内容列表
	 * @param count 总记录数
	 * @param pageIndex 页码
	 * @param pageSize 页容量
	 * @return
	 */
	public static <T> Page<T> valueOf(List<T> part, long count, int pageIndex, int pageSize) {
		final int offset = (pageIndex - 1) * pageSize;
		return new Page<T>(offset, count, pageSize, part);
	}

	/**
	 * 通过指定页的数据列表构建分页对象
	 * @param <T>
	 * @param part 页数据内容的排序集合
	 * @param count 总记录数
	 * @param pageIndex 页码
	 * @param pageSize 页容量
	 * @return
	 */
	public static <T> Page<T> valueOf(SortedSet<T> part, long count, long pageIndex, long pageSize) {
		final long offset = (pageIndex - 1) * pageSize;
		return new Page<T>(offset, count, pageSize, part);
	}

	/**
	 * 获取任一页第一条数据的位置
	 * @param pageIndex 页码
	 * @param pageSize 页容量
	 * @return 数据的位置从0开始
	 */
	public static long getStartOfPage(long pageIndex, long pageSize) {
		return (pageIndex - 1) * pageSize;
	}

	/**
	 * 获取指定的分页内容
	 * @param all
	 * @param page
	 * @param size
	 * @return
	 */
	@SuppressWarnings("unchecked")
	public static <T> List<T> getContentOfPage(List<T> all, int page, int size) {
		int count = all.size();
		int offset = (page - 1) * size;
		if (offset >= count) {
			return Collections.EMPTY_LIST;
		}
		int end = offset + size;
		if (end >= count) {
			end = count;
		}
		return all.subList(offset, end);
	}

}