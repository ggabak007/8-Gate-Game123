using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCtrl : MonoBehaviour
{
    public void ClickStart()
    {
        SceneManager.LoadScene(0);
    }
    public void ClickOption()
    {
        Debug.Log("옵션 버튼 클릭됨 (옵션 UI 아직 없음)");
    }
    public void ClickExit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
    public void ClickMenu()
    {
        SceneManager.LoadScene(2);
    }
}
