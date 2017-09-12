package com.engine.common.socket.filter;

import java.util.LinkedHashMap;
import java.util.Map.Entry;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantReadWriteLock;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.apache.mina.core.filterchain.IoFilterAdapter;
import org.apache.mina.core.session.IoSession;

import com.engine.common.socket.core.Attribute;
import com.engine.common.socket.other.IpUtils;

/**
 * 管理后台过滤器，用于为特定IP的访问设置管理后台标记
 * 
 * 
 */
public class ManagementFilter extends IoFilterAdapter {

	/** 管理后台标识属性 */
	public static final String MANAGEMENT = "management";

	/** 管理后台标识属性 */
	private static final Attribute<String> ATT_MANAGEMENT = new Attribute<String>(MANAGEMENT);

	@Override
	public void sessionOpened(NextFilter nextFilter, IoSession session) throws Exception {
		Lock readLock = lock.readLock();
		try {
			readLock.lock();

			String ip = IpUtils.getIp(session);
			for (Entry<Pattern, String> entry : allowIps.entrySet()) {
				Matcher matcher = entry.getKey().matcher(ip);
				if (matcher.matches()) {
					ATT_MANAGEMENT.setValue(session, entry.getValue());
					break;
				}
			}
		} finally {
			readLock.unlock();
		}
		super.sessionOpened(nextFilter, session);
	}

	private ReentrantReadWriteLock lock = new ReentrantReadWriteLock();
	private LinkedHashMap<Pattern, String> allowIps = new LinkedHashMap<Pattern, String>();

	/**
	 * 设置管理后台许可IP与对应的管理后台名称
	 * 
	 * @param allowIps key:许可IP的正则 value:名称
	 */
	public void setAllowIps(LinkedHashMap<String, String> config) {
		Lock writeLock = lock.writeLock();
		try {
			writeLock.lock();

			allowIps.clear();
			for (Entry<String, String> entry : config.entrySet()) {
				String ip = entry.getKey();
				String reg = ip.replace(".", "[.]").replace("*", "[0-9]*");
				Pattern pattern = Pattern.compile(reg);
				allowIps.put(pattern, entry.getValue());
			}
		} finally {
			writeLock.unlock();
		}
	}
	
	/**
	 * 设置管理后台许可IP与对应的管理后台名称
	 * @param config 内容条目间用","分隔，IP和管理后台名称之间用"="分隔。范例格式:[IP]=[NAME],...
	 */
	public void setAllowIpConfig(String config) {
		String[] ips = config.split(",");
		LinkedHashMap<String, String> result = new LinkedHashMap<String, String>(ips.length);
		for (String ip : ips) {
			String[] s = ip.split("=", 2);
			result.put(s[0], s[1]);
		}
		setAllowIps(result);
	}

	/**
	 * 添加许可IP
	 * @param ip 许可的IP
	 * @param name 许可名
	 */
	public void addAllowIp(String ip, String name) {
		Lock writeLock = lock.writeLock();
		writeLock.lock();
		try {
			String reg = ip.replace(".", "[.]").replace("*", "[0-9]*");
			Pattern pattern = Pattern.compile(reg);
			allowIps.put(pattern, name);
		} finally {
			writeLock.unlock();
		}
	}
}
