using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    public bool infinite;
    public int enemyCount;
    
    public float pounderSpawnChance = 25f;
    public float timeBetweenSpawns;
    public float enemySpeed;
    public int attackDamage;
    public int enemyHealth;
    public Color enemyColor;
    public Color attackColor;
}