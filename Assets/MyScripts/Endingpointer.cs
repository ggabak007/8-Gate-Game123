using UnityEngine;

public class Endingpointer : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;

        // 커서 활성화
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
