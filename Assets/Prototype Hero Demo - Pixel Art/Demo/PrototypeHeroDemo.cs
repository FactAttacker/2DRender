using UnityEngine;
using System.Collections;

public class PrototypeHeroDemo : MonoBehaviour {

    [Header("변수")]
    [SerializeField] float      m_maxSpeed = 4.5f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] bool       m_hideSword = false;
    [Header("이펙트")]
    [SerializeField] GameObject m_RunStopDust;
    [SerializeField] GameObject m_JumpDust;
    [SerializeField] GameObject m_LandingDust;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_Prototype    m_groundSensor;
    private AudioSource         m_audioSource;
    private AudioManager_PrototypeHero m_audioManager;
    private bool                m_grounded = false;
    private bool                m_moving = false;
    private int                 m_facingDirection = 1;
    private float               m_disableMovementTimer = 0.0f;

    // Use this for initialization
    // 초기화에 사용
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_audioSource = GetComponent<AudioSource>();
        m_audioManager = AudioManager_PrototypeHero.instance;
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Prototype>();
    }

    // Update is called once per frame
    // 업데이트는 프레임당 한 번 호출
    void Update ()
    {
        // Decrease timer that disables input movement. Used when attacking
        // 입력 이동을 비활성화하는 타이머를 줄입니다. 공격할 때 사용
        m_disableMovementTimer -= Time.deltaTime;

        // Check if character just landed on the ground
        // 캐릭터가 방금 땅에 떨어졌는지 확인
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // Check if character just started falling
        // 캐릭터가 방금 떨어지기 시작했는지 확인합니다.
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        // -- 입력 및 이동 --
        float inputX = 0.0f;

        if (m_disableMovementTimer < 0.0f)
            inputX = Input.GetAxis("Horizontal");

        // GetAxisRaw returns either -1, 0 or 1
        // GetAxisRaw는 -1, 0 또는 1을 반환
        float inputRaw = Input.GetAxisRaw("Horizontal");
        // Check if current move input is larger than 0 and the move direction is equal to the characters facing direction
        // 키 이동 입력이 0보다 크고 이동 방향이 캐릭터 방향과 동일한지 점검
        if (Mathf.Abs(inputRaw) > Mathf.Epsilon && Mathf.Sign(inputRaw) == m_facingDirection)
            m_moving = true;
        else
            m_moving = false;

        // Swap direction of sprite depending on move direction
        // 이동 방향에 따라 스프라이트의 방향 전환
        if (inputRaw > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputRaw < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // SlowDownSpeed helps decelerate the characters when stopping
        // SlowDownSpeed는 정지 시 캐릭터를 감속시키는 데 도움
        float SlowDownSpeed = m_moving ? 1.0f : 0.5f;

        // Set movement
        // 이동 
        m_body2d.velocity = new Vector2(inputX * m_maxSpeed * SlowDownSpeed, m_body2d.velocity.y);

        // Set AirSpeed in animator
        // 애니메이터에서 에어스피드 설정
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // Set Animation layer for hiding sword
        // 검을 숨기기 위한 애니메이션 레이어 설정
        int boolInt = m_hideSword ? 1 : 0;
        m_animator.SetLayerWeight(1, boolInt);

        // -- Handle Animations --
        // 애니메이션들
        // Jump
        // 점프
        if (Input.GetButtonDown("Jump") && m_grounded && m_disableMovementTimer < 0.0f)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        // Run
        // 뛰기
        else if (m_moving)
            m_animator.SetInteger("AnimState", 1);

        // Idle
        // 쉼
        else
            m_animator.SetInteger("AnimState", 0);
    }

    // Function used to spawn a dust effect
    // 먼지 효과를 생성하는 데 사용되는 기능
    // All dust effects spawns on the floor
    // 모든 먼지 효과는 바닥에 산란
    // dustXoffset controls how far from the player the effects spawns.
    // 더스트X 오프셋은 플레이어에서 효과가 발생하는 거리를 제어
    // Default dustXoffset is zero
    // 기본 dustXoffset은 0
    void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
    {
        if (dust != null)
        {
            // Set dust spawn position
            Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * m_facingDirection, 0.0f, 0.0f);
            GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity) as GameObject;
            // Turn dust in correct X direction
            newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(m_facingDirection, 1, 1);
        }
    }

    // Animation Events
    // 애니메이션 이벤트
    // These functions are called inside the animation files
    // 이러한 함수는 애니메이션 파일 내부에서 호출
    void AE_runStop()
    {
        m_audioManager.PlaySound("RunStop");
        // Spawn Dust
        float dustXOffset = 0.6f;
        SpawnDustEffect(m_RunStopDust, dustXOffset);
    }

    void AE_footstep()
    {
        m_audioManager.PlaySound("Footstep");
    }

    void AE_Jump()
    {
        m_audioManager.PlaySound("Jump");
        // Spawn Dust
        SpawnDustEffect(m_JumpDust);
    }

    void AE_Landing()
    {
        m_audioManager.PlaySound("Landing");
        // Spawn Dust
        SpawnDustEffect(m_LandingDust);
    }
}
