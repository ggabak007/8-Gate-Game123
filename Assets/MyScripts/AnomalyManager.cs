using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public bool Is_Anomaly_Present { get; private set; } = false; // 이상현상이 존재하는지
    public bool Is_Anomaly_Solved { get; set; } = false; // 플레이어가 이상현상을 해결했는지

    // 현재 씬에 있는 모든 잠재적 이상현상 오브젝트 (인스펙터에서 연결) - 각각의 코드로 작성후 연결
    public GameObject[] anomalyObjects;

    // 확률 상수: 0~100 사이, 75 미만이면 이상현상 발생 (75% 확률)
    private const int ANOMALY_THRESHOLD = 75;

    void Start()
    {
        InitializeStage();
    }

    //1. 스테이지 초기화 및 확률 계산 

    public void InitializeStage()
    {

        // 확률 계산 (75% 확률로 이상현상 발생)
        int randomValue = Random.Range(0, 100);

        if (randomValue < ANOMALY_THRESHOLD) // 75%확률로 이상현상 발생
        {
            Is_Anomaly_Present = true;
            // 디버깅용 코드 Debug.Log("AnomalyManager: [변칙 발생] - 확률값: " + randomValue);
        }
        else //25%확률로 스테이지 그냥 진행
        {
            Is_Anomaly_Present = false;
            // 디버깅용 코드 Debug.Log("AnomalyManager: [정상] - 확률값: " + randomValue);
        }

        // 오브젝트 활성화/비활성화 적용
        SetAnomalyObjectState(Is_Anomaly_Present);
    }

    // 2. 이상현상 오브젝트 상태 설정 
    private void SetAnomalyObjectState(bool isActive)
    {
        if (anomalyObjects != null)
        {
            foreach (GameObject obj in anomalyObjects)
            {
                // 이상현상이 존재하면 오브젝트를 활성화,아니면 비활성화
                obj.SetActive(isActive);
            }
        }
    }

    // 3. 외부(AnomalyObject)에서 호출되는 함수
    public void NotifyAnomalySolved()
    {
        if (Is_Anomaly_Present)
        {
            Is_Anomaly_Solved = true;
            // 디버깅용 코드 Debug.Log("AnomalyManager: 이상현상 해결 완료");

            // 만약 귀신 등장을 원하면  if문을 사용해 해결후 바로 등장하게 설정
        }
    }