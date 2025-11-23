using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    // 스테이지 씬 이름을 GameManager와 동일하게 정의 (하드 코딩 방지)
    private const string GENERIC_STAGE_SCENE = "Generic_Stage";
    private const string GAME_CLEAR_SCENE = "GameClear_Scene";

    private void Awake()
    {
        // ... (Singleton 로직은 그대로 유지)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 변경: 스테이지 인덱스를 받지 않고, 항상 단일 스테이지 씬을 로드합니다.
    public void LoadGenericStageScene(string sceneName = GENERIC_STAGE_SCENE)
    {
        Debug.Log($"SceneLoader: 단일 스테이지 씬 ({sceneName}) 로드 요청.");
        SceneManager.LoadScene(sceneName);
    }

    // 추가: 게임 클리어 씬 로드 함수 (별도 씬 로드)
    public void LoadClearScene()
    {
        Debug.Log($"SceneLoader: 게임 클리어 씬 ({GAME_CLEAR_SCENE}) 로드 요청.");
        SceneManager.LoadScene(GAME_CLEAR_SCENE);
    }

}ㄴ




// 씬이 바뀌어도 오브젝트가 파괴되지 않도록 설정