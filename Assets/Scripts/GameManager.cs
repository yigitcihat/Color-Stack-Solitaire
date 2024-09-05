using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    internal const float animationDuration = 0.35f;
    private const int shuffleTime = 3;
    
    [SerializeField] internal List<RectTransform> areaTransforms, finalTransforms;
    [SerializeField] internal RectTransform handTransform, deckTransform;
    [SerializeField] internal List<CardManager> cards = new();
    [SerializeField] private Transform cardParent, canvas;
    [SerializeField] internal GameObject Confetti;
    [SerializeField] internal TutorialOverlay tutorialPopUp;
    [SerializeField] private GameObject cardObject;
    [SerializeField] private List<string> tutorialTexts;
    
    internal CardManager selectedCard;
    internal GameObject hand;
    internal bool isHintActive, isAnimating;
    internal GameStatus status = GameStatus.starting;
    
    private readonly List<CardManager> tappedCards = new();
    private CardManager tutorialCard;
    private Vector2 cardSize;
    private float cardWidth, cardHeight, randomX, randomY, lastProcessTime, lastTapTime;
    private int playCount;

    private CardManager GetCardAt(Placement placement, int col, int row) => cards.OrderBy(c => c.position.row).ToList().First(c => c.position.placement == placement && c.position.col == col && c.position.row == row);
    internal List<CardManager> GetCardsAt(Placement placement, int col) => cards.Where(c => c.position.placement == placement && c.position.col == col).OrderBy(c => c.position.row).ToList().ToList();
    internal List<CardManager> GetCardsAt(Placement placement) => cards.Where(c => c.position.placement == placement).ToList();
    internal List<CardManager> GetCardsBelow(CardManager card) => cards.Where(c => c.position.placement == card.position.placement && c.position.col == card.position.col && (card.position.placement == Placement.hand ? c.position.row > card.position.row : c.card.number < card.card.number)).ToList();

    internal static int level
    {
        get => PlayerPrefs.GetInt("level", 1);
        private set => PlayerPrefs.SetInt("level", value);
    }
    
    internal static int moveCount
    {
        get => PlayerPrefs.GetInt("moveCount", 1);
        set => PlayerPrefs.SetInt("moveCount", value);
    }

    private void Awake()
    {
        lastTapTime = Time.time;
        SetCardSize();
    }

    private void Start()
    {
        if (level > 1) UIManager.Instance.TappedPlay(); 
    }
    
    private void Update()
    {
        ProcessTappedCards();
        // if (Input.GetKeyDown(KeyCode.H)) ActivateHint();
        // if (Input.GetKeyDown(KeyCode.S)) ShuffleAndDeal();
        // if (Input.GetKeyDown(KeyCode.R)) UIManager.Instance.TappedNextLevel();
        // if (Input.GetKeyDown(KeyCode.L)) Window.shuffle.Enter();
        // if (Input.GetKeyDown(KeyCode.Space))  Window.settings.Enter();

        if (level == 1) return;
        if (level == 2 && moveCount < 3) return;
        if (!(Time.time - lastTapTime > 20) || isHintActive || status != GameStatus.playing) return;
        ActivateHint();
        lastTapTime = Time.time + 100;
    }

    public void CreateCards()
    {
        var saveData = SaveManager.Load();

        foreach (var cardData in saveData.cards)
        {
            var card = Instantiate(cardObject, cardParent).GetComponent<CardManager>();
            card.Set(cardData);
            cards.Add(card);
        }

        StartGame();
    }

    private async void StartGame()
    {
        lastTapTime = Time.time;
        foreach (var cardManager in cards) cardManager.Sort();
        
        cards.ForEach(c => c.Lowlight());
        for (var i = 0; i < shuffleTime; i++)
        {
            Sound.cardShuffle.Play();

            foreach (var card in cards)
            {
                card.rect.DOAnchorPos(RandomPos(), animationDuration).SetEase(Ease.InSine);
                card.transform.DORotate(new(0, 0, Random.Range(-359, 359)), animationDuration).SetEase(Ease.InOutSine);
            }

            await Task.Delay((int)(animationDuration * 1000));
        }

        foreach (var card in cards)
        {
            card.rect.DOAnchorPos(Vector2.zero, animationDuration).SetEase(Ease.InSine);
            card.transform.DORotate(Vector3.zero, animationDuration).SetEase(Ease.InCirc);
            card.transform.DOLocalMove(Vector3.zero, animationDuration);
            card.transform.GetChild(1).localEulerAngles = Vector3.zero;
        }
        
        await Task.Delay((int)(animationDuration * 1000));
        

        for (var row = 0; row < GetCardsAt(Placement.hand).Count; row++)
        {
            var cardManager = GetCardAt(Placement.hand, 0, row);
            cardManager.Animate(true);
            await Task.Delay((int)(animationDuration * 200));
        }
        for (var col = 0; col < 5; col++)
        {
            for (var row = 0; row < GetCardsAt(Placement.area, col).Count; row++)
            {
                var cardManager = GetCardAt(Placement.area, col, row);
                cardManager.Animate(true);
                await Task.Delay((int)(animationDuration * 200));
            }
        }
        for (var col = 0; col < 4; col++)
        {
            for (var row = 0; row < GetCardsAt(Placement.final, col).Count; row++)
            {
                var cardManager = GetCardAt(Placement.final, col, row);
                Debug.Log($"Final: {cardManager.card.name} -> {cardManager.position.name}");
                cardManager.Animate(true);
                cardManager.transform.DOScale(Vector3.one * 0.7f, animationDuration * 0.15f);
                await Task.Delay((int)(animationDuration * 200));
            }
        }

        foreach (var cardManager in GetCardsAt(Placement.deck).OrderByDescending(c => c.position.row))
        {
            cardManager.Animate();
            await Task.Delay((int)(animationDuration * 50));
        }

        status = GameStatus.playing;
        isAnimating = false;

        if (level == 2 && moveCount < 4) ActivateHint();
        if (level > 1) return;
        
        hand = Instantiate(Resources.Load<GameObject>($"hand"), canvas, true);
        hand.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height - 500);
        hand.transform.localScale = Vector3.one;
        
        ProcessTutorial();
    }
    

    private async Task Shuffle()
    {
        cards.ForEach(c => c.Lowlight());
        for (var i = 0; i < shuffleTime; i++)
        {
            Sound.cardShuffle.Play();

            foreach (var card in GetCardsAt(Placement.deck))
            {
                card.backImage.GetComponent<Canvas>().sortingOrder += 100;
                card.rect.DOAnchorPos(RandomPos(), animationDuration).SetEase(Ease.InSine);
                card.transform.DORotate(new(0, 0, Random.Range(-359, 359)), animationDuration).SetEase(Ease.InOutSine);
            }

            await Task.Delay((int)(animationDuration * 1000));
        }

        foreach (var card in GetCardsAt(Placement.deck))
        {
            card.backImage.GetComponent<Canvas>().sortingOrder -= 100;
            card.rect.DOAnchorPos(Vector2.zero, animationDuration).SetEase(Ease.InSine);
            card.transform.DORotate(Vector3.zero, animationDuration).SetEase(Ease.InCirc);
            card.transform.DOLocalMove(Vector3.zero, animationDuration);
            card.transform.GetChild(1).localEulerAngles = Vector3.zero;
        }
        
        await Task.Delay((int)(animationDuration * 1000));
    }

    internal async void DealCardsToHand()
    {
        var handCardCount = GetCardsAt(Placement.hand).Count;
        handCardCount -= handCardCount == 0 ? 0 : 1;
        if (GetCardsAt(Placement.deck).Count == 0 || handCardCount >= 5) return;

        for (var index = 0; index < 5 - handCardCount; index++)
        {
            if (GetCardsAt(Placement.deck).Count == 0) return;
            if (GetCardsAt(Placement.hand).Count >=5) return;

            var card = level == 1 ? GetCardsAt(Placement.deck)[0] : GetCardsAt(Placement.deck).Choice();
            card.SetPosition(new(Placement.hand, 0, GetCardsAt(Placement.hand).Count));
            card.Animate(true);
            await Task.Delay((int)(animationDuration * 200));
        }
    }

    public void DealCardToArea(int col)
    {
        var deckCards = GetCardsAt(Placement.deck);
        if (deckCards.Count == 0) return;
        var card = deckCards.ToList()[0];
        card.SetPosition(new (Placement.area, col, 0));
        card.Animate(true);
    }

    internal async void ShuffleAndDeal()
    {
        Debug.Log("ShuffleAndDeal 1");
        if (isAnimating) return;
        isAnimating = true;
        Debug.Log("ShuffleAndDeal 2");
        var handCards = GetCardsAt(Placement.hand);
        foreach (var card in handCards)
        {
            
            card.SetPosition(new(Placement.deck, 0, GetCardsAt(Placement.deck).Count));
            card.Animate(true);
            await Task.Delay((int)(animationDuration * 150));
        }
        
        await Task.Delay((int)(animationDuration * 1350));
        
        await Shuffle();

        DealCardsToHand();
        
        foreach (var cardManager in GetCardsAt(Placement.deck).OrderByDescending(c => c.position.row))
        {
            cardManager.frontCanvas.sortingOrder -= 100;
            cardManager.Animate();
            await Task.Delay((int)(animationDuration * 200));
        }
        
        await Task.Delay((int)(animationDuration * 500));
        
        foreach (var cardManager in cards) cardManager.Sort();
        isAnimating = false;
    }

    internal void CheckGameStatus()
    {
        // Debug.Log("CheckGameStatus 1");
        
        if (GetCardsAt(Placement.final).Count == cards.Count)
        {
            foreach (var confetti in  Confetti.GetComponentsInChildren<ParticleSystem>())confetti.Play();
            UIManager.Instance.levelLabel.text = $"Level {level}\nCompleted";
            level++;
            cards.ForEach(c => c.Lowlight());
            Sound.levelCompleted.Play();
            SaveManager.Clear();
            status = GameStatus.won;
            moveCount = 0;
            PowerUp.allCases.ForEach(powerUp => powerUp.Add(5));
            Window.win.Enter();
            return;
        }

        // Debug.Log($"CheckGameStatus 2 {hasMoves()}");
        
        if (hasMoves()) return;
        
        Debug.Log($"CheckGameStatus 3 {GetCardsAt(Placement.hand).Count == 0}");

        if (GetCardsAt(Placement.hand).Count == 0 || PowerUp.shuffle.count == 0) 
        {
            status = GameStatus.lost;
            moveCount = 0;
            SaveManager.Clear();

            GameOver();
            return;
        }
        
        Debug.Log($"CheckGameStatus 4");
        Window.shuffle.Enter();
    }

    private static void GameOver() => Window.lose.Enter();

    private void SetCardSize()
    {
        var cardRect = cardObject.GetComponent<RectTransform>().rect;
        cardSize = new(cardRect.width, cardRect.height);
        cardWidth = (Screen.width - cardSize.x) / 2;
        cardHeight = (Screen.height- cardSize.y) / 2;
    }

    private Vector2 RandomPos()
    {
        randomX = Random.Range(-cardWidth/2, cardWidth/2);
        randomY = Random.Range(-cardHeight / 2, cardHeight/2);
        return new (randomX, randomY);
    }

    public void ActivateHint(bool isFree = true)
    {
        if (PowerUp.hint.count < 1 && !isFree) return;
        if (isHintActive) return;
        if (!isFree) Taptic.Medium();

        foreach (var cardManager in GetCardsAt(Placement.hand))
        {
            var cardGroup = cardManager.GetCardGroup();
            if (cardGroup.Count == 0) continue;
        
            var topCard = cardGroup[0];
            var bottomCard = cardGroup[^1];
        
            Debug.Log($"CheckForCardGroup {cardGroup.Count}");
            for (var i = 0; i < 5; i++)
            {
                var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                if (targetCards.Count == 0) continue;
                var targetAreaTopCard = targetCards[0];
                var targetAreaBottomCard = targetCards[^1];
        
                if (targetAreaTopCard.card.series == bottomCard.card.series && targetAreaTopCard.card.number == bottomCard.card.number - 1)
                {
                    bottomCard.ActivateHint(isFree);
                    return;
                }

                if (targetAreaBottomCard.card.series == topCard.card.series && targetAreaBottomCard.card.number == topCard.card.number + 1)
                {
                    topCard.ActivateHint(isFree);
                    return;                
                }
            }
        }

        for (var col = 0; col < 5; col++)
        {
            var areColCards = GetCardsAt(Placement.area, col).OrderByDescending(c => c.card.number).ToList();
            
            foreach (var cardManager in areColCards)
            {
                var cardGroup = cardManager.GetCardGroup();
                if (cardGroup.Count == 0) continue;
        
                var topCard = cardGroup[0];
                var bottomCard = cardGroup[^1];
        
                Debug.Log($"CheckForCardGroup {cardGroup.Count}");
                for (var i = 0; i < 5; i++)
                {
                    var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                    if (targetCards.Count == 0) continue;
                    var targetAreaTopCard = targetCards[0];
                    var targetAreaBottomCard = targetCards[^1];
        
                    if (targetAreaTopCard.card.series == bottomCard.card.series && targetAreaTopCard.card.number == bottomCard.card.number - 1)
                    {
                        bottomCard.ActivateHint(isFree);
                        return;
                    }

                    if (targetAreaBottomCard.card.series == topCard.card.series && targetAreaBottomCard.card.number == topCard.card.number + 1)
                    {
                        topCard.ActivateHint(isFree);
                        return;                
                    }
                }
            }
        }
        
        foreach (var cardManager in GetCardsAt(Placement.hand))
        {
            var cardGroup = cardManager.GetCardGroup();
            if (cardGroup.Count == 0) continue;
        
            var topCard = cardGroup[0];
            var bottomCard = cardGroup[^1];
        
            for (var i = 0; i < 5; i++)
            {
                var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                if (targetCards.Count == 0)
                {
                    cardManager.ActivateHint(isFree);
                    return;
                }
               
            }
        }

        for (var col = 0; col < 5; col++)
        {
            var areColCards = GetCardsAt(Placement.area, col).OrderByDescending(c => c.card.number).ToList();
            
            foreach (var cardManager in areColCards)
            {
                var cardGroup = cardManager.GetCardGroup();
                if (cardGroup.Count == 0) continue;
        
                var topCard = cardGroup[0];
                // var bottomCard = cardGroup[^1];
        
                for (var i = 0; i < 5; i++)
                {
                    var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                    if (targetCards.Count != 0) continue;
                    topCard.ActivateHint(isFree);
                    return;
                }
            }
        }
        
        foreach (var cardManager in GetCardsAt(Placement.hand))
        {
            var cardGroup = cardManager.GetCardGroup();
            if (cardGroup.Count == 0) continue;
        
            var topCard = cardGroup[0];
            var bottomCard = cardGroup[^1];
        
            for (var i = 0; i < 5; i++)
            {
                var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                if (targetCards.Count == 0) continue;
                var targetAreaTopCard = targetCards[0];
                var targetAreaBottomCard = targetCards[^1];
        
                if (targetAreaTopCard.card.number == bottomCard.card.number - 1)
                {
                    cardManager.ActivateHint(isFree);
                    return;
                }

                if (targetAreaBottomCard.card.number == topCard.card.number + 1)
                {
                    cardManager.ActivateHint(isFree);
                    return;   
                }
            }
        }

        for (var col = 0; col < 5; col++)
        {
            var areColCards = GetCardsAt(Placement.area, col).OrderByDescending(c => c.card.number).ToList();
            
            foreach (var cardManager in areColCards)
            {
                var cardGroup = cardManager.GetCardGroup();
                if (cardGroup.Count == 0) continue;
        
                var topCard = cardGroup[0];
                var bottomCard = cardGroup[^1];
        
                for (var i = 0; i < 5; i++)
                {
                    var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                    if (targetCards.Count == 0) continue;
                    var targetAreaTopCard = targetCards[0];
                    var targetAreaBottomCard = targetCards[^1];
        
                    if (targetAreaTopCard.card.number == bottomCard.card.number - 1)
                    {
                        bottomCard.ActivateHint(isFree);
                        return;
                    }

                    if (targetAreaBottomCard.card.number == topCard.card.number + 1)
                    {
                        topCard.ActivateHint(isFree);
                        return;   
                    }
                }
            }
        }

        CheckGameStatus();
    }

    private bool hasMoves()
    {
        if (new List<int> { 0, 1, 2, 3 , 4 }.Any(i => GetCardsAt(Placement.area, i).Count == 0)) return true;

        foreach (var cardManager in GetCardsAt(Placement.hand))
        {
            var cardGroup = cardManager.GetCardGroup();
            if (cardGroup.Count == 0) continue;
        
            var topCard = cardGroup[0];
            var bottomCard = cardGroup[^1];
        
            for (var i = 0; i < 5; i++)
            {
                var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                if (targetCards.Count == 0) continue;
                var targetAreaTopCard = targetCards[0];
                var targetAreaBottomCard = targetCards[^1];
        
                if (targetAreaTopCard.card.series == bottomCard.card.series && targetAreaTopCard.card.number == bottomCard.card.number - 1) return true;
                if (targetAreaBottomCard.card.series == topCard.card.series && targetAreaBottomCard.card.number == topCard.card.number + 1) return true;
            }
        }

        for (var col = 0; col < 5; col++)
        {
            var areColCards = GetCardsAt(Placement.area, col).OrderByDescending(c => c.card.number).ToList();
            
            foreach (var cardManager in areColCards)
            {
                var cardGroup = cardManager.GetCardGroup();
                if (cardGroup.Count == 0) continue;
        
                var topCard = cardGroup[0];
                var bottomCard = cardGroup[^1];
        
                for (var i = 0; i < 5; i++)
                {
                    var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                    if (targetCards.Count == 0) continue;
                    var targetAreaTopCard = targetCards[0];
                    var targetAreaBottomCard = targetCards[^1];
        
                    if (targetAreaTopCard.card.series == bottomCard.card.series && targetAreaTopCard.card.number == bottomCard.card.number - 1) return true;
                    if (targetAreaBottomCard.card.series == topCard.card.series && targetAreaBottomCard.card.number == topCard.card.number + 1) return true;
                }
            }
        }
        
        foreach (var cardManager in GetCardsAt(Placement.hand))
        {
            var cardGroup = cardManager.GetCardGroup();
            if (cardGroup.Count == 0) continue;
        
            var topCard = cardGroup[0];
            var bottomCard = cardGroup[^1];
        
            for (var i = 0; i < 5; i++)
            {
                var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                if (targetCards.Count == 0) continue;
                var targetAreaTopCard = targetCards[0];
                var targetAreaBottomCard = targetCards[^1];
        
                if (targetAreaTopCard.card.series == bottomCard.card.series && targetAreaTopCard.card.number == bottomCard.card.number - 1) return true;
                if (targetAreaBottomCard.card.series == topCard.card.series && targetAreaBottomCard.card.number == topCard.card.number + 1) return true;
            }
        }

        for (var col = 0; col < 5; col++)
        {
            var areColCards = GetCardsAt(Placement.area, col).OrderByDescending(c => c.card.number).ToList();
            
            foreach (var cardManager in areColCards)
            {
                var cardGroup = cardManager.GetCardGroup();
                if (cardGroup.Count == 0) continue;

                for (var i = 0; i < 5; i++)
                {
                    var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                    if (targetCards.Count != 0) continue;
                    return true;
                }
            }
        }
        
        foreach (var cardManager in GetCardsAt(Placement.hand))
        {
            var cardGroup = cardManager.GetCardGroup();
            if (cardGroup.Count == 0) continue;
        
            var topCard = cardGroup[0];
            var bottomCard = cardGroup[^1];
        
            for (var i = 0; i < 5; i++)
            {
                var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                if (targetCards.Count == 0) continue;
                var targetAreaTopCard = targetCards[0];
                var targetAreaBottomCard = targetCards[^1];
        
                if (targetAreaTopCard.card.number == bottomCard.card.number - 1) return true;
                if (targetAreaBottomCard.card.number == topCard.card.number + 1) return true;
            }
        }

        for (var col = 0; col < 5; col++)
        {
            var areaColCards = GetCardsAt(Placement.area, col).OrderByDescending(c => c.card.number).ToList();
            
            foreach (var cardManager in areaColCards)
            {
                var cardGroup = cardManager.GetCardGroup();
                if (cardGroup.Count == 0) continue;
        
                var topCard = cardGroup[0];
                var bottomCard = cardGroup[^1];
               
                for (var i = 0; i < 5; i++)
                {
                   
                    var targetCards = GetCardsAt(Placement.area, i).OrderByDescending(c => c.card.number).ToList();
                    if (targetCards.Count == 0) continue;
                    var targetAreaTopCard = targetCards[0];
                    var targetAreaBottomCard = targetCards[^1];
        
                    if (targetAreaTopCard.card.number == bottomCard.card.number - 1) return true;
                    if (targetAreaBottomCard.card.number == topCard.card.number + 1) return true;   
                    
                }
            }
        }

        return false;
    }
    
    internal async void ProcessTutorial()
    {
        // if (moveCount + 1 >= tutorialTexts.Count) tutorialPopUp.MoveUp();
        if (moveCount >= SaveData.tutorialCards.Count)
        {
            foreach (var cardManager in cards) cardManager.Lighten();
            tutorialCard = null;
            ActivateHint();
            if (hand is null) return;
            Destroy(hand);
            return;
        }

        foreach (var cardManager in cards.Where(c => !new List<Placement>() { Placement.final , Placement.deck }.Contains(c.position.placement)))
        {
            if (cardManager.card != SaveData.tutorialCards[moveCount])
            {
                cardManager.Darken();
                continue;
            }
            
            tutorialCard = cardManager;
            cardManager.Highlight();
            cardManager.Lighten();
            // tutorialPopUp.MoveDown();
            // tutorialPopUp.SetText(tutorialTexts[moveCount]);
            if (SaveData.tutorialCards.Count <= moveCount)continue;
        }
        
        if (tutorialCard is null) return;
        await Task.Delay((int)(animationDuration * 1000));
        hand.transform.position = tutorialCard.transform.position;
        hand.GetComponentInChildren<Image>().DOFade(1, GameManager.animationDuration/2);
        // hand.transform.DOMove(tutorialCard.transform.position, animationDuration).SetEase(Ease.InOutSine);
    }

    public void TappedOn(CardManager cardManager)
    {
        Debug.Log($"Tapped On -> {cardManager.card.name}");
        if (cardManager.isMoving) return;
        if (level == 1 && tutorialCard is not null && tutorialCard != cardManager) return;
        if (new List<Placement> { Placement.final, Placement.deck }.Contains(cardManager.position.placement)) return;
        if (!cardManager.HasPotentialTarget())
        {
            cardManager.Warn();
            return;
        }
        lastTapTime = Time.time;
        tappedCards.Add(cardManager);
    }

    private void ProcessTappedCards()
    {
        if (lastProcessTime > Time.time - 0.1) return;
        if (tappedCards.IsNullOrEmpty()) return;
        lastProcessTime = Time.time;
        
        var card = tappedCards.FirstAndPop();
        cards.Where(c => c.isHighlighted).ToList().ForEach(c => c.Lowlight());
        card.Tapped();
    }
  
}

internal enum GameStatus { starting, playing, won, lost }