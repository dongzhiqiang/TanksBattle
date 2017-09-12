package com.engine.common.ramcache.aop;

import java.lang.reflect.Method;
import java.util.concurrent.ConcurrentHashMap;

import org.aspectj.lang.ProceedingJoinPoint;
import org.aspectj.lang.Signature;
import org.aspectj.lang.annotation.Around;
import org.aspectj.lang.annotation.Aspect;
import org.aspectj.lang.reflect.MethodSignature;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.lock.ChainLock;

/**
 * 自动锁定方法切面
 * 
 */
@Aspect
public class LockAspect {
	
	private static final Logger logger = LoggerFactory.getLogger(LockAspect.class);

	/** 方法和锁提取器的映射表 */
	private ConcurrentHashMap<Method, LockExtractor> extractors = new ConcurrentHashMap<Method, LockExtractor>();
	/** 线程标记对象 */
	private ThreadLocal<Object> threadLocal = new ThreadLocal<Object>();
	
	/** 锁方法拦截处理*/
	@Around("@annotation(autoLocked)")
	public Object execute(ProceedingJoinPoint pjp, AutoLocked autoLocked) throws Throwable {
		Signature sign = pjp.getSignature();
		if (!(sign instanceof MethodSignature)) {
			logger.error("不支持的拦截切面:{}", sign);
			return pjp.proceed(pjp.getArgs());
		}

		// 强制加锁的处理
		if (autoLocked.value()) {
			return lockAndExecute(pjp, sign);
		}
		
		// 非强制加锁的处理
		if (threadLocal.get() == null) {
			threadLocal.set(new Object());
			try {
				Object result = lockAndExecute(pjp, sign);
				return result;
			} finally {
				threadLocal.remove();
			}
		} else {
			return pjp.proceed(pjp.getArgs());
		}
	}

	/** 加锁并执行 */
	private Object lockAndExecute(ProceedingJoinPoint pjp, Signature sign) throws Throwable {
		// 获取锁提取器
		Method method = ((MethodSignature) sign).getMethod();
		LockExtractor extractor = extractors.get(method);
		if (extractor == null) {
			extractor = createLockExtractor(method);
		}
		
		// 执行拦截方法
		Object[] args = pjp.getArgs();
		ChainLock lock = extractor.getLock(args);
		if (lock == null) {
			return pjp.proceed(args);
		}
		
		lock.lock();
		try {
			return pjp.proceed(args);
		} finally {
			lock.unlock();
		}
	}

	/** 创建锁提取器 */
	private LockExtractor createLockExtractor(Method method) {
		LockExtractor result = LockExtractor.valueOf(method);
		LockExtractor prev = extractors.putIfAbsent(method, result);
		return prev == null ? result : prev;
	}

}
