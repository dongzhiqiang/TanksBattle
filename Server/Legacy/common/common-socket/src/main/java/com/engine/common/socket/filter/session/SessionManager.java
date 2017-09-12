package com.engine.common.socket.filter.session;

import java.util.Collection;
import java.util.List;

import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Message;
import com.engine.common.socket.core.Request;

/**
 * 会话管理器接口
 * 
 */
public interface SessionManager {

	/** 用户身份标识键 */
	String IDENTITY = "identity";

	/**
	 * 向指定目标发送请求
	 * @param request
	 * @param ids
	 */
	void send(Request<?> request, Object... ids);

	/**
	 * 向指定目标发送请求
	 * @param request
	 * @param sessions
	 */
	void send(Request<?> request, IoSession... sessions);

	/**
	 * 向所有会话发送请求
	 * @param request
	 */
	void sendAll(Request<?> request);

	/**
	 * 向所有已经验证身份的会话发送请求
	 * @param request
	 */
	void sendAllIdentified(Request<?> request);

	/**
	 * 向所有匿名会话发送请求
	 * @param request
	 */
	void sendAllAnonymous(Request<?> request);

	/**
	 * 检查指定用户是否在线
	 * @param ids 用户标识
	 * @return
	 */
	boolean isOnline(Object... ids);

	/**
	 * 添加会话监听器，每一种事件类型的监听器只能添加一个
	 * @param listener 监听器实例
	 * @throws IllegalStateException 重复添加同一类型的监听器时抛出
	 */
	void addListener(SessionListener listener);

	/**
	 * 踢指定的用户下线
	 * @param cause 原因标识{@link SessionEventCause}
	 * @param ids 被踢下线的用户标识
	 */
	Collection<?> kick(int cause, Object... ids);

	/**
	 * 将全部的在线用户踢下线
	 * @param cause 原因标识{@link SessionEventCause}
	 */
	Collection<?> kickAll(int cause);

	/**
	 * 获取全部的在线用户标识
	 * @return
	 */
	Collection<?> getOnlineIdentities();

	/**
	 * 获取指定用户标识的会话对象
	 * @param id 用户标识
	 * @return 会返回null，被延时关闭的会话也会返回
	 */
	IoSession getSession(Object id);

	/**
	 * 替换会话
	 * @param src 来源对象
	 * @param dest 复制目的地
	 */
	void replace(IoSession src, IoSession dest);

	/**
	 * 设置会话身份
	 * @param session 来源对象
	 * @param id 玩家标识
	 */
	void bind(IoSession session, Object id);

	/**
	 * 统计当前的会话数量
	 * @param includeAnonymous 是否包含匿名会话
	 * @return
	 */
	int count(boolean includeAnonymous);
	
	/**
	 * 获取存储的消息
	 * @param owner
	 * @param csn
	 * @param session
	 * @return
	 */
	List<Message> getStoreMessage(long owner, int csn, IoSession session);
}
