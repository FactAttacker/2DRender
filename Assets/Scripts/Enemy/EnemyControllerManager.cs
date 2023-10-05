using UnityEngine;

public class EnemyControllerManager : MonoBehaviourSingleton<PlayerManager>
{
    [System.Serializable]
    class Stat
    {
        public float attack = 0f;
        public float power = 0f;

        public float searchXDir = 3f;
        public float searchYDir = 3f;

        public float attackXDir = 1f;
        public float attackYDir = 1f;

        public float moveSpeed = 1f;
    }
    [SerializeField]
    Stat stat;

    [System.Serializable]
    class Status
    {
        public enum FSM
        {
            Idle = 0,
            Follow = 1,
            Attack = 2,
            Die,
        }
        public FSM fsm;
        public FSM prevFsm;

        public enum SearchType
        {
            없음,
            플레이어_따라감,
            위치고정,
        }
        public enum AttackType
        {
            없음,
            원거리,
            단거리,
        }
        public float facingDirection = 0;
        public bool defaultDirRight = false;
        public SearchType attackType;
    }
    [SerializeField]
    Status status;

    Transform playerPos;

    [SerializeField]
    Animator anim;

    void Reset()
    {
        TryGetComponent(out anim);
    }

    void Start()
    {
        playerPos = PlayerControlloerManager.Instance.transform;
        Init();
    }

    void Init()
    {
        status.fsm = Status.FSM.Idle;
    }

    bool SearchPlayer()
    {
        if (Mathf.Abs(transform.position.x - playerPos.position.x) < stat.searchXDir
         && Mathf.Abs(transform.position.y - playerPos.position.y) < stat.searchYDir)
        {
            if (status.fsm != Status.FSM.Attack)
            {
                if (playerPos.position.x > transform.position.x)
                {
                    Debug.Log("오른");
                    if (status.facingDirection != 1) status.facingDirection = 1;
                    if (status.defaultDirRight)
                    {
                        if (transform.localRotation != Quaternion.identity) transform.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        if (transform.localRotation.y != 180) transform.localRotation = Quaternion.Euler(0, 180, 0);
                    }
                }
                else
                {
                    Debug.Log("왼");
                    if (status.facingDirection != -1) status.facingDirection = -1;
                    if (status.defaultDirRight)
                    {
                        if (transform.localRotation.y != 180) transform.localRotation = Quaternion.Euler(0, 180, 0);
                    }
                    else
                    {
                        if (transform.localRotation != Quaternion.identity) transform.localRotation = Quaternion.identity;
                    }
                }
            }

            if (Mathf.Abs(transform.position.x - playerPos.position.x) < stat.attackXDir)
            {
                status.fsm = Status.FSM.Attack;
                anim.SetInteger("State",2);
            }
            else
            {
                status.fsm = Status.FSM.Follow;
                transform.Translate(status.defaultDirRight ? Vector2.right : Vector2.left * stat.moveSpeed * Time.deltaTime);
                anim.SetInteger("State", 1);
            }
        }
        else
        {
            status.fsm = Status.FSM.Idle;
            anim.SetInteger("State", 0);
        }
        return false;
    }

    void Move()
    {
        if (anim.GetInteger("State") != (int)status.fsm) anim.SetBool("ChangeState", true);
        else anim.SetBool("ChangeState", false);

        switch (status.fsm)
        {
            case Status.FSM.Idle:
                break;
            case Status.FSM.Attack:
                break;
            case Status.FSM.Follow:
                // 플레이어에게 이동 
                transform.position += Vector3.right * status.facingDirection * Time.deltaTime * stat.moveSpeed;
                break;
            case Status.FSM.Die:
                break;
            default:
                break;
        }
        status.prevFsm = status.fsm;
    }

    void Update()
    {
        SearchPlayer();
        Move();
    }
}
