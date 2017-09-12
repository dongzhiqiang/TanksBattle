package com.game.gow.module.gm.facade;

import java.util.ArrayList;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.utils.model.Result;
import com.game.gow.module.gm.model.GMCommand;
import com.game.gow.module.gm.model.GMResultVo;
import com.game.gow.module.item.manager.Item;
import com.game.gow.module.item.service.ItemService;
import com.game.gow.module.player.manager.Player;
import com.game.gow.module.player.service.PlayerService;

@Component
public class GMFacadeImpl implements GMFacade {
	


	private static final Logger logger=LoggerFactory.getLogger(GMFacadeImpl.class);
	
	@Autowired
	private ItemService itemService;
	@Autowired
	private PlayerService playerService;


	@Override
	public Result<GMResultVo> processGMCMD(long accountId, String msg) {
		String[] msgs = msg.split(" ");
		GMResultVo result = new GMResultVo();
		result.setReqString(msgs[0]);
		try
		{
			String cmd = msgs[0];
			switch( cmd )
			{
			case GMCommand.CMD_ADD_ITEM:
				int baseId = Integer.parseInt(msgs[1]);
				int num = Integer.parseInt(msgs[2]);
				Item item = itemService.itemOf(baseId, num);
				ArrayList<Item> items = new ArrayList<Item>();
				items.add(item);
				itemService.addItem(accountId, items);
				result.setResult(true);
				break;
			case GMCommand.CMD_SET_LEVEL:
				int level = Integer.parseInt(msgs[1]);
				Player player = playerService.load(accountId);
				player.setLevel(level);
				result.setResult(true);
				break;
			}
			return Result.SUCCESS(result);
		}
		catch( Exception e)
		{
			logger.error("GM命令格式执行错误", e);
			return Result.ERROR(-255);
		}
	}

}
