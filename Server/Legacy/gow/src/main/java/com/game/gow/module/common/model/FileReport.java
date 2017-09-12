package com.game.gow.module.common.model;

import java.io.File;
import java.util.UUID;

import org.apache.commons.lang3.StringUtils;

/**
 * 文件战报对象
 * 
 */
public class FileReport {

	/** 构造方法 */
	public static FileReport valueOf(long owner, String content, FileReportType type, Object addition) {
		FileReport result = new FileReport();
		result.owner = owner;
		result.names = getUUID();
		result.content = content;
		result.type = type;
		result.addition = addition;
		return result;
	}

	/** 用户标识 */
	private long owner;
	/** 生成文件名 */
	private String names;
	/** 内容 */
	private String content;
	/** 战报类型 */
	private FileReportType type;
	/** 附加信息对象 */
	private Object addition;

	/**
	 * 获取文件战报的相对路径
	 * @return
	 */
	public String getUrl(String path) {
		String name = getUrl();
		if (StringUtils.endsWith(path, File.separator)) {
			return path + StringUtils.substring(name, 1);
		} else {
			return path + name;
		}
	}

	/**
	 * 获取文件战报的相对路径
	 * @return /类型/操作者ID+UUID.dat
	 */
	public String getUrl() {
		String name = type + File.separator + owner + File.separator + names + ".dat";
		return File.separator + name;
	}

	public static String getUUID() {
		UUID uuid = UUID.randomUUID();
		return uuid.toString();
	}

	// Getter and Setter ...

	public long getOwner() {
		return owner;
	}

	public void setOwner(long owner) {
		this.owner = owner;
	}

	public String getNames() {
		return names;
	}

	public void setNames(String names) {
		this.names = names;
	}

	public FileReportType getType() {
		return type;
	}

	public void setType(FileReportType type) {
		this.type = type;
	}

	public Object getAddition() {
		return addition;
	}

	public void setAddition(Object addition) {
		this.addition = addition;
	}

	public String getContent() {
		return content;
	}

	public void setContent(String content) {
		this.content = content;
	}
}
