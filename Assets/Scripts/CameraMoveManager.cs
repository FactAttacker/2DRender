using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveManager : MonoBehaviour
{
    Vector3 cameraPosition = Vector3.forward * -10;
    public float cameraMoveSpeed = 0.5f;

    [SerializeField]
    Transform cameraPos;

    void Reset()
    {
        cameraPos = GameObject.Find("CameraPos").transform;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, cameraPos.position + cameraPosition,
                                  Time.deltaTime * cameraMoveSpeed);

        //Vector3 Vector3.Lerp(PlayerManager.Instance.transform.position, cameraPosition, float t);
        //transform.position = PlayerManager.Instance.transform.position + cameraPosition;
    }
}
