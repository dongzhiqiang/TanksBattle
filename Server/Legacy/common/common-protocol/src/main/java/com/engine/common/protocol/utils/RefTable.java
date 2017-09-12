package com.engine.common.protocol.utils;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicInteger;

public final class RefTable<T> {

	private volatile List<T> refs = new RefList<T>();
	private volatile AtomicInteger index = new AtomicInteger();

	public int incrementAndGet() {
		return index.incrementAndGet();
	}

	public T get(int ref) {
		if (ref > refs.size()) {
			return null;
		}
		T t = (T) refs.get(ref - 1);
		return t;
	}

	public int get(T value) {
		int idx = refs.indexOf(value);
		return idx > -1 ? idx + 1 : -1;
	}

	public void put(int code, T value) {
		int idx = code - 1;
		if (idx < refs.size()) {
			refs.set(idx, value);
		}
		if (idx > index.get()) {
			// 插入空元素
			for (int i = index.get() - 1; i < idx; i++) {
				refs.add(null);
			}
		}
		refs.add(value);
	}

	/** 引用列表 */
	private static class RefList<T> extends ArrayList<T> {
		private static final long serialVersionUID = -6214784282104632316L;

		public int indexOf(Object o) {
			if (o == null) {
				for (int i = 0; i < size(); i++)
					if (get(i) == null)
						return i;
			} else {
				for (int i = 0; i < size(); i++)
					if (o == get(i))
						return i;
			}
			return -1;
		}
	}
}