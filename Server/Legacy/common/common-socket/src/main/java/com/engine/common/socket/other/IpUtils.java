package com.engine.common.socket.other;

import java.util.regex.Pattern;

import org.apache.commons.lang3.StringUtils;
import org.apache.mina.core.session.IoSession;

/**
 * 获取IP地址的简单工具类
 * 
 */
public abstract class IpUtils {

	/**
	 * 获取会话的IP地址
	 * @param session
	 * @return
	 */
	public static String getIp(IoSession session) {
		if (session == null || session.getRemoteAddress() == null) {
			return "UNKNOWN";
		}
		String ip = session.getRemoteAddress().toString();
		return StringUtils.substringBetween(ip, "/", ":");
	}

	/***ip地址正则配置开始**/
	private final static String ipPatternConfig = "[0-9\\*]+\\.";
	/***ip地址正则配置结束**/
	private final static String ipPatternEndConfig = "[0-9\\*]+";

	public final static Pattern ipPattern = Pattern.compile(ipPatternConfig + ipPatternConfig + ipPatternConfig
			+ ipPatternEndConfig);

	public static boolean isValidIp(String ip){
		return ipPattern.matcher(ip).matches();
	}
//	public static void main(String[] args) {
//		String ip = "192.12.10.*";
//		System.out.println(ipPattern.matcher(ip).matches());
//	}
}
