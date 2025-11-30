using UnityEngine;
using System.Collections.Generic; // 리스트를 쓸 경우 필요

public class AnomalyManager : MonoBehaviour
{
    // [설정] 외부에서 읽을 수는 있지만(get), 맘대로 바꿀 순 없음(private set)
    public bool Is_Anomaly_Present { get; private set; } = false; // 이상현상이 존재하는지 확인
    public bool Is_Anomaly_Solved { get; set; } = false; // 이상현상을 해결했는지 확인

    [Header("설정")]
    // 인스펙터에서 이상현상 오브젝트들(귀신, 피, 의자 등)을 여기에 다 넣으세요
    public GameObject[] anomalyObjects;

    // 75 미만이면 이상현상 발생 (즉, 75% 확률로 발생)
    private const int ANOMALY_THRESHOLD = 100;

    // 현재 켜져있는 이상현상을 기억하는 변수
    private GameObject currentActiveAnomaly = null;
    private List<int> availableIndices = new List<int>();
    void Start()
    {

    }

    // [추가] 1일차에만 호출해서 리스트를 꽉 채우는 함수
    public void FullResetPool()
    {
        availableIndices.Clear();
        for (int i = 0; i < anomalyObjects.Length; i++)
        {
            availableIndices.Add(i); // 0, 1, 2... 번호표 넣기
        }
        Debug.Log("이상현상 풀(Pool) 초기화 완료!");
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

    // ★ [핵심 수정] 남은 것 중에서만 뽑기
    private void SpawnRandomAnomaly()
    {
        if (anomalyObjects == null || anomalyObjects.Length == 0) return;

        // 만약 남은 이상현상이 없다면? (다 보여줬으면) -> 다시 리필하거나, 그냥 아무거나 뽑음
        if (availableIndices.Count == 0)
        {
            Debug.LogWarning("모든 이상현상을 다 소진했습니다! 다시 섞습니다.");
            FullResetPool();
        }

        // 1. 남은 리스트(availableIndices) 중에서 랜덤한 '순번'을 뽑음
        int listIndex = Random.Range(0, availableIndices.Count);

        // 2. 그 순번 안에 들어있는 진짜 '이상현상 ID'를 가져옴
        int realAnomalyID = availableIndices[listIndex];

        // 3. 사용했으니 리스트에서 삭제! (중복 방지)
        availableIndices.RemoveAt(listIndex);

        // 4. 활성화
        currentActiveAnomaly = anomalyObjects[realAnomalyID];
        if (currentActiveAnomaly != null)
        {
            currentActiveAnomaly.SetActive(true);

            // ID 부여 (리스트 순서가 아닌 실제 ID를 부여)
            AnomalyObject anomalyScript = currentActiveAnomaly.GetComponent<AnomalyObject>();
            if (anomalyScript != null)
            {
                anomalyScript.SetAnomalyID(realAnomalyID);
            }

            Debug.Log($"활성화된 이상현상 ID: {realAnomalyID} (남은 후보: {availableIndices.Count}개)");
        }
    }

    public void NotifyAnomalySolved()
    {
        if (Is_Anomaly_Present) Is_Anomaly_Solved = true;
    }

}
