using UnityEngine;
using UnityEngine.SceneManagement; 

public class StageEndTrigger : MonoBehaviour
{
    // ====== 1. 외부 변수 및 레퍼런스 ======

    // AnomalyManager 인스턴스 (현재 스테이지의 변칙 정보를 가져옴)
    // 실제 프로젝트에서는 Singleton 패턴 등으로 접근하는 것이 효율적입니다.
    private AnomalyManager anomalyManager;

    // 플레이어 
    public Transform player;

    // 플레이어의 위치
    private Vector3 entryPosition;

    // 트리거 활성화 플래그 (중복 판정 방지)
    private bool isTriggerActive = false;

    // ====== 2. 초기화 ======

    void Start()
    {
        // 씬 로드 시 AnomalyManager 인스턴스를 찾아 연결
        anomalyManager = FindObjectOfType<AnomalyManager>();
        if (anomalyManager == null)
        {
            Debug.LogError("AnomalyManager를 찾을 수 없습니다! 로직 구현 불가.");
        }

        // 플레이어 오브젝트 연결 (Inspector에서 연결하거나, 태그로 찾습니다)
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    // ====== 3. 트리거 진입/탈출 로직 ======

    private void OnTriggerEnter(Collider other) // 트리거 진입
    {
        // 플레이어가 진입했는지 확인
        if (other.CompareTag("Player") && !isTriggerActive)
        {
            entryPosition = player.position; // 진입 위치 저장
            isTriggerActive = true;
            // 디버깅 코드
            // Debug.Log("스테이지 끝 트리거");
        }
    }

    private void OnTriggerExit(Collider other) // 트리거 탈출
    {
        // 플레이어가 트리거를 완전히 벗어났을 때 최종 판정 실행
        if (other.CompareTag("Player") && isTriggerActive)
        {
            isTriggerActive = false;

            // 플레이어가 벗어난 위치
            Vector3 exitPosition = player.position;

            // 이동 방향 판단 및 최종 로직 실행
            CheckStageCompletion(entryPosition, exitPosition);
        }
    }

    // ====== 4. 핵심 판정 로직 (역방향 성공 로직) ======

    private void CheckStageCompletion(Vector3 entry, Vector3 exit)
    {
        // 씬 진행 방향 (복도의 Z축 방향이라고 가정) , 맵 구현시 수정 가능성 있음
        // 플레이어가 앞으로 전진했는지 뒤로 돌아갔는지 판단
        // 복도 끝을 향하는 방향을 +Z로 가정

        bool movedForward = exit.z > entry.z;
        bool movedBackward = exit.z < entry.z;

        // 이상현상 유무 및 해결 여부 확인
        bool anomalyPresent = anomalyManager.Is_Anomaly_Present; // AnomalyManager에서 가져옴
        bool anomalySolved = anomalyManager.Is_Anomaly_Solved;   // AnomalyManager에서 가져옴

        bool success = false;

        // 1. 이상 현상 O: 해결했고, 뒤로 돌아감
        if (anomalyPresent && anomalySolved && movedBackward)
        {
            success = true;
            // 디버깅용 코드  Debug.Log("이상현상 해결 후 되돌아감.");
        }
        // 2.이상 현상 X: 해결 시도 없이, 앞으로 전진함
        else if (!anomalyPresent && !anomalySolved && movedForward)
        {
            success = true;
            // 디버깅용 코드 Debug.Log("이상현상 없음 인지 후 전진.");
        }

        // 최종 결과 처리
        if (success)
        {
            // 성공 시 다음 스테이지로 이동 , 스테이지 설계에 따라 구현
            // SceneLoader.Instance.LoadNextStage(); 
            // 디버깅용 코드 Debug.Log("스테이지 통과 성공! 다음 씬으로 이동.");
        }
        else
        {
            // 실패 시 1 스테이지로 강제 리셋
            // SceneLoader.Instance.ResetToStage1();
            // 디버깅용 코드 Debug.LogWarning("FAIL! 로직 위반 또는 변칙 미해결. Stage 1로 리셋!");
        }
    }
}