package com.engine.common.utils.http;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeoutException;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.HttpClient;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.params.HttpConnectionParams;
import org.apache.http.util.EntityUtils;

/**
 * HttpClient
 * TODO 该对象需要改造，要支持不同实例有不同的配置，要支持REST。可基于Spring RestTemplate对象进行改造
 *
 */
public final class HttpClientRequest {

	/** 字符集 */
	private final static String ENCODE = "UTF-8";
	/** 请求超时设置（毫秒） */
	private final static int TIMEOUT = 3000;
	/** 线程池大小 */
	private final static int THREAD_SIZE = 3;
	/** 响应缓存初始化 */
	private final static String INIT_VALUE = "HTTP_CLIENT_DEFAULT_KEY";

	private final static Executor EXECUTOR = Executors.newFixedThreadPool(THREAD_SIZE);

	/** 最大连接数 */
	private static final Map<String, Semaphore> MAX_CONN_SIZE = new ConcurrentHashMap<String, Semaphore>();
	/** 异步响应集合 */
	private static final Map<String, String> RESPONSE_CACHE = new ConcurrentHashMap<String, String>();
	
	private HttpClientRequest() {
	}
	
	// Static Method's ...

	public static String request(final String request, final List<NameValuePair> pairs) throws InterruptedException {
		// 获取信号量
		Semaphore semaphore = MAX_CONN_SIZE.get(request);
		if (semaphore != null) {
			semaphore.acquire();
		}
		// 取得response Key
		final String key = getKey();
		try {
			EXECUTOR.execute(new Runnable() {
				@Override
				public void run() {
					RESPONSE_CACHE.put(key, INIT_VALUE);
					execute(request, pairs, key);
				}
			});
		} finally {
			if (semaphore != null) {
				semaphore.release();
			}
		}
		return key;
	}

	private static void execute(String request, List<NameValuePair> pairs, String key) {
		HttpClient client = null;
		try {
			client = new DefaultHttpClient();
			// 设置连接超时
			HttpConnectionParams.setConnectionTimeout(client.getParams(), TIMEOUT);
			HttpConnectionParams.setSoTimeout(client.getParams(), TIMEOUT);
			HttpPost httpPost = new HttpPost(request);

			// 检测参数
			for (NameValuePair pair : pairs) {
				if (pair.getName() == null || pair.getName().equals("")) {
					throw new IllegalArgumentException("参数名称不能为空！");
				}
				if (pair.getValue() == null) {
					throw new NullPointerException("参数[" + pair.getName() + "]值不能为空！");
				}
			}
			// 请求
			UrlEncodedFormEntity formEntity = new UrlEncodedFormEntity(pairs, ENCODE);
			httpPost.setEntity(formEntity);

			// 响应
			HttpResponse response = client.execute(httpPost);
			HttpEntity entity = response.getEntity();
			if (RESPONSE_CACHE.get(key) != null) {
				RESPONSE_CACHE.put(key, EntityUtils.toString(entity, ENCODE));
			}
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		} catch (ClientProtocolException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		} finally {
			client.getConnectionManager().shutdown();
		}
	}

	public static String getResponse(String key) throws TimeoutException {
		long start = System.currentTimeMillis();
		while (System.currentTimeMillis() - start < TIMEOUT) {
			if (RESPONSE_CACHE.get(key) != null && !RESPONSE_CACHE.get(key).equals(INIT_VALUE)) {
				String response = RESPONSE_CACHE.get(key);
				RESPONSE_CACHE.remove(key);
				return response;
			}
		}
		throw new TimeoutException("服务器响应超时");
	}

	public static synchronized void setMaxHttpClinetSize(String request, int size) {
		Semaphore semaphore = MAX_CONN_SIZE.get(request.trim());
		if (semaphore == null) {
			MAX_CONN_SIZE.put(request, new Semaphore(size, true));
		}
	}

	private static String getKey() {
		UUID uuid = UUID.randomUUID();
		return uuid.toString();
	}
}
