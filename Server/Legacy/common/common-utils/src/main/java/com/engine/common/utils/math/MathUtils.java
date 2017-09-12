package com.engine.common.utils.math;

import java.math.BigDecimal;

/**
 * 数学工具类
 * @author Hyint
 */
public class MathUtils {

	/**
	 * 提供精确的小数位四舍五入处理。
	 * @param value 需要四舍五入的数字
	 * @param scale 小数点后保留几位
	 * @return {@link Double} 四舍五入后的结果
	 */
	public static double round(double value, int scale) {
		BigDecimal b = new BigDecimal(Double.toString(value));
		BigDecimal one = new BigDecimal("1");
		return b.divide(one, scale, BigDecimal.ROUND_HALF_UP).doubleValue();
	}

	/**
	 * 提供精确的小数位向下取整处理。
	 * @param value 需要向下取证的数字
	 * @param scale 精度(小数点后保留几位)
	 * @return {@link Double} 向下取证后的数值
	 */
	public static double roundDown(double v, int scale) {
		BigDecimal b = new BigDecimal(Double.toString(v));
		BigDecimal one = new BigDecimal("1");
		return b.divide(one, scale, BigDecimal.ROUND_DOWN).doubleValue();
	}

	/**
	 * 提供精确的小数位向上取整处理。
	 * @param value 需要向上取整的数字
	 * @param scale 精度(小数点后保留几位)
	 * @return {@link Double} 向上取证后的数值
	 */
	public static double roundUp(double value, int scale) {
		BigDecimal b = new BigDecimal(Double.toString(value));
		BigDecimal one = new BigDecimal("1");
		return b.divide(one, scale, BigDecimal.ROUND_UP).doubleValue();
	}

	/**
	 * 提供精确相除向上取整方法
	 * @param value1 被除数
	 * @param value2 除数(不能为0)
	 * @param scale 精度(小数点后保留几位)
	 * @return {@link Double} 向上取证后的数值
	 */
	public static double divideAndRoundUp(double value1, double value2, int scale) {
		BigDecimal bd1 = new BigDecimal(value1);
		BigDecimal bd2 = new BigDecimal(value2);
		return bd1.divide(bd2, scale, BigDecimal.ROUND_UP).doubleValue();
	}

	/**
	 * 提供精确相除向下取整方法
	 * @param value1 被除数
	 * @param value2 除数(不能为0)
	 * @param scale 精度(小数点后保留几位)
	 * @return {@link Double} 向上取证后的数值
	 */
	public static double divideAndRoundDown(double value1, double value2, int scale) {
		BigDecimal bd1 = new BigDecimal(value1);
		BigDecimal bd2 = new BigDecimal(value2);
		return bd1.divide(bd2, scale, BigDecimal.ROUND_DOWN).doubleValue();
	}

	/**
	 * 汇总全部数值
	 * @param ns
	 * @return
	 */
	public static int sum(int... ns) {
		if (ns == null) {
			return 0;
		}
		int total = 0;
		for (int n : ns) {
			total += n;
		}
		return total;
	}
}
