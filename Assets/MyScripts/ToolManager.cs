using UnityEngine;
using System.Collections.Generic;

public class ToolManager : MonoBehaviour
{
    // 도구의 초기 상태를 저장할 구조체
    private struct ToolData
    {
        public GameObject toolObj;
        public Vector3 startPos;
        public Quaternion startRot;
        public Transform originalParent; // 원래 부모 (필요시)
    }

    private List<ToolData> allTools = new List<ToolData>();

    void Start()
    {
        // 1. 씬에 있는 모든 도구(Tools 스크립트 붙은 것)를 찾음
        Tools[] foundTools = FindObjectsByType<Tools>(FindObjectsSortMode.None);

        foreach (Tools t in foundTools)
        {
            // 도구의 "원래 위치"와 "정보"를 리스트에 저장해둠
            ToolData data = new ToolData();
            data.toolObj = t.gameObject;
            data.startPos = t.transform.position;
            data.startRot = t.transform.rotation;
            data.originalParent = t.transform.parent;

            allTools.Add(data);
        }
    }

    // ★ 매일 아침 호출될 함수
    public void ResetAllTools()
    {
        foreach (var data in allTools)
        {
            if (data.toolObj != null)
            {
                // 1. 위치/회전 원상복구
                data.toolObj.transform.position = data.startPos;
                data.toolObj.transform.rotation = data.startRot;
                data.toolObj.transform.SetParent(data.originalParent);

                // 2. 물리 켜기 (혹시 꺼져있을까봐)
                Rigidbody rb = data.toolObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.linearVelocity = Vector3.zero;
                }

                Collider col = data.toolObj.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                // 3. 다시 켜기 (활성화)

                data.toolObj.SetActive(true);
            }
        }
    }
}