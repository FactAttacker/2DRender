using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class PlayerControlloerManager : MonoBehaviourSingleton<PlayerControlloerManager>
{
    [System.Serializable]
    public class StatInfo
    {
        [System.Serializable]
        public class Move
        {
            public float maxSpeed = 4.5f;
            public float jumpForce = 7.5f;
            public float dashSpeed = 700f;
            public float knockBack = 0.5f;
        }
        public Move move;

        [System.Serializable]
        public class Damage
        {
            public float power = 10f;
        }
        public Damage damage;

        [System.Serializable]
        public class Health
        {
            public float hp = 500f;
        }
        public Health health;
    }
    [SerializeField]
    public StatInfo statInfo;

    [System.Serializable]
    class EffectInfo
    {
        public GameObject runStopDust;
        public GameObject jumpDust;
        public GameObject landingDust;
    }
    [SerializeField]
    EffectInfo effectInfo;

    [System.Serializable]
    class AnimInfo
    {
        public Animator player;
    }
    [SerializeField]
    AnimInfo animInfo;

    [System.Serializable]
    class PlayerInfo
    {
        public Rigidbody2D body2d;
        public Sensor_Prototype groundSensor;
        public AudioManager_PrototypeHero audioManager;
        public Transform poolBox;

        [Header("바라보는 방향")]
        public int facingDirection = 1;
        float disableMovementTimer = 0f;
        public float DisableMovementTimer
        {
            get
            {
                return disableMovementTimer;
            }
            set
            {
                if (value < -99f) value = -99f;
                disableMovementTimer = value;
            }
        }

        float attackComboTimer = 0f;
        public float AttackComboTimer
        {
            get
            {
                return attackComboTimer;
            }
            set
            {
                if (value < -999f) value = -999f;
                attackComboTimer = value;
            }
        }

        [System.Serializable]
        public class Status
        {
            public bool isGrounded = false;
            public bool isMoving = false;
            public bool isLookUp = false;
            public bool isLookUping = false;
            public bool isShotJump = false;
            public bool isDash = false;
            public bool isDashing = false;
        }
        public Status status;

        [System.Serializable]
        public class Attack
        {
            public bool isAttack = false;
            public bool isAttacking = false;
            public int AttackComboCnt = 0;
            public int currentAttackNum = 0;
        }
        public Attack attack;
    }
    [SerializeField]
    PlayerInfo playerInfo;

    [System.Serializable]
    class Pos
    {
        public Transform cameraPos;
        public Transform runStopPos;
    }
    [SerializeField]
    Pos pos;

    Dictionary<string, GameObject> prefabDic = new();
    Dictionary<string, Coroutine> coroutineDic = new();
    Dictionary<string, AnimationClip> animDic = new();

    private void Reset()
    {
        // 애니메이션
        TryGetComponent(out animInfo.player);

        // 레지드 바디
        TryGetComponent(out playerInfo.body2d);

        // 바닥 센서
        transform.Find("GroundSensor").TryGetComponent(out playerInfo.groundSensor);

        Transform _pos = transform.Find("Pos");
        pos.cameraPos = _pos.Find("CameraPos").transform;
        pos.runStopPos = _pos.Find("RunStopPos").transform;

        playerInfo.poolBox = GameObject.Find("PoolBox").transform;

        statInfo.move.maxSpeed = 4.5f;
        statInfo.move.jumpForce = 7.5f;

        // 먼지 프리팹 
        string prefabPathToFind = "Assets/Prefabs/FX";
        string[] sAssetGuids = AssetDatabase.FindAssets("Dust t:Prefab", new[] { prefabPathToFind });
        string[] sAssetPathList = System.Array.ConvertAll(sAssetGuids, AssetDatabase.GUIDToAssetPath);
        foreach (string sAssetPath in sAssetPathList)
        {
            GameObject sfxObj = (GameObject)AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(GameObject));
            switch (sfxObj.name)
            {
                case "JumpDust":
                    effectInfo.jumpDust = sfxObj;
                    break;
                case "LandingDust":
                    effectInfo.landingDust = sfxObj;
                    break;
                case "RunStopDust":
                    effectInfo.runStopDust = sfxObj;
                    break;
            }
        }
    }

    void Start()
    {
        // 먼지
        prefabDic.Add(effectInfo.jumpDust.name, effectInfo.jumpDust);
        prefabDic.Add(effectInfo.landingDust.name, effectInfo.landingDust);
        prefabDic.Add(effectInfo.runStopDust.name, effectInfo.runStopDust);

        // 애니메이션 클립
        foreach (AnimationClip clip in animInfo.player.runtimeAnimatorController.animationClips)
        {
            animDic.Add(clip.name, clip);
        }
    }

    void Update()
    {
        playerInfo.DisableMovementTimer -= Time.deltaTime;
        playerInfo.AttackComboTimer -= Time.deltaTime;

        // 캐릭터가 땅에 있는지 확인
        if (!playerInfo.status.isGrounded && playerInfo.groundSensor.State())
        {
            playerInfo.status.isGrounded = true;
            SetAnimator("Grounded", playerInfo.status.isGrounded);
        }

        // 캐릭터가 떨어지기 시작했는지 확인
        if (playerInfo.status.isGrounded && !playerInfo.groundSensor.State())
        {
            playerInfo.status.isGrounded = false;
            SetAnimator("Grounded", playerInfo.status.isGrounded);
        }

        if (playerInfo.status.isGrounded && playerInfo.status.isShotJump)
        {
            playerInfo.status.isShotJump = false;
            string coroutinName = nameof(CoJumpDown);
            if (coroutineDic.ContainsKey(coroutinName))
            {
                Coroutine endJumpDown = coroutineDic[coroutinName];
                if (endJumpDown != null) StopCoroutine(endJumpDown); 
            }
        }

        // -- 입력 및 이동 --
        float inputX = 0.0f;

        if (playerInfo.DisableMovementTimer < 0.0f) inputX = Input.GetAxis("Horizontal");

        // GetAxisRaw는 -1, 0 또는 1을 반환
        float inputRawX = Input.GetAxisRaw("Horizontal"); //방향 체크하기 위함
        float inputY = Input.GetAxis("Vertical");

        if (playerInfo.status.isDash) return;

        //공격 시 이곳에서 처리 하고 닫기 return
        if (playerInfo.status.isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            if (!playerInfo.attack.isAttack)
            {
                playerInfo.attack.isAttack = true;
                SetAnimator("AttackComboState", 1);
                playerInfo.attack.AttackComboCnt = 1;
                playerInfo.AttackComboTimer = animDic["attack_combo_01"].length;
                Debug.Log("z누름");
            }
            else
            {
                if ((animDic["attack_combo_01"].length / 2 ) > playerInfo.AttackComboTimer)
                {
                    playerInfo.attack.AttackComboCnt = 2;
                    playerInfo.DisableMovementTimer = animDic["attack_combo_02"].length;
                    SetAnimator("AttackComboState", 2);
                }
            }
        }

        // 공격 중 움직임 방지
        if (playerInfo.attack.isAttack)
        {
            playerInfo.body2d.velocity = Vector2.zero;
            return;
        }

        // 대쉬
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    if (!playerInfo.status.isDash) Dash();
        //}

        // 키 이동 입력이 0보다 크고 이동 방향이 캐릭터 방향과 동일한지 점검
        if (Mathf.Abs(inputRawX) > Mathf.Epsilon && Mathf.Sign(inputRawX) == playerInfo.facingDirection)
        {
            if (!playerInfo.status.isMoving) playerInfo.status.isMoving = true;
        }
        else
        {
            if (playerInfo.status.isMoving) playerInfo.status.isMoving = false;
        }

        // 이동 방향에 따라 스프라이트의 방향 전환
        FlipCharacher(inputRawX);

        // SlowDownSpeed는 정지시 캐릭터를 감속시키는 데 도움
        float SlowDownSpeed = playerInfo.status.isMoving ? 1.0f : 0.5f;

        //이동
        playerInfo.body2d.velocity = new Vector2(inputX * statInfo.move.maxSpeed * SlowDownSpeed, playerInfo.body2d.velocity.y);

        // 애니메이터에서 에어 스피드 설정
        SetAnimator("AirSpeedY", playerInfo.body2d.velocity.y);

        // 검을 숨기기 위한 애니메이션 레이어 설정
        //int boolInt = m_hideSword ? 1 : 0;
        //SetAnimator(1, boolInt);

        // 점프
        if (Input.GetButtonDown("Jump") && playerInfo.status.isGrounded && playerInfo.DisableMovementTimer < 0.0f)
        {
            SetAnimator("AnimState", 0);
            SetAnimator("Jump");
            playerInfo.status.isGrounded = false;
            if (playerInfo.status.isLookUp) playerInfo.status.isLookUp = false;
            SetAnimator("Grounded", playerInfo.status.isGrounded);
            playerInfo.body2d.velocity = new Vector2(playerInfo.body2d.velocity.x, statInfo.move.jumpForce);
            playerInfo.groundSensor.Disable(0.2f);
        }
        else if (playerInfo.status.isMoving) // 뛰기
        {
            SetAnimator("AnimState", 1);
        }
        else // 쉼
        {
            SetAnimator("AnimState", 0);
            if (!playerInfo.attack.isAttack && inputY > 0f)
            {
                if (!playerInfo.status.isLookUp) playerInfo.status.isLookUp = true;
            }
            else
            {
                if (playerInfo.status.isLookUp) playerInfo.status.isLookUp = false;
            }
        }

        // 소 점프
        if (Input.GetButtonUp("Jump") && playerInfo.body2d.velocity.y > 5f)
        {
            if (!playerInfo.status.isShotJump) JumpDown();
        }

        // 위 보기
        if (animInfo.player.GetInteger("AnimState") != 0 && playerInfo.status.isLookUp) playerInfo.status.isLookUp = false;
        if (playerInfo.attack.isAttack) playerInfo.status.isLookUp = false;
        LoockUp();
    }

    /// <summary>
    /// 대쉬
    /// </summary>
    void Dash()
    {
        playerInfo.status.isDash = true;
        playerInfo.status.isDashing = true;
        StartCoroutine(CoDash());
    }
    IEnumerator CoDash()
    {
        SetAnimator("Dash");
        SetAnimator("isDashing", true);
        float originalGravity = playerInfo.body2d.gravityScale;
        playerInfo.body2d.gravityScale = 0f;
        playerInfo.body2d.velocity = new Vector2(transform.localScale.x * statInfo.move.dashSpeed, 0f);
        yield return new WaitForSeconds(0.5f);
        playerInfo.body2d.gravityScale = originalGravity;
        playerInfo.status.isDash = false;
    }

    /// <summary>
    /// 대쉬 종료
    /// </summary>
    void EndDash()
    {
        AnimatorClipInfo[] animClips = animInfo.player.GetCurrentAnimatorClipInfo(0);
        string clipName = animClips[0].clip.name;
        if (clipName == "dash" && !playerInfo.status.isGrounded)
        {
            SetAnimator("isDashing", false);
            playerInfo.status.isDashing = false;
        }

        if (clipName == "dash_stop" && playerInfo.status.isGrounded)
        {
            SetAnimator("isDashing", false);
            playerInfo.status.isDashing = false;
        }
    }

    /// <summary>
    /// 소 점프 
    /// </summary>
    void JumpDown()
    {
        playerInfo.status.isShotJump = true;

        Coroutine coroutine;
        if (coroutineDic.ContainsKey(nameof(CoJumpDown)))
        {
            coroutine = StartCoroutine(CoJumpDown());
        }
        else
        {
            coroutineDic.Add(nameof(CoJumpDown), StartCoroutine(CoJumpDown()));
        }
    }
    IEnumerator CoJumpDown()
    {
        yield return new WaitUntil(() => playerInfo.body2d.velocity.y < 5f);
        playerInfo.body2d.velocity = new Vector2(playerInfo.body2d.velocity.x, 0);
        playerInfo.status.isShotJump = false;
    }

    /// <summary>
    /// [애니메이션 이벤트]
    /// 공격 시작
    /// </summary>
    void StartAttack()
    {
        AnimatorClipInfo[] animClips = animInfo.player.GetCurrentAnimatorClipInfo(0);
        string currentAttack = animClips[0].clip.name;
        string[] strArr = currentAttack.Split("_");
        playerInfo.attack.currentAttackNum = int.Parse(strArr[strArr.Length - 1]);
        SetAnimator("AttackComboState", 0);
        playerInfo.attack.isAttacking = true;

    }

    /// <summary>
    /// [애니메이션 이벤트]
    /// 공격 끝
    /// </summary>
    void EndAttack()
    {
        // 현재 애니메이션과 콤보와 같지 않으면 넘어감
        if (playerInfo.attack.currentAttackNum != playerInfo.attack.AttackComboCnt) return;

        playerInfo.DisableMovementTimer = 0.0f;
        playerInfo.attack.isAttack = false;
        SetAnimator("AttackComboState", 0);
        playerInfo.attack.AttackComboCnt = 0;
        playerInfo.attack.currentAttackNum = 0;
    }

    /// <summary>
    /// 카메라 위 보기
    /// </summary>
    void LoockUp()
    {
        Coroutine coroutine;
        if (playerInfo.status.isLookUp)
        {
            if (coroutineDic.ContainsKey(nameof(CoLookUp)))
            {
                coroutine = StartCoroutine(CoLookUp());
            }
            else
            {
                coroutine = StartCoroutine(CoLookUp());
                coroutineDic.Add(nameof(CoLookUp), coroutine);
            }
            if (!animInfo.player.GetBool("isLookUping")) SetAnimator("isLookUping", true);
        }
        else
        {
            if (coroutineDic.ContainsKey(nameof(CoLookUp)))
            {
                coroutine = coroutineDic[nameof(CoLookUp)];
                if (coroutine != null) StopCoroutine(coroutine);
                coroutineDic.Remove(nameof(CoLookUp));
                pos.cameraPos.localPosition = Vector3.zero;
                if (playerInfo.status.isLookUping) playerInfo.status.isLookUping = false;
            }
            if (animInfo.player.GetBool("isLookUping")) SetAnimator("isLookUping", false);
        }
    }
    IEnumerator CoLookUp()
    {
        if (playerInfo.status.isLookUping) yield break;
        else playerInfo.status.isLookUping = true;
        yield return new WaitForSeconds(2f);
        pos.cameraPos.localPosition = Vector3.up * 3f;
    }

    // 먼지 효과를 생성하는 데 사용되는 기능
    // 모든 먼지 효과는 바닥에 산란
    // 더스트X 오프셋은 플레이어에서 효과가 발생하는 거리를 제어
    // 기본 dustXoffset은 0
    void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
    {
        if (dust != null)
        {
            // 먼지 발생 위치 설정
            Vector3 dustSpawnPosition;
            dustSpawnPosition = transform.position + new Vector3(dustXOffset * playerInfo.facingDirection, 0.0f, 0.0f);
            //if (runStopPos == null) dustSpawnPosition = transform.position + new Vector3(dustXOffset * playerInfo.facingDirection, 0.0f, 0.0f);
            //else dustSpawnPosition = runStopPos.position;
            GameObject newDust = GetPool(dust.name);
            newDust.transform.position = dustSpawnPosition;
            newDust.transform.localScale = Vector3.one;
            newDust.SetActive(true);
            // 먼지를 올바른 X 방향으로 돌리십시오
            newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(playerInfo.facingDirection, 1, 1);
        }
    }

    /// <summary>
    /// Object Pool 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    GameObject GetPool(string name)
    {
        for (int i = 0; i < playerInfo.poolBox.childCount; i++)
        {
            GameObject go = playerInfo.poolBox.GetChild(i).gameObject;
            if (go.name.Contains(name) && !go.activeSelf)
            {
                return go;
            }
        }
        return Instantiate(prefabDic[name], playerInfo.poolBox.transform);
    }

    // 애니메이션 이벤트
    // 이러한 함수는 애니메이션 파일 내부에서 호출
    public void AE_runStop()
    {
        // 먼지
        float dustXOffset = 0.3f;
        SpawnDustEffect(effectInfo.runStopDust, dustXOffset);
    }

    public void AE_footstep()
    {
        //m_audioManager.PlaySound("Footstep");
    }

    public void AE_Jump()
    {
        //m_audioManager.PlaySound("Jump");
        // Spawn Dust
        SpawnDustEffect(effectInfo.jumpDust);
    }

    public void AE_Landing()
    {
        // m_audioManager.PlaySound("Landing");
        // Spawn Dust
        SpawnDustEffect(effectInfo.landingDust);
    }

    /// <summary>
    /// 애니메이션 실행
    /// </summary>
    /// <param name="key">애니메이션 파라메터 키</param>
    /// <param name="value">값</param>
    void SetAnimator(string key, object value = null)
    {
        if (value == null)
        {
            animInfo.player.SetTrigger(key);
            return;
        }
        System.Type type = value.GetType();

        if (type.Equals(typeof(bool)))
        {
            animInfo.player.SetBool(key, (bool)value);
        }
        else if (type.Equals(typeof(float)))
        {
            animInfo.player.SetFloat(key, (float)value);
        }
        else if (type.Equals(typeof(int)))
        {
            animInfo.player.SetInteger(key, (int)value);
        }
    }

    /// <summary>
    /// 방향 변경
    /// </summary>
    /// <param name="inputRaw"></param>
    void FlipCharacher(float inputRaw)
    {
        // 이동 방향에 따라 스프라이트의 방향 전환
        if (inputRaw > 0)
        {
            //GetComponent<SpriteRenderer>().flipX = false;
            if (transform.localRotation != Quaternion.identity) transform.localRotation = Quaternion.identity;
            if (playerInfo.facingDirection != 1) playerInfo.facingDirection = 1;
        }
        else if (inputRaw < 0)
        {
            //GetComponent<SpriteRenderer>().flipX = true;
            if (transform.localRotation.y != 180) transform.localRotation = Quaternion.Euler(0, 180, 0);
            if (playerInfo.facingDirection != -1) playerInfo.facingDirection = -1;
        }
    }
}
