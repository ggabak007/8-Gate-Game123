using UnityEngine;

public class InteractableTool : MonoBehaviour
{
    [Header("도구 설정")]
    public ToolType toolType; // 이 도구가 무엇인지 (Inspector에서 설정)

    // PlayerInventory가 이 도구를 주울 때 정보를 가져가기 위해 이 스크립트가 존재함
}