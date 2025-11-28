using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 움직임 설정
    public float walkSpeed = 2.5f;      // 걷는 속도
    public float runSpeed = 4.0f;       // 달리기 속도
    public float crouchSpeed = 1.5f;    // 앉기 속도
    public float gravity = -9.81f;

    // 시야 설정
    public float mouseSensitivity = 100f;
    public float lookXLimit = 80.0f;

    // 점프 설정
    public float jumpHeight = 1.5f;

    // 내부 변수들
    private CharacterController characterController;
    private AudioSource audioSource;
    private Vector3 moveDirection;
    private float rotationX = 0;

    private Camera playerCamera;

    public float crouchCameraY = 0.1f;      // 앉았을 때 카메라 높이
    public float crouchTransitionSpeed = 10f; // 앉을 때 부드러운 정도 (클수록 빠름)
    private float defaultCameraY;           // 서 있을 때 카메라 높이 (자동 저장)

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // 카메라 찾기 (최적화)
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;

        if (playerCamera != null) {
            defaultCameraY = playerCamera.transform.localPosition.y;
        }

        // 마우스 커서 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 발소리 설정
        if (audioSource != null) audioSource.loop = false; // 발소리는 loop 끄는게 낫습니다 (코드로 제어하니까)
    }

    void Update()
    {
        LookRotation();
        Movement();
        Footsteps();
        HandleCrouch();
    }

    private void HandleCrouch()
    {
        if (playerCamera == null) return;

        // 1. 컨트롤 키를 누르고 있으면 목표 높이를 '앉은 높이'로, 아니면 '원래 높이'로 설정
        float targetY = Input.GetKey(KeyCode.LeftControl) ? crouchCameraY : defaultCameraY;

        // 2. 부드럽게 높이 변경 (Lerp 사용)
        Vector3 newPos = playerCamera.transform.localPosition;
        newPos.y = Mathf.Lerp(newPos.y, targetY, Time.deltaTime * crouchTransitionSpeed);

        playerCamera.transform.localPosition = newPos;
    }

    private void LookRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // [에러 해결] 이제 playerCamera 변수가 있어서 에러 안 남
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void Movement()
    {
        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed; // 앉으면 제일 느리게
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;    // 서서 쉬프트 누르면 달리기
        }
        if (characterController.isGrounded)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Y축 이동 방지 (평지 이동)
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // 방향 계산
            Vector3 moveInput = (forward * z + right * x);
            if (moveInput.magnitude > 1) moveInput.Normalize();

            // 속도 적용 (X, Z) - 기존 moveDirection의 Y(중력)는 유지해야 함을 주의
            moveDirection.x = moveInput.x * currentSpeed;
            moveDirection.z = moveInput.z * currentSpeed;

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            // 공중에 있을 때 (떨어질 때)는 중력만 지속적으로 적용
            moveDirection.y += gravity * Time.deltaTime;
        }

        // 중력 적용 (공중이든 땅이든 항상 적용)
        moveDirection.y += gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void Footsteps()
    {
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.5f) // 0.1은 너무 예민해서 0.5로 수정
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}



//맵에 연결할 부분

//스크립트 부착: 이 PlayerMovement.cs 스크립트를 플레이어에 연결

//컴포넌트 추가: 플레이어 오브젝트에 CharacterController와 AudioSource 컴포넌트 추가

//카메라 설정: 메인 카메라를 플레이어 오브젝트의 자식 오브젝트로 만들어야함

//AudioClip에 발소리 파일 연결
