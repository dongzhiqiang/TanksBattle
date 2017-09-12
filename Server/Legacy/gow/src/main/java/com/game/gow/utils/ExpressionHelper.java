package com.game.gow.utils;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

//import com.game.gow.module.common.resource.Formula;


/**
 * 公式处理助手类
 * 
 */
public class ExpressionHelper {

	@SuppressWarnings("unused")
	private Logger log = LoggerFactory.getLogger(getClass());

//	private Map<String, Formula> caches = new ConcurrentHashMap<String, Formula>();
//
//	private <T> Formula getFormula(String expression, Class<T> clz) {
//		String key = expression + "_" + clz.getSimpleName();
//		Formula f = caches.get(key);
//		if (f == null) {
//			f = new Formula(expression, clz);
//			caches.put(key, f);
//
//			if (log.isDebugEnabled()) {
//				log.debug("创建公式[{}]对象...", expression);
//			}
//		}
//		return f;
//	}
//
//	/**
//	 * 执行公式表达式
//	 * @param expression 公式表达式
//	 * @param ctx 公式执行上下文
//	 * @return 公式表达式执行结果
//	 */
//	public <T> T _invoke(String expression, Class<T> clz, Object ctx) {
//		Formula formula = this.getFormula(expression, clz);
//		@SuppressWarnings("unchecked")
//		T result = (T) formula.calculate(ctx);
//		return result;
//	}

	// ----------------------------------------

	private static volatile ExpressionHelper instance;

	@SuppressWarnings("unused")
	private static ExpressionHelper getInstance() {
		if (instance == null) {
			synchronized (ExpressionHelper.class) {
				if (instance == null) {
					instance = new ExpressionHelper();
				}
			}
		}
		return instance;
	}

	/**
	 * 执行公式表达式
	 * @param expression 公式表达式
	 * @param ctx 公式执行上下文
	 * @param resultType 执行结果类型
	 * @return 公式表达式执行结果
	 */
//	public static <T> T invoke(String expression, Class<T> resultType, Object ctx) {
//		return getInstance()._invoke(expression, resultType, ctx);
//	}

}
