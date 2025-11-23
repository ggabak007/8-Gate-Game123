using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 씬이 여러 개일 때, 이 스크립트도 GameManager처럼 씬이 바뀌어도 파괴되지 않게 관리하는 것이 좋습니다.
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬 로더도 파괴되지 않게 설정 (씬이 로드되는 동안 호출되어야 하므로)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneByStageIndex(int stageIndex)
    {
        string sceneName;

        if (stageIndex >= 1 && stageIndex <= 4) // 1단계부터 4단계까지의 씬
        {
            sceneName = "Stage_" + stageIndex;
            Debug.Log($"SceneLoader: 스테이지 {stageIndex} 씬 로드 요청.");
        }
        else if (stageIndex > 4) // 4단계를 초과하면 게임 클리어 씬 로드
        {
            sceneName = "GameClear_Scene";
            Debug.Log("SceneLoader: 게임 클리어 씬 로드 요청.");
        }
        else
        {
            Debug.LogError($"SceneLoader: 유효하지 않은 스테이지 인덱스입니다: {stageIndex}");
            return;
        }

        // 실제로 씬을 로드하는 Unity 함수 호출
        SceneManager.LoadScene(sceneName);
    }

    // 이외에도 로딩 화면 연출이 필요하다면 여기에 추가 로직을 구현합니다.
}