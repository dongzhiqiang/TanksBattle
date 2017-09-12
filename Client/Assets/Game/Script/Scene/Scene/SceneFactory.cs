using UnityEngine;
using System.Collections;

public class SceneFactory
{
    public static LevelBase GetScene(RoomCfg roomCfg)
    {
        LevelBase scene;
        if (roomCfg.id == "100000")
            scene = new MainCityScene();
        else if (roomCfg.id == "jingjichang")
            scene = new ArenaScene();
        else if (roomCfg.id.StartsWith("jinbi"))
            scene = new GoldLevelScene();
        else if (roomCfg.id.StartsWith("hades"))
            scene = new HadesLevelScene();
        else if (roomCfg.id.StartsWith("shouhu"))
            scene = new GuardLevelScene();
        else if (roomCfg.id == "tili")
            scene = new VenusLevelScene();
        else if (roomCfg.id.StartsWith("yongshishilian"))
            scene = new WarriorLevelScene();
        else if (roomCfg.id.StartsWith("zsz"))
            scene = new EliteLevelScene();
        else if (roomCfg.id.StartsWith("prophetTower"))
            scene = new ProphetTowerLevel();
        else if (roomCfg.id == "shenqiqiangduo")
            scene = new TreasureRobScene();
        else if (roomCfg.id == IntroductionScene.ROOM_ID)
            scene = new IntroductionScene();
        else
            scene = new LevelScene();

        return scene;
    }
}
