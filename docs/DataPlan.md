```csharp
 public class TitleScene : IScene
{
    private MenuState _currentState = MenuState.Main;
    private SpriteFont _font;
  
    public void Initialize() { }

    public void LoadContent(ContentManager content)
    {
        _font = content.Load<SpriteFont>("File");
    }

    public void Update(GameTime gameTime)
    {
        switch (_currentState)
        {
            case MenuState.Main:
                // Handle main menu input and state transitions
                break;
        }
    }
}
```[cite: 13]

---

### **Part 2: Map Generation & Adjacency List**
* **File:** `MapNode.cs`[cite: 5]
* **Description:** Represents graph nodes and stores neighbor connections using an adjacency list[cite: 5].
```csharp
public class MapNode
{
    public string Name { get; set; } = "Node";
    public Vector2 Position;
    public NodeType Type;
    public List<MapNode> ConnectedNodes = new List<MapNode>(); // Graph Adjacency List
    public bool IsVisited = false;
    public bool IsAvailable = false;
}
```[cite: 5]

---

### **Part 3: Room / Node Transition Manager**
* **File:** `RoomManager.cs`[cite: 6]
* **Description:** Controls room progression and random node rolls across floors[cite: 6].
```csharp
public class RoomManager
{
    public int RoomCount { get; private set; } = 0;
    public RoomType CurrentRoom { get; private set; } = RoomType.Event;

    public RoomType ChooseNode()
    {
        RoomCount++;
        if (RoomCount % 13 == 0)
        {
            CurrentRoom = RoomType.Boss;
            return CurrentRoom;
        }
        return CurrentRoom;
    }
}
```[cite: 6]

---

### **Part 4: Player Controller & Deck Queue**
* **File:** `PlayerController.cs`[cite: 7]
* **Description:** Manages player stats and FIFO queue-based card drawing logic[cite: 7].
```csharp
public class PlayerController
{
    public int PlayerHP { get; set; }
    public int Sanity { get; set; }
    public List<CardManager> Deck { get; set; } = new List<CardManager>();
    public CardManager[] Hand { get; set; } = new CardManager[5];
    public List<CardManager> DiscardPile { get; set; } = new List<CardManager>();

    public void DrawCard()
    {
        if (Deck.Count <= 0) return;
        CardManager card = Deck[0];
        Deck.RemoveAt(0);
        // Assign to hand slot
    }
}
```[cite: 7]

---

### **Part 5: Card Data & Factory Pattern**
* **File:** `CardManager.cs`[cite: 8]
* **Description:** Configures card attributes, costs, and effects using pattern matching[cite: 8].
```csharp
public class CardManager
{
    public enum CardType { Attack, Defense, Heal, Utility, Special }
    public string Name { get; set; }
    public int Cost { get; set; }
    public CardType Type { get; set; }

    public void CardReader()
    {
        switch (Name)
        {
            case "Strike":
                InHandDamage = 6;
                Description = "Basic attack standard strike.";
                break;
        }
    }
}
```[cite: 8]

---

### **Part 6: Starter Deck Initializer**
* **File:** `CardStarterDeck.cs`[cite: 9]
* **Description:** Populates initial starting cards based on the selected player class[cite: 9].
```csharp
public static class CardStarterDeck
{
    public static List<CardManager> GetStarterDeck(CharacterClassV2 characterClass)
    {
        List<CardManager> starterDeck = new List<CardManager>();
        if (characterClass == CharacterClassV2.Human)
        {
            for (int i = 0; i < 4; i++) 
                starterDeck.Add(new CardManager(i + 1, "Strike", 1, CardManager.CardType.Attack));
        }
        return starterDeck;
    }
}
```[cite: 9]

---

### **Part 7: Monster AI & Intent System**
* **File:** `BaseEnemy.cs`[cite: 17]
* **Description:** Defines abstract enemy behaviors and intent roll cycles[cite: 17].
```csharp
public abstract class BaseEnemy
{
    public string Name { get; protected set; }
    public int HP { get; set; }
    public int Defense { get; set; }

    public abstract void RollNextAction(bool isSanityLow);
}
```[cite: 17]

---

### **Part 8: Item & Relic Dictionary Lookups**
* **File:** `ItemManager.cs`[cite: 11]
* **Description:** Implements quick LINQ-based lookups to fetch relic definitions by name[cite: 11].
```csharp
public class ItemManager
{
    private readonly List<RelicDefinition> _definitions = new List<RelicDefinition>();
    private readonly List<RelicInstance> _playerRelics = new List<RelicInstance>();

    public void AddRelicToPlayer(string relicName, int tier = 1)
    {
        var def = _definitions.FirstOrDefault(d => string.Equals(d.Name, relicName, StringComparison.OrdinalIgnoreCase));
        if (def == null) throw new ArgumentException($"Relic not found: {relicName}");
        _playerRelics.Add(new RelicInstance(def, tier));
    }
}
```[cite: 11]

---

### **Part 9: Battle Scene Loop**
* **File:** `BattleScene.cs`[cite: 15]
* **Description:** Handles turn progression, card inputs, and combat execution loops[cite: 15].
```csharp
public class BattleScene : IScene
{
    private PlayerController _player;
    private BaseEnemy _enemy;

    public void Update(GameTime gameTime)
    {
        if (isMouseClicked && cardBounds.Contains(currentMouseState.Position))
        {
            TryPlayCard(i);
        }
    }
}
```[cite: 15]

---

### **Part 10: Game Over & Score Calculation**
* **File:** `GameOverScene.cs`[cite: 18]
* **Description:** Computes run metrics and final scores upon victory or defeat[cite: 18].
```csharp
public class GameOverScene : IScene
{
    private int _nodesPassed;
    private int _finalScore;

    public GameOverScene(int nodesPassed, int enemiesCleared, int cardsPlayed, bool isVictory, bool isDefeat, int floorsPassed)
    {
        _nodesPassed = nodesPassed;
        _finalScore = CalculateTotalScore();
    }

    private int CalculateTotalScore()
    {
        return (_nodesPassed * 200) + (_enemiesCleared * 100) + (_cardsPlayed * 5);
    }
}```[cite: 18]
```
