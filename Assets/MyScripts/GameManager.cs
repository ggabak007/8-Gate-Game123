using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int currentStageIndex = 1; // 현재 날짜 (1일차)
    public int maxDays = 4;           // 목표 (4일차)

    public Transform startPoint;      // 플레이어가 돌아올 위치 (StartPoint)
    public GameObject player;          //플레이어

    private AnomalyManager anomalyManager; // 코드에서 찾아 연결

    private void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
        }
        else 
        {
            Destroy(gameObject); 
        }
    }

    void Start() 
    {
        anomalyManager = FindFirstObjectByType<AnomalyManager>();
        if (anomalyManager == null) Debug.LogError("GameManager: AnomalyManager가 없습니다!");

        StartNewDay();
    }

    // [핵심] 씬 로딩 없이, 위치만 옮겨서 하루를 시작하는 함수
    public void StartNewDay(Vector3 offset = default(Vector3))
    {
        Debug.Log(currentStageIndex + "일차 시작");
        // 이상현상 랜덤 배치 (AnomalyManager에게 시킴)
        if (anomalyManager != null)
        {
            anomalyManager.ResetStage();
        }


        // 플레이어 위치를 시작점으로 강제 이동하면 화면이 뚝 끊기는 현상을 방지하기위해 
        // 씬이 바뀔때 플레이어의 위치를 씬의 시작지점 중앙값(스테이지1에서 시작하는 공간)에서 플레이어의 거리를 계산하여 offset값으로 전달,
        //씬이 바뀔때 offset값을 더해주어 자연스럽게 이동하는 효과를 줌

        if (player != null && startPoint != null)
        {
            // (중요) CharacterController가 켜져 있으면 강제 이동이 안 될 때가 있음
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // 잠시 끄고

            player.transform.position = startPoint.position + offset ; // 이동 (회전 삭제)

            if (cc != null) cc.enabled = true; // 다시 켬
        }

        Debug.Log($"{currentStageIndex}일차");
    }

    // 다음 단계(다음 날)로 이동
    public void GoToNextStage(Vector3 offset = default(Vector3))
    {
        currentStageIndex++;

        if (currentStageIndex > maxDays)
        {
            GameClear(); // 게임 클리어
        }
        else
        {
            Debug.Log($"통과! {currentStageIndex}일차로 넘어갑니다.");
            StartNewDay(offset); // 씬 로딩 없이 바로 다음 날 시작
        }
    }

    // 실패해서 처음으로 리셋
    public void ResetToStage1()
    {
        Debug.Log("실패! 1일차로 돌아갑니다.");
        currentStageIndex = 1;
        StartNewDay(Vector3.zero); // 1일차, 스테이지 시작지점으로 이동(시작지점을 (0,0,0)으로 설정, 맵 구현시 바꾸거나 맞추어주어야함)
    }


    private void GameClear()
    {
        Debug.Log("🎉 탈출 성공! 게임 클리어! 🎉");
        SceneLoader.Instance.LoadClearScene(); // 엔딩씬 로드
    }
}
