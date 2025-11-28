using UnityEngine;

public class Door : MonoBehaviour
{
    public float openAngle = 90f;
    public float smoothSpeed = 2.0f; // 문이 열리는 속도 (클수록 빠름)

    public AudioClip doorSound;      // 문 열리고 닫힐 때 나는 소리
    public AnomalyObject linkedAnomaly; // 냉장고 안의 침대(이상현상)를 여기에 연결

    // 내부 변수
    private bool isOpen = false;
    private Quaternion defaultRotation; // 닫혀있을 때의 회전값 (0도)
    private Quaternion targetRotation;  // 목표 회전값
    private AudioSource audioSource;

    void Start()
    {
        // 게임 시작 시 현재 각도를 '닫힌 상태'로 기억
        defaultRotation = transform.localRotation;

        // 오디오 소스 가져오기 (없으면 안 씀)
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 1. 목표 각도 계산
        Quaternion openRot = defaultRotation * Quaternion.Euler(0, openAngle, 0);
        targetRotation = isOpen ? openRot : defaultRotation;

        // 2. 부드럽게 회전 (Slerp)
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    // [플레이어 상호작용] E키를 눌렀을 때 호출
    public void ToggleDoor()
    {
        isOpen = !isOpen; // 열림/닫힘 상태 반전
        PlaySound();

        if (!isOpen && linkedAnomaly != null)
        {
            // "인형을 놓고(solveOnDoorClose)" + "준비 완료(isReadyToSolve)" 상태라면
            if (linkedAnomaly.solveOnDoorClose && linkedAnomaly.isReadyToSolve)
            {
                linkedAnomaly.FinalizeSolve(); // 최종 해결 처리!
            }
        }
    }

    // [트리거용] 문을 강제로 닫을 때 호출 (루프 연출용)
    public void ForceClose()
    {
        isOpen = false;
        // 즉시 닫힌 상태로 보내려면 아래 주석 해제 (지금은 부드럽게 닫힘)
        // transform.localRotation = defaultRotation; 
    }

    // 사운드 재생 함수
    private void PlaySound()
    {
        if (audioSource != null && doorSound != null)
        {
            // 소리가 겹치지 않게 피치(음정)를 살짝 랜덤으로 주면 더 리얼함
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(doorSound);
        }
    }

    public void SetOpenState()
    {
        isOpen = true; // 열린 상태로 변경

        // 목표 각도를 열린 각도로 설정
        Quaternion openRot = defaultRotation * Quaternion.Euler(0, openAngle, 0);
        targetRotation = openRot;

        // 부드럽게 열리는 게 아니라, 처음부터 열려있어야 하니까 즉시 적용
        transform.localRotation = openRot;
    }
}