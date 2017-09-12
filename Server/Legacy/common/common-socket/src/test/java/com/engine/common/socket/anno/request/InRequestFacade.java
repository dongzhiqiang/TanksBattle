package com.engine.common.socket.anno.request;

import java.util.Arrays;

import org.springframework.stereotype.Component;

import com.engine.common.socket.core.Command;

@Component
public class InRequestFacade implements InRequestFacadeInf {

	@Override
	public long sn(long sn) {
		return sn;
	}

	private Command command = Command.valueOf(2, 3);
	
	@Override
	public boolean command(Command command) {
		return this.command.equals(command);
	}

	@Override
	public int state(int state) {
		return state;
	}

	@Override
	public String attachment(byte[] attachment) {
		return Arrays.toString(attachment);
	}

}
