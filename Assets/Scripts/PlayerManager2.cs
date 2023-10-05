using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerManager2 : MonoBehaviourSingleton<PlayerManager2>
{
    [System.Serializable]
    class Stat
    {
        public float walkSpeed = 5f;
        public float attCoolTime = 5f;
    }
    [SerializeField]
    Stat stat;

    [System.Serializable]
    class MoveInfo
    {
        public float h;
        public float v;
        public bool isHorizonMove;
        public Vector3 direction;
    }
    [SerializeField]
    MoveInfo moveInfo;

    [System.Serializable]
    class HandInfo
    {
        public Transform hLeft;
        public Transform hRight;
        public Transform vTop;
        public Transform vDown;
        public GameObject tool;
    }
    [SerializeField]
    HandInfo handInfo;

    [SerializeField]
    Rigidbody2D rigid;

    [SerializeField]
    Animator anim;

    public enum DirType
    {
        좌,
        우,
        상,
        하
    }
    public DirType currDirType;

    public enum FSMType
    {
        IDLE,
        WALK,
        ATTAK
    }
    public FSMType currFSMType;

    class BezierCurves
    {
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p4;
        public Vector3 p3;
        public float arcValue = 0f;
    }

    Dictionary<DirType, BezierCurves> attDic = new Dictionary<DirType, BezierCurves>();

    GameObject scanObject;
    bool isMove = true;
    Vector3 change;

    void Reset()
    {
        stat.walkSpeed = 10;
        TryGetComponent(out rigid);
        TryGetComponent(out anim);
        Transform handTool = transform.Find("HandTool");
        handInfo.hLeft = handTool.Find("h");
        handInfo.hRight = handInfo.hLeft.Find("EndPos");

        handInfo.vTop = handTool.Find("v");
        handInfo.vDown = handInfo.vTop.Find("EndPos");

        handInfo.tool = handTool.Find("Tool").gameObject;
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        currFSMType = FSMType.WALK;
        currDirType = DirType.하;
    }

    private void FixedUpdate()
    {
        currFSMType = FSMType.IDLE;

        change = Vector3.zero;
        moveInfo.h = Input.GetAxisRaw("Horizontal");
        moveInfo.v = Input.GetAxisRaw("Vertical");

        Vector3 moveVec = (Vector3.right * moveInfo.h) + (Vector3.up * moveInfo.v);
        moveVec.Normalize();
        change = moveVec;

        if (moveInfo.direction != null)
        {
            // Ray
            Debug.DrawRay(rigid.position, moveInfo.direction * 0.7f, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, moveInfo.direction, 0.7f, LayerMask.GetMask("Object"));

            if (rayHit.collider != null) scanObject = rayHit.transform.gameObject;
            else scanObject = null;
        }

        if (Input.GetButtonDown("Jump") && currFSMType != FSMType.ATTAK)
        {
            Attack();
        }
        else
        {
            if (change == Vector3.zero) Idle();
            else UpdateAnimationAndMove();
        }

       
    }

    void Idle()
    {
        if (currFSMType != FSMType.IDLE) currFSMType = FSMType.IDLE;
        if (anim.GetBool("isWalk")) anim.SetBool("isWalk", false);
        if (anim.GetBool("isAttack")) anim.SetBool("isAttack", false);
        isMove = true;
    }
    
    void Attack()
    {
        currFSMType = FSMType.ATTAK;
        isMove = false;
        if (!anim.GetBool("isAttack")) anim.SetBool("isAttack", true);
        if (anim.GetBool("isWalk")) anim.SetBool("isWalk", false);
        StartCoroutine(CoAttack());
    }
    IEnumerator CoAttack()
    {
        yield return new WaitForSeconds(stat.attCoolTime);
        Idle();
    }

    public void Look(DirType type)
    {
        if (type != currDirType) currDirType = type;
        switch (type)
        {
            case DirType.좌:
                if (moveInfo.direction != Vector3.left) moveInfo.direction = Vector3.left;
                break;
            case DirType.우:
                if (moveInfo.direction != Vector3.right) moveInfo.direction = Vector3.right;
                break;
            case DirType.상:
                if (moveInfo.direction != Vector3.up) moveInfo.direction = Vector3.up;
                break;
            case DirType.하:
                if (moveInfo.direction != Vector3.down) moveInfo.direction = Vector3.down;
                break;
        }
    }

    void UpdateAnimationAndMove()
    {
        if (currFSMType != FSMType.WALK) currFSMType = FSMType.WALK;
        if (!anim.GetBool("isWalk")) anim.SetBool("isWalk", true);
        if (anim.GetBool("isAttack")) anim.SetBool("isAttack", false);
        MoveCharacter();
        anim.SetFloat("h", change.x);
        anim.SetFloat("v", change.y);
    }

    void MoveCharacter()
    {
        if (isMove) rigid.MovePosition(transform.position + change * (stat.walkSpeed * Time.deltaTime));
    }
}
