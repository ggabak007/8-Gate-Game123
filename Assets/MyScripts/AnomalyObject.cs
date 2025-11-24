using UnityEngine;

[RequireComponent(typeof(Collider))] //콜라이더 자동 추가
public class AnomalyObject : MonoBehaviour
{

    public ToolType requiredTool = ToolType.None; //발생한 이상현상을 해결할 때 필요한 도구 타입

    // 이상현상 해결여부
    private bool isResolved = false;

    private AnomalyManager anomalyManager; // 이상현상 해결 참조를 위해 필요
    private PlayerInventory playerInventory; // 플레이어가 가진 도구 확인 및 사용한 도구 처리(파괴)

    void Start()
    {
        // 필요 인스턴스 연결
        anomalyManager = FindFirstObjectByType<AnomalyManager>();
        playerInventory = FindFirstObjectByType<PlayerInventory>();

        //오류 체크 ( 완성시 제거 가능 )
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
        // 현재는 필요 x
    }

    public void TrySolveAnomaly(PlayerInventory inventory)
    {
        //현재 소지한 도구 확인
        ToolType heldTool = inventory.GetCurrentTool();

        //이미 해결된 이상현상일 경우
        if (isResolved)
        {
            Debug.Log("이미 해결된 이상현상입니다.");
            return;
        }

        //정답 도구 사용시 [ 성공 조건 ]
        if (heldTool == requiredTool)
        {

            //도구 파괴 요청(인벤토리에서 제거) 
            //사용한 도구를 어떻게 처리하는지에 따라서 수정 필요
            inventory.DestroyToolUsed();

            // 이상현상 해결 연출함수 호출

            ExecuteAnomalyFix();

            // AnomalyManager에 해결성공을 알림
            anomalyManager.NotifyAnomalySolved();

            isResolved = true;
            Debug.Log($"SUCCESS: {requiredTool}을(를) 사용하여 이상현상을 해결했습니다!");
        }
        else //오답도구 사용시 [ 실패 조건 ]
        {

            Debug.LogWarning($"FAIL: 이 이상현상에는 {requiredTool}이(가) 필요합니다. 현재 도구: {heldTool}");

            // 페널티 부여 로직 함수 구현 추가 필요
            ExecuteFailFix();

        }
    }



    // 이상현상 해결시 실행되는 연출 함수 ( 추가 구현 필요 )
    //그림은 바닥으로 떨어뜨리고 부신다거나, 낙서는 깨끗하게 지워지는 연출 등 구현
    private void ExecuteAnomalyFix()
    {
        // 예시 1: 오브젝트 자체를 파괴하거나 숨김 (그림 액자를 떼어낸 경우)
        // gameObject.SetActive(false); - 오브젝트를 씬에서 숨긴다.
        // Destroy(gameObject); - 오브젝트를 완전히 파괴한다.

        // 예시 2: 텍스처를 깨끗한 것으로 변경 (창문 낙서를 닦은 경우)
        // GetComponent<Renderer>().material.mainTexture = cleanTexture; - 텍스쳐 교체 ( 추가 변수 및 컴포넌트 필요 )
        // 

    }


    // 이상현상 실패시(도구를 잘못 사용했거나 다른 물체에 도구 사용) 실행되는 연출 함수 ( 추가 구현 필요 )
    private void ExecuteFailFix()
    {
        // 예를 들면 시야가 흐려진다던지, 화면이 일시적으로 어두워지거나 섬광효과를 주거나 공포 소리를 주는 등의 연출
    }
}