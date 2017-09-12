package com.engine.common.utils.collection;

import java.io.Serializable;
import java.util.AbstractSet;
import java.util.Collection;
import java.util.Iterator;
import java.util.concurrent.ConcurrentHashMap;

/**
 * 基于{@link ConcurrentHashMap}的{@link HashSet}
 * 
 * @param <E> 元素类型
 * @see ConcurrentHashMap
 */
public class ConcurrentHashSet<E> extends AbstractSet<E> implements Serializable {

	private static final long serialVersionUID = -807744025477162175L;
	
	protected final ConcurrentHashMap<E, Boolean> map;
	
	/**
	 * 创建一个带有默认初始容量 (16)、加载因子 (0.75) 和 concurrencyLevel (16) 的新的 set。
	 * @see ConcurrentHashMap#ConcurrentHashMap() 
	 */
	public ConcurrentHashSet() {
		this.map = new ConcurrentHashMap<E, Boolean>();
	}
	
	/**
	 * 创建一个带有指定初始容量、默认加载因子 (0.75) 和 concurrencyLevel (16) 的新的 set。
	 * @param initialCapacity 初始容量。该实现执行内部的大小调整，以容纳这些元素。
	 * @see ConcurrentHashMap#ConcurrentHashMap(int)
	 */
	public ConcurrentHashSet(int initialCapacity) {
		this.map = new ConcurrentHashMap<E, Boolean>(initialCapacity);
	}
	
	/**
	 * 创建一个带有指定初始容量、加载因子和默认 concurrencyLevel (16) 的新的 set。
	 * @param initialCapacity 该实现执行内部的大小调整，以容纳这些元素。
	 * @param loadFactor 加载因子阈值，用来控制重新调整大小。在每 bin 中的平均元素数大于此阈值时，可能要重新调整大小。
	 * @see ConcurrentHashMap#ConcurrentHashMap(int, float)
	 */
	public ConcurrentHashSet(int initialCapacity, float loadFactor) {
		this.map = new ConcurrentHashMap<E, Boolean>(initialCapacity, loadFactor);
	}

	/**
	 * 创建一个带有指定初始容量、加载因子和并发级别的新的 set。
	 * @param initialCapacity 初始容量。该实现执行内部大小调整，以容纳这些元素。
	 * @param loadFactor 加载因子阈值，用来控制重新调整大小。在每 bin 中的平均元素数大于此阈值时，可能要重新调整大小。
	 * @param concurrencyLevel 当前更新线程的估计数。该实现将执行内部大小调整，以尽量容纳这些线程。
	 * @see ConcurrentHashMap#ConcurrentHashMap(int, float, int)
	 */
	public ConcurrentHashSet(int initialCapacity, float loadFactor, int concurrencyLevel) {
		this.map = new ConcurrentHashMap<E, Boolean>(initialCapacity, loadFactor, concurrencyLevel);
	}
	
	/**
	 * 构造一个包含指定 collection 中的元素的新 set。使用默认的加载因子 0.75 和足以包含指定 collection 中所有元素的初始容量来创建 ConcurrentHashMap。
	 * @param collection 其中的元素将存放在此 set 中的 collection
	 */
	public ConcurrentHashSet(Collection<E> collection) {
		this(collection.size());
		addAll(collection);
	}

    @Override
    public int size() {
        return map.size();
    }

    @Override
    public boolean contains(Object o) {
        return map.containsKey(o);
    }

    @Override
    public Iterator<E> iterator() {
        return map.keySet().iterator();
    }

    @Override
    public boolean add(E o) {
        return map.put(o, Boolean.TRUE) == null;
    }

    @Override
    public boolean remove(Object o) {
        return map.remove(o) != null;
    }

    @Override
    public void clear() {
        map.clear();
    }

}
