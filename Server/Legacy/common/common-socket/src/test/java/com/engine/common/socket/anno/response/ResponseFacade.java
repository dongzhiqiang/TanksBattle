package com.engine.common.socket.anno.response;

import org.apache.commons.lang3.ArrayUtils;

import com.engine.common.socket.core.Response;

public class ResponseFacade implements ResponseFacadeInf {

	@Override
	public Response<String> attachment(byte[] args) {
		return Response.valueOf("SUCCESS", args);
	}

	@Override
	public Response<Integer> inToOut(int value, byte[] attachment) {
		if (attachment != null) {
			ArrayUtils.reverse(attachment);
		}
		return Response.valueOf(value, attachment);
	}

	@Override
	public Response<Boolean> rawToOut(byte[] raw) {
		if (raw == null || raw.length == 0) {
			return Response.valueOf(false, null);
		} else {
			return Response.valueOf(true, raw);
		}
	}

}
