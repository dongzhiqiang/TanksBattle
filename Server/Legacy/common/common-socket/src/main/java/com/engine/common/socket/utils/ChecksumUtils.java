package com.engine.common.socket.utils;

import com.engine.common.protocol.utils.HashUtils;

public class ChecksumUtils {
	public static int checksum(byte[] bytes) {
		return (int) HashUtils.BPHash(bytes, bytes.length);
	}
}
