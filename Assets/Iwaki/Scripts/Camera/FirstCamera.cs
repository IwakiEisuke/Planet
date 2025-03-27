using Unity.Cinemachine;
using UnityEngine;

public class FirstCamera : MonoBehaviour
{
    void Start()
    {
        GetComponent<CinemachineCamera>().Prioritize();
    }
}
