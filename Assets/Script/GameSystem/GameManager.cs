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
        m_Player = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Player"));
        m_Player.name = "Player";
        m_Player.transform.SetParent(transform, true);

        // エネミー
        m_Enemy = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Enemy"));
        m_Enemy.name = "Enemy";
        m_Enemy.transform.SetParent(transform, true);

        // ピラー
        transform.FindChild("PillerContents").gameObject.SetActive(true);

        // カメラ
        GameData.GetCamera().GetComponent<CameraController>().enabled = true;

        // バトルマネージャー
        if (m_Enemy != null)
        {
            transform.FindChild("BattleManager").gameObject.SetActive(true);
        }
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {

        // プレイヤー更新
        PlayerUpdate();

        // エネミー更新
        //EnemyUpdate();
	}

    void PlayerUpdate()
    {
        // プレイヤー
        PlayerController playerController = m_Player.GetComponent<PlayerController>();
        TouchType touchType = TouchController.GetTouchType();
        playerController.m_TouchType = touchType;
        if (TouchController.IsPlanetTouch())
        {
            playerController.m_CurrentVelocity = -TouchController.GetVelocity(touchType);
        }
        else
        {
            playerController.m_CurrentVelocity = Vector3.zero;
        }
        playerController.m_CurrentRotate = Camera.main.transform.rotation;

        if (playerController.state.current == PlayerController.State.FLICK_MOVE)
        {
            // ビタ止め移行モーション変化
            {
                if (TouchController.IsPlanetTouch())
                {
                    if (TouchController.GetTouchTimer() > playerController.MOVE_TOUCH_STOP_TIME * 0.5f){
                         playerController.m_AnmController.ChangeAnimationLoop("Idle", 0.1f, 0);
                    }
                    else{
                         playerController.m_AnmController.ChangeAnimationLoop("Run", 0.1f, 0);
                    }

                    if (TouchController.GetTouchTimer() > playerController.MOVE_TOUCH_STOP_TIME)
                    {
                        playerController.m_TouchStopFlag = true;
                    }
                }
            }
        }
    }

    void EnemyUpdate()
    {
        PlayerController enemyController = m_Enemy.GetComponent<PlayerController>();
        TouchType touchType = TouchType.Frick;
        enemyController.m_TouchType = touchType;
        enemyController.m_CurrentVelocity = Vector3.forward * 0.1f;
        enemyController.m_CurrentRotate = enemyController.transform.rotation;
    }
}
