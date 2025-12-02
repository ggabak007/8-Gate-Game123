using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuCtrl : MonoBehaviour
{
    [Header("연결할 UI")]
    public GameObject optionPanel;   // 옵션 창 패널
    public Slider sensitivitySlider; // 감도 슬라이더
    public Slider volumeSlider;      // 소리 슬라이더

    void Start()
    {
        // [핵심] 메인 메뉴 시작하자마자 마우스 커서 보이게 하기!
        Cursor.lockState = CursorLockMode.None; // 잠금 해제
        Cursor.visible = true;                  // 커서 보이기

        // 1. 시작할 때 옵션창 끄기
        if (optionPanel != null)
            optionPanel.SetActive(false);

        // 2. 저장된 설정값 불러와서 슬라이더에 반영
        float savedSens = PlayerPrefs.GetFloat("Sensitivity", 100f);
        float savedVol = PlayerPrefs.GetFloat("Volume", 1f);

        if (sensitivitySlider != null)
            sensitivitySlider.value = savedSens;

        if (volumeSlider != null)
            volumeSlider.value = savedVol;

        AudioListener.volume = savedVol;
    }

    public void ClickStart()
    {
        // 게임 씬으로 이동 (씬 이름이 'morgue'가 맞는지 확인!)
        SceneManager.LoadScene("morgue");
    }

    public void ClickOption()
    {
        // 옵션 버튼 누르면 창 켜기
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    public void ClickCloseOption()
    {
        // 닫기 버튼 누르면 창 끄고 저장
        if (optionPanel != null)
            optionPanel.SetActive(false);

        PlayerPrefs.Save();
    }

    public void ClickExit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    // 메인 메뉴 버튼에 연결할 필요는 없지만, 혹시 몰라 둠
    public void ClickMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 슬라이더 조절 시 호출
    public void SetSensitivity(float val)
    {
        PlayerPrefs.SetFloat("Sensitivity", val);
    }

    public void SetVolume(float val)
    {
        AudioListener.volume = val;
        PlayerPrefs.SetFloat("Volume", val);
    }
}