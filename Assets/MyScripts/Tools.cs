using UnityEngine;

public class Tools : MonoBehaviour
{
    // 이 도구가 어떤 종류인지 정의합니다.
    public ToolType toolType;
    public GameObject toolPrefab;
    public Vector3 gripRotation = Vector3.zero;
    public Vector3 gripPosition = Vector3.zero;
    void Start()
    {
        // 디버깅용 코드
        if (toolType == ToolType.None)
        {
            Debug.LogError(gameObject.name + ":oolType이 설정되지 않았습니다! 유형을 지정해주세요.", this);
        }

        gameObject.tag = "Tool"; 
    }
}

//도구 오브젝트에 이 스크립트 부착, 인스펙터에서 오브젝트의 Tool Type 설정