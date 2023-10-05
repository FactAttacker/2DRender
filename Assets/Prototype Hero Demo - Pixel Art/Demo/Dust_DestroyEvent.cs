using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dust_DestroyEvent : MonoBehaviour
{
    public void destroyEvent()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
        transform.position = Vector2.zero;
    }
}
