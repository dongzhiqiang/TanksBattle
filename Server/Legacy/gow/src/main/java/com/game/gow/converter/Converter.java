package com.game.gow.converter;

/**
 * 非常抽象的转换器接口
 * <ul>
 * 	<li>S:原类型</li>
 * 	<li>T:目标类型</li>
 * </ul>
 * 
 */
public interface Converter<S, T> {

	/**
	 * 将原始信息转换为特定的内容
	 * @param source 原始信息对象
	 * @param additions 附加信息
	 * @return 无法转换或转换失败一般情况下通过返回 null 表示<br/>
	 * 个别情况无法通过返回值表示的，可考虑使用运行时异常表示。当在这种情况下，需在java doc中进行完整的说明。
	 */
	T convert(S source, Object...additions);
	
}
