---
type: gdd-core-loop
version: 0.17
date: [วันที่]
---

# [ชื่อเกม] — Core Loop & Gameplay

## Core Loop
```mermaid
flowchart LR
    A[เริ่มต้น] -- Monster --> B[Battle state]
    B --> C{Won battle?}
    C -- Yes --> D[Reward / Consequence]
    C -- No --> E[game over/Clear]
    D --> F{Is all monster clear?}
    F -- Yes --> E
    F -- No --> A
    

```

## Core Mechanics
1. [Mechanic หลักที่ 1 — Rng Serch rescoues ๆ]
2. [Mechanic หลักที่ 2 - Explortion]
3. [Mechanic ----- 3 - Health system]
4. [Mechanic ----- 4 = Sanity system]
5. [Mechanic ----- 5 - Currency/shop]
6. [Mechanic ----- 6 - Upgrade/Tier up]
7. [Mechanic ----- 7 - Perk/Level]
8. [Mechanic ----- 8 - Vision/Sense]
9. [Mechanic ----- 9 - Weapons/cards]
10. [Mechanic ----- 10 - Monster Vision]
11. [Mechanic ----- 11 - Inventory Bar slot]
12. [Mechanic ----- 12 - Rescores bar slot] // (from fortnite ref)

## Controls
| Key | Action |
|---|---|
| ← → | Move |
| Space | Jump |
| [E] | [Intraction] |
| [H] | [Hide] |
| [C] | [Counch] |
| [1-9] | [Slot 1-9] |
| [Esc] | Menu |
| [V] | tools Sense |
| [R] | Reload Gun |

## Win / Lose Condition
- **ชนะเมื่อ:** [Endless]
- **แพ้เมื่อ:** [Hp/sanity = 0 or Quit game during in game]
