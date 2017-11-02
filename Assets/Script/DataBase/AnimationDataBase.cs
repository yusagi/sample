using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType
{
    ATTACK = 0,                     // 攻撃
    GUARD,                          // ガード
    DAMAGE,                         // ダメージ
    NONE,                           // なし
}

// アニメーションデータ
public class AnmData
{

    public AnmData(string name, int layer, float durationTime, float offsetTime, int endFrame, float slowEndFrame = 0, float slowStartFrame = 0)
    { 
        _name = name;
        _layer = layer;
        _durationTime = durationTime;
        _offsetTime = offsetTime;
        _endFrame = endFrame;
       _slowEndFrame = slowEndFrame;
       _slowStartFrame = slowStartFrame;
    }

    public string _name;        // 名前
    public int _layer;          // アニメーターレイヤー
    public float _durationTime; // 補間時間(秒)
    public float _offsetTime;   // 次のアニメーションの再生開始時間(秒)
    public int _endFrame;       // 終了フレーム
    
    public float _slowEndFrame;     // スロー終了フレーム
    public float _slowStartFrame;   // スロー開始フレーム
}

public class AnimationDataBase
{
    public static Dictionary<string, AnmData> ANM_DATAS = new Dictionary<string, AnmData>(){
        // 待機
        {"Idle", new AnmData("Idle", 0, 0.25f, 0, 0, 0, 0)},
        // 走り
        {"Run", new AnmData("Run", 0, 0.25f, 0, 0, 0, 0)},
        
        // ジャブ
        {"Jab", new AnmData("Jab", 0, 0.08f, 0, 17, 0, 10)},
        // ハイキック
        {"Hikick", new AnmData("Hikick", 0, 0.15f, 0, 33, 8, 15)},
        // スピンキック
        {"Spinkick", new AnmData("Spinkick", 0, 0.1f, 0, 27, 9, 18)},

        // 防御
        {"Guard", new AnmData("Guard", 0, 0.1f, 0, 0, 0, 0)},

        // ダメージ吹っ飛び
        {"Damage_Down", new AnmData("Damage_Down", 0, 0.25f, 0, 45, 0, 0)},
        // ダメージ受け
        {"DAMAGED00", new AnmData("DAMAGED00", 0, 0.1f, 0, 34, 0, 0)},
    };
}
