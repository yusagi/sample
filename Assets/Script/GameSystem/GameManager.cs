using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameObject m_Planet { get; set; }
    public static GameObject m_Player { get; set; }
    public static GameObject m_Enemy { get; set; }

    private void Awake()
    {
        // インプット
        InputManager.InstanceCheck();

        // プラネット
        m_Planet = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Planet"));
        m_Planet.transform.SetParent(transform, true);

        // プレイヤー
        m_Player = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Charactor"));
        m_Player.name = "Player";
        m_Player.layer = (int)Layer.PLAYER;
        m_Player.transform.SetParent(transform, true);
        m_Player.AddComponent<BattleAreaLineRenderer>().arc = GameData.GetBattleManager().BATTLE_START_DISTANCE;
        foreach(var data in SkillDataBase.PLAYER_DATAS)
        {
            m_Player.GetComponent<GrgrCharCtrl>().skillManager.AddSkill(data.Value);
        }

        // エネミー
        m_Enemy = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Charactor"));
        m_Enemy.name = "Enemy";
        m_Enemy.layer = (int)Layer.ENEMY;
        m_Enemy.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.right) * Quaternion.identity;
        m_Enemy.transform.SetParent(transform, true);
        foreach (var data in SkillDataBase.ENEMY_DATAS)
        {
            m_Enemy.GetComponent<GrgrCharCtrl>().skillManager.AddSkill(data.Value);
        }

        // ピラー
        GameData.GetPillerGenerator().GetComponent<PillerGenerator>().enabled = true;

        // バトルマネージャー
        if (m_Enemy != null)
        {
            GameData.GetBattleManager().enabled = true;
        }
        // カメラ
        GameData.GetCamera().GetComponent<CameraController>().enabled = true;
        
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {

        // プレイヤー更新
        PlayerUpdate();

        // エネミー更新
        EnemyUpdate();
	}

    void PlayerUpdate()
    {
        // プレイヤー
        GrgrCharCtrl GrgrCharCtrl = m_Player.GetComponent<GrgrCharCtrl>();
        TouchType touchType = TouchController.GetTouchType();
        if (touchType == TouchType.Drag){
            touchType = (TouchController.IsFlickSuccess()) ? TouchType.None : TouchType.Drag;
        }
        if (TouchController.IsPlanetTouch())
        {
            if (touchType == TouchType.Drag){
                GrgrCharCtrl.m_CurrentVelocity = -TouchController.GetDragVelocity();
            }
            else if (touchType == TouchType.Frick){
                GrgrCharCtrl.m_CurrentVelocity = -TouchController.GetFlickVelocity();
            }
            GrgrCharCtrl.m_TouchType = touchType;
        }
        else
        {
            GrgrCharCtrl.m_CurrentVelocity = Vector3.zero;
            GrgrCharCtrl.m_TouchType = TouchType.None;
        }
        GrgrCharCtrl.m_CurrentRotate = Camera.main.transform.rotation;

        if (GrgrCharCtrl.state.current == GrgrCharCtrl.State.FLICK_MOVE)
        {
            // ビタ止め移行モーション変化
            {
                if (TouchController.IsPlanetTouch())
                {
                    if (TouchController.GetTouchTimer() > GrgrCharCtrl.MOVE_TOUCH_STOP_TIME * 0.5f){
                        GrgrCharCtrl.m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
                    }
                    else{
                        GrgrCharCtrl.m_AnmMgr.ChangeAnimationLoopInFixedTime("Run");
                    }

                    if (TouchController.GetTouchTimer() > GrgrCharCtrl.MOVE_TOUCH_STOP_TIME)
                    {
                        GrgrCharCtrl.m_TouchStopFlag = true;
                    }
                }
            }
        }
    }

    private float FRICK_TIMER = 1.0f;
    private int FRICK_COUNT = 10;
    private int m_FrickCount = 0;
    private float m_FrickTimer = 0.0f;

    void EnemyUpdate()
    {
        GrgrCharCtrl enemyController = m_Enemy.GetComponent<GrgrCharCtrl>();
        enemyController.m_TouchType = TouchType.None;

        if (m_FrickTimer > 0)
        {
            m_FrickTimer -= Time.deltaTime;
            enemyController.m_CurrentVelocity = Vector3.zero;
        }
        else
        {
            if (m_FrickCount == 0)
            {
                m_FrickCount = FRICK_COUNT;
                if (enemyController.state.current == GrgrCharCtrl.State.FLICK_MOVE){
                //enemyController.transform.rotation = Quaternion.AngleAxis(Random.Range(0.0f, 359.0f), enemyController.transform.up) * enemyController.transform.rotation;
                }
            }

            enemyController.m_CurrentVelocity = Vector3.forward * 1080.0f;
            enemyController.m_CurrentRotate = enemyController.transform.rotation;
            m_FrickCount -= 1;
            m_FrickTimer = FRICK_TIMER;
            enemyController.m_TouchType = TouchType.Frick;
        }
    }
}
