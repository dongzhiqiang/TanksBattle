package com.engine.common.socket.filter.firewall;

import java.util.Calendar;
import java.util.Collection;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.DelayQueue;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.apache.commons.lang3.StringUtils;
import org.apache.mina.core.buffer.IoBuffer;
import org.apache.mina.core.filterchain.IoFilterAdapter;
import org.apache.mina.core.session.IoSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.socket.core.Attribute;
import com.engine.common.socket.other.IpUtils;
import com.engine.common.utils.concurrent.DelayedElement;

/**
 * 防火墙过滤器
 * 
 * 
 */
public class FirewallFilter extends IoFilterAdapter implements FirewallManager {
	
	private static final Logger logger = LoggerFactory.getLogger(FirewallFilter.class);

	/** 分隔符 */
	public static final String SPLIT = ",";
	/** 白名单标识属性 */
	public static final String ALLOW = "firewall:allow";
	private static final Attribute<Boolean> ATT_ALLOW = new Attribute<Boolean>(ALLOW);
	/** 访问记录属性 */
	public static final String RECORD = "firewall:record";
	private static final Attribute<FirewallRecord> ATT_RECORD = new Attribute<FirewallRecord>(RECORD);

	/** 白名单IP集合 */
	private ConcurrentHashMap<String, Pattern> allows = new ConcurrentHashMap<String, Pattern>();
	/** 黑名单IP集合 */
	private ConcurrentHashMap<String, Pattern> blocks = new ConcurrentHashMap<String, Pattern>();
	/** 黑名单阻止时间(默认:10分钟) */
	private int blockTimes = 10 * 60;
	/** 黑名单移除队列 */
	private DelayQueue<DelayedElement<String>> blockRemoveQueue = new DelayQueue<DelayedElement<String>>();

	/** 最大客户端连接数 */
	private int maxClients = 5000;
	/** 当前客户端连接数 */
	private AtomicInteger currentClients = new AtomicInteger();
	
	/** 阻止全部连接状态(不包括白名单) */
	private boolean blockAll = false;
	
	// 对外方法

	@Override
	public Collection<String> getBlockList() {
		return blocks.keySet();
	}

	@Override
	public Collection<String> getAllowList() {
		return allows.keySet();
	}

	@Override
	public void block(String ip) {
		if (blocks.containsKey(ip)) {
			return;
		}
		Pattern pattern = ipToPattern(ip);
		blocks.put(ip, pattern);
		// 加延迟队列
		Calendar calendar = Calendar.getInstance();
		calendar.add(Calendar.SECOND, blockTimes);
		DelayedElement<String> e = DelayedElement.valueOf(ip, calendar.getTime());
		blockRemoveQueue.add(e);
	}

	@Override
	public void unblock(String ip) {
		blocks.remove(ip);
	}

	@Override
	public String blockByIoSession(IoSession session) {
		String ip = IpUtils.getIp(session);
		block(ip);
		return ip;
	}

	@Override
	public void blockAll() {
		blockAll = true;
	}

	@Override
	public void unblockAll() {
		blockAll = false;
	}

	@Override
	public void allow(String ip) {
		if (allows.containsKey(ip)) {
			return;
		}
		Pattern pattern = ipToPattern(ip);
		allows.put(ip, pattern);
	}

	@Override
	public void disallow(String ip) {
		allows.remove(ip);
	}

	@Override
	public int getCurrentConnections() {
		return currentClients.get();
	}

	// 会话管理方法
	
	@Override
	public void sessionOpened(NextFilter nextFilter, IoSession session) throws Exception {
		// 增加当前的在线客户端数量
		int clients = currentClients.getAndIncrement();
		
		// 检查是否白名单
		String ip = IpUtils.getIp(session);
		for (Pattern pattern : allows.values()) {
			Matcher matcher = pattern.matcher(ip);
			if (matcher.matches()) {
				if (logger.isDebugEnabled()) {
					logger.debug("白名单用户[{}]登录服务器", ip);
				}
				ATT_ALLOW.setValue(session, true);
				super.sessionOpened(nextFilter, session);
				return;	// 白名单不进行后续判断,直接跳过
			}
		}
		
		// 是否禁止全部连接
		if (blockAll) {
			if (logger.isDebugEnabled()) {
				logger.debug("由于阻止全部连接状态，阻止用户[{}]登录服务器", ip);
			}
			session.close(true);
			return;
		}
		
		// 最大连接数判断
		if (clients >= maxClients) {
			if (logger.isWarnEnabled()) {
				logger.warn("到达最大连接数[{}/{}]，非白名单连接将会被拒绝", clients, maxClients);
			}
			// 到达最大连接数，拒绝连接
			session.close(true);
			return;
		}

		// 过期黑名单清理
		for (;;) {
			DelayedElement<String> e = blockRemoveQueue.poll();
			if (e == null) {
				break;
			}
			blocks.remove(e.getContent());
		}
		
		// 检查是否黑名单
		for (Pattern pattern : blocks.values()) {
			Matcher matcher = pattern.matcher(ip);
			if (matcher.matches()) {
				// 是黑名单内的IP，拒绝会话打开
				if (logger.isDebugEnabled()) {
					logger.debug("黑名单用户[{}]登录服务器被拒绝", ip);
				}
				session.close(true);
				return;
			}
		}
		
		super.sessionOpened(nextFilter, session);
	}
	
	@Override
	public void sessionClosed(NextFilter nextFilter, IoSession session) throws Exception {
		// 减少当前的在线客户端数量
		currentClients.decrementAndGet();
		super.sessionClosed(nextFilter, session);
	}
	
	@Override
	public void messageReceived(NextFilter nextFilter, IoSession session, Object message) throws Exception {
		if (isAllow(session)) {
			super.messageReceived(nextFilter, session, message);
			return;
		}

		if (check(session, message)) {
			blockByIoSession(session);
			session.close(true);
			return;
		}
		super.messageReceived(nextFilter, session, message);
	}
	
	/**
	 * 检查是否需要阻止该会话
	 * 
	 * @param session 会话
	 * @param message 信息体
	 * @return true:需要;false:不需要
	 */
	private boolean check(IoSession session, Object message) {
		if (!(message instanceof IoBuffer)) {
			if (logger.isDebugEnabled()) {
				logger.debug("接收数据类型[{}]不是 IoBuffer", message.getClass().getName());
			}
			return false;
		}
		
		// 获取记录
		FirewallRecord record = null;
		if (ATT_RECORD.contain(session)) {
			record = ATT_RECORD.getValue(session);
		} else {
			// 记录不存在，创建记录对象
			record = new FirewallRecord();
			ATT_RECORD.setValue(session, record);
		}

		// 检查本次访问
		int bytes = ((IoBuffer) message).remaining();
		if (!record.check(bytes)) {
			return false;
		}
		if (logger.isDebugEnabled()) {
			String ip = IpUtils.getIp(session);
 			logger.error("会话[{}]发生违规，违规状态[总违规次数:{} 长度:{} 次数:{}]",
					new Object[] { ip, record.getViolateTime(), record.getBytesInSecond(), record.getTimesInSecond() });
		}
		
		// 检查是否超出许可
		if (record.isBlock()) {
			return true;
		}
		return false;
	}

	/**
	 * 检查是否是白名单
	 * 
	 * @param session
	 * @return
	 */
	private boolean isAllow(IoSession session) {
		return ATT_ALLOW.getValue(session, false);
	}

	/**
	 * 将IP地址转换为正则表示形式
	 * @param ip
	 * @return
	 */
	private Pattern ipToPattern(String ip) {
		String reg = ip.replace(".", "[.]").replace("*", "[0-9]*");
		Pattern pattern = Pattern.compile(reg);
		return pattern;
	}

	// Getter and Setter ...
	
	/**
	 * 设置阻止全部连接状态
	 * @param state 状态
	 */
	public void setBlockAllState(boolean state) {
		blockAll = state;
	}

	/**
	 * 设置白名单IP集合
	 * 
	 * @param allows 白名单集合
	 */
	public void setAllows(String allows) {
		if (StringUtils.isBlank(allows)) {
			return;
		}
		String[] ips = allows.split(SPLIT);
		for (String ip : ips) {
			this.allows.put(ip, ipToPattern(ip));
		}
	}

	/**
	 * 设置永久黑名单
	 * @param blocks 黑名单集合
	 */
	public void setBlocks(String blocks) {
		if (StringUtils.isBlank(blocks)) {
			return;
		}
		String[] ips = blocks.split(SPLIT);
		for (String ip : ips) {
			this.blocks.put(ip, ipToPattern(ip));
		}
	}
	
	/**
	 * 设置永久黑名单
	 * @param block 黑名单ip
	 */
	public void setBlock(String ip) {
		this.blocks.put(ip, ipToPattern(ip));
	}
	 

	/**
	 * 设置阻止延时(单位秒)
	 * @param blockTimes
	 */
	public void setBlockTimes(int blockTimes) {
		this.blockTimes = blockTimes;
	}

	/**
	 * 设置最大连接数
	 * @param maxClients
	 */
	public void setMaxClients(int maxClients) {
		this.maxClients = maxClients;
	}

	/**
	 * 设置最大违规次数
	 * 
	 * @param times
	 */
	public void setMaxViolateTimes(int times) {
		FirewallRecord.setMaxViolateTimes(times);
	}

	/**
	 * 设置每秒收到的字节数限制
	 * 
	 * @param size
	 */
	public void setBytesInSecondLimit(int size) {
		FirewallRecord.setBytesInSecondLimit(size);
	}

	/**
	 * 设置每秒收到的数据包次数限制
	 * 
	 * @param size
	 */
	public void setTimesInSecondLimit(int size) {
		FirewallRecord.setTimesInSecondLimit(size);
	}

	@Override
	public boolean isblockAll() {
		return this.blockAll;
	}
}
