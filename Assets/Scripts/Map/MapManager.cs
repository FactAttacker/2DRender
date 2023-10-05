using UnityEngine;

// [ExecuteInEditMode]
public class MapManager : MonoBehaviour
{
    void Reset()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).position = Vector3.forward * (-0.1f * i);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
