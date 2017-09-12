package com.engine.common.protocol;

import org.apache.commons.lang3.builder.CompareToBuilder;

public class IndexedClass implements Comparable<IndexedClass> {
	private Class<?> clz;
	private int idx;

	public Class<?> getClz() {
		return clz;
	}

	public int getIdx() {
		return idx;
	}

	@Override
	public int compareTo(IndexedClass o) {
		return new CompareToBuilder().append(this.idx, o.idx).append(this.clz.getName(), o.clz.getName())
				.toComparison();
	}

	@Override
	public String toString() {
		return "IndexedClass [" + idx + ", " + clz.getName() + "]";
	}

	public IndexedClass(Class<?> clz, int idx) {
		this.clz = clz;
		this.idx = idx;
	}

}