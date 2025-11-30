using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public int currentStageIndex = 1; // 현재 날짜 (1일차)
    public int maxDays = 4;           // 목표 (4일차)

    [Header("연결 필수")]
    public Transform startPoint;      // 플레이어 시작 위치
    public GameObject player;         // 플레이어
    public AnomalyManager anomalyManager;
    private ToolManager toolManager;

    [Header("코너 트리거 (새로 추가됨)")]
    // 코너를 막는 벽 (Return Type, 이상현상 있을 때 켜짐)
    public GameObject cornerBlockTrigger;
    // 코너를 열어주는 문 (Exit Type, 이상현상 없을 때 켜짐)
    public GameObject cornerPassTrigger;

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

    // [핵심] 하루 시작 (위치 이동 + 트리거 교체)
    public void StartNewDay(Vector3 offset = default(Vector3))
    {
        Debug.Log($"{currentStageIndex}일차 시작");
        if (currentStageIndex == 1)
        {
            anomalyManager.FullResetPool();
        }
        // 1. 이상현상 리셋 & 생성
        if (anomalyManager != null)
        {
            anomalyManager.ResetStage();

            // [추가된 부분] 이상현상 유무에 따라 코너 벽 교체
            // 이상현상이 있으면 -> Block(Return)을 켜서 못 지나가게 함
            // 이상현상이 없으면 -> Pass(Exit)를 켜서 지나가게 함
            bool isAnomaly = anomalyManager.Is_Anomaly_Present;
            SetCornerTriggers(isAnomaly);
        }
        if (toolManager != null)
        {
            toolManager.ResetAllTools();
        }

        // [추가] 플레이어 손도 비워줘야 함 (들고 있던 거 강제 반납)
        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            // 인벤토리 변수만 초기화 
            inv.ForceClearHand(); 
        }
        // 2. 플레이어 위치 이동 (오프셋 적용 - 님이 만드신 로직 유지)
        if (player != null && startPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 위치 이동
            player.transform.position = startPoint.position + offset;
            // 회전은 시작점 기준 (플레이어가 엉뚱한 곳 보는 것 방지)
            player.transform.rotation = startPoint.rotation;

            if (cc != null) cc.enabled = true;
        }
    }

    // [새 기능] 코너 트리거 스위칭 함수
    private void SetCornerTriggers(bool hasAnomaly)
    {
        if (cornerBlockTrigger == null || cornerPassTrigger == null) return;

        if (hasAnomaly)
        {
            // 이상현상 있음 -> 길을 막는다 (Return 활성화, Exit 비활성화)
            cornerBlockTrigger.SetActive(true);
            cornerPassTrigger.SetActive(false);
            Debug.Log("이상현상 발생! 코너가 막혔습니다. (Return Trigger ON)");
        }
        else
        {
            // 정상 -> 길을 연다 (Return 비활성화, Exit 활성화)
            cornerBlockTrigger.SetActive(false);
            cornerPassTrigger.SetActive(true);
            Debug.Log("정상 복도입니다. 코너가 열렸습니다. (Exit Trigger ON)");
        }
    }

    public void GoToNextStage(Vector3 offset = default(Vector3))
    {
        currentStageIndex++;

        if (currentStageIndex > maxDays)
        {
            GameClear();
        }
        else
        {
            Debug.Log($"통과! {currentStageIndex}일차로 넘어갑니다.");
            StartNewDay(offset);
        }
    }

    public void ResetToStage1()
    {
        Debug.Log("실패! 1일차로 돌아갑니다.");
        currentStageIndex = 1;
        StartNewDay(Vector3.zero); // 정중앙 리셋
    }

    private void GameClear()
    {
        Debug.Log("탈출 성공! 게임 클리어!");
        // [수정됨] SceneLoader 삭제했으므로 직접 이동
        SceneManager.LoadScene("GameClear_Scene");
    }
}