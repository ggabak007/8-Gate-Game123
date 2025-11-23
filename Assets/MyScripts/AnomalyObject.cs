/* using UnityEngine;

[RequireComponent(typeof(Collider))] //콜라이더 자동 추가
public class AnomalyObject : MonoBehaviour
{
 
    public ToolType requiredTool = ToolType.None;

    // 이상현상 해결여부
    private bool isResolved = false;

    private AnomalyManager anomalyManager;
    private PlayerInventory playerInventory;

    void Start()
    {
        // 필요 인스턴스 연결
        anomalyManager = FindFirstObjectByType< AnomalyManager>();
        playerInventory = FindFirstObjectByType< PlayerInventory>();
        anomalyManager = FindFirstObjectByType<AnomalyManager>();
        playerInventory = FindFirstObjectByType<PlayerInventory>();

        if (anomalyManager == null || playerInventory == null)
        {
            Debug.LogError("필수 Manager 스크립트(AnomalyManager 또는 PlayerInventory)를 찾을 수 없습니다! AnomalyObject 작동 불가.", this);
        }

        if (requiredTool == ToolType.None)
        {
            Debug.LogError(gameObject.name + ": 해결에 필요한 requiredTool이 설정되지 않았습니다!", this);
        }
    }

    void Update()
    {
        //구현해야함
    }

    public void TrySolveAnomaly(PlayerInventory inventory)
    {
        //현재 소지한 도구 확인
        ToolType heldTool = inventory.GetCurrentTool();

        if (isResolved)
        {
            Debug.Log("이미 해결된 이상현상입니다.");
            return;
        }
        
        //정답 도구 사용시
        if (heldTool == requiredTool)
        {

            //도구 파괴 요청
            inventory.DestroyToolUsed();

            // 이상현상 해결 연출함수 호출
            ExecuteAnomalyFix();

            // 5. AnomalyManager에 해결성공을 알림
            anomalyManager.NotifyAnomalySolved();

            isResolved = true;
            Debug.Log($"SUCCESS: {requiredTool}을(를) 사용하여 이상현상을 해결했습니다!");
        }
        else //오답도구 사용시
        {
            
            Debug.LogWarning($"FAIL: 이 이상현상에는 {requiredTool}이(가) 필요합니다. 현재 도구: {heldTool}");

            // 페널티 부여 로직 구현 추가 필요

        }
    }



    // 이상현상 해결시 실행되는 연출 함수 ( 추가 구현 필요 )
    private void ExecuteAnomalyFix()
    {
        // 예시 1: 오브젝트 자체를 파괴하거나 숨김 (그림 액자를 떼어낸 경우)
        // gameObject.SetActive(false); 
        // Destroy(gameObject);

        // 예시 2: 텍스처를 깨끗한 것으로 변경 (창문 낙서를 닦은 경우)
        // GetComponent<Renderer>().material.mainTexture = cleanTexture; 
        
    }
}

*/
