package com.engine.common.protocol.utils;

/*
 **************************************************************************
 *                                                                        *
 *          General Purpose Hash Function Algorithms Library              *
 *                                                                        *
 * Author: Arash Partow - 2002                                            *
 * URL: http://www.partow.net                                             *
 * URL: http://www.partow.net/programming/hashfunctions/index.html        *
 *                                                                        *
 * Copyright notice:                                                      *
 * Free use of the General Purpose Hash Function Algorithms Library is    *
 * permitted under the guidelines and in accordance with the most current *
 * version of the Common Public License.                                  *
 * http://www.opensource.org/licenses/cpl1.0.php                          *
 *                                                                        *
 **************************************************************************
 */

public class HashUtils {

	public static long RSHash(byte[] bytes, int len) {
		int b = 378551;
		int a = 63689;
		long hash = 0;

		for (int i = 0; i < len; i++) {
			hash = hash * a + bytes[i];
			a = a * b;
		}

		return hash;
	}

	/* End Of RS Hash Function */

	public static long JSHash(byte[] bytes, int len) {
		long hash = 1315423911;

		for (int i = 0; i < len; i++) {
			hash ^= ((hash << 5) + bytes[i] + (hash >> 2));
		}

		return hash;
	}

	/* End Of JS Hash Function */

	public static long PJWHash(byte[] bytes, int len) {
		long BitsInUnsignedInt = (long) (4 * 8);
		long ThreeQuarters = (long) ((BitsInUnsignedInt * 3) / 4);
		long OneEighth = (long) (BitsInUnsignedInt / 8);
		long HighBits = (long) (0xFFFFFFFF) << (BitsInUnsignedInt - OneEighth);
		long hash = 0;
		long test = 0;

		for (int i = 0; i < len; i++) {
			hash = (hash << OneEighth) + bytes[i];

			if ((test = hash & HighBits) != 0) {
				hash = ((hash ^ (test >> ThreeQuarters)) & (~HighBits));
			}
		}

		return hash;
	}

	/* End Of P. J. Weinberger Hash Function */

	public static long ELFHash(byte[] bytes, int len) {
		long hash = 0;
		long x = 0;

		for (int i = 0; i < len; i++) {
			hash = (hash << 4) + bytes[i];

			if ((x = hash & 0xF0000000L) != 0) {
				hash ^= (x >> 24);
			}
			hash &= ~x;
		}

		return hash;
	}

	/* End Of ELF Hash Function */

	public static long BKDRHash(byte[] bytes, int len) {
		long seed = 131; // 31 131 1313 13131 131313 etc..
		long hash = 0;

		for (int i = 0; i < len; i++) {
			hash = (hash * seed) + bytes[i];
		}

		return hash;
	}

	/* End Of BKDR Hash Function */

	public static long SDBMHash(byte[] bytes, int len) {
		long hash = 0;

		for (int i = 0; i < len; i++) {
			hash = bytes[i] + (hash << 6) + (hash << 16) - hash;
		}

		return hash;
	}

	/* End Of SDBM Hash Function */

	public static long DJBHash(byte[] bytes, int len) {
		long hash = 5381;

		for (int i = 0; i < len; i++) {
			hash = ((hash << 5) + hash) + bytes[i];
		}

		return hash;
	}

	/* End Of DJB Hash Function */

	public static long DEKHash(byte[] bytes, int len) {
		long hash = len;

		for (int i = 0; i < len; i++) {
			hash = ((hash << 5) ^ (hash >> 27)) ^ bytes[i];
		}

		return hash;
	}

	/* End Of DEK Hash Function */

	public static long BPHash(byte[] bytes, int len) {
		long hash = 0;

		for (int i = 0; i < len; i++) {
			hash = hash << 7 ^ bytes[i];
		}

		return hash;
	}

	/* End Of BP Hash Function */

	public static long FNVHash(byte[] bytes, int len) {
		long fnv_prime = 0x811C9DC5;
		long hash = 0;

		for (int i = 0; i < len; i++) {
			hash *= fnv_prime;
			hash ^= bytes[i];
		}

		return hash;
	}

	/* End Of FNV Hash Function */

	public static long APHash(byte[] bytes, int len) {
		long hash = 0xAAAAAAAA;

		for (int i = 0; i < len; i++) {
			if ((i & 1) == 0) {
				hash ^= ((hash << 7) ^ bytes[i] * (hash >> 3));
			} else {
				hash ^= (~((hash << 11) + (bytes[i] & 0xFF) ^ (hash >> 5)));
			}
		}

		return hash;
	}
	/* End Of AP Hash Function */

}
