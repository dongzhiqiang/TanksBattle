package com.engine.common.socket.core;


/**
 * TEA加密算法
 * 
 */
public class Tea {
	
	private static int times = 16;
	private static int[] keys = {0x3687C5E3,0xB7EF3327,0xE3791011,0x84E2D3BC};
	
	private static long UINT32_MAX = 0xFFFFFFFFL;
    private static long BYTE_1 = 0xFFL;
    private static long BYTE_2 = 0xFF00L;
    private static long BYTE_3 = 0xFF0000L;
    private static long BYTE_4 = 0xFF000000L;
    
    private static long delta = 0x9e3779b9L;
    
    private static void processLeftBytes(byte[] bytes, int start, int len)
    {
	    for (int i = 0; i < len; ++i)
	    {
		    int index = start + i;
		    byte b = bytes[index];
		    byte k = (byte)keys[i % 4];
		    b = (byte)(b ^ k);
		    bytes[index] = b;
	    }
    }

    /**
     * TEA算法加密
     * @param src
     */
	public static void encrypt(byte[] src) {
		int i, j;
		long v0, v1, sum;
		int step = src.length / 8;
		for(i=0; i<step; i++){
			sum = 0;
			v0 = bytes_to_uint32(src[i*8], src[i*8 + 1], src[i*8 + 2], src[i*8 + 3]);
			v1 = bytes_to_uint32(src[i*8 + 4], src[i*8 + 5], src[i*8 + 6], src[i*8 + 7]);

			for (j = 0; j < times; j++) {
				sum = long_to_uint32(sum + delta);
				v0 = long_to_uint32(v0 + long_to_uint32( 
						long_to_uint32(long_to_uint32(v1 << 4) + keys[0]) ^ long_to_uint32(v1 + sum) ^ long_to_uint32((v1 >> 5) + keys[1])) );
	            v1 = long_to_uint32(v1 + long_to_uint32(
	            		long_to_uint32(long_to_uint32(v0 << 4) + keys[2]) ^ long_to_uint32(v0 + sum) ^ long_to_uint32((v0 >> 5) + keys[3])) );
			}
			long_to_bytes(v0, src, i*8);
			long_to_bytes(v1, src, i*8 + 4);
		}
		processLeftBytes(src, step * 8, src.length % 8);
		return;
	}
	
	/**
	 * TEA算法解密
	 * @param src
	 */
	public static void decrypt(byte[] src) {
		int i, j;
		long v0, v1, sum;
		int step = src.length / 8;
		for(i=0; i<step; i++){
			if(times == 16){
				sum = 0xE3779B90;
			}else { //times == 32
				sum = 0xC6EF3720;
			}
			v0 = bytes_to_uint32(src[i*8], src[i*8 + 1], src[i*8 + 2], src[i*8 + 3]);
			v1 = bytes_to_uint32(src[i*8 + 4], src[i*8 + 5], src[i*8 + 6], src[i*8 + 7]);

			for(j = 0; j < times; j++) {
				v1 = long_to_uint32(v1 - long_to_uint32(
	            		long_to_uint32(long_to_uint32(v0 << 4) + keys[2]) ^ long_to_uint32(v0 + sum) ^ long_to_uint32((v0 >> 5) + keys[3])) );
				v0 = long_to_uint32(v0 - long_to_uint32( 
						long_to_uint32(long_to_uint32(v1 << 4) + keys[0]) ^ long_to_uint32(v1 + sum) ^ long_to_uint32((v1 >> 5) + keys[1])) );
				sum = long_to_uint32(sum - delta);
			}
			long_to_bytes(v0, src, i*8);
			long_to_bytes(v1, src, i*8 + 4);
		}
		processLeftBytes(src, step * 8, src.length % 8);
		return;
	}
	
	/**
	 * 将4个字节转换成无符号整数，以long型返回
	 * @param b0
	 * @param b1
	 * @param b2
	 * @param b3
	 * @return
	 */
    private static long bytes_to_uint32(byte b0, byte b1, byte b2, byte b3) {
    	return ((b0 << 24) & BYTE_4) + ((b1 << 16) & BYTE_3) + ((b2 << 8) & BYTE_2) + (b3 & BYTE_1);
    }
    
    /**
     * 将long型的无符号整数转换成4个字节
     * @param n
     * @param dest
     * @param offset
     */
    private static void long_to_bytes(long n, byte[] dest, int offset) {
        byte b0 = (byte)((n & BYTE_4) >> 24);
        byte b1 = (byte)((n & BYTE_3) >> 16);
        byte b2 = (byte)((n & BYTE_2) >> 8);
        byte b3 = (byte)(n & BYTE_1);
        dest[offset] = b0;
        dest[offset + 1] = b1;
        dest[offset + 2] = b2;
        dest[offset + 3] = b3;
    }
    
    /**
     * 将long型的无符号整数的高32位清除
     * @param n
     * @return
     */
    private static long long_to_uint32(long n) {
        return n & UINT32_MAX;
    }
}
