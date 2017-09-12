package com.game.gow.module.gm.model;

import com.engine.common.protocol.annotation.Transable;

/**
 * gm命令结果VO
 */
@Transable
public class GMResultVo {
	private boolean result;
	private String reqString;
	public boolean isResult() {
		return result;
	}
	public void setResult(boolean result) {
		this.result = result;
	}
	public String getReqString() {
		return reqString;
	}
	public void setReqString(String reqString) {
		this.reqString = reqString;
	}
}
