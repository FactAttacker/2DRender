using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [System.Serializable]
    class Info
    {
        public float damage = 0.0f;
    }
    [SerializeField]
    Info info;

    void Start()
    {
        info.damage = PlayerControlloerManager.Instance.statInfo.damage.power;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
            
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        
    }
}
