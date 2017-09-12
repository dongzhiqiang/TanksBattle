package com.engine.common.socket.anno.impl;

import com.engine.common.socket.anno.InBody;

public class TestTargetImpl {
	
	public void testInBodyDefault(@InBody("name") String name, @InBody("password") String password) {
	}

}
