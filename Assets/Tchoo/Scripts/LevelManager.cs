using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public PlayerController PlayerControllerRef;
    public CinemachineCamera CineCamRef;
    public Image transitionImage;

    public Dictionary<string, AsyncOperation> dict = new();

    public LevelConnection ActiveConnection { get; set; }
    private static string activeLevel;

    //TODO Mettre dans un autre script plz
    [SerializeField] private Image startImage;
    [SerializeField] private TextMeshProUGUI startText;
    private bool gameIsStarted = false;

    private void Awake()
    {
        if (!Instance.IsUnityNull())
        {
            Destroy(Instance);
        }
        Instance = this;
        startImage.DOFade(1,0);
        transitionImage.color = Color.black;
    }


    private void Start()
    {
        Debug.Log("Start LevelManager -//");
        //ConnectionToNewLevel("Level_0", true);
        if (PlayerControllerRef.IsUnityNull()) PlayerControllerRef = FindFirstObjectByType<PlayerController>();
        if(CineCamRef.IsUnityNull()) CineCamRef = FindFirstObjectByType<CinemachineCamera>();
        DoFadeLoop();
        InputSystem.onAnyButtonPress.CallOnce(ctrl => {
            Debug.Log($"{ctrl} pressed"); 
            StartGame();
            });
    }

    private void DoFadeLoop()
    {
        if (gameIsStarted) return;
        startText.DOFade(1, 1f).OnComplete(() =>
        {
            startText.DOFade(0, 1.5f).OnComplete(() =>
            {
                DoFadeLoop();
            });
        });
    }

    private void StartGame()
    {
        startImage.DOFade(0, 1);
        gameIsStarted = true;
        startText.enabled = false;
        ConnectionToNewLevel("Level_0", true);
    }
    public void SpawnPlayerAtPoint(Transform transformPoint)
    {
        Debug.Log("spawn player at " + transformPoint.position.ToString());
        PlayerControllerRef.transform.position = transformPoint.position;
        PlayerControllerRef.ResetPower();
    }

    public async void ConnectionToNewLevel(string levelName, bool transition)
    {
        Debug.Log("StartFade");
        if (transition)
        {
            await LoadTransition();
        }
        Time.timeScale = 0f;
        //await transitionImage.DOFade(0.2f, 0.1f).AsyncWaitForCompletion();
        await transitionImage.DOFade(1f, 0.3f).SetUpdate(true).AsyncWaitForCompletion();
        //await LoadTransition();

        if(activeLevel != null)
        {
            Debug.Log("Start Unload level " + activeLevel);
            await SceneManager.UnloadSceneAsync(activeLevel);
            //    .completed += handle =>
            //{
            //    Resources.UnloadUnusedAssets().completed += e =>
            //    {

            //    };
            //};
            Debug.Log("End Unload");

            Debug.Log("Start - Unload Assets");
            await Resources.UnloadUnusedAssets();
            Debug.Log("End - Unload Assets");
        }

        Debug.Log("Start - - Load level");
        await SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        Debug.Log("End - - Load level");
        CineCamRef.CancelDamping();
        activeLevel = levelName;
        if (transition)
        {
            await Task.Delay(1900);
            await UnloadTransition();
        }
        await Task.Delay(100);
        await transitionImage.DOFade(0.67f, 0.2f).SetUpdate(true).AsyncWaitForCompletion();
        Time.timeScale = 1f;
        transitionImage.DOFade(0f, 0.4f);
    }

    private AsyncOperation LoadTransition()
    {
        return SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }    
    
    private AsyncOperation UnloadTransition()
    {
        return SceneManager.UnloadSceneAsync("LoadingScene");
    }
    //public void LoadScene(string addressToAdd)
    //{
    //    SceneManager.LoadSceneAsync(addressToAdd, LoadSceneMode.Additive).completed += handle => OnSceneLoaded(addressToAdd, handle);
    //}
    //void OnSceneLoaded(string address, AsyncOperation obj)
    //{
    //    dict.Add(address, obj);
    //}

}
