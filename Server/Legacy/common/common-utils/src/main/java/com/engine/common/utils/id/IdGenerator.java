package com.engine.common.utils.id;

import java.util.concurrent.atomic.AtomicLong;

/**
 * 主键生成器<br>
 * [保留位:4][服务器位:3][主键自增位:9]
 * 
 */
public class IdGenerator {

	/** 服务器标识 */
	private final short server;
	/** 当前自增值 */
	private final AtomicLong current;
	/** 溢出边界 */
	private final long limit;

	/**
	 * 构造主键生成器
	 * @param server 服务器标识
	 * @param current 当前的主键值
	 */
	public IdGenerator(short server, Long current) {
		if (!vaildValue(server, 12)) {
			throw new IllegalArgumentException("服务器标识[" + server + "]超过了12位二进制数的表示范围");
		}
		this.server = server;

		final long[] limits = getLimits(server);
		if (current != null) {
			if (current < limits[0] || current > limits[1]) {
				throw new IllegalArgumentException("当前主键值[" + current + "],不符合服务器标识[" + server + "]的要求");
			}
			this.current = new AtomicLong(current);
		} else {
			this.current = new AtomicLong(limits[0]);
		}
		this.limit = limits[1];
	}

	/**
	 * 获取当前的主键值
	 * @return
	 */
	public long getCurrent() {
		return current.get();
	}

	/**
	 * 获取下一个主键值
	 * @return
	 * @throws IllegalStateException 到达边界值时会抛出该异常
	 */
	public long getNext() {
		long result = current.incrementAndGet();
		if (result > limit) {
			throw new IllegalStateException("主键值[" + result + "]已经超出了边界[" + limit + "]");
		}
		return result;
	}

	// Getter and Setter ...

	public short getServer() {
		return server;
	}

	public long getLimit() {
		return limit;
	}

	// Static Method's ...

	/**
	 * 获取主键中的服标识
	 * @param id 主键值
	 * @return
	 */
	public static short toServer(long id) {
		if ((0xFFFF000000000000L & id) != 0) {
			throw new IllegalArgumentException("无效的ID标识值:" + id);
		}
		// 将高位置0(保留位+运营商位+服务器位)
		return (short) ((id >> 36) & 0x0000000000000FFFL);
	}

	/**
	 * 检查值是否超过 byte 的表示范围
	 * @param value 被检查的值
	 * @return true:合法,false:非法或超过范围
	 */
	public static boolean vaildByteValue(short value) {
		if (value >= 0 && value <= 255) {
			return true;
		}
		return false;
	}

	/**
	 * 检查值是否超过指定位数的2进制表示范围
	 * @param value 被检查的值
	 * @param digit 2进制位数
	 * @return true:合法,false:非法或超过范围
	 */
	public static boolean vaildValue(short value, int digit) {
		if (digit <= 0 || digit > 64) {
			throw new IllegalArgumentException("位数必须在1-64之间");
		}
		int max = (2 << (digit - 1)) - 1;
		if (value >= 0 && value <= max) {
			return true;
		}
		return false;
	}

	/**
	 * 获取ID值边界
	 * @param server 服务器值
	 * @return [0]:最小值,[1]:最大值
	 */
	public static long[] getLimits(short server) {
		if (!vaildValue(server, 12)) {
			throw new IllegalArgumentException("服务器标识[" + server + "]超过了12位二进制数的表示范围");
		}

		long min = ((long) server) << 36;
		long max = min | 0x0000000FFFFFFFFFL;
		return new long[] { min, max };
	}

	/** ID信息 */
	public static class IdInfo {
		/** 服务器标识 */
		private final short server;
		/** 标识 */
		private final long id;

		/** 构造方法 */
		public IdInfo(long id) {
			if ((0xFFFF000000000000L & id) != 0) {
				throw new IllegalArgumentException("无效的ID标识值:" + id);
			}
			this.id = id & 0x0000000FFFFFFFFFL; // 将高位置0(保留位+运营商位+服务器位)
			this.server = (short) ((id >> 36) & 0x0000000000000FFFL);
		}

		// Getter and Setter ...

		/**
		 * 获取服务器标识值
		 * @return
		 */
		public short getServer() {
			return server;
		}

		/**
		 * 获取去除运营商标识和服务器标识的ID值
		 * @return
		 */
		public long getId() {
			return id;
		}
	}

}
