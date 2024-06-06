using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class Pot : MonoBehaviour
{
    [SerializeField] Transform visualSpawnPos;
    [SerializeField] GameObject chipPrefab;
    [SerializeField] SoundContainer potDingDingSound;

    public bool IsThrowing { get; private set; }

    public int Capacity { get; private set; }
    public int FillAmount { get; private set; }

    public void SetCapacity(int capacity)
    {
        Capacity = capacity;
    }

    public IEnumerator AddValue(int value)
    {
        Debug.Log($"adding {value} to pot; value after: {FillAmount + value}/{Capacity}");

        int overflowTimes = 0;
        for (int i = 0; i < value; i++)
        {
            FillAmount++;

            if (FillAmount > Capacity)
            {
                FillAmount -= Capacity;
                overflowTimes++;
            }

            yield return QOL.GetWaitForSeconds(0.12f);
            Transform chip = Instantiate(chipPrefab, visualSpawnPos.position + Random.insideUnitSphere * 0.1f, Maf.GetRandomRotation()).transform;
            chip.parent = visualSpawnPos;
        }

        if (overflowTimes > 0)
        {
            int removeCount = overflowTimes * Capacity;
            for (int i = removeCount - 1; i >= 0; i--)
            {
                Destroy(visualSpawnPos.GetChild(i).gameObject);
            }
            EventManager.PotOverflow(overflowTimes);
        }
    }


    public void ThrowChips()
    {
        SoundManager.Instance.PlaySound(potDingDingSound);
        StartCoroutine(ThrowChipsCor());


        IEnumerator ThrowChipsCor()
        {
            IsThrowing = true;
            // ToDo: actually throw chips accumulated over the course of the game
            yield return null;
            IsThrowing = false;
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

    void OnDrawGizmosSelected()
    {
        if (!visualSpawnPos) return;

        Gizmos.DrawWireSphere(visualSpawnPos.position, 0.1f);
    }
}
