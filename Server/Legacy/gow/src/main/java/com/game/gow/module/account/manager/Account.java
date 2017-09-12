package com.game.gow.module.account.manager;

import java.util.Calendar;
import java.util.Date;

import javax.persistence.Column;
import javax.persistence.Embedded;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;

import org.apache.commons.lang3.StringUtils;
import org.hibernate.annotations.Index;
import org.hibernate.annotations.Table;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.enhance.Enhance;
import com.engine.common.utils.time.DateUtils;
import com.game.gow.module.account.exception.AccountException;
import com.game.gow.module.account.exception.AccountExceptionCode;
import com.game.gow.module.account.model.AccountState;
import com.game.gow.module.common.CachedSizes;

/**
 * 账户实体
 * 
 * @author wenkin
 */
@Entity
@NamedQueries({
	// @NamedQuery(name = Account.NAME, query = "from Account a where a.name = ?"),
	@NamedQuery(name = Account.REGISTER, query = "select count(t.id) from Account as t where t.id between ? and ? and t.createdOn between ? and ?"),
	@NamedQuery(name = Account.MAX_ID, query = "select max(t.id) from Account as t where t.id between ? and ?"),
	@NamedQuery(name = Account.NAME2ID, query = "select t.name, t.id from Account as t"),
	@NamedQuery(name = Account.COUNT, query = "select count(*) from Account as t") })
@Cached(size = CachedSizes.DEFAULT)
@Table(appliesTo = "Account")
public class Account implements IEntity<Long> {

	/** 账号名与服标识之间的分隔符 */
	public static final String SPLIT = ".";

	/* 命名查询名 */
	public static final String NAME2ID = "Account_name2Id";
	public static final String MAX_ID = "Account_maxId";
	public static final String COUNT = "Account_count";
	public static final String REGISTER = "Account_registered";

	/* 索引名定义 */
	static final String ACCOUNT_NAME = "Account_name";

	/** 账户编号 */
	@Id
	private Long id;
	/** 账号 */
	@Index(name = ACCOUNT_NAME)
	@Column(unique = true, nullable = false)
	private String name;
	/** 渠道Id */
	private String channelId;
	/**用户Id*/
	private String userId;
	/**用户Id*/
	private short serverId;
	/**客户端版本*/
	private String clientVer;
	/**客户端设备信息*/
	@Embedded
	private DeviceInf deviceInf;

	/** 创建时间 */
	@Column(nullable = false)
	private Date createdOn;
	/** 状态 */
	@Column(nullable = false)
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

	/** 登入数据处理 */
	@Enhance
	synchronized void loginTimeCount(Date now) {
		// 首次登入的处理
		if (loginOn == null) {
			loginOn = now;
			online = true;
			return;
		}

		// 补全丢失的时间(有登入无登出的情况)
		if (logoutOn == null || logoutOn.before(loginOn)) {
			// 修正在线天数
			int day = DateUtils.calcIntervalDays(loginOn, now);
			dayByTotal += day;
			dayByContinuous += day;
			// 修正累计在线时间
			timeByTotal += now.getTime() - loginOn.getTime();
			// 修正当天累计时间
			if (DateUtils.isSameDay(loginOn, now)) {
				timeByDay = now.getTime() - loginOn.getTime();
			} else {
				timeByDay = now.getTime() - DateUtils.getFirstTime(now).getTime();
			}
			logoutOn = DateUtils.addTime(now, 0, 0, -1);
			loginOn = now;
			online = true;
			return;
		}

		// 正常登入的处理
		int day = DateUtils.calcIntervalDays(logoutOn, now);
		if (day > 1) {
			dayByTotal++;
			dayByContinuous = 0;
		} else if (day == 1) {
			dayByTotal++;
			dayByContinuous++;
		}

		loginOn = now;
		online = true;
	}

	/** 登出数据处理 */
	@Enhance
	synchronized void logout(Date now) {
		int day = DateUtils.calcIntervalDays(loginOn, now);
		if (day == 0) {
			// 登入与登出在同一天
			long times = now.getTime() - loginOn.getTime();
			timeByDay += times;
			timeByTotal += times;
		} else {
			// 登入与登出不在同一天
			dayByTotal += day;
			dayByContinuous += day;
			timeByTotal += now.getTime() - loginOn.getTime();
			timeByDay += now.getTime() - DateUtils.getFirstTime(now).getTime();
		}

		logoutOn = now;
		online = false;
	}

	/**
	 * 获取累计在线天数
	 * @return
	 */
	public int getTotalDays() {
		return dayByTotal + DateUtils.calcIntervalDays(loginOn, new Date());
	}

	/**
	 * 获取连续在线天数
	 * @return
	 */
	public int getContinuousDays() {
		return dayByContinuous + DateUtils.calcIntervalDays(loginOn, new Date());
	}

	/**
	 * 获取累计在线分钟数
	 * @return
	 */
	public int getTotalMinutes() {
		return (int) (getTotalTimes() / DateUtils.MILLIS_PER_MINUTE);
	}

	/**
	 * 获取当天的累计在线分钟数
	 * @return
	 */
	public int getDayMinutes() {
		return (int) (getDayTimes() / DateUtils.MILLIS_PER_MINUTE);
	}

	/** 获取累计在线毫秒数 */
	private long getTotalTimes() {
		Date now = new Date();
		return timeByTotal + now.getTime() - loginOn.getTime();
	}

	/** 获取当天的累计在线毫秒数 */
	private long getDayTimes() {
		Date now = new Date();
		if (DateUtils.calcIntervalDays(loginOn, now) == 0) {
			return timeByDay + now.getTime() - loginOn.getTime();
		} else {
			return DateUtils.getFirstTime(now).getTime() - now.getTime();
		}
	}

	// Getter and Setter ...

	public Long getId() {
		return id;
	}

	protected void setId(Long id) {
		this.id = id;
	}
    
	public String getName() {
		return name;
	}

	protected void setName(String name) {
		this.name = name;
	}
	
	public String getChannelId() {
		return channelId;
	}

	protected void setChannelId(String channelId) {
		this.channelId = channelId;
	}
	
	public String getUserId() {
		return userId;
	}

	protected void setUserId(String userId) {
		this.userId = userId;
	}
	
	public short getServerId() {
		return serverId;
	}

	protected void setServerId(short serverId) {
		this.serverId = serverId;
	}
	
	public String getClientVer() {
		return clientVer;
	}

	protected void setClientVer(String clientVer) {
		this.clientVer = clientVer;
	}
   
	public DeviceInf getDeviceInf() {
		return deviceInf;
	}
    
	@Enhance
	protected void setDeviceInf(DeviceInf deviceInf) {
		this.deviceInf = deviceInf;
	}

	public Date getCreatedOn() {
		return createdOn;
	}

	protected void setCreatedOn(Date createdOn) {
		this.createdOn = createdOn;
	}

	public AccountState getState() {
		return state;
	}

	@Enhance
	public void setState(AccountState state) {
		this.state = state;
	}

	public Date getLoginOn() {
		return loginOn;
	}

	protected void setLoginOn(Date loginOn) {
		this.loginOn = loginOn;
	}

	public Date getLogoutOn() {
		return logoutOn;
	}

	protected void setLogoutOn(Date logoutOn) {
		this.logoutOn = logoutOn;
	}

	public long getTimeByDay() {
		return timeByDay;
	}

	protected void setTimeByDay(long timeByDay) {
		this.timeByDay = timeByDay;
	}

	public long getTimeByTotal() {
		return timeByTotal;
	}

	protected void setTimeByTotal(long timeByTotal) {
		this.timeByTotal = timeByTotal;
	}

	public int getDayByTotal() {
		return dayByTotal;
	}

	protected void setDayByTotal(int dayByTotal) {
		this.dayByTotal = dayByTotal;
	}

	public int getDayByContinuous() {
		return dayByContinuous;
	}

	protected void setDayByContinuous(int dayByContinuous) {
		this.dayByContinuous = dayByContinuous;
	}

	public boolean isOnline() {
		return online;
	}

	protected void setOnline(boolean online) {
		this.online = online;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((getId() == null) ? 0 : getId().hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (!(obj instanceof Account))
			return false;
		Account other = (Account) obj;
		if (getId() == null) {
			if (other.getId() != null)
				return false;
		} else if (!getId().equals(other.getId()))
			return false;
		return true;
	}

	// Static Method's ...
	/**
	 * 根据一些参数组成账号名
	 * @param channelId
	 * @param userId
	 * @param serverId
	 * @return
	 */
	public static String getAccountName(String channelId, String userId, short serverId) {
		return channelId + SPLIT + userId + SPLIT + serverId;
	}

	/** 构造方法 */
	public static Account valueOf(long id, String name, String channelId, String userId, short serverId, String clientVer, DeviceInf deviceInf) {
		Account result = new Account();
		result.id = id;
		result.name = name;
		result.channelId = channelId;
		result.userId = userId;
		result.serverId = serverId;
		result.clientVer = clientVer;		
		result.deviceInf = deviceInf;

		Date now = new Date();
		result.createdOn = now;
		result.loginOn = now;
		result.logoutOn = now;
		result.state = AccountState.NORMAL;
		return result;
	}

	/**
	 * 检查帐号是否在指定日期前已经流失
	 * @param account 被检查帐号
	 * @param days 在多少天前
	 * @return
	 */
	public static boolean isTurnover(Account account, int days) {
		if(account.getLoginOn()==null) {
			return true;
		}
		if(account.getLogoutOn()==null) {
			return false;
		}
		if(account.getLoginOn().after(account.getLogoutOn())) {
			// 在线
			return false;
		}
		
		final Calendar calendar = Calendar.getInstance();
		calendar.add(Calendar.DATE, -days);
		final Date turnover = calendar.getTime();
		
		return account.getLogoutOn().before(turnover);
	}

	/** 获取存储帐号名 */
	public static String toAccountName(int server, String name) {
		return name + SPLIT + server;
	}
}
