using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSideViewManager : MonoBehaviourSingleton<PlayerSideViewManager>
{
    [ContextMenuItem("실행",nameof(StartWallJump))]
    [SerializeField]
    DirType currDir;

    [System.Serializable]
    public class Stat
    {
        public float moveSpeed = 5;
        public float slidingSpeed = -3f;
        public float jumpForce = 400f;
        public float wallChkDistance = 0.5f;
        public int chanceJumpCnt = 0;
    }
    public Stat stat;

    [System.Serializable]
    class MoveInfo
    {
        public float moveX = 0;
        public float moveY = 0;
        public bool facingRight = true; // 방향
    }
    [SerializeField]
    MoveInfo moveInfo;

    [System.Serializable]
    class Status
    {
        public bool isMove = true;
        public bool isCanJump = false;
        public bool isJumping = false;
        public bool isWallJump = false;
        public bool isJumping2 = false;
        public bool isGround = false;
        public bool isWall = false;
        public bool isCrouch = false;
        public bool isLookUp = false;
        public bool isLookUping = false;
        public bool isClimb = false;
        public bool isAir = false;
        public bool isAirDown = false;
    }
    [SerializeField]
    Status status;

    enum PlayerFSM
    {
        Idle,
        Run,
        Climb,
        Air,
    }
    PlayerFSM playerFSM;

    [System.Serializable]
    class Pos
    {
        public Transform cameraPos;
        public Transform wallJumpPos;
        public float startAirY;
        public float currentAirY;
    }
    [SerializeField]
    Pos pos;

    [System.Serializable]
    class TimeInfo
    {
        public float maxJumpTime;
        public float currjumpTime; 
    }
    [SerializeField]
    TimeInfo timeInfo;

    Dictionary<string, Coroutine> coroutineDic = new Dictionary<string, Coroutine>();

    [System.Serializable]
    class DetectCheck
    {
        public Transform wall;
    }
    [SerializeField]
    DetectCheck detectCheck;

    [SerializeField]
    Animator anim;

    [SerializeField]
    Rigidbody2D rb;

    enum DirType
    {
        왼,
        오른,
    }

    [SerializeField]
    List<ColliderState> collStateList = new();

    float time = 0f;

    void Reset()
    {
        TryGetComponent(out rb);
        TryGetComponent(out anim);

        Transform coll = transform.Find("Coll");
        for(int i = 0; i < coll.childCount; i++)
        {
            coll.GetChild(i).TryGetComponent(out ColliderState col);
            if (col != null) collStateList?.Add(col);
        }

        detectCheck.wall = transform.Find("DetectCheck").Find("Wall");

        Transform _pos = transform.Find("Pos");
        pos.cameraPos = _pos.Find("CameraPos").transform;
        pos.wallJumpPos = _pos.Find("WallJump").transform;
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        currDir = DirType.오른;

        foreach (ColliderState item in collStateList)
        {
            switch (item.type)
            {
                case TileType.Ground:
                    item.collAction.stay = (col) =>
                    {
                        if (col.name.Contains("Ground"))
                        {
                            if (!status.isGround)
                            {
                                status.isGround = true;
                                status.isWall = false;
                                status.isJumping = false;
                                status.isCanJump = true;
                                //moveInfo.isJumping2 = false;
                            }
                        }
                    };
                    item.collAction.exit = (col) =>
                    {
                        if (col.name.Contains("Ground"))
                        {
                            if (status.isGround)
                            {
                                status.isGround = false;
                                //moveInfo.isJumping = true;
                            }
                        }
                    };

                    break;
                case TileType.Wall:
                    // item.collAction.stay = (col) =>
                    // {
                    //     bool wall = col.name.Contains("Wall");
                    //     bool ground = col.name.Contains("Ground");
                    //     if (wall)
                    //     {
                    //         if (!status.isWall)
                    //         {
                    //             status.isWall = true;
                    //             //moveInfo.isJumping = false;
                    //             //moveInfo.isJumping2 = false;
                    //         }
                    //     }
                    //     if (ground)
                    //     {
                    //         if (!status.isGround) status.isGround = true; 
                    //     }
                    // };
                    // item.collAction.exit = (col) =>
                    // {
                    //     bool wall = col.name.Contains("Wall");
                    //     bool ground = col.name.Contains("Ground");
                    //     if (wall)
                    //     {
                    //         if (status.isWall)
                    //         {
                    //             status.isWall = false;
                    //         }
                    //     }
                    //     if (ground)
                    //     {
                    //         if (status.isGround) status.isGround = false;
                    //     }
                    // };
                    break;
            }
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {

    }

    void OnTriggerExit2D(Collider2D col)
    {

    }

    void Update()
    {
        // 입력 처리
        ProcessInputs();

        // 애니메이션
        Animate();
    }

    // 물리 처리에 더 적합하며 업데이트 프레임당 여러 번 호출할 수 있음.
    private void FixedUpdate()
    {
        // 움직임 처리
        Move();
    }

    /// <summary>
    /// 입력 처리
    /// </summary>
    void ProcessInputs()
    {
        moveInfo.moveX = Input.GetAxis("Horizontal");
        moveInfo.moveY = Input.GetAxis("Vertical");

        if (!status.isJumping && status.isGround) //|| !moveInfo.isJumping2
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (status.isGround)
                {
                    // 1단 점프
                    status.isJumping = true;
                    time = 0f;
                    // pos.prevJumpY = transform.position.y;
                    // timeInfo.currjumpTime
                }
                // else
                // {
                //     // 2단 점프
                //     moveInfo.isJumping = true;
                //     moveInfo.isJumping = true;
                // }
                // moveInfo.isJumping = true;
            }
            if (Input.GetButtonUp("Jump")) time += Time.deltaTime;
        }

        if (moveInfo.moveY < 0 && status.isGround)
        {
            if (!status.isCrouch) status.isCrouch = true;
        }
        else
        {
            if (status.isCrouch) status.isCrouch = false;
        }

        // 위 올려 보기 [위 키 누름 && 좌우 키 안누름 && 땅 ]
        if (moveInfo.moveY > 0 && moveInfo.moveX == 0 && status.isGround)
        {
            if (!status.isLookUp) status.isLookUp = true;
        }
        else
        {
            if (status.isLookUp) status.isLookUp = false;
        }

        // 땅이 아닐때
        if (!status.isGround)
        {
            status.isCrouch = false;
            status.isLookUp = false;
        }

        if (status.isGround && status.isWall)
        {
            if (!status.isClimb) status.isClimb = true;
        }
        else
        {
            if (status.isClimb) status.isClimb = false;
        }

        //if (!status.isWallJump)
        {
            // 방향 오른쪽 
            if (moveInfo.moveX > 0 && !moveInfo.facingRight)
            {
                FlipCharacher();
            }
            // 방향 왼쪽
            else if (moveInfo.moveX < 0 && moveInfo.facingRight)
            {
                FlipCharacher();
            }
        }

        bool isWall = Physics2D.Raycast(detectCheck.wall.position, Vector2.right * (moveInfo.facingRight ? 1 : -1), stat.wallChkDistance, LayerMask.GetMask("Wall"));
        if (status.isWall != isWall) status.isWall = isWall;

        // 벽 점프 가능 여부
        if (isWall)
        {
            if (Input.GetButtonDown("Jump"))
            {
                //status.isWallJump = true;
                status.isJumping = true;
            }
        }
        else
        {
            //if (status.isWallJump) status.isWallJump = false;
        }

        if (status.isWall || status.isGround)
        {
            if (status.isAir)
            {
                status.isAir = false;
                if (pos.startAirY != 0) pos.startAirY = 0f;
            }
            if (status.isAirDown) status.isAirDown = false;
        }
        else
        {
            if (!status.isAir)
            {
                pos.startAirY = transform.position.y;
                pos.currentAirY = transform.position.y;
                status.isAir = true;
            }
            if (transform.position.y - pos.startAirY > -0.1) status.isAirDown = true;
        }

        Debug.DrawRay(rb.position, (moveInfo.facingRight ? 1 : -1) * 0.7f * Vector2.right, new Color(0, 1, 0));
    }

    /// <summary>
    /// 애니메이션 처리
    /// </summary>
    void Animate()
    {
        // 땅 일때만
        if (moveInfo.moveX == 0)
        {
            if (anim.GetBool("isRun")) anim.SetBool("isRun", false);
        }
        else
        {
            if (!anim.GetBool("isRun")) anim.SetBool("isRun", true);
        }

        // 올려다 보기
        if (anim.GetBool("isLookUp") != status.isLookUp) anim.SetBool("isLookUp", status.isLookUp);

        // 숙이기
        if (anim.GetBool("isCrouch") != status.isCrouch) anim.SetBool("isCrouch", status.isCrouch);

        // 땅 체크
        if (anim.GetBool("isGround") != status.isGround) anim.SetBool("isGround", status.isGround);
        // 벽 체크
        if (anim.GetBool("isWall") != status.isWall) anim.SetBool("isWall", status.isWall);
        // 벽 매달리기
        if (anim.GetBool("isClimb") != status.isClimb) anim.SetBool("isClimb", status.isClimb);

        // 1단 점프
        if (anim.GetBool("isJump") != status.isJumping) anim.SetBool("isJump", status.isJumping);
        // 2단 점프
        if (anim.GetBool("isJump2") != status.isJumping2) anim.SetBool("isJump2", status.isJumping2);

        if (anim.GetBool("isAirDown") != status.isAirDown) anim.SetBool("isAirDown", status.isAirDown);

        if (anim.GetFloat("x") != moveInfo.moveX) anim.SetFloat("x", moveInfo.moveX);
        if (anim.GetFloat("y") != moveInfo.moveY) anim.SetFloat("y", moveInfo.moveY);
    }

    //void ColliderCheck()
    //{
    //    switch (currDir)
    //    {
    //        case DirType.왼:
                
    //            break;
    //        case DirType.오른:
    //            break;
    //    }
    //}

    /// <summary>
    /// 움직임 처리
    /// </summary>
    void Move()
    {
        if (status.isWall)
        {
            if (status.isWallJump)
            {
                // status.isWallJump = false;
                // StartCoroutine(CoWallJump());
                // Invoke(nameof(StopWallJump),0.3f);
                // Vector2 vec = new(moveInfo.facingRight ? 1f : -1f, 1);
                // vec.Normalize();
                // rb.velocity = new Vector2(moveInfo.facingRight ? 1f : -1f * stat.jumpForce, 0.9f * 50f);
                // rb.velocity = new Vector2(moveInfo.moveX * stat.moveSpeed, rb.velocity.y);
                // rb.AddForce(vec * stat.jumpForce * 0.5f);
            }
            else
            {
            }
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * stat.slidingSpeed);
        }
        else
        {
            //if (status.isWallJump) return;
            //TODO : 점프다시만들기 [ 천장에 부딪혔을때와 점프 높이 체크가 가능해야함 방식을 바꿔야함 ]
            if (status.isJumping && status.isCanJump)
            {
                rb.AddForce(Vector2.up * stat.jumpForce);
                //Debug.Log(time);
                status.isCanJump = false;
            }

            rb.velocity = new Vector2(moveInfo.moveX * stat.moveSpeed, rb.velocity.y);
            if (status.isGround)
            {
                status.isJumping = false;
                status.isJumping2 = false;
            }
        }
        
        //if(pos.currentAirY != transform.position.y) pos.currentAirY = transform.position.y - pos.startAirY;

        Coroutine coLookUp;
        if (status.isLookUp)
        {
            if (coroutineDic.ContainsKey(nameof(CoLookUp)))
            {
                coLookUp = StartCoroutine(CoLookUp());
            }
            else
            {
                coLookUp = StartCoroutine(CoLookUp());
                coroutineDic.Add(nameof(CoLookUp), coLookUp);
            }
        }
        else
        {
            if (coroutineDic.ContainsKey(nameof(CoLookUp)))
            {
                coLookUp = coroutineDic[nameof(CoLookUp)];
                if (coLookUp != null) StopCoroutine(coLookUp);
                coroutineDic.Remove(nameof(CoLookUp));
                pos.cameraPos.localPosition = Vector3.zero;
                if (status.isLookUping) status.isLookUping = false;
            }
        }
    }

    void StartWallJump()
    {
        StartCoroutine(CoWallJump());
    }

    IEnumerator CoWallJump()
    {
        Vector3 jumpPos = pos.wallJumpPos.position;
        FlipCharacher();
        while (true)
        {
            if (Mathf.Abs(transform.position.x) - Mathf.Abs(jumpPos.x) < 0.2f && Mathf.Abs(transform.position.y) - Mathf.Abs(jumpPos.y) < 0.2f) break;
            transform.position = Vector3.MoveTowards(transform.position, jumpPos, Time.deltaTime * stat.jumpForce);
            yield return null;
        }

        status.isWallJump = false;
    }
    IEnumerator CoLookUp()
    {
        if (status.isLookUping) yield break;
        else status.isLookUping = true;
        yield return new WaitForSeconds(2f);
        pos.cameraPos.localPosition = Vector3.up * 6f;
    }

    void StopWallJump()
    {
        status.isWallJump = false;
    }
   
    // 방향처리
    void FlipCharacher()
    {
        moveInfo.facingRight = !moveInfo.facingRight;
        if (moveInfo.facingRight)
        {
            currDir = DirType.오른;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            currDir = DirType.왼;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
