using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour {
    // キャンバス
    public Transform m_Canvas;
    // スキルバトルマネージャー
    public SkillBattleManager m_SkillBattleManager;

    private void Awake()
    {
        // フレームレート設定
        Application.targetFrameRate = 30;

        // InputManager生成
        InputManager.InstanceCheck();

        // プラネットマネージャー生成
        GameObject planet = new GameObject("PlanetManager");
        PlanetManager planetManager = planet.AddComponent<PlanetManager>();
        
        // プレイヤー生成
        GameObject player = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Charactor/Player"));
        CharCore playerCore = player.GetComponent<CharCore>();

        // 敵生成
        GameObject enemy = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Charactor/Enemy"));
        CharCore enemyCore = enemy.GetComponent<CharCore>();

        // バトルマネージャー生成
        GameObject battleManagerObj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/BattleManager"));
        BattleManager battleManager = battleManagerObj.GetComponent<BattleManager>();

        // ピラージェネレーター生成
        GameObject pillerGeneratorObj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/PillerGenerator"));
        PillerGenerator pillerGenerator = pillerGeneratorObj.GetComponent<PillerGenerator>();

        // カメラ生成
        GameObject camera = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Main Camera"));
        CameraController cameraCtrl = camera.GetComponent<CameraController>();

        // プラネットマネージャー初期化
        planet.transform.SetParent(transform);
        // 球体生成
        planetManager.CreatePlanet(new PlanetData("VanillaPlanet", Vector3.zero, (int)PlanetID.PLANET_1, 100.0f));

        // プレイヤー初期化
        player.transform.SetParent(transform);
        playerCore.m_BattleManager = battleManager;
        playerCore.m_PlanetManager = planetManager;
        playerCore.GetBrain().SetLookBase(camera.transform);
        playerCore.SetPlanetID(PlanetID.PLANET_1);
        // プレイヤースキル初期化
        SkillManager pSkillManager = playerCore.GetBrain().GetState().GetSkillManager();
        foreach (var data in SkillDataBase.PLAYER_DATAS){
            pSkillManager.AddSkill(data.Value);
        }

        // 敵初期化
        enemy.transform.SetParent(transform);
        enemyCore.m_BattleManager = battleManager;
        enemyCore.m_PlanetManager = planetManager;
        enemyCore.GetBrain().SetLookBase(enemy.transform);
        enemyCore.SetPlanetID(PlanetID.PLANET_1);
        // 敵スキル初期化
        SkillManager eSkillManager = enemyCore.GetBrain().GetState().GetSkillManager();
        foreach (var data in SkillDataBase.ENEMY_DATAS){
            eSkillManager.AddSkill(data.Value);
        }


        // バトルマネージャー初期化
        battleManagerObj.transform.SetParent(transform);
        battleManager.SetBattleCharInfo(playerCore, enemyCore);
        battleManager.SetBattleManagerInfo(planetManager.GetPlanet((int)PlanetID.PLANET_1).transform, m_Canvas, cameraCtrl, m_SkillBattleManager);

        // スキルマネージャー初期化
        m_SkillBattleManager.SetPlayerObject(player);

        // ピラージェネレター初期化
        pillerGenerator.SetPlanet(planetManager);

        // カメラ初期化
        cameraCtrl.SetMainPlayer(player.transform);
        cameraCtrl.SetBattleManager(battleManager);
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
	}
}
