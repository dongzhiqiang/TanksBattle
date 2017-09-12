package com.game.gow.module.common.model;

import java.util.Collection;

/**
 * 战报清理类型
 *
 */
public class ReportFileClear {
	// 默认的过期天数
	public static int DEFAULTLASTDAY = 0;

	/** 战报类型 */
	private FileReportType type;
	/** 过期天数 */
	private int lastDay;
	/** 排除集合 */
	private Collection<String> excepts;

	// Getter and Setter ...

	public FileReportType getType() {
		return type;
	}

	public void setType(FileReportType type) {
		this.type = type;
	}

	public int getLastDay() {
		return lastDay;
	}

	public void setLastDay(int lastDay) {
		this.lastDay = lastDay;
	}

	public Collection<String> getExcepts() {
		return excepts;
	}

	public void setExcepts(Collection<String> excepts) {
		this.excepts = excepts;
	}

	// Static method's

	public static ReportFileClear valueOf(FileReportType type, int lastDay, Collection<String> excepts) {
		ReportFileClear clear = new ReportFileClear();
		clear.type = type;
		clear.lastDay = lastDay;
		clear.excepts = excepts;
		return clear;
	}

}
