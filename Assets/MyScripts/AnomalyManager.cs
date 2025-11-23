using UnityEngine;
// using System.Collections.Generic; // 리스트를 쓸 경우 필요

public class AnomalyManager : MonoBehaviour
{
    // [설정] 외부에서 읽을 수는 있지만(get), 맘대로 바꿀 순 없음(private set)
    public bool Is_Anomaly_Present { get; private set; } = false;
    public bool Is_Anomaly_Solved { get; set; } = false;

    [Header("설정")]
    // 인스펙터에서 이상현상 오브젝트들(귀신, 피, 의자 등)을 여기에 다 넣으세요
    public GameObject[] anomalyObjects;

    // 75 미만이면 이상현상 발생 (즉, 75% 확률로 발생)
    private const int ANOMALY_THRESHOLD = 75;

    // 현재 켜져있는 이상현상을 기억하는 변수
    private GameObject currentActiveAnomaly = null;

    void Start()
    {
        // 게임 시작 시 초기화
        ResetStage();
    }

    // 1. 스테이지 초기화 (다음 날로 넘어갈 때도 이 함수를 부르면 됨)
    public void ResetStage()
    {
        // (1) 기존 상태 초기화
        Is_Anomaly_Solved = false;

        // 켜져있던 이상현상이 있다면 끄기
        if (currentActiveAnomaly != null)
        {
            currentActiveAnomaly.SetActive(false);
            currentActiveAnomaly = null;
        }

        // 혹시 모르니 모든 이상현상 끄기 (안전장치)
        if (anomalyObjects != null)
        {
            foreach (var obj in anomalyObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // (2) 확률 계산
        CalculateAnomalyChance();
    }

    private void CalculateAnomalyChance()
    {
        int randomValue = Random.Range(0, 100);

        if (randomValue < ANOMALY_THRESHOLD)
        {
            // [이상현상 발생!]
            Is_Anomaly_Present = true;
            SpawnRandomAnomaly(); // 하나만 골라서 켜기
            Debug.Log($"<color=red>[변칙 발생!] 확률값: {randomValue}</color>");
        }
        else
        {
            // [정상]
            Is_Anomaly_Present = false;
            Debug.Log($"<color=green>[정상 스테이지] 확률값: {randomValue}</color>");
        }
    }

    // 2. 랜덤으로 하나만 골라서 켜는 함수 (코드의 문제점 수정)
    private void SpawnRandomAnomaly()
    {
        if (anomalyObjects == null || anomalyObjects.Length == 0)
        {
            Debug.LogWarning("AnomalyManager: 할당된 이상현상 오브젝트가 없습니다!");
            return;
        }

        // 배열 중에서 랜덤하게 하나 뽑기
        int randomIndex = Random.Range(0, anomalyObjects.Length);

        currentActiveAnomaly = anomalyObjects[randomIndex];

        if (currentActiveAnomaly != null)
        {
            currentActiveAnomaly.SetActive(true);
            Debug.Log($"활성화된 이상현상: {currentActiveAnomaly.name}");
        }
    }

    // 3. 외부(AnomalyObject)에서 호출되는 함수
    public void NotifyAnomalySolved()
    {
        if (Is_Anomaly_Present)
        {
            Is_Anomaly_Solved = true;
            Debug.Log("AnomalyManager: 플레이어가 이상현상을 해결했습니다!");

            // (선택사항) 해결되자마자 눈앞에서 사라지게 하고 싶으면 아래 주석 해제
            // if (currentActiveAnomaly != null) currentActiveAnomaly.SetActive(false);
        }
    }
} // (코드의 문제점 수정)