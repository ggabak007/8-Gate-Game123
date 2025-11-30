using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public int currentStageIndex = 1;
    public int maxDays = 4;

    [Header("연결 필수")]
    public Transform startPoint;
    public GameObject player;
    public AnomalyManager anomalyManager;

    [Header("코너 트리거")]
    public GameObject cornerBlockTrigger; // Return (막는 벽)
    public GameObject cornerPassTrigger;  // Exit (여는 문)

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (anomalyManager == null)
            anomalyManager = FindFirstObjectByType<AnomalyManager>();

        StartNewDay();
    }

    public void StartNewDay(Vector3 offset = default(Vector3))
    {
        Debug.Log($"{currentStageIndex}일차 시작");

        if (anomalyManager != null)
        {
            anomalyManager.ResetStage();
            // 코너 벽 세팅
            SetCornerTriggers(anomalyManager.Is_Anomaly_Present);
        }

        if (player != null && startPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = startPoint.position + offset;
            player.transform.rotation = startPoint.rotation;

            if (cc != null) cc.enabled = true;
        }
    }

    private void SetCornerTriggers(bool hasAnomaly)
    {
        if (cornerBlockTrigger == null || cornerPassTrigger == null) return;

        if (hasAnomaly) // 이상현상 O -> 막힘
        {
            cornerBlockTrigger.SetActive(true);
            cornerPassTrigger.SetActive(false);
        }
        else // 이상현상 X -> 뚫림
        {
            cornerBlockTrigger.SetActive(false);
            cornerPassTrigger.SetActive(true);
        }
    }

    public void GoToNextStage(Vector3 offset = default(Vector3))
    {
        currentStageIndex++;

        if (currentStageIndex > maxDays)
        {
            GameClear(); // 엔딩
        }
        else
        {
            StartNewDay(offset); // 다음 날 루프
        }
    }

    public void ResetToStage1()
    {
        Debug.Log("실패! 1일차로 돌아갑니다.");
        currentStageIndex = 1;
        StartNewDay(Vector3.zero);
    }

    private void GameClear()
    {
        Debug.Log("탈출 성공! 엔딩 씬으로 이동합니다.");
        // [수정] SceneLoader 없이 직접 이동
        SceneManager.LoadScene("GameClear_Scene");
    }
}