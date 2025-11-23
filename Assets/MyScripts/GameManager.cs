using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public int currentStageIndex = 1;

    private SceneLoader sceneLoader;

    private void Awake()
    {
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

    void Start()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader를 찾을 수 없습니다! 씬 전환 불가.");
        }
    }

    public void GoToNextStage()
    {
        currentStageIndex++;
        // 다음 씬 이름은 "Stage_2", "Stage_3" 등으로 가정합니다.
        sceneLoader.LoadSceneByStageIndex(currentStageIndex);
    }

    public void ResetToStage1()
    {
        currentStageIndex = 1;
        // 실패 시 Stage 1 씬으로 리셋
        sceneLoader.LoadSceneByStageIndex(currentStageIndex);
    }
}