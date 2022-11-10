using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public enum Status
    {
        Normal, Cold, Die
    }
    public static Status status;

    [SerializeField] float hp;
    [SerializeField] float maxHp;
    [SerializeField] int money;

    // Callback Methods
    void Start()
    {
        hp = maxHp;
    }

    void Update()
    {
        
    }

    // Public Methods
    // 가명
    public void AddHp(float _hp)
    {
        hp += _hp;
    }

    // 가명
    public void AddMoney(int _money)
    {
        money += _money;
    }
}
