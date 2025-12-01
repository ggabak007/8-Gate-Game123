using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))] //콜라이더 자동 추가
public class AnomalyObject : MonoBehaviour
{

    public ToolType requiredTool = ToolType.None; //발생한 이상현상을 해결할 때 필요한 도구 타입
    public GameObject objectToShow;
    public GameObject objectToHide;// (침대 위 인형)
    public Texture cleanTexture;
    public Material cleanMaterial;

    public float currentFixTime = 0f;
    public float requiredTime = 2.0f; // ★ 설정: 해결하는 데 걸리는 시간 (초)
    public GameObject itemToSpawn;
    // 문을 닫아야 해결되는지 여부( 이상현상중 사람 모형이 바깥에 꺼내져있을떄 지정된 냉장고(문이 열려있음) 에 넣고 문을 닫으면 True(해결)
    public bool solveOnDoorClose = false; 
    public bool isReadyToSolve = false;   // 인형은 놓았는데 문을 아직 안 닫은 상태
    public Door doorToOpen;
    // 이상현상 해결여부
    private bool isResolved = false;
    public int anomalyID = 0;
    private Vector3 initialPos;
    private Quaternion initialRot;

    private AnomalyManager anomalyManager; // 이상현상 해결 참조를 위해 필요
    private PlayerInventory playerInventory; // 플레이어가 가진 도구 확인 및 사용한 도구 처리(파괴)

    void Awake()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }
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
            Debug.Log("이미 해결된 이상현상입니다." + anomalyID);
            if (solveOnDoorClose)
            {
                // [대기 모드] : 해결했다고 보고 안 함. 준비 상태만 켬.
                isReadyToSolve = true;
                Debug.Log("도구 배치 완료! 문을 닫으면 해결됩니다.");
            }
            else
            {
                FinalizeSolve();
            }
           
        }
        else //오답도구 사용시 [ 실패 조건 ]
        {

            Debug.LogWarning($"FAIL: 이 이상현상에는 {requiredTool}이(가) 필요합니다. 현재 도구: {heldTool}");

            // 페널티 부여 로직 함수 구현 추가 필요
            ExecuteFailFix();

        }
    }
    public void FinalizeSolve()
    {
        if (!isResolved && anomalyManager != null)
        {
            anomalyManager.NotifyAnomalySolved();
            isResolved = true;
            Debug.Log("최종 해결 완료!");
        }
    }


    // 이상현상 해결시 실행되는 연출 함수 ( 추가 구현 필요 )
    //그림은 바닥으로 떨어뜨리고 부신다거나, 낙서는 깨끗하게 지워지는 연출 등 구현
    private void ExecuteAnomalyFix()
    {
        Debug.Log($"[해결 시도] {gameObject.name} 로직 실행 중...");

        // 1. [시체/인형] : 켤 물체가 연결되어 있다면? -> 무조건 켬!
        if (objectToShow != null)
        {
            objectToShow.SetActive(true);
            Debug.Log(">> 성공: 시체(ObjectToShow)를 켰습니다!");
            return; // 여기서 끝! (아래 코드 실행 안 함)
        }
        else
        {
            // 혹시 연결을 까먹었을까봐 경고 로그 띄움
            Debug.LogWarning(">> 주의: Object To Show가 비어있습니다! 시체를 연결하세요.");
        }

        // 2. [낙서] : 텍스처 변경이 있다면?
        if (cleanTexture != null || cleanMaterial != null)
        {
            Renderer ren = GetComponent<Renderer>();
            if (ren != null)
            {
                if (cleanTexture != null) ren.material.mainTexture = cleanTexture;
                else if (cleanMaterial != null) ren.material = cleanMaterial;
            }
            else
            {
                StartCoroutine(HideObject(0.1f));
            }
            return;
        }

        // 3. [액자] : 위 둘 다 아니라면 물리 파괴!
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(-transform.forward * 5.0f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10.0f, ForceMode.Impulse);
            StartCoroutine(HideObject(2.0f));
        }
        else
        {
            StartCoroutine(HideObject(0.1f));
        }
    }

    IEnumerator HideObject(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }


    // 이상현상 실패시(도구를 잘못 사용했거나 다른 물체에 도구 사용) 실행되는 연출 함수 ( 추가 구현 필요 )
    private void ExecuteFailFix()
    {
        // 예를 들면 시야가 흐려진다던지, 화면이 일시적으로 어두워지거나 섬광효과를 주거나 공포 소리를 주는 등의 연출
    }

    private void OnEnable()
    {
        Debug.Log($"[이상현상 발생] {gameObject.name} 활성화됨!");

        // 1. 문 열기 (연결되어 있으면 무조건 염)
        if (doorToOpen != null)
        {
            doorToOpen.UnlockAndOpen();
        }

        // 2. 바닥 도구 켜기 (연결되어 있으면 무조건 켬)
        if (itemToSpawn != null)
        {
            itemToSpawn.SetActive(true);
        }

        // 3. ★ 시체 숨기기 (연결되어 있으면 무조건 숨김!)
        // (ID 1번인지 검사 안 함 -> 연결만 되어있으면 작동)
        if (objectToHide != null)
        {
            objectToHide.SetActive(false);
            Debug.Log(">> 시체(ObjectToHide)를 숨겼습니다.");
        }

        // 4. 액자 리셋 (리지드바디가 있으면 무조건 리셋)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            transform.position = initialPos;
            transform.rotation = initialRot;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            gameObject.SetActive(true);
        }
    }
  
    public void SetAnomalyID(int id)
    {
        anomalyID = id;
        Debug.Log($"[AnomalyObject] ID가 {id}번으로 자동 설정되었습니다.");
    }

    
}