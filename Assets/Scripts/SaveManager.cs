using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

internal static class SaveManager
{
    private const string key = "save";

    private static string save
    {
        get => PlayerPrefs.GetString(key, "");
        set => PlayerPrefs.SetString(key, value);
    }
    
    internal static SaveData Load()
    {
        if (save.Length < 5 || GameManager.level == 1) return new(GameManager.level);
        
        var data = JsonConvert.DeserializeObject<SaveData>(save);
            // , new JsonSerializerSettings
            // {
            //     Error = delegate(object sender, ErrorEventArgs args)
            //     {
            //         Debug.Log(args.ErrorContext.Error.Message);
            //         args.ErrorContext.Handled = true;
            //     }
            // });
        Debug.Log($"Save Cards -> {data.cards.Length}");
        data.cards.ToList().ForEach(c => Debug.Log($"Card: {c.card.name} -> {c.position.name}"));
        return data;
    }

    internal static void Save()
    {
        var data = new SaveData(GameManager.Instance);
        save = JsonConvert.SerializeObject(data, Formatting.None);
    }

    internal static void Clear() => save = "";
}

[Serializable]
[Preserve]
public class CardData {
    public CardPosition position;
    public Card card;

    public CardData() {}

    internal CardData(CardPosition position, Card card)
    {
        this.position = position;
        this.card = card;
    }
}
 
[Serializable]
[Preserve]
public class SaveData
{
    public CardData[] cards;

    public SaveData() {}
    
    internal SaveData(CardData[] cards)
    {
        cards.ToList().ForEach(c => Debug.Log($"Card: {c.card.name} -> {c.position.name}"));
        this.cards = cards;
    }
    internal SaveData(GameManager manager)
    {
        cards = manager.cards.Select(c => new CardData(c.position, c.card)).ToArray();
    }

    internal static readonly List<Card> playOrder = new()
    {
        Card.blue1,
        Card.red3,
        Card.red2,
        Card.red1,
        Card.red4,
        Card.blue2,
        Card.blue1,
    };

    // private static readonly CardData[] tutorialDeck  = new List<CardData>() { 
    //     new(new(Placement.hand, 0, 0), Card.yellow2), 
    //     new(new(Placement.hand, 0, 1), Card.blue3), 
    //     new(new(Placement.hand, 0, 2), Card.red1), 
    //     new(new(Placement.hand, 0, 3), Card.blue4), 
    //     new(new(Placement.hand, 0, 4), Card.yellow4), 
    //     new(new(Placement.area, 0, 0), Card.green3), 
    //     new(new(Placement.area, 0, 1), Card.green2), 
    //     new(new(Placement.area, 0, 2), Card.green1), 
    //     new(new(Placement.area, 1, 0), Card.red4), 
    //     new(new(Placement.area, 1, 1), Card.red3), 
    //     new(new(Placement.area, 2, 0), Card.blue1), 
    //     new(new(Placement.area, 3, 0), Card.red2), 
    //     new(new(Placement.area, 4, 0), Card.yellow1), 
    //     new(new(Placement.deck, 0, 0), Card.blue2), 
    //     new(new(Placement.deck, 0, 1), Card.green4) ,
    //     new(new(Placement.deck, 0, 2), Card.yellow3)
    // }.ToArray();
    
    // internal static readonly List<Card> tutorialCards = new () { Card.yellow2, Card.red3, Card.yellow4, Card.red1};
    internal static readonly List<Card> tutorialCards = new () { Card.red2, Card.yellow3, Card.blue1, Card.green4, Card.red1, Card.yellow4};

    private static readonly CardData[] tutorialDeck  = new List<CardData>() { 
        new(new(Placement.hand, 0, 0), Card.red2), 
        new(new(Placement.hand, 0, 1), Card.yellow3), 
        new(new(Placement.hand, 0, 2), Card.blue1), 
        new(new(Placement.hand, 0, 3), Card.green4), 
        new(new(Placement.hand, 0, 4), Card.red1), 
        new(new(Placement.area, 0, 0), Card.yellow4), 
        new(new(Placement.area, 1, 0), Card.green3), 
        new(new(Placement.area, 1, 1), Card.green2), 
        new(new(Placement.area, 1, 2), Card.green1), 
        new(new(Placement.area, 2, 0), Card.blue4), 
        new(new(Placement.area, 2, 1), Card.blue3), 
        new(new(Placement.area, 2, 2), Card.blue2), 
        new(new(Placement.area, 3, 0), Card.red4),
        new(new(Placement.area, 3, 1), Card.red3), 
        new(new(Placement.area, 4, 0), Card.yellow2), 
        new(new(Placement.area, 4, 1), Card.yellow1)
    }.ToArray();
    
    internal SaveData(int level)
    {
        GameManager.moveCount = 0;
        if (level == 1)
        {
            cards = tutorialDeck;
            return;
        }
        List<CardData> cardDatas = new();
        var deck = Card.Deck().Shuffled();
        for (var index = 0; index < deck.Count; index++)
        {
            var card = deck[index];
            var placement = index < 5 ? Placement.hand : index < 10 ? Placement.area : Placement.deck;
            var col = index < 5 ? 0 : index < 10 ? index % 5 : 0;
            var row = index < 5 ? index : index < 10 ? 0 % 5 : index - 10;
            cardDatas.Add(new(new(placement, col, row), card));
        }

        cards = cardDatas.ToArray();
    }
}