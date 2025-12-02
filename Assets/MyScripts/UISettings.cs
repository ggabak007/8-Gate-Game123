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

        // =========================================================
        // [추가됨] 3. 메인 메뉴에서 저장된 설정값 불러오기 (핵심!)
        // =========================================================
        float savedSens = PlayerPrefs.GetFloat("Sensitivity", 100f); // 저장된 감도 (없으면 100)
        float savedVol = PlayerPrefs.GetFloat("Volume", 1f);         // 저장된 소리 (없으면 1)

        // 슬라이더 UI 위치 맞추기
        if (sensitivitySlider != null) sensitivitySlider.value = savedSens;
        if (volumeSlider != null) volumeSlider.value = savedVol;

        // 실제 게임(플레이어/소리)에 적용하기
        if (playerScript != null) playerScript.mouseSensitivity = savedSens;
        AudioListener.volume = savedVol;
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

            PlayerPrefs.Save();
        }
    }

    // [슬라이더 이벤트용] 마우스 감도 조절
    public void SetSensitivity(float val)
    {
        if (playerScript != null)
        {
            playerScript.mouseSensitivity = val;
        }
        PlayerPrefs.SetFloat("Sensitivity", val);
    }

    // [슬라이더 이벤트용] 소리 크기 조절
    public void SetVolume(float val)
    {
        AudioListener.volume = val;
        PlayerPrefs.SetFloat("Volume", val);
    }

    // [버튼 이벤트용] 게임 종료
    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 눌림!");
        Application.Quit();
    }
}