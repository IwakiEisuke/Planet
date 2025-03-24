using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelRestarter : MonoBehaviour
{
    [SerializeField] InputAction restartAction;
    [SerializeField] InputAction forceRestartAction;

    bool _IsGameOver;

    private void Start()
    {
        restartAction.performed += RestartLevel;
        forceRestartAction.performed += ForceRestartLevel;
        restartAction.Enable();
        forceRestartAction.Enable();

        var player = FindAnyObjectByType<Player>();
        if (player)
        {
            player.OnDead += () => _IsGameOver = true;
        }
        else _IsGameOver = true;
    }

    public void RestartLevel(InputAction.CallbackContext context)
    {
        print("Restart");
        if (_IsGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            SceneManager.sceneLoaded += OnReload;
        }
    }

    public void ForceRestartLevel(InputAction.CallbackContext context)
    {
        if (!_IsGameOver)
        {
            _IsGameOver = true;
            RestartLevel(context);
        }
    }

    private void OnDisable()
    {
        restartAction.Disable();
        forceRestartAction.Disable();
    }

    private void OnReload(Scene scene, LoadSceneMode mode)
    {
        var checkPointManager = FindAnyObjectByType<CheckPointManager>();
        print(checkPointManager.GetCheckPoint());
        FindAnyObjectByType<Player>().transform.position = checkPointManager.GetCheckPoint().transform.position;
        SceneManager.sceneLoaded -= OnReload;
    }
}
