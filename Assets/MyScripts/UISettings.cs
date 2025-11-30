using UnityEngine;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    [Header("UI 오브젝트 연결")]
    public GameObject pauseMenuPanel; // ESC 눌렀을 때 뜰 패널
    public Slider sensitivitySlider;  // 감도 조절 슬라이더
    public Slider volumeSlider;       // 소리 조절 슬라이더

    [Header("플레이어 연결")]
    public PlayerMovement playerScript;

    private bool isPaused = false;

    void Start()
    {
        // 1. [CursorController 기능 통합] 게임 시작 시 마우스 커서 숨기고 잠그기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 2. 시작할 때 메뉴 꺼두기 (안전장치)
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // 3. 플레이어 스크립트 자동 찾기
        if (playerScript == null)
            playerScript = FindFirstObjectByType<PlayerMovement>();

        // 4. 슬라이더 초기값 세팅
        if (sensitivitySlider != null && playerScript != null)
        {
            sensitivitySlider.value = playerScript.mouseSensitivity;
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
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
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f; // 시간 멈춤

            // 마우스 커서 보이게 하고 풀기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // [게임 재개]
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f; // 시간 흐름

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
        AudioListener.volume = val;
    }

    // [버튼 이벤트용] 게임 종료
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 눌림!");
        Application.Quit();
    }
}