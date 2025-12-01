using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DayLoadingManager : MonoBehaviour
{
    public static DayLoadingManager Instance;

    [Header("UI References")]
    public CanvasGroup dayLoadingGroup; // DayLoadingImage의 CanvasGroup
    public Image dayLoadingImage;       // 실제 이미지

    [Header("Day Loading Sprites (Day1 = index0)")]
    public Sprite[] daySprites;

    [Header("Settings")]
    public float showTime = 1.0f;   // 이미지 보여주는 시간
    public float fadeTime = 0.3f;   // 페이드 속도

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dayLoadingGroup.alpha = 0f;
        dayLoadingGroup.gameObject.SetActive(false);
    }

    // 날짜 시작 시 호출되는 함수
    public IEnumerator ShowDayLoading(int day)
    {
        // 게임 정지
        Time.timeScale = 0f;

        // 이미지 설정
        dayLoadingImage.sprite = daySprites[day - 1];

        // UI 활성화
        dayLoadingGroup.gameObject.SetActive(true);

        // 페이드 인
        yield return StartCoroutine(FadeCanvasGroup(dayLoadingGroup, 0f, 1f, fadeTime));

        // 지정된 시간 동안 이미지 유지
        yield return new WaitForSecondsRealtime(showTime);

        // 페이드 아웃 (중요: unscaledDeltaTime 사용)
        yield return StartCoroutine(FadeCanvasGroup(dayLoadingGroup, 1f, 0f, fadeTime));

        // UI 비활성화
        dayLoadingGroup.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }


    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float t = 0f;
        cg.alpha = start;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        cg.alpha = end;
    }

}
