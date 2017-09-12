package com.game.gow.converter.common;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.game.gow.converter.Converter;
import com.game.gow.module.account.service.AccountService;

/**
 * 账号登陆账号信息转化ID
 * @author JYC103
 */
@Component("accountName2IdConverter")
public class AccountName2IdConverter implements Converter<String, Long> {

	@Autowired
	private AccountService accountService;

	@Override
	public Long convert(String source, Object... additions) {
		
//		return accountService.name2Id(source);
		return (long) 0;
	}
}
