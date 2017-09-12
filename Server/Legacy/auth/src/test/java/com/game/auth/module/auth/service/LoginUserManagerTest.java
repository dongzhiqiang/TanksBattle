package com.game.auth.module.auth.service;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.support.ClassPathXmlApplicationContext;

import com.game.auth.module.auth.manager.LoginUserManager;
import com.game.auth.module.auth.manager.LoginUser;
import com.game.auth.module.auth.manager.Role;


@SuppressWarnings("unused")
public class LoginUserManagerTest 
{
	/*
	private static final Logger logger = LoggerFactory.getLogger(LoginUserManagerTest.class);
	private LoginUserManager loginUserManger;
	public LoginUserManagerTest()
	{
	}
	
	public void testInsert()
	{
		loginUserManger.insert("abc");
	}
	
	public LoginUser testLoad()
	{
		LoginUser loginUser = loginUserManger.queryByUserId("abc");
		return loginUser;
	}
	
	public void testUpdate(LoginUser loginUser)
	{
		Role role = new Role();
		role.setName("1");
		loginUser.setPreRole(role);
		role = new Role();
		role.setName("2");
		loginUser.getRoles().add(role);
		loginUserManger.update(loginUser);
	}
	
	@org.junit.Test
	public void test()
	{
		ClassPathXmlApplicationContext applicationContext = null;
		try {
			applicationContext = new ClassPathXmlApplicationContext("applicationContext.xml");
			loginUserManger  = (LoginUserManager) applicationContext.getBean(LoginUserManager.class);
		} catch (Exception e) {
			logger.error("错误", e);
			Runtime.getRuntime().exit(-1);
		}
		applicationContext.registerShutdownHook();
		applicationContext.start();
		//testInsert();
		testUpdate(testLoad());
		testLoad();
	}
	
	public static void main(String[] args) 
	{
		new LoginUserManagerTest().test();
	}*/
}
