package com.game.module.migration.converter;

import java.io.Serializable;
import java.util.Arrays;
import java.util.Date;
import java.util.HashSet;
import java.util.Set;

import org.apache.commons.lang3.ArrayUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.impl.HibernateAccessor;
import com.engine.common.utils.json.JsonUtils;
import com.game.gow.module.common.manager.GlobalInfo;
import com.game.gow.module.common.manager.GlobalKey;
import com.game.module.migration.IgnoreMigrateException;
import com.game.module.migration.MigratorContext;
import com.game.module.migration.MigratorConverter;

public class GlobalInfoMigrator implements MigratorConverter {

	private final Logger logger = LoggerFactory.getLogger(getClass());

	// TODO 注意合服的忽略列表
	private static final GlobalKey[] ignoreKeys = { GlobalKey.IMPERIAL_RANKS, GlobalKey.MALL_RELEASE,
		GlobalKey.MALL_LOG, GlobalKey.LADDER_NODE_LIST, GlobalKey.SIEGE_BEFORE_HAND, GlobalKey.REWARD_LIMIT,
		GlobalKey.DRAGON_ACTIVITY, GlobalKey.SLAVE_ACTIVITY };

	// TODO 注意合服的覆盖列表
	private static final GlobalKey[] overlayKeys = { GlobalKey.SWORN, GlobalKey.WORLD_LEVEL, GlobalKey.IMPERIAL,
		GlobalKey.SIEGE_TIME, GlobalKey.SIEGE_LEVEL, GlobalKey.WORLD_LEVEL_TIME, GlobalKey.REGISTABLE,
		GlobalKey.WORLD_BOSS };

	@Override
	public void convert(MigratorContext context, HibernateAccessor oldRawStore, HibernateAccessor newRawStore,
			IEntity<Serializable> entity) throws Exception {
		final GlobalInfo info = GlobalInfo.class.cast(entity);
		final GlobalKey key = info.getId();

		if (ArrayUtils.contains(ignoreKeys, key)) {
			throw new IgnoreMigrateException("GlobalKey:" + key + "属于忽略的列表");
		}

		final GlobalInfo current = newRawStore.load(GlobalInfo.class, key);
		if (ArrayUtils.contains(overlayKeys, key) && current != null) {
			throw new IgnoreMigrateException("GlobalKey:" + key + "已经存在,无需保存");
		}

		// 开服时间
		if (GlobalKey.START_SERVERDATE.equals(key) && current != null) {
			Date date1 = JsonUtils.string2Object(current.getContent(), Date.class);
			Date date2 = JsonUtils.string2Object(info.getContent(), Date.class);
			// 取最早的时间
			if (date1.before(date2)) {
				logger.error("修正开服时间为[{}]", current.getContent());
				info.setContent(current.getContent());
			}
		}

		// 活动ID
		if (GlobalKey.ACTIVITY_OVERLAY.equals(key) && current != null) {
			Integer n1 = Integer.valueOf(current.getContent());
			Integer n2 = Integer.valueOf(info.getContent());
			// 取最大值
			if (n1 > n2) {
				info.setContent(current.getContent());
			}
		}

		if (GlobalKey.HONOUR_INCREASE.equals(key) && current != null) {
			// 这个属性很SB, 做了两次JSON转换
			Long[] list1 = JsonUtils.string2Array(current.getValue(String.class), Long.class);
			Long[] list2 = JsonUtils.string2Array(info.getValue(String.class), Long.class);
			Set<Long> set = new HashSet<Long>();
			set.addAll(Arrays.asList(list1));
			set.addAll(Arrays.asList(list2));
			info.setContent(JsonUtils.object2String(JsonUtils.object2String(set)));
		}
	}

}