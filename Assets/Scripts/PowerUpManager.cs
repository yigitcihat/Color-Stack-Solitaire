using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : Singleton<PowerUpManager>
{
    [SerializeField] private TextMeshProUGUI hintLabel, shuffleLabel;
    [SerializeField] private Image hintIcon, shuffleIcon;

    private Image Icon(PowerUps powerUp) => powerUp switch
    {
        PowerUps.shuffle => shuffleIcon,
        PowerUps.hint => hintIcon,
        _ => shuffleIcon
    };

    private TextMeshProUGUI Label(PowerUps powerUp) => powerUp switch
    {
        PowerUps.shuffle => shuffleLabel,
        PowerUps.hint => hintLabel,
        _ => shuffleLabel
    };
    
    private void Start() => PowerUp.allCases.ForEach(Set);
    
    internal void Use(PowerUp powerUp)
    {
        powerUp.count--;
        Set(powerUp);
    }

    internal void Set(PowerUp powerUp)
    {
        Label(powerUp.type).gameObject.SetActive(true);
        Icon(powerUp.type).gameObject.SetActive(false);
        powerUp.setText(Label(powerUp.type));
    }
}

public enum PowerUps { shuffle, hint }


public class PowerUp
{
    internal static readonly PowerUp shuffle = new (PowerUps.shuffle);
    internal static readonly PowerUp hint = new (PowerUps.hint);

    internal static readonly List<PowerUp> allCases = new() { shuffle, hint };

    internal readonly PowerUps type;

    private string name => Enum.GetName(typeof(PowerUps), type);
    
    internal int count
    {
        get => PlayerPrefs.GetInt(name, 0);
        set => PlayerPrefs.SetInt(name, value);
    }
    
    private PowerUp(PowerUps powerUp) => type = powerUp;
    
    internal void Use() => Set(count - 1);
    public void Add(int value) => Set(count + value);

    private void Set(int value)
    {
        count = value;
        PowerUpManager.Instance.Set(this);
    }
    
    internal void setText(TextMeshProUGUI label) => label.text = $"{count}";
}