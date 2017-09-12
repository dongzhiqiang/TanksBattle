package com.engine.common.protocol.utils;

import java.nio.charset.Charset;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.ThreadFactory;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;
import java.util.concurrent.atomic.AtomicInteger;

public class QuickLZUtils {
	/** 默认压缩级别 */
	private static final int DEFAULT_LEVEL = 1;
	
	/**
	 * 将指定字符串内容压缩为byte[]
	 * @param src 字符串内容
	 * @return
	 */
	public static byte[] zip(String src) {
		byte[] bytes = src.getBytes(Charset.forName("UTF8"));
		return zip(bytes);
	}
	
	/**
	 * 将输入的byte[]进行压缩
	 * @param src
	 * @return
	 */
	public static byte[] zip(byte[] src) {
		return zip(src, DEFAULT_LEVEL);
	}
	
	/** 解压任务线程池 */
	private static ExecutorService executorService = Executors.newFixedThreadPool(
			Math.max(1, Runtime.getRuntime().availableProcessors() - 1), 
			new ThreadFactory() {
				ThreadGroup group = new ThreadGroup("QuickLZ解压");
				String prefix = "解压线程 - ";
				AtomicInteger sn = new AtomicInteger();
				public Thread newThread(Runnable r) {
					return new Thread(group, r, prefix + sn.incrementAndGet());
				}
			});

	/**
	 * 将输入的byte[]进行解压
	 * @param src
	 * @return
	 */
	public static byte[] unzip(final byte[] src, long timeout, TimeUnit unit) {
		Future<byte[]> future = executorService.submit(new Callable<byte[]>() {
			public byte[] call() throws Exception {
				return QuickLZ.decompress(src);
			}
		});
		
		try {
			return future.get(timeout, unit);
		} catch (InterruptedException e) {
			throw new IllegalStateException("解压时被打断:" + e.getMessage());
		} catch (ExecutionException e) {
			throw new IllegalStateException("解压时发生错误:" + e.getMessage());
		} catch (TimeoutException e) {
			throw new IllegalStateException("解压处理超时");
		}
	}
	
	public static byte[] zip(byte[] src, int level) {
		return QuickLZ.compress(src, level);
	}

}
