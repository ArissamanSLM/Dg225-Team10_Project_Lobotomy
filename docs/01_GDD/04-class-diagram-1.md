---
type: gdd-class-diagram
version: 0.2
date: 2026-07-21
---
# Class Diagram — Dream Slayer

Set the `hp` variable to `50` in 

```mermaid

flowchart TD
    Start([Start Run]) --> Node[Node Selection]

    subgraph CoreLoop [Core Game Loop]
        Node --> Explore[Explore / Combat / Event]
        Explore --> Reward[Reward / Consequence]
        Reward --> Node
    end

    Explore -->|Player Dies| Defeat([Game Over])
    Explore -->|Boss Defeated| Victory([Victory])
```

```csharp

using System;

public class Program
{
    public static void Main()
    {
        int hp = 50;
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine(hp);
        }
    }
}
```

```

```

```mermaid
graph TD
    Start([Game Start]) --> TitleScene
  
    TitleScene -->|Start Game| SceneStory1
    TitleScene -->|Quit Game| EndGame([Exit Game])
  
    subgraph StorySequence [Story Intro]
        SceneStory1 --> SceneStory2
        SceneStory2 --> SceneStory3
        SceneStory3 --> SceneStory4
        SceneStory4 --> SceneStory5
    end

    SceneStory5 --> NodeSelectScene

    NodeSelectScene -->|Select Node| RoomNode{Node Type}

    RoomNode -->|Encounter / Elite / Boss| BattleScene
    RoomNode -->|Event| EventChoice[Event Scene via EventSceneFactory]
    RoomNode -->|Shop| ShopRestScene

    BattleScene -->|Victory| RewardScene
    BattleScene -->|Player Defeated| GameOverScene
  
    RewardScene -->|Continue| NodeSelectScene
    ShopRestScene -->|Leave| NodeSelectScene
    EventChoice -->|Resolve Choice| NodeSelectScene

    NodeSelectScene -.->|View Stats| PlayerStatsScene
    PlayerStatsScene -.->|Back| NodeSelectScene

    GameOverScene -->|Restart| TitleScene
    GameOverScene -->|Quit Game| EndGame

```
