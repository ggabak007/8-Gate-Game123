using UnityEngine;

public class CursorController : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 중앙 고정 유지
        Cursor.visible = false; // OS 마우스 포인터 숨기기 (깔끔하게)
    }

    void Update()
    {
        // 사용자가 ESC를 누르면 커서를 해제하고 보이게 하는 등의 예외 처리를 여기에 추가할 수 있습니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None; // 잠금 해제
            Cursor.visible = true; // 커서 보이게
        }
    }
}