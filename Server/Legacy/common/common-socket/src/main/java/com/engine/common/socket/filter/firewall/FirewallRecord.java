package com.engine.common.socket.filter.firewall;

/**
 * 防火墙信息记录
 * 
 * 
 */
public class FirewallRecord {

	/** 最大违规次数 */
	private static int MAX_VIOLATE_TIMES = 5;
	/** 每秒收到的字节数限制 */
	private static int BYTES_IN_SECOND_LIMIT = 8192;
	/** 每秒收到的数据包次数限制 */
	private static int TIMES_IN_SECOND_LIMIT = 128;

	/**
	 * 设置最大违规次数
	 * 
	 * @param times
	 */
	public static void setMaxViolateTimes(int times) {
		MAX_VIOLATE_TIMES = times;
	}

	/**
	 * 设置每秒收到的字节数限制
	 * 
	 * @param size
	 */
	public static void setBytesInSecondLimit(int size) {
		BYTES_IN_SECOND_LIMIT = size;
	}

	/**
	 * 设置每秒收到的数据包次数限制
	 * 
	 * @param size
	 */
	public static void setTimesInSecondLimit(int size) {
		TIMES_IN_SECOND_LIMIT = size;
	}

	/** 最后记录时间 */
	private long lastSecond = 0;
	/** 当前秒收到的字节数 */
	private int bytesInSecond = 0;
	/** 当前秒收到的数据包次数 */
	private int timesInSecond = 0;

	/** 违规次数 */
	private int violateTime = 0;

	/**
	 * 检查是否违规
	 * 
	 * @param bytes 当次收到的数据包大小
	 * @return true:本次违规；false:没有违规
	 */
	public boolean check(int bytes) {
		long ms = System.currentTimeMillis(); // 当前毫秒
		long currentSecond = ms / 1000; // 当前秒数

		if (currentSecond != lastSecond) {
			// 重置状态
			lastSecond = currentSecond;
			bytesInSecond = 0;
			timesInSecond = 0;
		}

		// 累计数据
		bytesInSecond += bytes;
		timesInSecond++;

		// 边界判断
		if (bytesInSecond >= BYTES_IN_SECOND_LIMIT || timesInSecond >= TIMES_IN_SECOND_LIMIT) {
			violateTime++;
			bytesInSecond = 0;
			timesInSecond = 0;
			return true;
		}
		return false;
	}

	/**
	 * 检查是否需要被阻止
	 * 
	 * @return true:要阻止；false:不需要阻止
	 */
	public boolean isBlock() {
		return violateTime >= MAX_VIOLATE_TIMES;
	}

	// Getter and Setter ...
	
	public long getLastSecond() {
		return lastSecond;
	}

	public int getBytesInSecond() {
		return bytesInSecond;
	}

	public int getTimesInSecond() {
		return timesInSecond;
	}

	public int getViolateTime() {
		return violateTime;
	}

}
