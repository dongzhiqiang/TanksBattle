package com.engine.common.socket.filter.firewall;

import org.springframework.stereotype.Component;

import com.engine.common.socket.anno.Raw;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;

@Component
@SocketModule(1)
public class Facade {

	/**
	 * 回应测试方法
	 * @param number
	 * @return
	 */
	@SocketCommand(1)
	public int echo(int number) {
		return number;
	}
	
	/**
	 * 测试通信字节数
	 * @param bytes
	 */
	@SocketCommand(value = 2, raw = @Raw(request = true))
	public void bytes(byte[] bytes) {
		return;
	}
}
