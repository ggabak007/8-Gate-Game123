using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
    public float soundVolume = 0.2f;
    public AudioSource audioSource; // 플레이어에게 붙어있는 AudioSource 컴포넌트
    public AudioClip wipeSound;    
    public AudioClip smashSound;

    //UI
    public TextMeshProUGUI interactionText;

    void Start()
    {
        if (progressSlider != null) progressSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        interactionText.enabled = false;
        TryInteract();

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
            if (door != null)
            {
                interactionText.text = "E : open/close";
                interactionText.enabled = true;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    door.ToggleDoor();
                    return;
                }
            }

            // B. 도구 (Tools) - 즉시 습득 (님 요청대로 2초 딜레이 뺌)
            Tools tool = hit.collider.GetComponent<Tools>();
            if (tool != null)
            {
                interactionText.text = "E : get";
                interactionText.enabled = true;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SwitchTool(tool);
                    return;
                }
            }

            // C. 이상현상 (Anomaly) - 2초 꾹
            AnomalyObject anomaly = hit.collider.GetComponent<AnomalyObject>();
            if (anomaly != null)
            {
                interactionText.text = "E : interact";
                interactionText.enabled = true;
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
        if (!isPlaying)
        {
            // 애니메이션 중단
            if (currentAnimCoroutine != null)
            {
                StopCoroutine(currentAnimCoroutine);
                currentAnimCoroutine = null;
                if (handPosition) handPosition.localRotation = Quaternion.identity;
            }

            // (망치 소리는 PlayOneShot이라 굳이 안 꺼도 자연스럽게 끝남)
            if (audioSource.isPlaying && audioSource.clip == wipeSound)
            {
                audioSource.Stop();
                audioSource.loop = false; // 반복 끄기 (중요!)
                audioSource.clip = null;  // 클립 비우기
            }
            return;
        }


        // [수건] : 닦는 모션 + 연속 소리
        if (currentTool == ToolType.Towel)
        {
            // 모션 시작
            if (currentAnimCoroutine == null)
                currentAnimCoroutine = StartCoroutine(WipeMotion());

            //  재생 시작
            if (!audioSource.isPlaying && wipeSound != null)
            {
                audioSource.clip = wipeSound; // 1. 수건 소리 장착
                audioSource.volume = soundVolume; // 3. 볼륨 설정
                audioSource.Play();           // 4. 재생 시작
            }
        }
        // [망치] : 들어 올리는 모션 (소리 없음)
        else if (currentTool == ToolType.Hammer)
        {
            if (handPosition != null)
            {
                float progress = Mathf.Clamp01(currentInteractTimer / interactTime);
                Quaternion targetRot = Quaternion.Euler(raiseAngle, 0, 0);
                handPosition.localRotation = Quaternion.Slerp(Quaternion.identity, targetRot, progress);
            }
        }
        // 그 외
        else
        {
            PlayToolAnimation(false);
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
