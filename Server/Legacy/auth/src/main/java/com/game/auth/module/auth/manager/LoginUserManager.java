package com.game.auth.module.auth.manager;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.game.auth.utils.HibernateAccessorEx;


@Component
public class LoginUserManager
{
	private static final Logger logger = LoggerFactory.getLogger(LoginUserManager.class);

	@Autowired
	private HibernateAccessorEx accessor;

	@PostConstruct
	public void init()
	{
	}
	


	public LoginUser queryByUserId(String userId)
	{
		
		try
		{
			LoginUser user = accessor.load(LoginUser.class, userId);
			user.unpack();
			return user;
		}
		catch (Exception e)
		{
			logger.error("根据userId获取玩家信息出错", e);
		}
		return null;

	}

	public LoginUser insert(String userId)
	{
		try
		{
			LoginUser user = new LoginUser();
			user.setUserId(userId);
			user.pack();
			accessor.save(user);
			return user;
		}
		catch (Exception e)
		{
			logger.error("插入玩家失败", e);
		}
		return null;
	}

	public LoginUser update(LoginUser user)
	{
		try
		{
			user.pack();
			accessor.update(user);
			return user;
		}
		catch (Exception e)
		{
			logger.error("插入玩家失败", e);
		}
		return null;
	}


}
