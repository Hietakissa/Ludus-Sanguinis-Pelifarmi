using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class Pot : MonoBehaviour
{
    [SerializeField] Transform visualSpawnPos;
    [SerializeField] GameObject chipPrefab;

    public int Capacity { get; private set; }
    public int FillAmount { get; private set; }

    public void SetCapacity(int capacity)
    {
        Capacity = capacity;
    }

    public IEnumerator AddValue(int value)
    {
        Debug.Log($"adding {value} to pot; value after: {FillAmount + value}/{Capacity}");

        for (int i = 0; i < value; i++)
        {
            FillAmount++;

            if (FillAmount > Capacity)
            {
                FillAmount -= Capacity;
                visualSpawnPos.DestroyChildren();
                EventManager.PotOverflow(1);
            }

            yield return QOL.GetWaitForSeconds(0.15f);
            Transform chip = Instantiate(chipPrefab, visualSpawnPos).transform;
            chip.position = visualSpawnPos.position;
            chip.rotation = Maf.GetRandomRotation();
        }
    }


    void ResetPot()
    {
        visualSpawnPos.DestroyChildren();
        FillAmount = 0;
    }

    void OnEnable()
    {
        EventManager.OnEndGame += ResetPot;
    }

    void OnDisable()
    {
        EventManager.OnEndGame -= ResetPot;
    }
}
