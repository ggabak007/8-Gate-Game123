using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 움직임
    public float moveSpeed = 2.0f;
    public float gravity = -9.81f;

    // 시야
    public float mouseSensitivity = 100f;
    public float lookXLimit = 80.0f;

    //컴포넌트 및 내부변수
    private CharacterController characterController;
    private AudioSource audioSource;
    private float rotationX = 0;
    private Vector3 moveDirection;

    //점프 설정
    public float jumpHeight = 1.5f; 
    private float jumpVelocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // 마우스 커서를 화면 중앙에 잠그고 보이지 않게 설정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 발소리 반복
        if (audioSource != null)
        {
            audioSource.loop = true;
        }
    }

    void Update()
    {
        LookRotation();
        Movement();
        Footsteps();
    }


    // 시야 회전 로직
    private void LookRotation()
    {
        // 마우스 입력값 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 카메라 시야 각도 제한
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // 카메라 회전 적용 (카메라를 Player의 자식으로 설정)
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // 몸체 좌우 회전 (Y축)
        transform.Rotate(Vector3.up * mouseX);
    }

    // 플레이어 이동 로직
    private void Movement()
    {
        // 지면에 닿아있을 때만 이동 입력 처리
        if (characterController.isGrounded)
        {
            float x = Input.GetAxis("Horizontal"); // A, D 키 입력
            float z = Input.GetAxis("Vertical");   // W, S 키 입력

            // 플레이어의 방향으로 계산
            moveDirection = (transform.right * x + transform.forward * z);

            // 속도 적용
            moveDirection *= moveSpeed;

            // 지면에 붙어있게 하기 위해 중력 적용
            moveDirection.y = gravity;

            if (Input.GetButtonDown("Jump")) // 점프 구현
            {
                // 점프 속도 공식: v = sqrt(h * -2 * gravity)
                jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // 점프 속도를 moveDirection.y에 할당
            moveDirection.y = jumpVelocity;
        }
        else
        {
            // 공중에 있을 때 (떨어질 때)는 중력만 지속적으로 적용
            moveDirection.y += gravity * Time.deltaTime;
        }

        // 최종 이동 적용
        characterController.Move(moveDirection * Time.deltaTime);
    }


    // 움직일때 발소리 로직
    private void Footsteps()
    {
        // 현재 이동 속도 (Y축 제외) 측정
        Vector3 flatVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);

        // 플레이어가 이동 중인지, 지면에 있는지 판단하고, True면 발소리 재생, False면 발소리 정지 
        if (characterController.isGrounded && flatVelocity.magnitude > 0.1f)
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
}

//맵에 연결할 부분

//스크립트 부착: 이 PlayerMovement.cs 스크립트를 플레이어에 연결

//컴포넌트 추가: 플레이어 오브젝트에 CharacterController와 AudioSource 컴포넌트 추가

//카메라 설정: 메인 카메라를 플레이어 오브젝트의 자식 오브젝트로 만들어야함

//AudioClip에 발소리 파일 연결