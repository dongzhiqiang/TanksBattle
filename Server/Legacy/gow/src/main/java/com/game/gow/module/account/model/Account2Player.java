package com.game.gow.module.account.model;

import com.game.gow.module.account.manager.Account;
import com.game.gow.module.player.manager.Player;

/**
 * 账号角色
 * 
 *@author wenkin
 */
public class Account2Player {
   
	private Account account;
	
	private Player player;
	
	
	public static Account2Player valueOf(Account account,Player player){
		Account2Player result=new Account2Player();
		result.account=account;
		result.player=player;
		return result;
	}


	public Account getAccount() {
		return account;
	}


	public void setAccount(Account account) {
		this.account = account;
	}


	public Player getPlayer() {
		return player;
	}


	public void setPlayer(Player player) {
		this.player = player;
	}
	
	
}
