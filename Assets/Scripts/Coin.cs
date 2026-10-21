using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1;

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            ScoreAndDataManager.TriggerCoinCollected(coinValue);
            gameObject.SetActive(false);
        }
    }
}