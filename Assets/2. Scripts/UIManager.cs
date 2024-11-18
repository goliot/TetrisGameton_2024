using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    [Header("# UI")]
    public Slider LifeSlider;

    [Header("# PlayerControl")]
    private int life = 8;

    private void Awake()
    {
        instance = this;
    }

    public void DecreaseLife()
    {
        LifeSlider.value = --life * 0.125f;
        if(life < 1)
        {
            GameOver();
        }
    }

    public void GameOver()
    {

    }
}
