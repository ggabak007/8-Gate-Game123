using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private AnomalyManager anomalyManager;
    // 씬 하나만 사용하여 삭제 private SceneLoader sceneLoader;
    //플레이어가 소지한 도구
    public ToolType currentTool = ToolType.None;

    //레이캐스팅 설정
    public float interactDistance = 3f;

    // 이전에 들고 있던 도구 오브젝트 (스위칭 및 드랍을 위해 필요)
    private GameObject heldToolObject = null;
    void Start()
    {
        anomalyManager = FindFirstObjectByType<AnomalyManager>();
<<<<<<< Updated upstream:Assets/MyScripts/PlayerInventory.cs
        // 씬 하나만 사용하여 삭제 sceneLoader =FindFirstObjectByType<SceneLoader>();
=======
        sceneLoader = FindFirstObjectByType<SceneLoader>();
>>>>>>> Stashed changes:Assets/Future/PlayerInventory.cs
    }
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
<<<<<<< Updated upstream:Assets/MyScripts/PlayerInventory.cs
            InteractableTool tool = hit.collider.GetComponent<InteractableTool>();
=======
            Tools tool = hit.collider.GetComponent<Tools>();
>>>>>>> Stashed changes:Assets/Future/PlayerInventory.cs

            if (tool != null)
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
         }
    }

    private void SwitchTool(Tools newTool)
    {
        // 1. 현재 도구가 있다면 (스위칭)
        if (currentTool != ToolType.None)
        {
            // 이전 도구를 현재 위치에 드롭 (heldToolObject는 이전에 들고 있던 오브젝트)
            DropHeldToolObject(transform.position + transform.forward * 0.5f);
        }

        // 2. 새로운 도구를 습득하고 상태 업데이트
        currentTool = newTool.toolType;
        heldToolObject = newTool.gameObject;

        // 3. 습득한 도구 오브젝트 처리 (비활성화 또는 플레이어 손에 붙이기)
        heldToolObject.SetActive(false); // 오브젝트를 숨김 (인벤토리에 들어갔다고 간주)

        Debug.Log($"도구 스위칭: {newTool.toolType} 습득.");
    }

    // Q키 상호작용
    public void DropTool()
    {
        if (currentTool != ToolType.None && heldToolObject != null)
        {
            // 드롭할 위치 계산 (플레이어 앞)
            Vector3 dropPosition = transform.position + transform.forward * 0.5f;
            DropHeldToolObject(dropPosition);

            // 인벤토리 초기화
            currentTool = ToolType.None;
            heldToolObject = null;
            Debug.Log("도구를 내려놓았습니다.");
        }
        else
        {
            Debug.Log("내려놓을 도구가 없습니다.");
        }
    }


    // 실제 오브젝트를 월드에 다시 활성화하고 물리 처리
    private void DropHeldToolObject(Vector3 position)
    {
        if (heldToolObject != null)
        {
            heldToolObject.transform.position = position;
            heldToolObject.SetActive(true);
            // 오브젝트에 Rigidbody가 있다면 물리 효과를 줄 수 있습니다.
            Rigidbody rb = heldToolObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * 50f); // 살짝 밀어내는 힘
            }
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