using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStop : MonoBehaviour {

	public float HIT_STOP_TIME = 1.0f;
	public float BLOWOFF_STOP_TIME = 1.0f;
	public float BLOWOFF_DISTANCE = 50.0f;

	public bool isCameraAction = true;

	public static bool isStart = false;
	private static bool isEnd = false;
	private static float phaseUpTime = 0.0f;

	private bool isHitStop = true;
	private bool isEnemyStart = false;

	// カメラ用
	IEnumerator<Quaternion> rotCameLerp;
	IEnumerator<Vector3> posCameLerp;

	// エネミー用
	IEnumerator<Vector3> posEneLerp;

	void Update(){
		if (!isStart)
			return;

		float nowTime = Time.realtimeSinceStartup;
			
		// 初期化処理
		if (phaseUpTime == 0.0f){
			phaseUpTime = nowTime;
			Initialize();
		}


		// 開始前処理
		if (isHitStop){
			if (nowTime - phaseUpTime < HIT_STOP_TIME){
				return;
			}
			else
				isHitStop = false;
		}

		Transform camera = GameData.GetCamera();
		Transform player = GameData.GetPlayer();
		Transform enemy = GameData.GetEnemy();

		// カメラの処理
		if (isCameraAction){
			rotCameLerp.MoveNext();
			camera.rotation = rotCameLerp.Current;

			if (!posCameLerp.MoveNext())
				if (!isEnemyStart){
					isEnemyStart = true;
					phaseUpTime = nowTime = Time.realtimeSinceStartup;
				}
			camera.position = posCameLerp.Current;
		}

		// エネミーの処理
		if (isCameraAction){
			if (isEnemyStart){
				if (nowTime == phaseUpTime){
					enemy.GetComponent<Animator>().speed = 1.0f;
					enemy.GetComponent<Animator>().SetBool("Run", false);
					enemy.GetComponent<Animator>().SetTrigger("DamageDown");
					posEneLerp = UtilityMath.VLerp(enemy.position, camera.position + camera.forward * 3.5f + camera.up * -0f, 1.0f, UtilityMath.EaseType.IN_SINE);
					enemy.GetComponent<Animator>().speed = 0.35f;
				}

				if (!posEneLerp.MoveNext()){
						if (!isEnd){
							isEnd = true;
							phaseUpTime = Time.realtimeSinceStartup;
						}
				}
				enemy.position = posEneLerp.Current;
			}
		}

		// プレイヤーの処理
		

		// 終了処理
		{
			if (isEnd && nowTime - phaseUpTime > BLOWOFF_STOP_TIME  || !isCameraAction){
				isStart = false;
				isEnd = false;
				phaseUpTime = 0.0f;

				isHitStop = false;
				isEnemyStart = false;

				camera.GetComponent<CameraController3>().phase.Change(CameraController3.CameraPhase.HITSTOP_END);
				player.GetComponent<PlayerControllerBase>().state = PlayerControllerBase.State.Move;
				player.GetComponent<Animator>().speed = 1.0f;
				enemy.GetComponent<EnemyControllerBase>().state = EnemyControllerBase.State.Ascension;
				enemy.GetComponent<Animator>().speed = 1.0f;

				if (!isCameraAction){
					enemy.GetComponent<Animator>().SetBool("Run", false);
					enemy.GetComponent<Animator>().SetTrigger("DamageDown");
					enemy.GetComponent<Rigidbody_grgr>().isMove = true;
					camera.GetComponent<CameraController3>().phase.Change(CameraController3.CameraPhase.NONE);
				}
			}
		}
	}

	void Initialize(){
		Transform camera = GameData.GetCamera();
		Transform player = GameData.GetPlayer();
		Transform enemy = GameData.GetEnemy();

		isHitStop = true;

		// カメラ初期化
		{
			Vector3 front = enemy.GetComponent<Rigidbody_grgr>().velocity.normalized;
			Vector3 up = Vector3.ProjectOnPlane(enemy.up, front);
			Quaternion rotate = Quaternion.AngleAxis(180, up) * Quaternion.LookRotation(front, up);
		
			rotCameLerp = UtilityMath.QLerp(camera.rotation, rotate, 1.0f);
			posCameLerp = UtilityMath.VLerp(camera.position, enemy.position + front*BLOWOFF_DISTANCE, 1.0f);

			camera.GetComponent<CameraController3>().phase.Change(CameraController3.CameraPhase.HITSTOP);
		}

		// プレイヤー初期化
		{
			player.GetComponent<Animator>().speed = 0.0f;
			player.GetComponent<Rigidbody_grgr>().friction = 0.0f;
			player.GetComponent<PlayerControllerBase>().state = PlayerControllerBase.State.HitStop;
		}

		// エネミー初期化
		{
			enemy.GetComponent<PlanetWalk>().enabled = false;
			enemy.GetComponent<Animator>().speed = 0.0f;
			enemy.GetComponent<EnemyControllerBase>().state = EnemyControllerBase.State.HitStop;
		}
	}
}