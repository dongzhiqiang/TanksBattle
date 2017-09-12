package com.game.auth.module.auth.manager;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Transient;

import org.hibernate.annotations.Table;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.TypeReference;
import com.engine.common.utils.json.JsonUtils;



/**
 * 
 * 周期订单
 * @author LJJ
 *
 */

@Entity
@Table(appliesTo = "LoginUser")
public class LoginUser {


	/**
	 * 玩家uid
	 */
	@Id
	private String userId;
	
	private String preRolePacked;
	
	private String rolesPacked;
	
	/**
	 * 上一次登录的角色
	 */
	@Transient
	private Role preRole ;
	/**
	 * 玩家角色列表
	 */
	@Transient
	private List<Role> roles = new ArrayList<Role>();
	
	public void pack()
	{
		preRolePacked = JsonUtils.object2String(preRole);
		rolesPacked = JsonUtils.object2String(roles);
	}
	
	public void unpack()
	{
		preRole = JSON.parseObject(preRolePacked,
				new TypeReference<Role>()
				{
				});
		roles = JSON.parseObject(rolesPacked,
				new TypeReference<ArrayList<Role>>()
				{
				});
	}
	
	public String getPreRolePacked() {
		return preRolePacked;
	}
	public void setPreRolePacked(String preRolePacked) {
		this.preRolePacked = preRolePacked;
	}
	
	public String getRolesPacked() {
		return rolesPacked;
	}
	public void setRolesPacked(String rolesPacked) {
		this.rolesPacked = rolesPacked;
	}

	public static LoginUser valueOf(String userId)
	{
		LoginUser result = new LoginUser();
		result.userId = userId;
		return result;
	}


	public String getUserId() {
		return userId;
	}
	public void setUserId(String userId) {
		this.userId = userId;
	}


	public List<Role> getRoles() {
		return roles;
	}

	public void setRoles(List<Role> roles) {
		this.roles = roles;
	}






	public Role getPreRole() {
		return preRole;
	}


	public void setPreRole(Role preRole) {
		this.preRole = preRole;
	}



}
