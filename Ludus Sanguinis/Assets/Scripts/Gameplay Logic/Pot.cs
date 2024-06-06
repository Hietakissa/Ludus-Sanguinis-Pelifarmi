using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class Pot : MonoBehaviour
{
    [SerializeField] Transform visualSpawnPos;
    [SerializeField] GameObject chipPrefab;
    [SerializeField] SoundContainer potDingDingSound;
    [SerializeField] Transform chipThrowPos;
    [SerializeField] float throwForce;

    public bool IsThrowing { get; private set; }

    public int Capacity { get; private set; }
    public int FillAmount { get; private set; }

    int totalAddedChips;


    public void SetCapacity(int capacity)
    {
        Capacity = capacity;
        totalAddedChips = 0;
    }

    public IEnumerator AddValue(int value)
    {
        Debug.Log($"adding {value} to pot; value after: {FillAmount + value}/{Capacity}");

        totalAddedChips += value;
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
            for (int i = 0; i < totalAddedChips; i++)
            {
                GameObject chip = Instantiate(chipPrefab, chipThrowPos.position, Maf.GetRandomRotation());
                Vector3 randomForce = new Vector3(Random.Range(-1f, 1f), Random.Range(0.2f, 0.6f), Random.Range(-1f, 1f)) * throwForce;
                chip.GetComponent<Rigidbody>().AddForce(randomForce, ForceMode.Impulse);
                Destroy(chip, 10f);
                QOL.GetWaitForSeconds(0.1f);
            }
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
