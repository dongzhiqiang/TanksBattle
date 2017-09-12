package com.game.gow.module.account.model;

import java.util.Date;

import com.engine.common.protocol.annotation.Transable;
import com.game.gow.module.account.manager.Account;

/**
 * 账户VO
 * 
 * @author wenkin
 */
@Transable
public class AccountVo {

	/** 账户编号 */
	private Long id;
	/** 账号 */
	private String name;
	/** 创建时间 */
	private Date createdOn;
	/** 状态 */
	private AccountState state;

	/** 最后登录时间 */
	private Date loginOn;
	/** 最后登出时间 */
	private Date logoutOn;

	/** 当天累计时间 */
	private long timeByDay;
	/** 累计在线时间 */
	private long timeByTotal;

	/** 累计在线天数(从0开始) */
	private int dayByTotal;
	/** 连续登录天数(从0开始) */
	private int dayByContinuous;

	/** 是否在线状态 */
	private boolean online;

	// ---------- 构造器 ----------

	private AccountVo() {

	}

	// ---------- Getter/Setter ----------

	public Long getId() {
		return id;
	}

	public void setId(Long id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public Date getCreatedOn() {
		return createdOn;
	}

	public void setCreatedOn(Date createdOn) {
		this.createdOn = createdOn;
	}

	public AccountState getState() {
		return state;
	}

	public void setState(AccountState state) {
		this.state = state;
	}

	public Date getLoginOn() {
		return loginOn;
	}

	public void setLoginOn(Date loginOn) {
		this.loginOn = loginOn;
	}

	public Date getLogoutOn() {
		return logoutOn;
	}

	public void setLogoutOn(Date logoutOn) {
		this.logoutOn = logoutOn;
	}

	public long getTimeByDay() {
		return timeByDay;
	}

	public void setTimeByDay(long timeByDay) {
		this.timeByDay = timeByDay;
	}

	public long getTimeByTotal() {
		return timeByTotal;
	}

	public void setTimeByTotal(long timeByTotal) {
		this.timeByTotal = timeByTotal;
	}

	public int getDayByTotal() {
		return dayByTotal;
	}

	public void setDayByTotal(int dayByTotal) {
		this.dayByTotal = dayByTotal;
	}

	public int getDayByContinuous() {
		return dayByContinuous;
	}

	public void setDayByContinuous(int dayByContinuous) {
		this.dayByContinuous = dayByContinuous;
	}

	public boolean isOnline() {
		return online;
	}

	public void setOnline(boolean online) {
		this.online = online;
	}

	// ---------- 静态方法 ----------

	/** 构造方法 */
	public static AccountVo valueOf(Account account) {
		AccountVo vo = new AccountVo();
		vo.id = account.getId();
		vo.name = account.getName();
		vo.createdOn = account.getCreatedOn();
		vo.state = account.getState();
		vo.loginOn = account.getLoginOn();
		vo.logoutOn = account.getLogoutOn();
		vo.timeByDay = account.getTimeByDay();
		vo.timeByTotal = account.getTimeByTotal();
		vo.dayByTotal = account.getDayByTotal();
		vo.dayByContinuous = account.getDayByContinuous();
		vo.online = account.isOnline();
		return vo;
	}

}
