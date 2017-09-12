package com.engine.common.utils.model;

import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Map.Entry;

import com.engine.common.utils.math.MathUtils;
import com.engine.common.utils.math.RandomUtils;

/**
 * 人品(RP = Ratio of Probability)对象
 * 
 */
public class RatioProbability<T> {

	/** 概率与结果的映射关系 */
	private LinkedHashMap<Key, T> odds = new LinkedHashMap<Key, T>();

	/** 构造器 */
	private RatioProbability(Map<T, Integer> values) {
		double total = 0.0;
		for (int i : values.values()) {
			total += i;
		}
		double count = 0.0;
		int i = 0;
		for (Entry<T, Integer> entry : values.entrySet()) {
			int v = entry.getValue();
			if (v > 0) {
				Key key;
				if (i < values.size()) {
					count += v;
					key = Key.valueOf(i, count / total);
				} else {
					key = Key.valueOf(i, 1.0);
				}
				odds.put(key, entry.getKey());
			}
			i++;
		}
	}

	/** 构造器 */
	private RatioProbability(T[] values, int[] weight) {
		double total = MathUtils.sum(weight);
		double count = 0.0;
		for (int i = 0; i < values.length; i++) {
			int v = weight[i];
			if (v > 0) {
				Key key;
				if (i != values.length - 1) {
					count += v;
					key = Key.valueOf(i, count / total);
				} else {
					key = Key.valueOf(i, 1.0);
				}
				odds.put(key, values[i]);
			}
		}
	}

	/** 获取人品结果 */
	public T getResult() {
		double value = RandomUtils.nextDouble();
		double prev = 0.0;
		for (Entry<Key, T> entry : odds.entrySet()) {
			Key key = entry.getKey();
			if (key.getRate() == prev) {
				continue;
			}
			double odd = key.getRate();
			if (value >= prev && value <= odd) {
				return entry.getValue();
			}
			prev = odd;
		}
		throw new IllegalStateException("概率内容无法获取随机结果:" + odds.toString());
	}

	/** 获取人品结果 */
	public Ratio<T> getRatioResult() {
		double value = RandomUtils.nextDouble();
		double prev = 0.0;
		for (Entry<Key, T> entry : odds.entrySet()) {
			Key key = entry.getKey();
			if (key.getRate() == prev) {
				continue;
			}
			double odd = key.getRate();
			if (value >= prev && value <= odd) {
				return Ratio.valueOf(key.getSeed(), entry.getValue());
			}
			prev = odd;
		}
		throw new IllegalStateException("概率内容无法获取随机结果:" + odds.toString());
	}

	// Static Method's ...

	/** 构造方法 */
	public static <T> RatioProbability<T> valueOf(Map<T, Integer> values) {
		return new RatioProbability<T>(values);
	}

	/** 构造方法 */
	public static <T> RatioProbability<T> valueOf(T[] values, int[] weight) {
		return new RatioProbability<T>(values, weight);
	}

	// 内部类
	public static class Ratio<T> {
		private int seed;
		private T value;

		public int getSeed() {
			return seed;
		}

		public T getValue() {
			return value;
		}

		private Ratio() {
		}

		private static <T> Ratio<T> valueOf(int seed, T value) {
			Ratio<T> e = new Ratio<T>();
			e.seed = seed;
			e.value = value;
			return e;
		}
	}

	/**
	 * 
	 */
	private static class Key {
		private int seed;
		private double rate;

		public int getSeed() {
			return seed;
		}

		public double getRate() {
			return rate;
		}

		@Override
		public int hashCode() {
			final int prime = 31;
			int result = 1;
			long temp;
			temp = Double.doubleToLongBits(rate);
			result = prime * result + (int) (temp ^ (temp >>> 32));
			result = prime * result + seed;
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
			Key other = (Key) obj;
			if (Double.doubleToLongBits(rate) != Double.doubleToLongBits(other.rate))
				return false;
			if (seed != other.seed)
				return false;
			return true;
		}

		@Override
		public String toString() {
			return "Key [seed=" + seed + ", rate=" + rate + "]";
		}

		private static Key valueOf(int seed, double rate) {
			Key e = new Key();
			e.seed = seed;
			e.rate = rate;
			return e;
		}
	}
}
