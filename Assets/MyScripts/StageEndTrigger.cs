
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageEndTrigger : MonoBehaviour
{
    // ====== 1. 설정 ======
    public enum TriggerType
    {
        ExitDoor,   // 복도 끝 (출구)
        ReturnDoor  // 복도 시작 (되돌아감)
    }

    [Header("설정")]
    [Tooltip("이 트리거가 출구입니까, 아니면 되돌아가는 곳입니까?")]
    public TriggerType triggerType = TriggerType.ExitDoor;

    // ====== 2. 외부 레퍼런스 ======
    private AnomalyManager anomalyManager;

    void Start()
    {
        // 씬 로드 시 AnomalyManager 인스턴스를 찾아 연결
        anomalyManager = FindFirstObjectByType<AnomalyManager>();
        if (anomalyManager == null)
        {
            Debug.LogError("AnomalyManager를 찾을 수 없습니다! 로직 구현 불가.");
        }
    }

    // ====== 3. 트리거 진입 로직 ======
    // 플레이어가 트리거에 닿는 순간 바로 판정합니다. (Exit 로직보다 훨씬 안전함)\
    // 구현할때 empty object로 지나가는 통로를 막는 투명벽(스크립트 연결)을 설치해 지나가도록 판정. Is Trigger 체크 해야함
    // 위치는 입구(스테이지 변경시 시작위치 뒤에 설치해야함) 
    // 출구(스테이지 로직 성공시 다음스테이지, 실패시 1스테이지로 이동)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckCompletion(other);
        }
    }

    // ====== 4. 핵심 판정 로직 ======
    private void CheckCompletion(Collider playerset)
    {
        // 현재 상황 가져오기
        bool hasAnomaly = anomalyManager.Is_Anomaly_Present; // 이상현상 있음?
        bool isSolved = anomalyManager.Is_Anomaly_Solved;    // 해결함?

        // 성공 여부 판단
        bool isSuccess = false;

        // [상황 A] 출구(ExitDoor)에 도착했을 때
        if (triggerType == TriggerType.ExitDoor)
        {
            // 정답: 이상현상이 없을 때(Normal)만 통과 가능
            // (혹은 이상현상이 있었는데 해결하고 오는 건 Return이 아니라 전진인가요? 
            // 보통 8번출구는 해결 후에도 '원래 길'로 가야 하니 여기서는 '없을 때'만 성공으로 칩니다.)

            if (!hasAnomaly)
            {
                isSuccess = true;
                Debug.Log("[정상] 출구 통과 성공!");
            }
            else
            {
                // 이상현상이 있는데 출구로 옴 -> 실패
                Debug.LogWarning("[실패] 이상현상이 있는데 출구로 옴!");
                isSuccess = false;
            }
        }
        // [상황 B] 되돌아가는 문(ReturnDoor)에 도착했을 때
        else if (triggerType == TriggerType.ReturnDoor)
        {
            // 정답: 이상현상이 있고 + 해결했을 때 통과 가능
            if (hasAnomaly && isSolved)
            {
                isSuccess = true;
                Debug.Log("[해결] 이상현상 해결 후 복귀 성공!");
            }
            // 실패 - 이상현상 해결하지않고 되돌아가는 문으로 이동
            else if (hasAnomaly && !isSolved)
            {
                Debug.LogWarning("[실패] 이상현상을 해결하지 않고 도망침!");
                isSuccess = false;
            }
            else
            {
                // 이상현상도 없는데 돌아옴 -> 실패
                Debug.LogWarning("[실패] 아무것도 없는데 돌아옴!");
                isSuccess = false;
            }
        }

        // 결과 처리
        if (isSuccess)
        {
            Vector3 offset = playerset.transform.position - transform.position;
            GameManager.Instance.GoToNextStage(offset); // 다음스테이지로
        }
        else
        {
            GameManager.Instance.ResetToStage1(); // 1스테이지로
        }
    }
}

// 맵 시작과 끝에 투명블록 설치해야함