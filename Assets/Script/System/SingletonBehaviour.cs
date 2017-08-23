using UnityEngine;
using System.Collections;

public abstract class SingletonBefaviour<T> : MonoBehaviour where T : MonoBehaviour
{

    protected virtual void Awake()
    {
        OnCreateSingleton();
    }

    protected virtual void OnDestroy()
    {
        OnDestroySingleton();
    }

    protected virtual void OnCreateSingleton()
    {

    }

    protected virtual void OnDestroySingleton()
    {

    }

    public static T instance
    {
        get
        {
            return InstanceCheck();
        }
    }

    public static T InstanceCheck()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<T>();
            if (_instance == null)
            {
                GameObject obj = new GameObject(typeof(T).ToString());
                _instance = obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }
        }
        return _instance;
    }

    private static T _instance;
}

