using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score {get; private set;}
    public static event System.Action<int> OnMultiKill;
    //float lastEnemyKilledTime;
    //public float multikillInterval = 0.5f;
    //int multikillCount = 1;

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKilled;    
    }

    void OnEnemyKilled(bool notUsed)
    {
        /*if (Time.time < lastEnemyKilledTime + multikillInterval){
            multikillCount++;
            OnMultiKill?.Invoke(multikillCount);
        } else {
            multikillCount = 1;
        }

        lastEnemyKilledTime = Time.time; */
        score++;
    }

    void OnDestroy()
    {
        score = 0;
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
