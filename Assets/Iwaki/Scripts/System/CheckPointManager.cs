using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    [SerializeField] List<CheckPoint> checkPointList;
    [SerializeField] LevelRestarter levelRestarter;
    [SerializeField] CheckPointSO pointSO;

    private void OnValidate()
    {
        for (int i = 0; i < checkPointList.Count; i++)
        {
            checkPointList[i].id = i;
            checkPointList[i].manager = this;
        }
    }

    public CheckPoint GetCheckPoint(int id)
    {
        if (id < checkPointList.Count)
        {
            return checkPointList[id];
        }

        return null;
    }

    public CheckPoint GetCheckPoint()
    {
        return checkPointList[pointSO.checkPointID];
    }

    public void SetCheckPoint(int id)
    {
        if (id < checkPointList.Count)
        {
            pointSO.checkPointID = id;
        }
    }
}
