using UnityEngine;
/* 기초설정


player에 메인카메라, 손 위치 (empty GameObject) 추가
그리고 습득할수있는 물건 프리팹에 Tools 스크립트 추가 및 ToolType 설정

e키를 누르면 물건 습득, q키를 누르면 물건 드랍
물건을 지니고있을때 e키 누를시 물건 스위칭
*/

public class PlayerInventory : MonoBehaviour
{
    //private AnomalyManager anomalyManager;
    //플레이어가 소지한 도구
    public ToolType currentTool = ToolType.None;
    public Transform handPosition;      // 도구가 부착될 위치 (카메라 자식 오브젝트 추천)
    public float interactDistance = 3f; // 상호작용 거리 ( 레이캐스팅
    public float dropForce = 2.0f;

    // 이전에 들고 있던 도구 오브젝트 (스위칭 및 드랍을 위해 필요)
    private GameObject heldToolObject = null;
    /*void Start()
    {
        anomalyManager = FindFirstObjectByType<AnomalyManager>();
        sceneLoader = FindFirstObjectByType<SceneLoader>();
    }
    */
    void Update()
    {
        // E 키를 누르면 습득/스위칭
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        // Q 키를 누르면 드랍
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropTool();
        }
    }

    // E키 상호작용
    private void TryInteract()
    {
        RaycastHit hit;

        // 레이캐스팅으로 도구 오브젝트 감지
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactDistance))
        {
            // 상호작용한 도구가 'InteractableTool' 컴포넌트를 가지고 있는지 확인
            Tools tool = hit.collider.GetComponent<Tools>();

            if (tool != null )
            {
                SwitchTool(tool);
                return;
            }
            //이상현상 사용 로직
            AnomalyObject anomaly = hit.collider.GetComponent<AnomalyObject>();
            if (anomaly != null)
            {
                anomaly.TrySolveAnomaly(this);
                return;
            }

            Door door = hit.collider.GetComponentInParent<Door>();
            if (door != null)
            {
                door.ToggleDoor();
            }

        }
    }

    private void SwitchTool(Tools newTool)
    {
        // 1. 현재 도구가 있다면 (스위칭)
        if (currentTool != ToolType.None)
        {
            DropTool();
        }

        // 2. 새로운 도구를 습득하고 상태 업데이트
        currentTool = newTool.toolType;
        heldToolObject = newTool.gameObject;

        if (handPosition != null)
        {
            heldToolObject.transform.SetParent(handPosition);
            heldToolObject.transform.localPosition = newTool.gripPosition;
            heldToolObject.transform.localRotation = Quaternion.Euler(newTool.gripRotation);
        }
        else
        {
            // handPosition이 없다면 오브젝트를 숨김
            heldToolObject.SetActive(false);
        }

        Debug.Log($"도구 스위칭: {newTool.toolType} 습득.");

        //물체 물리효과 비활성화
        Rigidbody rb = heldToolObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = heldToolObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }


    // Q키 상호작용
    public void DropTool()
    {
        if (currentTool != ToolType.None && heldToolObject != null)
        {
            // 1. 손에서 해제 및 월드 좌표계로 복귀
            heldToolObject.transform.SetParent(null);

            // 2. 물리 활성화 및 힘 부여
            Rigidbody rb = heldToolObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // 물리 엔진에 다시 맡김

                // 플레이어의 시야 방향(Camera.main.transform.forward)으로 힘을 가해 살짝 밀어냅니다.
                rb.AddForce(Camera.main.transform.forward * dropForce, ForceMode.Impulse);
            }

            // 3. 콜라이더 활성화 (바닥과 충돌하고 다시 주울 수 있게)
            Collider col = heldToolObject.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            // 4. 인벤토리 상태 초기화
            currentTool = ToolType.None;
            heldToolObject = null;
            Debug.Log("도구를 내려놓았습니다.");
        }
        else
        {
            Debug.Log("내려놓을 도구가 없습니다.");
        }
    }


    //도구 사용 및 파괴 (AnomalyObject에서 호출- 이상현상에 맞는 도구면 호출, 아닐시 페널티 부여)
    public void DestroyToolUsed()
    {
        if (currentTool != ToolType.None)
        {
            Debug.Log($"{currentTool}이(가) 이상현상 해결에 사용되어 파괴되었습니다.");

            // 1. 오브젝트 파괴
            if (heldToolObject != null)
            {
                Destroy(heldToolObject);
            }

            // 2. 인벤토리 상태 초기화
            currentTool = ToolType.None;
            heldToolObject = null;
        }
    }

    // 외부 스크립트(AnomalyObject)가 현재 소지 도구를 확인하기 위한 함수
    public ToolType GetCurrentTool()
    {
        return currentTool;
    }
}

//플레이어에 연결