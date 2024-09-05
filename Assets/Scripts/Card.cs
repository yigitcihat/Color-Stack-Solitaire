using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Scripting;


[Serializable]
[Preserve]
public class Card
{
    public Series series;
    public Cards cards;

    internal static int cardsPerSeries;
    internal int number => Convert.ToInt16(Enum.GetName(typeof(Cards), cards)!.Replace(Enum.GetName(typeof(Series), series)!, ""));
    internal string name => $"{Enum.GetName(typeof(Series), series)} {number}";
    internal Sprite sprite => Resources.Load<Sprite>($"Cards/{cards}");

    internal static readonly Card red1 = new(Series.red, Cards.red1);
    internal static readonly Card red2 = new(Series.red, Cards.red2);
    internal static readonly Card red3 = new(Series.red, Cards.red3);
    internal static readonly Card red4 = new(Series.red, Cards.red4);
    internal static readonly Card red5 = new(Series.red, Cards.red5);
    internal static readonly Card red6 = new(Series.red, Cards.red6);
    internal static readonly Card red7 = new(Series.red, Cards.red7);
    internal static readonly Card red8 = new(Series.red, Cards.red8);
    internal static readonly Card red9 = new(Series.red, Cards.red9);
    internal static readonly Card red10 = new(Series.red, Cards.red10);
    internal static readonly Card blue1 = new(Series.blue, Cards.blue1);
    internal static readonly Card blue2 = new(Series.blue, Cards.blue2);
    internal static readonly Card blue3 = new(Series.blue, Cards.blue3);
    internal static readonly Card blue4 = new(Series.blue, Cards.blue4);
    internal static readonly Card blue5 = new(Series.blue, Cards.blue5);
    internal static readonly Card blue6 = new(Series.blue, Cards.blue6);
    internal static readonly Card blue7 = new(Series.blue, Cards.blue7);
    internal static readonly Card blue8 = new(Series.blue, Cards.blue8);
    internal static readonly Card blue9 = new(Series.blue, Cards.blue9);
    internal static readonly Card blue10 = new(Series.blue, Cards.blue10);
    internal static readonly Card yellow1 = new(Series.yellow, Cards.yellow1);
    internal static readonly Card yellow2 = new(Series.yellow, Cards.yellow2);
    internal static readonly Card yellow4 = new(Series.yellow, Cards.yellow4);
    internal static readonly Card yellow3 = new(Series.yellow, Cards.yellow3);
    internal static readonly Card yellow5 = new(Series.yellow, Cards.yellow5);
    internal static readonly Card yellow6 = new(Series.yellow, Cards.yellow6);
    internal static readonly Card yellow7 = new(Series.yellow, Cards.yellow7);
    internal static readonly Card yellow8 = new(Series.yellow, Cards.yellow8);
    internal static readonly Card yellow9 = new(Series.yellow, Cards.yellow9);
    internal static readonly Card yellow10 = new(Series.yellow, Cards.yellow10);
    internal static readonly Card green1 = new(Series.green, Cards.green1);
    internal static readonly Card green2 = new(Series.green, Cards.green2);
    internal static readonly Card green3 = new(Series.green, Cards.green3);
    internal static readonly Card green4 = new(Series.green, Cards.green4);
    internal static readonly Card green5 = new(Series.green, Cards.green5);
    internal static readonly Card green6 = new(Series.green, Cards.green6);
    internal static readonly Card green7 = new(Series.green, Cards.green7);
    internal static readonly Card green8 = new(Series.green, Cards.green8);
    internal static readonly Card green9 = new(Series.green, Cards.green9);
    internal static readonly Card green10 = new(Series.green, Cards.green10);

    internal static List<Card> Deck()
    {
        var cardsForLevel = new List<Card>();
        var allCards = new Dictionary<Series, List<Card>>()
        {
            { Series.red, new() { red1, red2, red3, red4, red5, red6, red7, red8, red9, red10 } },
            { Series.blue, new() { blue1, blue2, blue3, blue4, blue5, blue6, blue7, blue8, blue9, blue10 } },
            { Series.yellow, new() { yellow1, yellow2, yellow3, yellow4, yellow5, yellow6, yellow7, yellow8, yellow9, yellow10 } },
            { Series.green, new() { green1, green2, green3, green4, green5, green6, green7, green8, green9, green10 } },
        };

        cardsPerSeries = GameManager.level / 2 + 3;
        //     GameManager.level switch
        // {
        //     1 or 2 or 3 => 4,
        //     4 or 5 or 6 => 5,
        //     6 or 5 => 6,
        //     7 or 5 => 7,
        //     8 or 5 => 8,
        //     9 or 5 => 9,
        //     _ => 10
        // };
        

        Debug.Log("cardsPerSeries: " + cardsPerSeries);
        
        foreach (var entry in allCards)
        {
            cardsForLevel.AddRange(entry.Value.Where(card => card.number <= cardsPerSeries));
        }

        // return new() { red1, red1, red1, red1, red1, red1, red1, red1, red1, red1, red1, red1, red1, red1, red1, red1 };
        return cardsForLevel;
    }

    public Card() {}
    private Card(Series series, Cards cards)
    {
        this.series = series;
        this.cards = cards;
    }
}

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
[Preserve]
public enum Cards
{
    red1, red2, red3, red4, red5, red6, red7, red8, red9, red10,
    blue1, blue2, blue3, blue4, blue5, blue6, blue7, blue8, blue9, blue10,
    yellow1, yellow2, yellow3, yellow4, yellow5, yellow6, yellow7, yellow8, yellow9, yellow10,
    green1, green2, green3, green4, green5, green6, green7, green8, green9, green10
}

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
[Preserve]
public enum Series { red, blue, yellow, green }


[Serializable]
[Preserve]
public class CardPosition
{
    public Placement placement;
    public int col, row;

    public CardPosition() {}

    public CardPosition(Placement placement, int col, int row)
    {
        this.placement = placement;
        this.col = col;
        this.row = row;
    }

    internal string name => $"{Enum.GetName(typeof(Placement), placement)} {col} {row}";

    internal void setPlacement(Placement placement) => this.placement = placement;
    internal void setCol(int col) => this.col = col;
    internal void setRow(int row) => this.row = row;
}

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
[Preserve]
public enum Placement { deck, hand, area, final }