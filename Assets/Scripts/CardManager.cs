using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using LeTai.TrueShadow;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[SuppressMessage("ReSharper", "ParameterHidesMember")]
public class CardManager : MonoBehaviour, IPointerDownHandler
{
    private static GameManager manager => GameManager.Instance;

    [SerializeField] internal Image frontImage, backImage;
    [SerializeField] internal Canvas frontCanvas;
    [SerializeField] internal TrueShadow shadow;
    [SerializeField] internal RectTransform rect;
    [SerializeField] private Color darkColor, redColor, finalShadowColor;

    internal bool isHighlighted, isMoving;
    internal Card card;
    internal CardPosition position { get; private set; }

    private Tween selection, highlight;
    private bool isVisible;

    internal void Set(CardData cardData)
    {
        card = cardData.card;
        position = cardData.position;
        frontImage.sprite = card.sprite;
    }

    public void OnPointerDown(PointerEventData eventData) => manager.TappedOn(this);

    internal void Tapped()
    {
        Taptic.Medium();
        var cardGroup = GetCardGroup();
        if (cardGroup.Count == 0) return;

        var topCard = cardGroup[0];
        var bottomCard = cardGroup[^1];

        for (var i = 0; i < 5; i++)
        {
            var targetCards = manager.GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
            if (targetCards.Count == 0) continue;
            var targetAreaTopCard = targetCards[0];
            var targetAreaBottomCard = targetCards[^1];

            if (targetAreaTopCard.card.series == bottomCard.card.series && targetAreaTopCard.card.number == bottomCard.card.number - 1)
            {
                MoveCards(cardGroup, new(Placement.area, i, 0));
                return;
            }

            if (targetAreaBottomCard.card.series == topCard.card.series && targetAreaBottomCard.card.number == topCard.card.number + 1)
            {
                MoveCards(cardGroup, new(Placement.area, i, 0));
                return;
            }
        }

        for (var i = 0; i < 5; i++)
        {
            var targetCards = manager.GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
            if (targetCards.Count != 0) continue;
            MoveCards(cardGroup, new(Placement.area, i, 0));
            return;
        }

        var numbers = new List<int>() { 0, 1, 2, 3, 4 };
        numbers.Shuffle();

        foreach (var i in numbers)
        {
            var targetCards = manager.GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
            if (targetCards.Count == 0) continue;
            var targetAreaTopCard = targetCards[0];
            var targetAreaBottomCard = targetCards[^1];

            if (targetAreaTopCard.card.number == bottomCard.card.number - 1)
            {
                // if (previousPosition.placement == Placement.area && previousPosition.col == i) continue;
                MoveCards(cardGroup, new(Placement.area, i, 0));
                return;
            }

            if (targetAreaBottomCard.card.number == topCard.card.number + 1)
            {
                // if (previousPosition.placement == Placement.area && previousPosition.col == i) continue;
                MoveCards(cardGroup, new(Placement.area, i, 0));
                return;
            }
        }

        Warn();
    }

    public bool HasPotentialTarget()
    {
        if (!isVisible || manager.status == GameStatus.starting) return false;
        if (position.placement != Placement.area) return hasMovement();
        var group = GetCardGroup();
        if (group.Count == 0) return false;
        // if (this != group[0] && this != group[^1])  return false;
        return hasMovement();
    }

    private bool hasMovement() => CheckIfAvailableSameColor() || CheckIfAvailableEmptyArea() || CheckIfAvailableAnyColor();

    private bool CheckIfAvailableSameColor()
    {
        var cardGroup = GetCardGroup();
        var topGroupCard = cardGroup[0];
        var bottomGroupCard = cardGroup[^1];
        
        for (var i = 0; i < 5; i++)
        {
            var areaCards = manager.GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
            if (areaCards.Count == 0) continue;
            if (areaCards.Any(c => c.card.number == card.number)) continue;

            var topAreaCard = areaCards[0];
            var bottomAreaCard = areaCards[^1];

            // if (topCard.card.series == card.series && topCard.card.number == card.number - 1) return true;
            // if (bottomCard.card.series == card.series && bottomCard.card.number == card.number + 1) return true;
            
            if (topAreaCard.card.series == bottomGroupCard.card.series && topAreaCard.card.number == bottomGroupCard.card.number - 1) return true;
            if (bottomAreaCard.card.series == topGroupCard.card.series && bottomAreaCard.card.number == topGroupCard.card.number + 1) return true;
        }

        return false;
    }

    private bool CheckIfAvailableAnyColor()
    {
        var cardGroup = GetCardGroup();
        var topGroupCard = cardGroup[0];
        var bottomGroupCard = cardGroup[^1];
        
        for (var i = 0; i < 5; i++)
        {
            var areaCards = manager.GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
            if (areaCards.Any(c => c.card.number == card.number)) continue;
            
            var topAreaCard = areaCards[0];
            var bottomAreaCard = areaCards[^1];
            
            if (topAreaCard.card.number == bottomGroupCard.card.number - 1) return true;
            if (bottomAreaCard.card.number == topGroupCard.card.number + 1) return true;
        }

        return false;
    }

    internal bool CheckIfAvailableEmptyArea() => new List<int> { 0, 1, 2, 3 , 4 }.Any(i => manager.GetCardsAt(Placement.area, i).Count == 0);

    // for (var i = 0; i < 5; i++)
    // {
    //     if (manager.GetCardsAt(Placement.area, i).Count == 0) return true;
    // }

        
    

    internal List<CardManager> GetCardGroup()
    {
        if (position.placement == Placement.hand) return new() { this };
        var cards = manager.GetCardsAt(Placement.area).Where(c => c.position.col == position.col).ToList();
        if (cards.Count == 0) return new List<CardManager>();
        List<List<CardManager>> cardsGrouped = new();
        var cardsSorted = cards.OrderByDescending(c => c.card.number).ToList();
        List<CardManager> cardsColored = new();

        foreach (var cardSorted in cardsSorted)
        {
            if (cardsColored.IsNullOrEmpty())
            {
                cardsColored.Add(cardSorted);
                continue;
            }

            if (cardsColored[0].card.series == cardSorted.card.series)
            {
                cardsColored.Add(cardSorted);
                continue;
            }

            var array = new CardManager[cardsColored.Count];
            cardsColored.CopyTo(array);
            cardsGrouped.Add(array.ToList());
            cardsColored.Clear();
            cardsColored.Add(cardSorted);
        }

        cardsGrouped.Add(cardsColored);
        cardsGrouped = cardsGrouped.Count > 2 ? new() { cardsGrouped[0], cardsGrouped[^1] } : cardsGrouped;
        foreach (var cardsInGroup in cardsGrouped.Where(cardsInGroup => cardsInGroup.Contains(this))) return cardsInGroup;
        return new();
    }

    private async void MoveCards(List<CardManager> cardGroup, CardPosition targetPosition)
    {
        manager.cards.ForEach(c => c.Lowlight());

        if (GameManager.level == 1) manager.hand.GetComponentInChildren<Image>().DOFade(0,GameManager.animationDuration/2);
        
        if (GameManager.moveCount == SaveData.tutorialCards.Count - 1 && GameManager.level == 1)
            if (GameManager.Instance.hand is not null)
                Destroy(GameManager.Instance.hand);

        var originalPlacement = position.placement;
        var originalCol = position.col;
        var areaCards = manager.GetCardsAt(targetPosition.placement, targetPosition.col);
        areaCards.AddRange(cardGroup);
        var maxNumber = areaCards.Select(c => c.card.number).Max();

        foreach (var cardManager in areaCards)
        {
            var neighbors = manager.GetCardsBelow(cardManager);
            foreach (var neighbor in neighbors) neighbor.SetPosition(new(neighbor.position.placement, neighbor.position.col, neighbor.position.row - 1));
            cardManager.SetPosition(new(targetPosition.placement, targetPosition.col, maxNumber - cardManager.card.number));
        }

        areaCards = manager.GetCardsAt(Placement.area, position.col).OrderByDescending(c => c.card.number).ToList();
        maxNumber = areaCards.Select(c => c.card.number).Max();
        foreach (var cardManager in areaCards) cardManager.SetPosition(new(cardManager.position.placement, cardManager.position.col, maxNumber - cardManager.card.number));
        foreach (var cardManager in manager.cards) cardManager.Animate();
        switch (originalPlacement)
        {
            case Placement.hand:
                manager.DealCardsToHand();
                await Task.Delay((int)(GameManager.animationDuration * 500));
                break;
            case Placement.area when manager.GetCardsAt(originalPlacement, originalCol).Count == 0:
                manager.DealCardToArea(originalCol);
                await Task.Delay((int)(GameManager.animationDuration * 500));
                break;
        }

        if (areaCards.Count(c => c.card.series == card.series) == (manager.cards.Count / 4))
        {
            await Task.Delay((int)(GameManager.animationDuration * 1000));
            //Sound.tirt.Play();
        
            foreach (var cardManager in areaCards)
            {
                cardManager.Celebrate();
                await Task.Delay((int)(GameManager.animationDuration * 370));
                shadow.Color = finalShadowColor;
            }


            var finalCol = 0;
            for (var i = 0; i < 4; i++)
            {
                var completedCards = manager.GetCardsAt(Placement.final, i).Count;
                for (var j = 0; j < completedCards; j++)
                {
                    manager.GetCardsAt(Placement.final, i)[j].shadow.Color = finalShadowColor;
                }
                if (completedCards != 0) continue;
                finalCol = i;
                break;
            }

            for (var index = 0; index < areaCards.Count; index++)
            {
                shadow.Color = finalShadowColor;
                var cardManager = areaCards[index];
                cardManager.SetPosition(new(Placement.final, finalCol, index));
                cardManager.Animate();
                await Task.Delay((int)(GameManager.animationDuration * 500));
            }

            Sound.success.Play();
            Taptic.Light();

            await Task.Delay((int)(GameManager.animationDuration * 500));

            areaCards[0].Explode();

            if (manager.GetCardsAt(Placement.deck).Count > 0)
            {
                manager.DealCardToArea(targetPosition.col);
                await Task.Delay((int)(GameManager.animationDuration * 1000));
            }
        }

        foreach (var cardManager in manager.cards) cardManager.Sort();
        GameManager.moveCount++;
        SaveManager.Save();
        manager.CheckGameStatus();
        Sound.move.Play();
        if (manager.status != GameStatus.playing) return;
        if (GameManager.level == 2 && GameManager.moveCount < 3) manager.ActivateHint();
        if (GameManager.level > 1) return;
        manager.ProcessTutorial();
    }

    internal void SetPosition(CardPosition position) => this.position = position;

    public void Animate(bool isFlipping = false)
    {
        Sort();
        var offset = Offset();

        if (position.placement == Placement.final) transform.DOScale(Vector3.one * 0.7f, GameManager.animationDuration).SetEase(Ease.InOutSine);
        else DOTween.To(() => shadow.Color, x => shadow.Color = x, position.placement == Placement.deck ? Color.clear : Color.black, GameManager.animationDuration / 2);

        rect.DOJumpAnchorPos(offset, 0.8f, 1, GameManager.animationDuration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            if (isFlipping) Flip();
        });
    }

    internal void Sort()
    {
        name = $"{card.name} - {position.name}";

        switch (position.placement)
        {
            case Placement.deck:
                var index = manager.GetCardsAt(Placement.deck).OrderBy(c => c.position.row).ToList().IndexOf(this);
                frontCanvas.sortingOrder = 60 - index;
                transform.SetSiblingIndex(index);
                break;
            case Placement.hand:
            {
                frontCanvas.sortingOrder = 20 + position.row;
                var previousCards = manager.GetCardsAt(Placement.deck).Count;
                transform.SetSiblingIndex(previousCards + position.row);
                break;
            }
            case Placement.area:
            {
                frontCanvas.sortingOrder = 20 - card.number;
                var previousCards = manager.GetCardsAt(Placement.deck).Count;
                previousCards += manager.GetCardsAt(Placement.hand).Count;
                for (var i = 0; i < position.col; i++) previousCards += manager.GetCardsAt(Placement.area, i).Count;
                transform.SetSiblingIndex(previousCards + position.row);
                break;
            }
            case Placement.final:
            default:
            {
                frontCanvas.sortingOrder = 10 + card.number;
                var previousCards = manager.GetCardsAt(Placement.deck).Count;
                previousCards += manager.GetCardsAt(Placement.hand).Count;
                previousCards += manager.GetCardsAt(Placement.area).Count;
                for (var i = 0; i < position.col; i++) previousCards += manager.GetCardsAt(Placement.final, i).Count;

                transform.SetSiblingIndex(previousCards + card.number);
                break;
            }
        }
    }

    private Vector2 Offset()
    {
        float x = 0, y = 0;

        switch (position.placement)
        {
            case Placement.deck:
                var positionDeck = manager.deckTransform.anchoredPosition;
                x = positionDeck.x;
                y = positionDeck.y;
                break;

            case Placement.hand:
                var positionHand = manager.handTransform.anchoredPosition;
                x = positionHand.x + (position.row - 2) * rect.rect.width / 1.3f;
                y = positionHand.y;
                break;

            case Placement.area:
                var positionArea = manager.areaTransforms[position.col].anchoredPosition;
                x = positionArea.x;
                y = positionArea.y - (position.row * rect.rect.height / 4);
                break;

            case Placement.final:
                var positionFinal = manager.finalTransforms[position.col].anchoredPosition;
                x = positionFinal.x; // + (position.col - 1.5f) * rect.rect.width * 1.1f;
                y = positionFinal.y;
                break;
        }

        return new(x, y); //,  ((position.placement == Placement.final ? card.number : position.row) / -10.0f)- 1);
    }

    private void Flip()
    {
        Sound.flip.Play();
        if (!isVisible) backImage.transform.DORotate(-Vector3.up * 90, GameManager.animationDuration * 0.5f).SetEase(Ease.InSine).OnComplete(() => { frontImage.transform.DORotate(Vector3.zero, 0.25f).SetEase(Ease.InSine); });
        else frontImage.transform.DORotate(Vector3.up * 90, GameManager.animationDuration * 0.5f).SetEase(Ease.InSine).OnComplete(() => { backImage.transform.DORotate(Vector3.zero, 0.25f).SetEase(Ease.InSine); });

        isMoving = false;
        isVisible = !isVisible;
    }

    private void Celebrate()
    {
        
        Lighten();
        Sound.tirt.Play();
        var stars = Instantiate(Resources.Load<GameObject>("starExplosion"));
        stars.transform.position = transform.position;
        transform.DOScale(Vector3.one * 1.35f, GameManager.animationDuration * 0.2f).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.one, GameManager.animationDuration * 0.2f).SetEase(Ease.InSine);
                shadow.Color = finalShadowColor;
            });
        // Debug.Log("-----" +Card.cardsPerSeries);
        // if (card.number == manager.cards.Count / Card.cardsPerSeries) DOTween.To(() => shadow.Color, x => shadow.Color = x, Color.black, GameManager.animationDuration * 0.7f);
    }

    private void Explode()
    {
        transform.DOScale(Vector3.one * 0.88f, GameManager.animationDuration * 0.15f).SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOScale(Vector3.one * 0.7f, GameManager.animationDuration * 0.15f).SetEase(Ease.OutSine));
    }

    internal void ActivateHint(bool isFree)
    {
        if (!isFree) PowerUp.hint.Use();
        Highlight();
    }

    internal void Warn()
    {
        Taptic.Light();
        Sound.wrong.Play();
        frontImage.transform.localPosition = Vector3.zero;
        frontImage.transform.DOLocalMove(frontImage.transform.localPosition + Vector3.right * 10, 0.05f)
            .OnComplete(() =>
                frontImage.transform.DOLocalMove(frontImage.transform.localPosition + Vector3.left * 20, 0.05f)
                    .OnComplete(() =>
                        frontImage.transform.DOLocalMove(frontImage.transform.localPosition + Vector3.right * 20, 0.05f)
                            .OnComplete(() =>
                                frontImage.transform.DOLocalMove(frontImage.transform.localPosition + Vector3.left * 20, 0.05f)
                                    .OnComplete(() =>
                                        frontImage.transform.DOLocalMove(frontImage.transform.localPosition + Vector3.right * 20, 0.05f)
                                            .OnComplete(() =>
                                                frontImage.transform.DOLocalMove(frontImage.transform.localPosition + Vector3.left * 10, 0.05f)
                                            )
                                    )
                            )
                    )
            );

        DOTween.To(() => frontImage.color, x => frontImage.color = x, redColor, GameManager.animationDuration / 2)
            .OnComplete(() => DOTween.To(() => frontImage.color, x => frontImage.color = x, Color.white, GameManager.animationDuration / 2));
    }

    internal void Highlight()
    {
        Sound.ding.Play();
        manager.isHintActive = true;
        isHighlighted = true;
        highlight = transform.DOScale(1.35f, GameManager.animationDuration * 2).SetLoops(-1, LoopType.Yoyo);
        highlight.onKill = () => transform.DOScale(1, GameManager.animationDuration / 2);
        DOTween.To(() => shadow.Color, x => shadow.Color = x, Color.white, GameManager.animationDuration / 2);
    }


    internal void Lowlight()
    {
        if (!isHighlighted) return;
        isHighlighted = false;
        manager.isHintActive = false;
        highlight.Kill();
        transform.DOScale(1, GameManager.animationDuration / 2);
        DOTween.To(() => shadow.Color, x => shadow.Color = x, Color.black, GameManager.animationDuration / 2);
    }

    internal void Darken()
    {
        DOTween.To(() => frontImage.color, x => frontImage.color = x, darkColor, GameManager.animationDuration / 2);
    }

    internal void Lighten()
    {
        // frontCanvas.sortingOrder = 99;
        DOTween.To(() => frontImage.color, x => frontImage.color = x, Color.white, GameManager.animationDuration / 2);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}