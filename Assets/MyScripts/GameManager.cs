using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public int currentStageIndex = 1; // 현재 날짜 (1일차)
    public int maxDays = 4;           // 목표 (4일차)

    [Header("연결 필수!")]
    public Transform startPoint;      // 플레이어가 돌아올 위치 (StartPoint)
    public GameObject player;         // 플레이어 오브젝트
    public AnomalyManager anomalyManager; // 이상현상 관리자

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 게임 시작하자마자 1일차 세팅 실행
        StartNewDay();
    }

    // [핵심] 씬 로딩 없이, 위치만 옮겨서 하루를 시작하는 함수
    public void StartNewDay()
    {
        // 1. 이상현상 랜덤 배치 (AnomalyManager에게 시킴)
        if (anomalyManager != null)
        {
            anomalyManager.ResetStage();
        }

        // 2. 플레이어 위치를 시작점으로 강제 이동
        if (player != null && startPoint != null)
        {
            // (중요) CharacterController가 켜져 있으면 강제 이동이 안 될 때가 있음
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // 잠시 끄고

            player.transform.position = startPoint.position; // 이동시키고
            player.transform.rotation = startPoint.rotation; // 회전시키고

            if (cc != null) cc.enabled = true; // 다시 켬
        }

        Debug.Log($"{currentStageIndex}일차 시작!");
    }

    // 다음 단계(다음 날)로 이동
    public void GoToNextStage()
    {
        currentStageIndex++;

        if (currentStageIndex > maxDays)
        {
            Debug.Log("탈출 성공! 엔딩!");
            // 나중에 엔딩 씬 만들면 아래 주석 풀기
            // SceneManager.LoadScene("EndingScene"); 
        }
        else
        {
            Debug.Log($"통과! {currentStageIndex}일차로 넘어갑니다.");
            StartNewDay(); // 씬 로딩 없이 바로 다음 날 시작
        }
    }

    // 실패해서 처음으로 리셋
    public void ResetToStage1()
    {
        Debug.Log("실패! 1일차로 돌아갑니다.");
        currentStageIndex = 1;
        StartNewDay(); // 1일차 세팅으로 바로 시작
    }
}