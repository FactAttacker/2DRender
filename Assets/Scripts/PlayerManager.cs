using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
{
    [System.Serializable]
    class Stat
    {
        public float walkSpeed = 5f;
        public float attSpeed = 5f;
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
        currFSMType = FSMType.IDLE;
        currDirType = DirType.하;
        SetBezierCurves();
    }

    void SetBezierCurves()
    {
        Vector3 pos1 = new();
        Vector3 pos2 = new();
        Vector3 pos3 = new();
        Vector3 pos4 = new();
        foreach (DirType type in Enum.GetValues(typeof(DirType)))
        {
            switch (type)
            {
                case DirType.좌:
                    pos1 = handInfo.hLeft.position;
                    pos2 = new Vector3(pos1.x, pos1.y - 0.6f, pos1.z);
                    pos3 = handInfo.hRight.position;
                    pos4 = new Vector3(pos3.x, pos3.y - 0.6f, pos3.z);
                    break;
                case DirType.우:
                    pos1 = handInfo.hRight.position;
                    pos2 = new Vector3(pos1.x, pos1.y - 0.6f, pos1.z);
                    pos3 = handInfo.hLeft.position;
                    pos4 = new Vector3(pos3.x, pos3.y - 0.6f, pos3.z);
                    break;
                case DirType.상:
                    pos1 = handInfo.vTop.position;
                    pos2 = new Vector3(pos1.x + 0.6f, pos1.y, pos1.z);
                    pos3 = handInfo.vDown.position;
                    pos4 = new Vector3(pos3.x + 0.6f, pos3.y, pos3.z);
                    break;
                case DirType.하:
                    pos1 = handInfo.vTop.position;
                    pos2 = new Vector3(pos1.x + 0.6f, pos1.y, pos1.z);
                    pos3 = handInfo.vDown.position;
                    pos4 = new Vector3(pos3.x + 0.6f, pos3.y, pos3.z);
                    break;
            }
            attDic.Add(type, new BezierCurves
            {
                p1 = pos1,
                p2 = pos2,
                p3 = pos3,
                p4 = pos4,
            });
        }
    }

    Vector3 GetArc()
    {
        Vector3 a = Vector3.Lerp(attDic[currDirType].p1, attDic[currDirType].p2, attDic[currDirType].arcValue);
        Vector3 b = Vector3.Lerp(attDic[currDirType].p2, attDic[currDirType].p3, attDic[currDirType].arcValue);
        Vector3 c = Vector3.Lerp(attDic[currDirType].p3, attDic[currDirType].p4, attDic[currDirType].arcValue);
        Vector3 d = Vector3.Lerp(a, b, attDic[currDirType].arcValue);
        Vector3 e = Vector3.Lerp(b, c, attDic[currDirType].arcValue);
        Vector3 f = Vector3.Lerp(d, e, attDic[currDirType].arcValue);
        
        return f;
    }

    void Idle()
    {

    }

    void Walk()
    {

    }

    void Attack()
    {
        anim.SetBool("isAttack", false);
        anim.SetBool("isWalk", false);
        currFSMType = FSMType.ATTAK;
        attDic[currDirType].arcValue = 0;
        handInfo.tool.transform.position = attDic[currDirType].p1;

        //StartCoroutine(CoAttack());
        switch (currDirType)
        {
            case DirType.좌:
                anim.SetInteger("Attack", 3);
                break;
            case DirType.우:
                anim.SetInteger("Attack", 1);
                break;
            case DirType.상:
                anim.SetInteger("Attack", 0);
                break;
            case DirType.하:
                anim.SetInteger("Attack", 2);
                break;
        }
    }

    public void EndIdle()
    {
        currFSMType = FSMType.IDLE;
        anim.SetInteger("Attack", -1);
        anim.SetBool("isAttack", true);
        anim.SetBool("isWalk", true);
        handInfo.tool.transform.position = attDic[currDirType].p1;
    }

    IEnumerator CoAttack()
    {
        while (true)
        {
            attDic[currDirType].arcValue += Time.deltaTime * stat.attSpeed;
            handInfo.tool.transform.position = GetArc();
            handInfo.tool.transform.GetChild(0).transform.right = GetArc() - transform.position;
            if (attDic[currDirType].arcValue >= 1) break;
            yield return null;
        }
        currFSMType = FSMType.IDLE;
    }

    void Update()
    {
        // Check Button Down & Up
        //bool hDown = Input.GetButtonDown("Horizontal");
        //bool vDown = Input.GetButtonDown("Vertical");

        //bool hUp = Input.GetButtonUp("Horizontal");
        //bool vUp = Input.GetButtonUp("Vertical");

        //if (hDown) moveInfo.isHorizonMove = true;
        //else if (vDown) moveInfo.isHorizonMove = false;
        //else if (hUp || vUp) moveInfo.isHorizonMove = h != 0;

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

    void FixedUpdate()
    {
        switch (currFSMType)
        {
            case FSMType.IDLE:
                break;
            case FSMType.WALK:
                break;
            case FSMType.ATTAK:
                return;
        }

        // Move Value
        moveInfo.h = Input.GetAxisRaw("Horizontal");
        moveInfo.v = Input.GetAxisRaw("Vertical");

        int h = (int)moveInfo.h;
        int v = (int)moveInfo.v;

        if (currFSMType != FSMType.ATTAK)
        {
            if (anim.GetInteger("v") != v)
            {
                currFSMType = FSMType.WALK;
                anim.SetBool("isChange", true);
                anim.SetBool("isWalk", false);
                anim.SetInteger("v", v);
            }
            else if (v == 0 && anim.GetInteger("h") != h)
            {
                currFSMType = FSMType.WALK;
                anim.SetBool("isChange", true);
                anim.SetBool("isWalk", false);
                anim.SetInteger("h", h);
            }
            else anim.SetBool("isChange", false);

            Vector2 moveVec = (Vector2.right * moveInfo.h) + (Vector2.up * moveInfo.v);
            moveVec.Normalize();
            rigid.velocity = moveVec * stat.walkSpeed;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (scanObject != null)
            {

            }
            else
            {
                Attack();
            }
        }

        if (moveInfo.direction != null)
        {
            // Ray
            Debug.DrawRay(rigid.position, moveInfo.direction * 0.7f, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, moveInfo.direction, 0.7f, LayerMask.GetMask("Object"));

            if (rayHit.collider != null) scanObject = rayHit.transform.gameObject;
            else scanObject = null;
        }
    }
}
