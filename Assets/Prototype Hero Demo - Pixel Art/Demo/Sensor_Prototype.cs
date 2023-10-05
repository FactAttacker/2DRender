using UnityEngine;
using System.Collections;

public class Sensor_Prototype : MonoBehaviour {

    public int m_ColCount = 0;

    float disableTimer;
    public float DisableTimer
    {
        get
        {
            return disableTimer;
        }
        set
        {
            if (value < -99f) value = -99f;
            disableTimer = value;
        }
    }

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        if (disableTimer > 0)
            return false;
        return m_ColCount > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        m_ColCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        m_ColCount--;
    }

    void Update()
    {
        disableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        disableTimer = duration;
    }
}
