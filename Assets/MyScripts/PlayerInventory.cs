using UnityEngine;
using System.Collections;
using UnityEngine.UI; // 이상현상 진행 ui 추가시 필요
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
    public float interactTime = 2.0f;

    public Slider progressSlider; // 상호작용 진행바 UI
    public float wipeSpeed = 15f; // 닦는 속도
    public float wipeAngle = 30f; // 닦는 각도
    public float smashSpeed = 20f; // 내리찍는 속도
    public float raiseAngle = -60f; // 망치 들어 올리는 각도 (뒤로 젖힘)
    private Coroutine currentAnimCoroutine = null;
    private float currentInteractTimer = 0f;
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
        TryInteract();

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
            AnomalyObject anomaly = hit.collider.GetComponent<AnomalyObject>();
            Door door = hit.collider.GetComponentInParent<Door>();

            if (door != null && Input.GetKeyDown(KeyCode.E))
            {
                door.ToggleDoor();
                return; // 문을 열었으면 다른 행동은 하지 않음
            }

            if (tool != null || anomaly != null)
            {
                // 이상현상인데 도구가 틀렸으면 무시
                if (anomaly != null)
                {
                    // 정답 도구가 아니고 + 맨손도 아니라면 -> 반응 안 함 (모션 X)
                    if (anomaly.requiredTool != currentTool && currentTool != ToolType.None)
                    {
                        
                        return;
                    }
                }
                // [E키 누르는 중]
                if (Input.GetKey(KeyCode.E))
                {
                    // 도구별 애니메이션 실행 (닦기 등)
                    PlayToolAnimation(true);

                    // 타이머 증가
                    currentInteractTimer += Time.deltaTime;

                    // UI 업데이트
                    if (progressSlider != null)
                    {
                        progressSlider.gameObject.SetActive(true);
                        progressSlider.value = currentInteractTimer / interactTime;
                    }

                    // 2초 지나면 성공
                    if (currentInteractTimer >= interactTime)
                    {
                        // 마무리 동작 (내리찍기 등)
                        FinishToolAnimation();

                        // 각자 기능 실행
                        if (tool != null) SwitchTool(tool);
                        if (door != null) door.ToggleDoor();
                        if (anomaly != null) anomaly.TrySolveAnomaly(this);

                        ResetInteraction(); // 초기화
                    }
                }
                // e키 떼면 초기화
                else if (Input.GetKeyUp(KeyCode.E))
                {
                    ResetInteraction();
                }
                return;

            }
        }
    }

    private void ResetInteraction()
    {
        currentInteractTimer = 0f;
        if (progressSlider != null)
        {
            progressSlider.value = 0;
            progressSlider.gameObject.SetActive(false);
        }
        PlayToolAnimation(false); // 애니메이션 멈춤
    }


    private void PlayToolAnimation(bool isPlaying)
    {
        // 수건(Towel)일 때 -> 닦는 모션
        if (currentTool == ToolType.Towel)
        {
            if (isPlaying)
            {
                if (currentAnimCoroutine == null)
                    currentAnimCoroutine = StartCoroutine(WipeMotion());
            }
            else
            {
                if (currentAnimCoroutine != null)
                {
                    StopCoroutine(currentAnimCoroutine);
                    currentAnimCoroutine = null;
                    if (handPosition) handPosition.localRotation = Quaternion.identity;
                }
            }
        }
        else if (currentTool == ToolType.Hammel)
        {
            if (isPlaying && handPosition != null)
            {
                float progress = Mathf.Clamp01(currentInteractTimer / interactTime);
                Quaternion targetRot = Quaternion.Euler(raiseAngle, 0, 0); // 목표: 뒤로 젖힘

                handPosition.localRotation = Quaternion.Slerp(Quaternion.identity, targetRot, progress);
            }
            else if (!isPlaying && handPosition != null)
            {
                handPosition.localRotation = Quaternion.Lerp(handPosition.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            }
        }
    }

    private void FinishToolAnimation()
    {
        // 해머일 때 -> 내리찍는 모션
        if (currentTool == ToolType.Hammel)
        {
            StartCoroutine(SmashMotion());
        }
    }

    // 모션 동작 코루틴

    IEnumerator WipeMotion()
    {
        Quaternion startRot = Quaternion.identity;
        while (true)
        {
            float z = Mathf.Sin(Time.time * wipeSpeed) * wipeAngle;
            if (handPosition) handPosition.localRotation = startRot * Quaternion.Euler(0, 0, z);
            yield return null;
        }
    }

    IEnumerator SmashMotion()
    {
        if (handPosition == null) yield break;

        float t = 0;
        Quaternion startRot = handPosition.localRotation;
        Quaternion upRot = Quaternion.Euler(-45, 0, 0);

        // 찍기
        Quaternion downRot = Quaternion.Euler(60, 0, 0);
        while (t < 1)
        {
            t += Time.deltaTime * smashSpeed * 2;
            handPosition.localRotation = Quaternion.Lerp(upRot, downRot, t);
            yield return null;
        }
        //여기에 소리 추가를 하면 될거같습니다.
        yield return new WaitForSeconds(0.2f);
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f; // 부드럽게 복귀
            handPosition.localRotation = Quaternion.Lerp(downRot, Quaternion.identity, t);
            yield return null;
        }
        handPosition.localRotation = Quaternion.identity;
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