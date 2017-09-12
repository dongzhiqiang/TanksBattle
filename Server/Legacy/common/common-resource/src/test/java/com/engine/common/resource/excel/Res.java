package com.engine.common.resource.excel;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Index;
import com.engine.common.resource.anno.Resource;
import com.engine.common.resource.anno.Static;

@Resource
public class Res {
	static final String name_idx="nameIdx";
	static final String type_idx="typeIdx";
	@Id
	private Integer id;
	@Index(name=name_idx,unique=true)
	private String name;
	private long  resId;
	private Short serNumber;
	@Index(name=type_idx)
	private byte  type;
	private boolean isRead;
	private int[] intArray;
    private String[] strArray;
    private Grade grade;
    private Grade[] grades;

	public Integer getId() {
		return id;
	}

	public void setId(Integer id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public Long getResId() {
		return resId;
	}

	public void setResId(Long resId) {
		this.resId = resId;
	}

	public Short getSerNumber() {
		return serNumber;
	}

	public void setSerNumber(Short serNumber) {
		this.serNumber = serNumber;
	}

	public byte getType() {
		return type;
	}

	public void setType(byte type) {
		this.type = type;
	}

	public boolean isRead() {
		return isRead;
	}

	public void setRead(boolean isRead) {
		this.isRead = isRead;
	}

	public int[] getIntArray() {
		return intArray;
	}

	public void setIntArray(int[] intArray) {
		this.intArray = intArray;
	}

	public String[] getStrArray() {
		return strArray;
	}

	public void setStrArray(String[] strArray) {
		this.strArray = strArray;
	}

	public Grade getGrade() {
		return grade;
	}

	public void setGrade(Grade grade) {
		this.grade = grade;
	}

	public Grade[] getGrades() {
		return grades;
	}

	public void setGrades(Grade[] grades) {
		this.grades = grades;
	}
	
	
	

}
