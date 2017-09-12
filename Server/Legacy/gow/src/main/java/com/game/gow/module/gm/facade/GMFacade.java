package com.game.gow.module.gm.facade;

import static com.engine.common.socket.filter.session.SessionManager.IDENTITY;
import static com.game.gow.module.gm.facade.GMModule.COMMAND_PROCESS_GM_CMD;
import static com.game.gow.module.gm.facade.GMModule.MODULE;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.InSession;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.utils.model.Result;
import com.game.gow.module.gm.model.GMResultVo;

/**
 * gm命令门面
 */
@SocketModule(MODULE)
public interface GMFacade {
	/**
	 * 升级装备
	 * @param accountId
	 * @param equipId 装备唯一ID
	 * @return {@link EquipVo}
	 */
	@SocketCommand(COMMAND_PROCESS_GM_CMD)
	Result<GMResultVo> processGMCMD(@InSession(IDENTITY) long accountId,@InBody String msg);
}
