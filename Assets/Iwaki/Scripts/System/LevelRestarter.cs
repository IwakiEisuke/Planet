using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelRestarter : MonoBehaviour
{
    [SerializeField] InputAction restartAction;
    [SerializeField] CheckPointManager checkPointManager;

    bool _IsGameOver;

    private void Start()
    {
        print(checkPointManager.GetCheckPoint());

        restartAction.performed += RestartLevel;
        restartAction.Enable();

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
        }
    }
}
