using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// アニメーションデータ
public class AnmData
{

    public AnmData(string name, int layer, float durationTime, float endNormalizeTime, float fixedTime)
    {
        _name = name;
        _layer = layer;
        _durationTime = durationTime;
        _endNormalizeTime = endNormalizeTime;
        _fixedTime = fixedTime;

    }

    public string _name;
    public int _layer;
    public float _durationTime;
    public float _endNormalizeTime;
    public float _fixedTime;
}

public class AnimationDataBase
{
    // 右からname, layer, duration, endNormalizeTime, fixedTimeの順
    public static Dictionary<string, Dictionary<string, AnmData>> TRANS_DATAS = new Dictionary<string, Dictionary<string, AnmData>>()
    {
        // 再生するアニメーションのキー
        {"Idle", new Dictionary<string, AnmData>
            {
                // 前または次のアニメーションのキー
                // durationTimeとfixedTimeは前のアニメーションキーから参照
                // endTimeは次に再生するアニメーションキーから参照
                //{ "Idle",       new AnmData("Idle", 0, 0.1f, 0.483f, 0) },
                { "Run",        new AnmData("Run",          0, 0.25f,   0, 0) },

                { "Jab",        new AnmData("Jab",          0, 0.25f,   0, 0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.25f,   0, 0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.25f,   0, 0) },
                { "Land",       new AnmData("Land",         0, 0,       0, 0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.25f,   0, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f, 0, 0)},

                { "DamageDown", new AnmData("DamageDown",       0, 0.25f,   0, 0) },
                { "DAMAGED00",     new AnmData("DAMAGED00",     0, 0.25f,   0, 0) },
            }
        },
        {"Run", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle", 0, 0.25f, 0, 0) },
                //{ "Run",        new AnmData("Run", 0, 0.05f, 0.483f, 0) },

                { "Jab",        new AnmData("Jab",          0, 0.25f,   0, 0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.25f,   0, 0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.25f,   0, 0) },
                { "Land",       new AnmData("Land",         0, 0,       0, 0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.25f,   0, 0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f, 0, 0)},

                { "DamageDown", new AnmData("DamageDown",   0, 0,       0, 0) },
                { "DAMAGED00",     new AnmData("DAMAGED00",       0, 0.25f,   0, 0) },
            }
        },

        {"Jab", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0.1f,    0.7f,   0) },
                { "Run",        new AnmData("Run",          0, 0.05f,   0.483f, 0) },

                { "Jab",        new AnmData("Jab",          0, 0.1f,    1,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.1f,    0.483f, 0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.15f,   0.483f, 0) },
                { "Land",       new AnmData("Land",         0, 0.25f,   0.483f, 0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.1f,    0,      0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.1f, 0, 0)},

                { "DamageDown", new AnmData("DamageDown",   0, 0,       0.483f, 0) },
                { "DAMAGED00",     new AnmData("DAMAGED00",       0, 0.1f,    0.483f, 0) },
            }
        },
        {"Hikick", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0.25f,   0.66f,  0) },
                { "Run",        new AnmData("Run",          0, 0.1f,    0.66f,  0) },

                { "Jab",        new AnmData("Jab",          0, 0.25f,   1,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.1f,    0.66f,  0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.1f,    0.66f,  0) },
                { "Land",       new AnmData("Land",         0, 0,       0.66f,  0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.15f,   0,      0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f, 0, 0)},

                { "DamageDown", new AnmData("DamageDown",   0, 0,       0.66f,  0) },
                { "DAMAGED00",     new AnmData("DAMAGED00",       0, 0.1f,    0.66f,  0) },
            }
        },
        {"Spinkick", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0.1f,    0.68f,  0) },
                { "Run",        new AnmData("Run",          0, 0.1f,    0.68f,  0) },

                { "Jab",        new AnmData("Jab",          0, 0.25f,   1,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.1f,    0.68f,  0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.1f,    0.68f,  0) },
                { "Land",       new AnmData("Land",         0, 0,       0.69f,  0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.1f,    0,      0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.15f, 0, 0)},

                { "DamageDown", new AnmData("DamageDown",   0, 0.25f,   0.69f,  0) },
                { "DAMAGED00",     new AnmData("DAMAGED00",       0, 0.1f,    0.69f,  0) },
            }
        },
        {"Land", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0.25f,   0,      0) },
                { "Run",        new AnmData("Run",          0, 0.25f,   0,      0) },

                { "Jab",        new AnmData("Jab",          0, 0.25f,   0,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.25f,   0,      0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.25f,   0,      0) },
                //{ "Land",       new AnmData("Land",         0, 0,       0,      0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.25f,   0.4f,   0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f, 0.2f, 0)},

                //{ "DamageDown", new AnmData("DamageDown",   0, 0,       0,      0) },
                { "DAMAGED00",     new AnmData("DAMAGED00",       0, 0.25f,   0,      0) },
            }
        },
        {"RISING_P", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0,       1,      0) },
                { "Run",        new AnmData("Run",          0, 0,       1,      0) },

                { "Jab",        new AnmData("Jab",          0, 0,       1,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0,       1,      0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0,       1,      0) },
                { "Land",       new AnmData("Land",         0, 0.4f,    1,      0) },
                //{ "RISING_P",   new AnmData("RISING_P",     0, 0,       0,      0) },

                { "DamageDown", new AnmData("DamageDown",   0, 0,       0.77f,  0) },
                //{ "DAMAGED00",     new AnmData("DAMAGED00",       0, 0,       0,      0) },
            }
        },
        {"Counter_Slash", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0,       0.92f,      0) },
                { "Run",        new AnmData("Run",          0, 0,       0.92f,      0) },

                { "Jab",        new AnmData("Jab",          0, 0,       1,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0,       0.92f,      0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0,       0.92f,      0) },
                { "Land",       new AnmData("Land",         0, 1,    0.92f,      0.1f) },
                //{ "RISING_P",   new AnmData("RISING_P",     0, 0,       0,      0) },
                //{ "Counter_Slash", new AnmData("Counter_Slash", 0, 0, 0, 0)},

                { "DamageDown", new AnmData("DamageDown",   0, 0,       0.92f,  0) },
                //{ "DAMAGED00",     new AnmData("DAMAGED00",       0, 0,       0,      0) },
            }
        },

        {"DamageDown", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0.25f,   1.5f,   0) },
                { "Run",        new AnmData("Run",          0, 0.25f,   0,      0) },

                { "Jab",        new AnmData("Jab",          0, 0.25f,   0,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.25f,   0,      0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.25f,   0,      0) },
                //{ "Land",       new AnmData("Land",         0, 0,       0,      0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.25f,   0,      0) },
                { "Counter_Slash", new AnmData("Counter_Slash", 0, 0.25f, 0, 0)},

                //{ "DamageDown", new AnmData("DamageDown",   0, 0,       0,      0) },
                { "DAMAGED00",     new AnmData("DAMAGED00",       0, 0.25f,       0,      0) },
            }
        },

        {"DAMAGED00", new Dictionary<string, AnmData>
            {
                { "Idle",       new AnmData("Idle",         0, 0.25f,   0.78f,   0) },
                { "Run",        new AnmData("Run",          0, 0.25f,   0.78f,      0) },

                { "Jab",        new AnmData("Jab",          0, 0.25f,   0.78f,      0) },
                { "Hikick",     new AnmData("Hikick",       0, 0.25f,   0.78f,      0) },
                { "Spinkick",   new AnmData("Spinkick",     0, 0.25f,   0.78f,      0) },
                { "Land",       new AnmData("Land",         0, 0,       0.78f,      0) },
                { "RISING_P",   new AnmData("RISING_P",     0, 0.25f,   0,      0) },

                { "DamageDown", new AnmData("DamageDown",   0, 0,       0.1f,      0) },
                //{ "Damage",     new AnmData("Damage",       0, 0,       0,      0) },
            }
        },
    };

    // 再生アニメーションデータ取得
    //public static Dictionary<string, AnmData> GetPlayData(string key)
    //{

    //}

    // 遷移データ取得
    public static AnmData GetTransData(string playKey, string transKey)
    {
        if (string.IsNullOrEmpty(playKey) || string.IsNullOrEmpty(transKey))
        {
            return new AnmData("NONE", 0, 0, 1, 0);
        }

        if (!TRANS_DATAS.ContainsKey(playKey) || !TRANS_DATAS[playKey].ContainsKey(transKey))
        {
            Debug.LogError("CONTAINS");
            Debug.LogError("playKey " + playKey + " transKey " + transKey);
            return new AnmData(playKey, 0, 0, 1, 0);
        }

        return TRANS_DATAS[playKey][transKey];
    }
}
