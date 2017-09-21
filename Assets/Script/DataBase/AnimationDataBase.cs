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

    public AnmData(string name, int layer, float durationTime, int endFrame, float fixedTime)
    {
        _name = name;
        _layer = layer;
        _durationTime = durationTime;
        _endFrame = endFrame;
        _fixedTime = fixedTime;

    }

    public string _name;
    public int _layer;
    public float _durationTime;
    public int _endFrame;
    public float _fixedTime;
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
                { "Idle",       new AnmData("Idle",             0, 0.1f,    14, 0) },
                { "Run",        new AnmData("Run",              0, 0.1f,    14, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.1f,    14, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.1f,    14, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.1f,    28, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.1f,    14, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.1f,    14, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.1f,    14, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.1f,    14, 0) },
                { "SAMK",       new AnmData("SAMK",             0, 0.1f,    14, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.1f,    14, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.1f,    14, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.1f,   14, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.1f,   14, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // ハイキック（攻撃）
        {"Hikick", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.25f,   29, 0) },
                { "Run",        new AnmData("Run",              0, 0.25f,   29, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.25f,   29, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.25f,   29, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.25f,   29, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.25f,   29, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.25f,   29, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.25f,   29, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f,   29, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.25f,   29, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.25f,   29, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.25f,   29, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.25f,   29, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
            }
        },
        // スピンキック（攻撃）
        {"Spinkick", new Dictionary<string, AnmData>
            {
                // 状態系アニメーション
                { "Idle",       new AnmData("Idle",             0, 0.1f,    31, 0) },
                { "Run",        new AnmData("Run",              0, 0.1f,    31, 0) },
                { "Damage_Down", new AnmData("Damage_Down",     0, 0.1f,    31, 0) },
                { "DAMAGED00",  new AnmData("DAMAGED00",        0, 0.1f,    31, 0) },

                // 攻撃系アニメーション
                { "Jab",        new AnmData("Jab",              0, 0.1f,    31, 0) },
                { "Hikick",     new AnmData("Hikick",           0, 0.1f,    31, 0) },
                { "Spinkick",   new AnmData("Spinkick",         0, 0.1f,    31, 0) },
                { "RISING_P",   new AnmData("RISING_P",         0, 0.1f,    31, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.1f,    31, 0) },

                // 防御系アニメーション
                { "Land",       new AnmData("Land",             0, 0.1f,    31, 0) },
                { "Guard",      new AnmData("Guard",            0, 0.1f,    31, 0) },

                // 崩壊系アニメーション
                { "GuardBreak", new AnmData("GuardBreak",       0, 0.1f,    31, 0) },
                { "SpinkickBreak", new AnmData("SpinkickBreak", 0, 0.1f,    31, 0) },

                // バトルモード中共通終了時間
                { "AttackCommon", new AnmData("AttackCommon", 0, 0, 14, 0)},
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
