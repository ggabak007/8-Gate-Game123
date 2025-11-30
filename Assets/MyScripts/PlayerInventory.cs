using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInventory : MonoBehaviour
{
    [Header("설정")]
    public Transform handPosition;      // 도구가 부착될 위치 (MainCamera의 자식)
    public float interactDistance = 5f; // 상호작용 거리
    public float dropForce = 2.0f;      // 버릴 때 밀어내는 힘
    public float interactTime = 2.0f;   // 꾹 눌러야 하는 시간 (2초)

    [Header("UI 연결")]
    public GameObject crosshairDot;     // 평소에 보이는 흰 점 (Dot)
    public GameObject progressUIObject; // 상호작용 때 켜질 도넛 부모 (InteractionUI)
    public Image progressFillImage;     // 채워질 도넛 이미지 (FillImage)

    [Header("상태")]
    public ToolType currentTool = ToolType.None;
    private GameObject heldToolObject = null;

    private Coroutine currentAnimCoroutine = null;
    private float currentInteractTimer = 0f;

    // 현재 들고 있는 도구의 기본 각도를 기억하는 변수 (손떨림 방지용)
    private Quaternion currentGripRotation = Quaternion.identity;

    [Header("모션 변수")]
    public float wipeSpeed = 15f;
    public float wipeAngle = 30f;
    public float smashSpeed = 20f;
    public float raiseAngle = -60f;

    void Start()
    {
        // 게임 시작 시 UI 초기화
        ResetInteraction();
    }

    void Update()
    {
        // 1. [즉시 실행] 문 열기 (E키 딸깍)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }

        // 2. [지속 실행] 도구 줍기 & 이상현상 해결 (E키 꾹)
        if (Input.GetKey(KeyCode.E))
        {
            TryContinuousInteract();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            ResetInteraction(); // 키 떼면 초기화
        }

        // 3. 버리기 (Q키)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropTool();
        }

        // [손떨림 방지] 아무것도 안 하고 있을 때는, 손을 '잡는 각도'로 부드럽게 유지
        if (currentInteractTimer == 0 && currentTool != ToolType.None && handPosition != null)
        {
            // 닦는 중이 아닐 때만 원래 각도로 복귀 (코루틴 충돌 방지)
            if (currentTool != ToolType.Towel || currentAnimCoroutine == null)
            {
                handPosition.localRotation = Quaternion.Lerp(handPosition.localRotation, currentGripRotation, Time.deltaTime * 10f);
            }
        }
    }

    // 문 열기 (즉발)
    private void TryOpenDoor()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactDistance))
        {
            Door door = hit.collider.GetComponentInParent<Door>();
            if (door != null)
            {
                door.ToggleDoor();
            }
        }
    }

    // 도구 줍기 + 이상현상 해결 (게이지 채우기)
    private void TryContinuousInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactDistance))
        {
            // A. 도구를 보고 있을 때 -> 2초 꾹 눌러서 습득
            Tools tool = hit.collider.GetComponent<Tools>();
            if (tool != null)
            {
                ProcessInteractTimer(() => SwitchTool(tool));
                return;
            }

            // B. 이상현상을 보고 있을 때 -> 2초 꾹 눌러서 해결
            AnomalyObject anomaly = hit.collider.GetComponent<AnomalyObject>();
            if (anomaly != null)
            {
                // 조건: 정답 도구를 들고 있거나, 맨손이어도 되는 경우
                if (currentTool != ToolType.None && anomaly.requiredTool == currentTool)
                {
                    PlayToolAnimation(true); // 도구 모션 재생
                    ProcessInteractTimer(() => {
                        FinishToolAnimation(); // 마무리 동작(내리찍기 등)
                        anomaly.TrySolveAnomaly(this); // 해결 로직 실행
                    });
                    return;
                }
            }
        }

        // 아무것도 안 보고 있거나 조건이 안 맞으면 초기화
        ResetInteraction();
    }

    // 타이머 및 UI 처리 공통 함수
    private void ProcessInteractTimer(System.Action onComplete)
    {
        currentInteractTimer += Time.deltaTime;

        // 1. 점 끄고 도넛 켜기
        if (crosshairDot != null) crosshairDot.SetActive(false);
        if (progressUIObject != null) progressUIObject.SetActive(true);

        // 2. 도넛 채우기
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = currentInteractTimer / interactTime;
        }

        // 3. 시간 다 되면 행동 실행
        if (currentInteractTimer >= interactTime)
        {
            onComplete?.Invoke();
            ResetInteraction();
        }
    }

    private void ResetInteraction()
    {
        currentInteractTimer = 0f;

        // 1. 도넛 끄고 비우기
        if (progressUIObject != null)
        {
            progressFillImage.fillAmount = 0f;
            progressUIObject.SetActive(false);
        }

        // 2. 점 다시 켜기
        if (crosshairDot != null) crosshairDot.SetActive(true);

        PlayToolAnimation(false); // 애니메이션 멈춤
    }

    // [모션 관련] Hammel 오타 수정됨 -> Hammer
    private void PlayToolAnimation(bool isPlaying)
    {
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
                    // 코루틴 멈추면 Update에서 자동으로 currentGripRotation으로 돌아감
                }
            }
        }
        else if (currentTool == ToolType.Hammer)
        {
            if (isPlaying && handPosition != null)
            {
                float progress = Mathf.Clamp01(currentInteractTimer / interactTime);
                // 기본 잡는 각도(currentGripRotation)에서 -> 뒤로 젖히는 각도(raiseAngle)로 변경
                Quaternion targetRot = currentGripRotation * Quaternion.Euler(raiseAngle, 0, 0);
                handPosition.localRotation = Quaternion.Slerp(currentGripRotation, targetRot, progress);
            }
            // isPlaying이 false일 때는 Update문에서 자동으로 원래대로 돌아감
        }
    }

    private void FinishToolAnimation()
    {
        if (currentTool == ToolType.Hammer)
        {
            StartCoroutine(SmashMotion());
        }
    }

    IEnumerator WipeMotion()
    {
        // 닦을 때도 기본 잡는 각도(currentGripRotation)를 기준으로 흔들어야 함
        Quaternion startRot = Quaternion.identity;
        // startRot 대신 currentGripRotation을 써야 하지만, WipeMotion은 계속 도는 거라 
        // 아래 while문 안에서 currentGripRotation을 반영함.

        while (true)
        {
            float z = Mathf.Sin(Time.time * wipeSpeed) * wipeAngle;
            if (handPosition)
                handPosition.localRotation = currentGripRotation * Quaternion.Euler(0, 0, z);
            yield return null;
        }
    }

    IEnumerator SmashMotion()
    {
        if (handPosition == null) yield break;
        float t = 0;

        // 시작: 현재 젖혀진 상태 / 목표: 내리찍은 상태
        // (여기서 upRot은 현재 젖혀진 상태를 가져오는 게 자연스러움)
        Quaternion startRot = handPosition.localRotation;
        Quaternion downRot = currentGripRotation * Quaternion.Euler(60, 0, 0);

        // 내리찍기
        while (t < 1)
        {
            t += Time.deltaTime * smashSpeed * 2;
            handPosition.localRotation = Quaternion.Lerp(startRot, downRot, t);
            yield return null;
        }

        // 찍고 잠시 대기 (타격감)
        yield return new WaitForSeconds(0.2f);

        // 복귀
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            // 복귀할 때도 원래 잡는 각도(currentGripRotation)로 복귀
            handPosition.localRotation = Quaternion.Lerp(downRot, currentGripRotation, t);
            yield return null;
        }
        handPosition.localRotation = currentGripRotation;
    }

    private void SwitchTool(Tools newTool)
    {
        // 이미 든 게 있으면 버림
        if (currentTool != ToolType.None) DropTool();

        currentTool = newTool.toolType;
        heldToolObject = newTool.gameObject;

        // [핵심] 잡는 각도 저장! (이게 있어야 손이 안 떨림)
        currentGripRotation = Quaternion.Euler(newTool.gripRotation);

        if (handPosition != null)
        {
            heldToolObject.transform.SetParent(handPosition);
            heldToolObject.transform.localPosition = newTool.gripPosition;
            heldToolObject.transform.localRotation = Quaternion.Euler(newTool.gripRotation);
        }
        else
        {
            heldToolObject.SetActive(false);
        }

        // 물리 끄기
        Rigidbody rb = heldToolObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        Collider col = heldToolObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log($"도구 습득: {newTool.toolType}");
    }

    public void DropTool()
    {
        if (currentTool != ToolType.None && heldToolObject != null)
        {
            heldToolObject.transform.SetParent(null);
            heldToolObject.SetActive(true);

            Rigidbody rb = heldToolObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Camera.main.transform.forward * dropForce, ForceMode.Impulse);
            }
            Collider col = heldToolObject.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            currentTool = ToolType.None;
            heldToolObject = null;

            // 손 각도 초기화 (빈손)
            if (handPosition != null) handPosition.localRotation = Quaternion.identity;
            currentGripRotation = Quaternion.identity;

            Debug.Log("도구 버림");
        }
    }

    public void DestroyToolUsed()
    {
        if (currentTool != ToolType.None)
        {
            if (heldToolObject != null) Destroy(heldToolObject);
            currentTool = ToolType.None;
            heldToolObject = null;

            // 손 각도 초기화
            if (handPosition != null) handPosition.localRotation = Quaternion.identity;
            currentGripRotation = Quaternion.identity;
        }
    }

    public ToolType GetCurrentTool()
    {
        return currentTool;
    }
}