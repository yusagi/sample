using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationType
{
    ATTACK = 0,                     // 攻撃
    ATTACK_REPELLED,                // 攻撃はじかれ
    GUARD_BREAK_ATTACK_REPELLED,    // ガード崩壊攻撃はじかれ
    COUNTER_ATTACK,                 // カウンター攻撃
    COUNTER_MATCH,                  // カウンタースカし 
    GUARD,                          // ガード
    GUARD_BREAK,                    // ガード崩壊
    NONE,                           // なし
}

// アニメーションデータ
public class AnmData
{

    public AnmData(string name, int layer, float durationTime, int endFrame, float offsetTime, float slowEndFrame = 0, float slowStartFrame = 0)
    { 
        _name = name;
        _layer = layer;
        _durationTime = durationTime;
        _endFrame = endFrame;
        _offsetTime = offsetTime;
       _slowEndFrame = slowEndFrame;
       _slowStartFrame = slowStartFrame;
    }

    public string _name;        // 名前
    public int _layer;          // アニメーターレイヤー
    public float _durationTime; // 補間時間(秒)
    public int _endFrame;       // 終了フレーム
    public float _offsetTime;   // 次のアニメーションの再生開始時間(秒)
    
    public float _slowEndFrame;     // スロー終了フレーム
    public float _slowStartFrame;   // スロー開始フレーム
}

public class AnimationDataBase
{
    // 右からname, layer, duration, endNormalizeTime, fixedTimeの順
    public static Dictionary<string, Dictionary<string, AnmData>> TRANS_DATAS = new Dictionary<string, Dictionary<string, AnmData>>()
    {
        // 再生するアニメーションのキー

        // 待機(ループ)
        {"Idle", new Dictionary<string, AnmData>
            {
                // 前または次のアニメーションのキー
                // durationTimeとfixedTimeは前のアニメーションキーから参照
                // endTimeは次に再生するアニメーションキーから参照
                
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   0, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   0, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   0, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   0, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   0, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   0, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   0, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   0, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   0, 0) },
                { "SAMK",       new AnmData("SAMK",             0, 0.25f,   0, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   0, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   0, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   0, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   0, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // 走り(ループ)
        {"Run", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   0, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   0, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   0, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   0, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   0, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   0, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   0, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   0, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   0, 0) },
                { "SAMK",       new AnmData("SAMK",             0, 0.25f,   0, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   0, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   0, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   0, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   0, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // ジャブ（攻撃）
        {"Jab", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.05f,    14, 0, 0, 25) },
                { "Run",        new AnmData("Run",              0, 0.05f,    14, 0, 0, 25) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.05f,    14, 0, 0, 25) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.05f,    14, 0, 0, 25) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.05f,    28, 0, 0, 25) },
                { "Hikick",     new AnmData("Hikick",           0, 0.05f,    14, 0, 0, 25) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.05f,    14, 0, 0, 25) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.05f,    14, 0, 0, 25) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.05f,    14, 0, 0, 25) },
                { "SAMK",       new AnmData("SAMK",             0, 0.05f,    14, 0, 0, 25) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.05f,    14, 0, 0, 25) },
                { "Guard",      new AnmData("Guard",            0, 0.05f,    14, 0, 0, 25) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.05f,   14, 0, 0, 25) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.05f,   14, 0, 0, 25) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0, 0, 25)},
            }
        },
        // ハイキック（攻撃）
        {"Hikick", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.1f,   29, 0, 5, 17) },
                { "Run",        new AnmData("Run",              0, 0.1f,   29, 0, 5, 17) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.1f,   29, 0, 5, 17) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.1f,   29, 0, 5, 17) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.1f,   29, 0, 5, 17) },
                { "Hikick",     new AnmData("Hikick",           0, 0.1f,   29, 0, 5, 17) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.1f,   29, 0, 5, 17) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.1f,   29, 0, 5, 17) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.1f,   29, 0, 5, 17) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.1f,   29, 0, 5, 17) },
                { "Guard",      new AnmData("Guard",            0, 0.1f,   29, 0, 5, 17) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.1f,   29, 0, 5, 17) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.1f,   29, 0, 5, 17) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0, 10, 17)},
            }
        },
        // スピンキック（攻撃）
        {"Spinkick", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.05f,    31, 0, 7, 20) },
                { "Run",        new AnmData("Run",              0, 0.05f,    31, 0, 7, 20) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.05f,    31, 0, 7, 20) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.05f,    31, 0, 7, 20) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.05f,    31, 0, 7, 20) },
                { "Hikick",     new AnmData("Hikick",           0, 0.05f,    31, 0, 7, 20) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.05f,    31, 0, 7, 20) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.05f,    31, 0, 7, 20) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.05f,    31, 0, 7, 20) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.05f,    31, 0, 7, 20) },
                { "Guard",      new AnmData("Guard",            0, 0.05f,    31, 0, 7, 20) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.05f,    31, 0, 7, 20) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.05f,    31, 0, 7, 20) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0, 7, 20)},
            }
        },
        // しゃがみ（カウンター）
        {"Land", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   51, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   51, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   14, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   51, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   51, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   51, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   51, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   26, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   13, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   51, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   51, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   51, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   51, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // 昇竜拳（カウンター攻撃）
        {"RISING_P", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   64, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   64, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   64, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   64, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   64, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   64, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   64, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   64, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   64, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   64, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   64, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   64, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   64, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // 斬り上げ(カウンター攻撃)
        {"Counter_Slash", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   94, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   94, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   94, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   94, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   94, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   94, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   94, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   94, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   94, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   94, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   94, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   94, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   94, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // サマーソルト（カウンター攻撃）
        {"SAMK", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   35, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   35, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   35, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   35, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   35, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   35, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   35, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   35, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   35, 0) },
                { "SAMK",       new AnmData("SAMK",             0, 0.25f,   35, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   35, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   35, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   35, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   35, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // ダメージ(吹っ飛び)
        {"Damage_Down", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   44, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   44, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   44, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   44, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   44, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   44, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   44, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   44, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   44, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   44, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   44, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   44, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   44, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // ダメージ(軽め)
        {"DAMAGED00", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.1f,   33, 0) },
                { "Run",        new AnmData("Run",              0, 0.1f,   33, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.1f,   33, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.1f,   33, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.1f,   33, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.1f,   33, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.1f,   33, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.1f,   33, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.1f,   33, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.1f,   33, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.1f,   33, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.1f,   33, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.1f,   33, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // 防御
        {"Guard", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   54, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   54, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   54, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   54, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   54, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   54, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   54, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   54, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   54, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   54, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   54, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   54, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   54, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // 防御崩壊
        {"GuardBreak", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   39, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   39, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   39, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   39, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   39, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   39, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   39, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   39, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   39, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   39, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   39, 0) },
                
                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   39, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   39, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // スピンキック崩壊
        {"SpinkickBreak", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   42, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   42, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   42, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   42, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   42, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   42, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   42, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   42, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   42, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   42, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   42, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   42, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   42, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        }
    };
}
