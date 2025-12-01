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
    private bool isSoundPlayingState = false;

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
    public float soundVolume = 0.2f;
    public AudioSource audioSource; // 플레이어에게 붙어있는 AudioSource 컴포넌트
    public AudioClip wipeSound;    
    public AudioClip smashSound;    

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

    // PlayerInventory.cs

    private void PlayToolAnimation(bool isPlaying)
    {
        // 1. [멈춤 요청] 손을 뗐거나, 조준이 빗나갔을 때
        if (!isPlaying)
        {
            // 모션 정지
            if (currentAnimCoroutine != null)
            {
                StopCoroutine(currentAnimCoroutine);
                currentAnimCoroutine = null;
                if (handPosition) handPosition.localRotation = Quaternion.identity;
            }

            // 소리가 켜져있었다면 끈다
            if (isSoundPlayingState)
            {
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.clip = null;
                isSoundPlayingState = false; // "나 껐어!" 기억
            }
            return;
        }

        // 2. [재생 요청] 수건으로 닦고 있을 때
        if (currentTool == ToolType.Towel)
        {
            // 모션 시작
            if (currentAnimCoroutine == null)
                currentAnimCoroutine = StartCoroutine(WipeMotion());

            // ★핵심 해결법★
            // "소리가 켜진 상태(isSoundPlayingState)"가 아니라면 -> 켠다!
            // 이미 켜져 있다면(true) -> 아무것도 안 하고 내버려 둔다! (끊김 방지)
            if (!isSoundPlayingState)
            {
                audioSource.clip = wipeSound;
                audioSource.loop = true; // 끊기지 않게 반복 설정
                audioSource.volume = soundVolume;
                audioSource.Play();

                isSoundPlayingState = true; // "나 켰어!" 기억
            }
        }
        // [망치] 로직
        else if (currentTool == ToolType.Hammer)
        {
            // 기존 망치 코드 유지
            if (handPosition != null)
            {
                float progress = Mathf.Clamp01(currentInteractTimer / interactTime);
                Quaternion targetRot = Quaternion.Euler(raiseAngle, 0, 0);
                handPosition.localRotation = Quaternion.Slerp(Quaternion.identity, targetRot, progress);
            }
        }
    }

    private void FinishToolAnimation()
    {
        if (currentTool == ToolType.Hammer) StartCoroutine(SmashMotion());
    }

    // 닦기 모션 
    IEnumerator WipeMotion()
    {
        Quaternion startRot = Quaternion.identity;

        while (true)
        {
            float wave = Mathf.Sin(Time.time * wipeSpeed) * wipeAngle;

            if (handPosition)
                handPosition.localRotation = startRot * Quaternion.Euler(0, 0, wave);


            yield return null;
        }
    }

    IEnumerator SmashMotion() // 해머 모션
    {
        if (handPosition == null) yield break;
        float t = 0;
        Quaternion startRot = handPosition.localRotation;
        Quaternion upRot = Quaternion.Euler(-45, 0, 0);
        Quaternion downRot = Quaternion.Euler(60, 0, 0);

        while (t < 1) { t += Time.deltaTime * smashSpeed * 2; handPosition.localRotation = Quaternion.Lerp(upRot, downRot, t); yield return null; }
        yield return new WaitForSeconds(0.2f);
        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            handPosition.localRotation = Quaternion.Lerp(downRot, Quaternion.identity, t);
        }
            
                if (audioSource != null && smashSound != null)
                {
                    Debug.Log("소리 재생 시도!");
                    audioSource.pitch = Random.Range(0.5f, 0.50001f); // 묵직하게 낮은 음
                    audioSource.PlayOneShot(smashSound);
                }
        
        yield return new WaitForSeconds(0.2f);
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

        SetLayerRecursively(heldToolObject, LayerMask.NameToLayer("Ignore Raycast"));
        Debug.Log($"도구 습득: {newTool.toolType}");
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer; // 본체 변경

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer); // 자식도 변경 (재귀)
        }
    }

public void DropTool()
    {
        if (currentTool != ToolType.None && heldToolObject != null)
        {
            SetLayerRecursively(heldToolObject, LayerMask.NameToLayer("Default"));
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
