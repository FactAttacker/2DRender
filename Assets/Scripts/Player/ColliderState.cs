using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderState : MonoBehaviourSingleton<ColliderState>
{
    public TileType type;

    [System.Serializable]
    public class CollAction
    {
        public delegate void Enter(Collider2D collision);
        public Enter enter;

        public delegate void Stay(Collider2D collision);
        public Stay stay;

        public delegate void Exit(Collider2D collision);
        public Exit exit;
    }
    public CollAction collAction;

    private void Awake()
    {
        collAction.enter = (col) => { };
        collAction.stay = (col) => { };
        collAction.exit = (col) => { };
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        collAction.enter(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        collAction.stay(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        collAction.exit(collision);
    }
    
}
