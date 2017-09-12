package com.engine.common.ramcache.aop;

import java.lang.annotation.Annotation;
import java.lang.reflect.Array;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.lock.ChainLock;
import com.engine.common.ramcache.lock.LockUtils;

/**
 * 锁提取器
 * 
 */
public class LockExtractor {

	private static final Logger logger = LoggerFactory.getLogger(LockExtractor.class);

	private HashMap<Integer, IsLocked> lockArgs = new HashMap<Integer, IsLocked>();

	/**
	 * 获取参数对应的锁对象
	 * @param args
	 * @return 会返回null
	 */
	@SuppressWarnings("rawtypes")
	public ChainLock getLock(Object[] args) {
		ArrayList<Object> lockObjs = new ArrayList<Object>();

		for (Entry<Integer, IsLocked> entry : lockArgs.entrySet()) {
			IsLocked isLocked = entry.getValue();
			Object arg = args[entry.getKey()];
			if (arg == null) {
				continue;
			}

			if (!isLocked.element()) {
				lockObjs.add(arg);
				continue;
			}

			if (arg instanceof Collection) {
				for (Object obj : (Collection) arg) {
					if (obj == null) {
						continue;
					}
					lockObjs.add(obj);
				}
				continue;
			}

			if (arg.getClass().isArray()) {
				for (int i = 0; i < Array.getLength(arg); i++) {
					Object obj = Array.get(arg, i);
					if (obj == null) {
						continue;
					}
					lockObjs.add(obj);
				}
				continue;
			}

			if (arg instanceof Map) {
				for (Object obj : ((Map) arg).values()) {
					if (obj == null) {
						continue;
					}
					lockObjs.add(obj);
				}
				continue;
			}

			logger.error("不支持的类型[{}]", arg.getClass().getName());
		}

		if (lockObjs.isEmpty()) {
			if (logger.isDebugEnabled()) {
				logger.debug("没有获得锁目标");
			}
			return null;
		}

		if (logger.isDebugEnabled()) {
			logger.debug("锁目标为:{}", Arrays.toString(lockObjs.toArray()));
		}
		return LockUtils.getLock(lockObjs.toArray());
	}

	/** 构造方法 */
	public static LockExtractor valueOf(Method method) {
		LockExtractor result = new LockExtractor();
		Annotation[][] annos = method.getParameterAnnotations();
		for (int i = 0; i < annos.length; i++) {
			IsLocked isLocked = null;
			for (Annotation anno : annos[i]) {
				if (anno instanceof IsLocked) {
					isLocked = (IsLocked) anno;
					break;
				}
			}
			if (isLocked != null) {
				result.lockArgs.put(i, isLocked);
			}
		}
		return result;
	}

}
