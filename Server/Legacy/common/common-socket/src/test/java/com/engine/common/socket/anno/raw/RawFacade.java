package com.engine.common.socket.anno.raw;

import org.apache.commons.lang3.ArrayUtils;

public class RawFacade implements RawFacadeInf {

	@Override
	public void in(byte[] raw) {
	}

	@Override
	public byte[] out(byte[] args) {
		return args;
	}

	@Override
	public byte[] inAndOut(byte[] args) {
		ArrayUtils.reverse(args);
		return args;
	}

	@Override
	public byte[] attachmentToRaw(byte[] attachment) {
		return attachment;
	}

}
