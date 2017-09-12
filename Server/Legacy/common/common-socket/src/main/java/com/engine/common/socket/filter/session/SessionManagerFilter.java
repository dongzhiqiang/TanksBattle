package com.engine.common.socket.filter.session;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.DelayQueue;

import javax.annotation.PostConstruct;
import javax.swing.text.AbstractDocument.Content;

import org.apache.mina.core.filterchain.IoFilterAdapter;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.core.write.WriteRequest;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.BeansException;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.util.Assert;

import com.engine.common.socket.core.Attribute;
import com.engine.common.socket.core.Message;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Session;
import com.engine.common.socket.filter.session.SessionEvent.Type;
import com.engine.common.socket.handler.Handler;
import com.engine.common.utils.concurrent.DelayedElement;

/**
 * 会话管理过滤器，作为{@link SessionManager}的过滤器实现方式，用于管理的会话创建与关闭
 * 
 */
public class SessionManagerFilter extends IoFilterAdapter implements SessionManager, ApplicationContextAware {

	private static final Logger logger = LoggerFactory.getLogger(SessionManagerFilter.class);

	/** 用户身份标识键 */
	private static String IDENTITY = "identity";
	/** 身份处理完成标记键 */
	private static String PROCEED = "proceed";
	/** 忽略事件通知的标记键 */
	private static String IGNORE_EVENT = "ignore_event";

	/** 用户身份标识会话属性 */
	private static Attribute<Object> ATT_IDENTITY = new Attribute<Object>(IDENTITY);
	/** 身份处理完成标记的会话属性 */
	private static Attribute<Boolean> ATT_PROCEED = new Attribute<Boolean>(PROCEED);
	/** 忽略事件通知的会话属性 */
	private static Attribute<Boolean> ATT_IGNORE_EVENT = new Attribute<Boolean>(IGNORE_EVENT);

	// IoFilterAdapter 对应方法的实现

	@Override
	public void messageSent(NextFilter nextFilter, IoSession session, WriteRequest writeRequest) throws Exception {
		if (ATT_IDENTITY.contain(session) && !ATT_PROCEED.getValue(session, false)) {
			if (logger.isDebugEnabled()) {
				logger.debug("发现未处理的用户身份标识[{}]", SessionManagerFilter.ATT_IDENTITY.getValue(session));
			}
			onIdentified(session);
			ATT_PROCEED.setValue(session, true);
		}
		super.messageSent(nextFilter, session, writeRequest);
	}

	@Override
	public void sessionClosed(NextFilter nextFilter, IoSession session) throws Exception {
		onSessionClosed(session);
		if (logger.isDebugEnabled()) {
			if (ATT_IDENTITY.contain(session)) {
				logger.debug("用户[{}]会话[{}]关闭移除", ATT_IDENTITY.getValue(session), session.getId());
			} else {
				logger.debug("会话[{}]关闭移除", session.getId());
			}
		}
		super.sessionClosed(nextFilter, session);
	}

	@Override
	public void sessionOpened(NextFilter nextFilter, IoSession session) throws Exception {
		onSessionOpened(session);
		if (logger.isDebugEnabled()) {
			logger.debug("会话[{}]创建添加", session.getId());
		}
		super.sessionOpened(nextFilter, session);
	}

	// SessionManager 的逻辑代码

	/** 已经鉴别用户身份的会话，Key:用户身份标识，Value:{@link IoSession} */
	private Map<Object, IoSession> identities = new ConcurrentHashMap<Object, IoSession>();
	/** 匿名会话，Key:{@link IoSession#getId()}，Value:{@link IoSession} */
	private Map<Long, IoSession> anonymous = new ConcurrentHashMap<Long, IoSession>();
	/** 已经关闭的已鉴权会话，Key:用户身份标识，Value:{@link IoSession} */
	private Map<Object, IoSession> closeds = new ConcurrentHashMap<Object, IoSession>();

	/** 延迟删除队列 */
	private DelayQueue<DelayedElement<Object>> removeQueue;
	/** 延迟时间(单位:秒) */
	private int delayTimes;

	/** 延迟移除处理线程代码 */
	private class DelayRunner implements Runnable {
		@Override
		public void run() {
			while (true) {
				try {
					DelayedElement<Object> e = removeQueue.take();
					Object identity = e.getContent();
					IoSession closed = closeds.remove(identity);
					if (closed != null) {
						int cause = ATT_CAUSE.getValue(closed, SessionEventCause.NORMAL);
						fireClosedEvent(identity, cause, closed);
					}
				} catch (InterruptedException e) {
					logger.error("会话延迟移除处理线程被非法打断", e);
				}
			}
		}
	}

	/** 当前的通信控制器 */
	private Handler handler;
	/** 当前的事件监听器 */
	private Map<Type, SessionListener> listeners = new HashMap<Type, SessionListener>();

	@PostConstruct
	public void initialize() throws Exception {
		if (!isDebug()) {
			handler = this.applicationContext.getBean(handlerName, Handler.class);
			Assert.notNull(handler, "通信控制器不能为空，请通过setHandlerName(String)设置正确的通信控制器名");
		}

		if (hasDelayTimes()) {
			removeQueue = new DelayQueue<DelayedElement<Object>>();
			Thread thread = new Thread(new DelayRunner(), "会话延迟移除处理");
			thread.setDaemon(true);
			thread.start();
		}
	}

	@Override
	public void send(Request<?> request, Object... ids) {
		if (ids == null || ids.length == 0) {
			return;
		}
		// 获取对应的会话
		List<IoSession> sessions = new ArrayList<IoSession>(ids.length);
		for (Object id : ids) {
			IoSession session = identities.get(id);
			if (session == null) {
				continue;
			}
			sessions.add(session);
		}

		// 发送信息
		if (sessions.isEmpty()) {
			return;
		}
		handler.send(request, sessions.toArray(new IoSession[0]));
	}

	@Override
	public void send(Request<?> request, IoSession... sessions) {
		if (sessions == null) {
			return;
		}
		handler.send(request, sessions);
	}

	@Override
	public void sendAll(Request<?> request) {
		sendAllIdentified(request);
		sendAllAnonymous(request);
	}

	@Override
	public void sendAllIdentified(Request<?> request) {
		handler.send(request, identities.values().toArray(new IoSession[0]));
	}

	@Override
	public void sendAllAnonymous(Request<?> request) {
		handler.send(request, anonymous.values().toArray(new IoSession[0]));
	}

	@Override
	public boolean isOnline(Object... ids) {
		for (Object id : ids) {
			if (!identities.containsKey(id)) {
				return false;
			}
		}
		return true;
	}

	@Override
	public Collection<Object> getOnlineIdentities() {
		HashSet<Object> result = new HashSet<Object>();
		result.addAll(identities.keySet());
		return result;
	}

	@Override
	public void addListener(SessionListener listener) {
		synchronized (this) {
			Type type = listener.getType();
			if (listeners.containsKey(type)) {
				throw new IllegalStateException("事件类型[" + type + "]对应的监听器已经存在");
			}
			listeners.put(listener.getType(), listener);
		}
	}

	/** 被踢下线的会话属性 */
	private static final Attribute<Boolean> ATT_KICKED = new Attribute<Boolean>("kicked");
	/** 原因标识 */
	private static final Attribute<Integer> ATT_CAUSE = new Attribute<Integer>("cause");

	@Override
	public Collection<?> kick(int cause, Object... ids) {
		HashSet<Object> result = new HashSet<Object>();
		for (Object id : ids) {
			IoSession session = identities.get(id);
			if (session == null) {
				continue;
			}

			// 设置会话状态
			ATT_KICKED.setValue(session, true);
			ATT_CAUSE.setValue(session, cause);
			if (cause < 0) {
				ATT_IGNORE_EVENT.setValue(session, true);
			}
			// 先移除被踢的会话再关闭会话，这是为了避免sessionClosed被异步触发产生的未知性
			remove(session);
			session.close(false);
			// 发出会话关闭事件
			if (cause >= 0) {
				fireClosedEvent(id, cause, session);
			}
			result.add(id);
		}
		return result;
	}

	@Override
	public Collection<?> kickAll(int cause) {
		HashSet<Object> result = new HashSet<Object>();
		for (Entry<Object, IoSession> entry : identities.entrySet()) {
			Object id = entry.getKey();
			IoSession session = entry.getValue();
			// 设置会话状态
			ATT_KICKED.setValue(session, true);
			ATT_CAUSE.setValue(session, cause);
			if (cause < 0) {
				ATT_IGNORE_EVENT.setValue(session, true);
			}
			// 先移除被踢的会话再关闭会话，这是为了避免sessionClosed被异步触发产生的未知性
			remove(session);
			session.close(true);
			// 发出会话关闭事件
			if (cause >= 0) {
				fireClosedEvent(id, cause, session);
			}
			result.add(id);
		}
		return result;
	}

	@Override
	public IoSession getSession(Object id) {
		IoSession result = identities.get(id);

		if (result != null) {
			return result;
		}
		result = closeds.get(id);
		return result;
	}

	@SuppressWarnings("unchecked")
	@Override
	public void replace(IoSession src, IoSession dest) {
		if (src == null || dest == null) {
			throw new IllegalArgumentException("复制源或目标对象不能为空");
		}
		// 进行会话内容复制
		ConcurrentHashMap<String, Content> content = (ConcurrentHashMap<String, Content>) src
				.getAttribute(Session.MAIN_KEY);
		if (content == null) {
			return;
		}
		content = new ConcurrentHashMap<String, Content>(content);
		content.remove(ATT_KICKED.getKey());
		content.remove(ATT_CAUSE.getKey());
		dest.setAttribute(Session.MAIN_KEY, content);

		Object id = ATT_IDENTITY.getValue(dest);
		if (logger.isInfoEnabled()) {
			logger.info("*** SESSION[{}]替代SESSION[{}], 绑定用户身份[{}] ***", new Object[] { dest.getId(), src.getId(), id });
		}
		// 设置会话身份
		onIdentified(dest);
		// 设置已处理标记
		ATT_PROCEED.setValue(dest, true);

		// 清空被T的源属性
		src.removeAttribute(Session.MAIN_KEY);
		if (logger.isDebugEnabled()) {
			logger.debug("清空被复制的源SESSION属性...");
		}
		
		
	}

	@Override
	public void bind(IoSession session, Object id) {
		if (logger.isInfoEnabled()) {
			logger.info("*** SESSION[{}]绑定用户身份[{}] ***", session.getId(), id);
		}

		// 设置会话身份
		ATT_IDENTITY.setValue(session, id);
		onIdentified(session);
		// 设置已处理标记
		ATT_PROCEED.setValue(session, true);
	}

	@Override
	public int count(boolean includeAnonymous) {
		int result = 0;
		result += identities.size();
		if (includeAnonymous) {
			result += anonymous.size();
		}
		return result;
	}

	// 监听的方法

	/**
	 * 响应会话创建
	 * @param session 新创建的会话实例
	 */
	void onSessionOpened(IoSession session) {
		add(session);
	}

	/**
	 * 响应会话关闭
	 * @param session 被关闭的会话实例
	 */
	void onSessionClosed(IoSession session) {
		remove(session);

		Object identity = getIdentity(session);
		if (identity == null) {
			return;
		}

		if (hasDelayTimes()) {
			// 在有延时设置的情况下，将已经鉴权的会话放入延迟队列中
			Calendar calendar = Calendar.getInstance();
			calendar.add(Calendar.SECOND, delayTimes);
			DelayedElement<Object> e = new DelayedElement<Object>(identity, calendar.getTime());
			removeQueue.put(e);

			closeds.put(identity, session);
		} else {
			int cause = ATT_CAUSE.getValue(session, SessionEventCause.NORMAL);
			fireClosedEvent(identity, cause, session);
		}
	}

	/**
	 * 响应用户验证
	 * @param session
	 */
	void onIdentified(IoSession session) {
		Object identity = getIdentity(session);
		if (identity == null) {
			return;
		}

		anonymous.remove(session.getId());
		identities.put(identity, session);

		// 处理有延迟移除情况下的会话内容复制
		if (hasDelayTimes()) {
			IoSession prev = closeds.remove(identity);
			if (prev != null && prev != session) {
				// 复制会话属性
				// copyAttributes(prev, session);
				fireReplacedEvent(identity, session, prev);
				return;
			}
		}
		fireIdentifiedEvent(identity, session);
	}

	// 内部方法

	/** 会话替换事件 */
	private void fireReplacedEvent(Object identity, IoSession current, IoSession replaced) {
		SessionListener listener = listeners.get(Type.REPLACED);
		if (listener == null) {
			return;
		}
		if (!ATT_IGNORE_EVENT.getValue(current, false)) {
			listener.onEvent(new SessionReplacedEvent(SessionEventCause.ENFORCE_LOGOUT, identity, current, replaced));
		}
	}

	/** 发出完成身份验证事件 */
	private void fireIdentifiedEvent(Object identity, IoSession session) {
		SessionListener listener = listeners.get(Type.IDENTIFIED);
		if (listener == null) {
			return;
		}
		if (!ATT_IGNORE_EVENT.getValue(session, false)) {
			listener.onEvent(new SessionEvent(SessionEventCause.NORMAL, Type.IDENTIFIED, identity, session));
		}
	}

	/** 发出关闭事件 */
	private void fireClosedEvent(Object identity, int cause, IoSession session) {
		SessionListener listener = listeners.get(Type.CLOSED);
		if (listener == null) {
			return;
		}
		if (!ATT_IGNORE_EVENT.getValue(session, false)) {
			listener.onEvent(new SessionEvent(cause, Type.CLOSED, identity, session));
		}
	}

	private void add(IoSession session) {
		anonymous.put(session.getId(), session);
	}

	private void remove(IoSession session) {
		anonymous.remove(session.getId());

		Object identity = getIdentity(session);
		if (identity != null) {
			identities.remove(identity);
		}
	}

	private Object getIdentity(IoSession session) {
		return ATT_IDENTITY.getValue(session);
	}

	/**
	 * 检查是否有延迟时间设置
	 * @return
	 */
	private boolean hasDelayTimes() {
		if (delayTimes > 0) {
			return true;
		}
		return false;
	}

	// Getter and Setter ...

	/** 配置的控制器名 */
	private String handlerName;

	public void setHandlerName(String handlerName) {
		this.handlerName = handlerName;
	}

	/** 开启调试模式 */
	private boolean debug;

	public boolean isDebug() {
		return debug;
	}

	public void setDebug(boolean debug) {
		this.debug = debug;
	}

	private ApplicationContext applicationContext;

	@Override
	public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
		this.applicationContext = applicationContext;
	}

	/**
	 * 设置会话真实移除的延迟时间
	 * @param delayTimes 延迟时间(单位:秒)
	 */
	public void setDelayTimes(int delayTimes) {
		if (delayTimes < 0) {
			throw new IllegalArgumentException("延迟时间[" + delayTimes + "]不能小于等于0");
		}
		this.delayTimes = delayTimes;
	}

	public void setListeners(Map<Type, SessionListener> listeners) {
		this.listeners = listeners;
	}

	@Override
	public List<Message> getStoreMessage(long owner, int csn, IoSession session) {
		return handler.getStoreMessage(owner, csn, session);
	}

}
