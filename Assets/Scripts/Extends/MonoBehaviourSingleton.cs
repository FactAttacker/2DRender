using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject coreGameObject = new GameObject(typeof(T).Name);
                instance = coreGameObject.AddComponent<T>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null) instance = GetComponent<T>();
        else
        {
            DestroyImmediate(this);
        }
        
    }

}
