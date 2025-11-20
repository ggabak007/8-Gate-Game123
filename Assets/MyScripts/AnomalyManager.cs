using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    // ====== 핵심 상태 변수 (Public Access) ======
    public bool Is_Anomaly_Present { get; private set; } = false;
    public bool Is_Anomaly_Solved { get; set; } = false;

    // 현재 씬에 있는 모든 잠재적 이상현상 오브젝트 (인스펙터에서 연결)
    public GameObject[] anomalyObjects;

    // 확률 상수: 0~100 사이, 76 미만이면 이상현상 발생 (76% 확률)
    private const int ANOMALY_THRESHOLD = 76;

    void Start()
    {
        InitializeStage();
    }

    // ====== 1. 스테이지 초기화 및 확률 계산 ======
    public void InitializeStage()
    {
        // 1. 상태 초기화
        Is_Anomaly_Solved = false;

        // 2. 확률 계산 (76% 확률로 이상현상 발생)
        int randomValue = Random.Range(0, 100);

        if (randomValue < ANOMALY_THRESHOLD)
        {
            Is_Anomaly_Present = true;
            Debug.Log("AnomalyManager: [변칙 발생] - 확률값: " + randomValue);
        }
        else
        {
            Is_Anomaly_Present = false;
            Debug.Log("AnomalyManager: [정상] - 확률값: " + randomValue);
        }

        // 3. 오브젝트 활성화/비활성화 적용
        SetAnomalyObjectState(Is_Anomaly_Present);
    }

    // ====== 2. 이상현상 오브젝트 상태 설정 ======
    private void SetAnomalyObjectState(bool isActive)
    {
        if (anomalyObjects != null)
        {
            foreach (GameObject obj in anomalyObjects)
            {
                // 이상현상이 존재하면 오브젝트를 활성화하고, 아니면 비활성화합니다.
                obj.SetActive(isActive);
            }
        }
    }

    // ====== 3. 외부(AnomalyObject)에서 호출되는 함수 ======
    public void NotifyAnomalySolved()
    {
        if (Is_Anomaly_Present)
        {
            Is_Anomaly_Solved = true;
            Debug.Log("AnomalyManager: 이상현상 해결 완료 상태로 설정됨.");

            // Stage 3일 경우, 여기서 GhostEventController를 호출할 수 있습니다.
            // if (SceneManager.GetActiveScene().buildIndex == 3)
            // {
            //     FindObjectOfType<GhostEventController>()?.Stage3_OnSolve();
            // }
        }
    }
}