package com.game.gow.utils;

import java.util.concurrent.TimeUnit;

import org.springframework.http.client.ClientHttpRequestFactory;
import org.springframework.http.client.SimpleClientHttpRequestFactory;

public class AsyncRestOperationsFactory {

	private static int corePoolSize = 4;
	private static int maximumPoolSize = 6;
	private static long keepAliveTime = 5L;
	private static TimeUnit unit = TimeUnit.SECONDS;

	/**
	 * 创建一个异步的RestOperations，采用内部默认的线程池配置
	 * @return
	 */
	public static AsyncRestOperations createAsyncRestOperations() {
		ClientHttpRequestFactory requestFactory = new SimpleClientHttpRequestFactory();
		return createAsyncRestOperations(corePoolSize, maximumPoolSize, keepAliveTime, unit, requestFactory);
	}

	/**
	 * 创建一个异步的RestOperations，采用内部默认的线程池配置
	 * @param requestFactory 指定的request工厂类(同org.springframework.http.HttpMethod. ClientHttpRequestFactory)
	 * @return
	 */
	public static AsyncRestOperations createAsyncRestOperations(ClientHttpRequestFactory requestFactory) {
		return createAsyncRestOperations(corePoolSize, maximumPoolSize, keepAliveTime, unit, requestFactory);
	}

	/**
	 * 创建一个异步的RestOperations
	 * @param corePoolSize 池中所保存的线程数，包括空闲线程
	 * @param maximumPoolSize 池中允许的最大线程数
	 * @param keepAliveTime 当线程数大于核心时，此为终止前多余的空闲线程等待新任务的最长时间
	 * @param unit 参数的时间单位
	 * @return
	 */
	public static AsyncRestOperations createAsyncRestOperations(int corePoolSize, int maximumPoolSize,
			long keepAliveTime, TimeUnit unit) {
		ClientHttpRequestFactory requestFactory = new SimpleClientHttpRequestFactory();
		return createAsyncRestOperations(corePoolSize, maximumPoolSize, keepAliveTime, unit, requestFactory);
	}

	/**
	 * 创建一个异步的RestOperations
	 * @param corePoolSize 池中所保存的线程数，包括空闲线程
	 * @param maximumPoolSize 池中允许的最大线程数
	 * @param keepAliveTime 当线程数大于核心时，此为终止前多余的空闲线程等待新任务的最长时间
	 * @param unit 参数的时间单位
	 * @param requestFactory y 指定的request工厂类(同org.springframework.http.HttpMethod. ClientHttpRequestFactory)
	 * @return
	 */
	public static AsyncRestOperations createAsyncRestOperations(int corePoolSize, int maximumPoolSize,
			long keepAliveTime, TimeUnit unit, ClientHttpRequestFactory requestFactory) {
		return new AsyncRestOperationsImpl(corePoolSize, maximumPoolSize, keepAliveTime, unit, requestFactory);
	}
}
