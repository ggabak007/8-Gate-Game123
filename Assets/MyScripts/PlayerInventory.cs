using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInventory : MonoBehaviour
{
    [Header("설정")]
    public Transform handPosition;
    public float interactDistance = 5f;
    public float dropForce = 2.0f;
    public float interactTime = 2.0f;

    [Header("UI (기본 슬라이더)")]
    public Slider progressSlider; // 아까 쓰시던 기본 슬라이더

    [Header("상태")]
    public ToolType currentTool = ToolType.None;
    private GameObject heldToolObject = null;

    private Coroutine currentAnimCoroutine = null;
    private float currentInteractTimer = 0f;

    // 모션 변수들
    public float wipeSpeed = 15f;
    public float wipeAngle = 30f;
    public float smashSpeed = 20f;
    public float raiseAngle = -60f;

    void Start()
    {
        if (progressSlider != null) progressSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1. 상호작용 (E키)
        // 문 열기는 즉시, 도구/이상현상은 상황에 따라 처리
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKey(KeyCode.E))
        {
            TryInteract();
        }

        // 2. 키 뗐을 때 초기화
        if (Input.GetKeyUp(KeyCode.E))
        {
            ResetInteraction();
        }

        // 3. 버리기 (Q키)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropTool();
        }
    }

    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactDistance))
        {
            // A. 문 (Door) - 즉시 열림
            Door door = hit.collider.GetComponentInParent<Door>();
            if (door != null && Input.GetKeyDown(KeyCode.E))
            {
                door.ToggleDoor();
                return;
            }

            // B. 도구 (Tools) - 즉시 습득 (님 요청대로 2초 딜레이 뺌)
            Tools tool = hit.collider.GetComponent<Tools>();
            if (tool != null && Input.GetKeyDown(KeyCode.E))
            {
                SwitchTool(tool);
                return;
            }

            // C. 이상현상 (Anomaly) - 2초 꾹
            AnomalyObject anomaly = hit.collider.GetComponent<AnomalyObject>();
            if (anomaly != null)
            {
                // 조건: 도구가 맞거나 맨손이어도 되는 경우
                if (currentTool != ToolType.None && anomaly.requiredTool == currentTool)
                {
                    if (Input.GetKey(KeyCode.E))
                    {
                        PlayToolAnimation(true); // 모션 재생

                        currentInteractTimer += Time.deltaTime;

                        // UI 슬라이더 표시
                        if (progressSlider != null)
                        {
                            progressSlider.gameObject.SetActive(true);
                            progressSlider.value = currentInteractTimer / interactTime;
                        }

                        // 시간 다 되면 해결
                        if (currentInteractTimer >= interactTime)
                        {
                            FinishToolAnimation();
                            anomaly.TrySolveAnomaly(this);
                            ResetInteraction();
                        }
                    }
                }
            }
        }
        else
        {
            ResetInteraction();
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
        PlayToolAnimation(false);
    }

    // --- 아래는 모션 및 도구 처리 (기존 코드 유지) ---

    private void PlayToolAnimation(bool isPlaying)
    {
        if (currentTool == ToolType.Towel)
        {
            if (isPlaying)
            {
                if (currentAnimCoroutine == null) currentAnimCoroutine = StartCoroutine(WipeMotion());
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
            return;
        }
        else if (currentTool == ToolType.Hammer) // Hammel 오타 수정
        {
            if (isPlaying && handPosition != null)
            {
                float progress = Mathf.Clamp01(currentInteractTimer / interactTime);
                Quaternion targetRot = Quaternion.Euler(raiseAngle, 0, 0);
                handPosition.localRotation = Quaternion.Slerp(Quaternion.identity, targetRot, progress);
            }
            else if (!isPlaying && handPosition != null)
            {
                handPosition.localRotation = Quaternion.Lerp(handPosition.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            }
        }
        else
        {
            // 혹시라도 수건 코루틴이 남아있으면 끔
            if (currentAnimCoroutine != null)
            {
                StopCoroutine(currentAnimCoroutine);
                currentAnimCoroutine = null;
                if (handPosition) handPosition.localRotation = Quaternion.identity;
            }
        }
    }

    private void FinishToolAnimation()
    {
        if (currentTool == ToolType.Hammer) StartCoroutine(SmashMotion());
    }

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
        Quaternion upRot = Quaternion.Euler(-45, 0, 0);
        Quaternion downRot = Quaternion.Euler(60, 0, 0);

        while (t < 1) { t += Time.deltaTime * smashSpeed * 2; handPosition.localRotation = Quaternion.Lerp(upRot, downRot, t); yield return null; }
        yield return new WaitForSeconds(0.2f);
        t = 0;
        while (t < 1) { t += Time.deltaTime * 5f; handPosition.localRotation = Quaternion.Lerp(downRot, Quaternion.identity, t); yield return null; }
        handPosition.localRotation = Quaternion.identity;
    }

    private void SwitchTool(Tools newTool)
    {
        ResetInteraction();
        if (currentTool != ToolType.None) DropTool();

        currentTool = newTool.toolType;
        heldToolObject = newTool.gameObject;

        if (handPosition != null)
        {
            heldToolObject.transform.SetParent(handPosition);
            // 조원이 만든 위치 보정 기능 사용
            heldToolObject.transform.localPosition = newTool.gripPosition;
            heldToolObject.transform.localRotation = Quaternion.Euler(newTool.gripRotation);
        }
        else
        {
            heldToolObject.SetActive(false);
        }

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
        }
    }

    public void DestroyToolUsed()
    {
        if (currentTool != ToolType.None)
        {
            if (currentAnimCoroutine != null) StopCoroutine(currentAnimCoroutine);
            if (handPosition != null) handPosition.localRotation = Quaternion.identity;

            if (heldToolObject != null)
            {
                // 1. 비활성화
                heldToolObject.SetActive(false);

                // 2. 손에서 떼어내기 
                heldToolObject.transform.SetParent(null);
            }
            currentTool = ToolType.None;
            heldToolObject = null;
            ResetInteraction();
        }
    }

    public ToolType GetCurrentTool() { return currentTool; }
    public void ForceClearHand()
    {
        currentTool = ToolType.None;
        heldToolObject = null;
        ResetInteraction();
        if (handPosition != null) handPosition.localRotation = Quaternion.identity;
    }
}
