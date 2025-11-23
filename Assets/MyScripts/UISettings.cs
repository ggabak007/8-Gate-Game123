using UnityEngine;
using UnityEngine.UI; // UI(슬라이더 등)를 다루기 위해 필수

public class UISettings : MonoBehaviour
{
    [Header("UI 오브젝트 연결")]
    public GameObject pauseMenuPanel; // ESC 눌렀을 때 뜰 패널 (배경+버튼들)
    public Slider sensitivitySlider;  // 마우스 감도 조절 슬라이더
    public Slider volumeSlider;       // 소리 조절 슬라이더

    [Header("플레이어 연결")]
    public PlayerMovement playerScript; // 플레이어의 감도 설정을 바꾸기 위해 필요

    // 내부 변수
    private bool isPaused = false;

    void Start()
    {
        // 1. 시작할 때 메뉴는 꺼두기
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // 2. 플레이어 스크립트 자동 찾기 (연결 안 했을 경우 대비)
        if (playerScript == null) playerScript = FindFirstObjectByType<PlayerMovement>();

        // 3. 슬라이더 초기값 세팅 (현재 적용된 값으로)
        if (sensitivitySlider != null && playerScript != null)
        {
            sensitivitySlider.value = playerScript.mouseSensitivity;
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume; // 전체 볼륨 가져오기
        }
    }

    void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // 일시정지 토글 함수
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // [일시정지 상태]
            pauseMenuPanel.SetActive(true); // 메뉴 켜기
            Time.timeScale = 0f;            // 시간 멈춤

            // 마우스 커서 보이게 하고 풀기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // [게임 재개]
            pauseMenuPanel.SetActive(false); // 메뉴 끄기
            Time.timeScale = 1f;             // 시간 흐름

            // 마우스 커서 다시 중앙에 잠그고 숨기기
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // [슬라이더 이벤트용] 마우스 감도 조절
    public void SetSensitivity(float val)
    {
        if (playerScript != null)
        {
            playerScript.mouseSensitivity = val;
        }
    }

    // [슬라이더 이벤트용] 소리 크기 조절
    public void SetVolume(float val)
    {
        AudioListener.volume = val; // 게임 전체 소리 조절
    }

    // [버튼 이벤트용] 게임 종료
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 눌림!"); // 에디터에서는 안 꺼지므로 로그로 확인
        Application.Quit();
    }
}