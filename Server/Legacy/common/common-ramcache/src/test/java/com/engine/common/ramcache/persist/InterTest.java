package com.engine.common.ramcache.persist;

public class InterTest {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		Thread t = new Thread() {
			@Override
			public void run() {
				for (int i = 0; i < 10000000; i++) {
					Thread.yield();
				}
				System.out.println("Over");
			}
		};
		t.start();
		t.interrupt();
	}

}
