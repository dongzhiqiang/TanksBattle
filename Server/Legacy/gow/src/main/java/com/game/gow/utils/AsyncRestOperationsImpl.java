package com.game.gow.utils;

import java.net.URI;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.ThreadFactory;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;

import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.http.client.ClientHttpRequestFactory;
import org.springframework.web.client.RequestCallback;
import org.springframework.web.client.ResponseExtractor;
import org.springframework.web.client.RestClientException;
import org.springframework.web.client.RestTemplate;

public class AsyncRestOperationsImpl extends RestTemplate implements AsyncRestOperations {
	private ThreadPoolExecutor threadPool;

	// 构造方法

	public AsyncRestOperationsImpl(int coreSize, int maxSize, long liveTime, TimeUnit unit,
			ClientHttpRequestFactory requestFactory) {
		// 调用父类构造
		super(requestFactory);
		// 初始化线程池
		LinkedBlockingQueue<Runnable> workQueue = new LinkedBlockingQueue<Runnable>();
		ThreadFactory threadFactory = new ThreadFactory() {
			AtomicInteger sn = new AtomicInteger();

			public Thread newThread(Runnable r) {
				int next = sn.getAndIncrement();
				return new Thread(r, "AsyncRestClient:" + next);
			}
		};
		this.threadPool = new ThreadPoolExecutor(coreSize, maxSize, liveTime, unit, workQueue, threadFactory);
	}

	@Override
	public <T> void getForObject(final AsyncCallback<T> callback, final String url, final Class<T> responseType,
			final Object... uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = getForObject(url, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void getForObject(final AsyncCallback<T> callback, final String url, final Class<T> responseType,
			final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = getForObject(url, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});

	}

	@Override
	public <T> void getForObject(final AsyncCallback<T> callback, final URI url, final Class<T> responseType)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = getForObject(url, responseType);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void getForEntity(final AsyncCallback<ResponseEntity<T>> callback, final String url,
			final Class<T> responseType, final Object... uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = getForEntity(url, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void getForEntity(final AsyncCallback<ResponseEntity<T>> callback, final String url,
			final Class<T> responseType, final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = getForEntity(url, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void getForEntity(final AsyncCallback<ResponseEntity<T>> callback, final URI url, final Class<T> responseType)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = getForEntity(url, responseType);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void headForHeaders(final AsyncCallback<HttpHeaders> callback, final String url, final Object... uriVariables)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					HttpHeaders result = headForHeaders(url, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void headForHeaders(final AsyncCallback<HttpHeaders> callback, final String url, final Map<String, ?> uriVariables)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					HttpHeaders result = headForHeaders(url, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void headForHeaders(final AsyncCallback<HttpHeaders> callback, final URI url) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					HttpHeaders result = headForHeaders(url);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void postForLocation(final AsyncCallback<URI> callback, final String url, final Object request,
			final Object... uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					URI result = postForLocation(url, request, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void postForLocation(final AsyncCallback<URI> callback, final String url, final Object request,
			final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					URI result = postForLocation(url, request, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void postForLocation(final AsyncCallback<URI> callback, final URI url, final Object request)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					URI result = postForLocation(url, request);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void postForObject(final AsyncCallback<T> callback, final String url, final Object request,
			final Class<T> responseType, final Object... uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = postForObject(url, request, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void postForObject(final AsyncCallback<T> callback, final String url, final Object request,
			final Class<T> responseType, final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = postForObject(url, request, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void postForObject(final AsyncCallback<T> callback, final URI url, final Object request,
			final Class<T> responseType) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = postForObject(url, request, responseType);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void postForEntity(final AsyncCallback<ResponseEntity<T>> callback, final String url, final Object request,
			final Class<T> responseType, final Object... uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = postForEntity(url, request, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void postForEntity(final AsyncCallback<ResponseEntity<T>> callback, final String url, final Object request,
			final Class<T> responseType, final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = postForEntity(url, request, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void postForEntity(final AsyncCallback<ResponseEntity<T>> callback, final URI url, final Object request,
			final Class<T> responseType) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = postForEntity(url, request, responseType);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void put(final AsyncCallback<Void> callback, final String url, final Object request, final Object... uriVariables)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					put(url, request, uriVariables);
					callback.onSuccess(null);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void put(final AsyncCallback<Void> callback, final String url, final Object request,
			final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					put(url, request, uriVariables);
					callback.onSuccess(null);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void put(final AsyncCallback<Void> callback, final URI url, final Object request) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					put(url, request);
					callback.onSuccess(null);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void delete(final AsyncCallback<Void> callback, final String url, final Object... uriVariables)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					delete(url, uriVariables);
					callback.onSuccess(null);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void delete(final AsyncCallback<Void> callback, final String url, final Map<String, ?> uriVariables)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					delete(url, uriVariables);
					callback.onSuccess(null);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void delete(final AsyncCallback<Void> callback, final URI url) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					delete(url);
					callback.onSuccess(null);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void optionsForAllow(final AsyncCallback<Set<HttpMethod>> callback, final String url,
			final Object... uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					Set<HttpMethod> result = optionsForAllow(url, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void optionsForAllow(final AsyncCallback<Set<HttpMethod>> callback, final String url,
			final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					Set<HttpMethod> result = optionsForAllow(url, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public void optionsForAllow(final AsyncCallback<Set<HttpMethod>> callback, final URI url) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					Set<HttpMethod> result = optionsForAllow(url);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void exchange(final AsyncCallback<ResponseEntity<T>> callback, final String url, final HttpMethod method,
			final HttpEntity<?> requestEntity, final Class<T> responseType, final Object... uriVariables)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = exchange(url, method, requestEntity, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void exchange(final AsyncCallback<ResponseEntity<T>> callback, final String url, final HttpMethod method,
			final HttpEntity<?> requestEntity, final Class<T> responseType, final Map<String, ?> uriVariables)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = exchange(url, method, requestEntity, responseType, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void exchange(final AsyncCallback<ResponseEntity<T>> callback, final URI url, final HttpMethod method,
			final HttpEntity<?> requestEntity, final Class<T> responseType) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					ResponseEntity<T> result = exchange(url, method, requestEntity, responseType);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void execute(final AsyncCallback<T> callback, final String url, final HttpMethod method,
			final RequestCallback requestCallback, final ResponseExtractor<T> responseExtractor,
			final Object... uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = execute(url, method, requestCallback, responseExtractor, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void execute(final AsyncCallback<T> callback, final String url, final HttpMethod method,
			final RequestCallback requestCallback, final ResponseExtractor<T> responseExtractor,
			final Map<String, ?> uriVariables) throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = execute(url, method, requestCallback, responseExtractor, uriVariables);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}

	@Override
	public <T> void execute(final AsyncCallback<T> callback, final URI url, final HttpMethod method,
			final RequestCallback requestCallback, final ResponseExtractor<T> responseExtractor)
			throws RestClientException {
		threadPool.execute(new Runnable() {
			public void run() {
				try {
					T result = execute(url, method, requestCallback, responseExtractor);
					callback.onSuccess(result);
				} catch (Exception ex) {
					callback.onError(ex);
				}
			}
		});
	}
}
