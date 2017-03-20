using UnityEngine;
using System.Collections;

/// <summary>
/// The singleton try to find the object in the scene.
/// If the object is not find, it creates the object.
/// </summary>
/// <typeparam name="T">Your class name</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    /// <summary>
    /// Not necessary, unless you have a specific object that works with the Singleton
    /// </summary>
    public static string PrefabName;

    /// <summary>
    /// Instance of the singleton object.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    if(string.IsNullOrEmpty(PrefabName))
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        _instance.name = typeof(T).Name;
                    }
                    else
                    {
                        T singletonPrefab = Resources.Load<T>("Manager/" + PrefabName);
                        _instance = Instantiate(singletonPrefab);

                    }
                    
                }
            }
            return _instance;
        }
    }

    private static T _instance;

    public static bool IsInstantiated()
    {
        if(_instance==null)
        {
            _instance = FindObjectOfType<T>();
        }
       
        return _instance != null;
    }

    public virtual void OnDestroy()
    {
        _instance = null;
    }

    public static void DestroySingleton()
    {
        if(_instance!=null)
        {
            Destroy(_instance.gameObject);
        }
    }
}
