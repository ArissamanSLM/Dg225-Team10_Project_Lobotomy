---
type: gdd-mechanics
version: 0.1
date: [วันที่]
---
# Mechanic Design — [ชื่อ Mechanic]

## State Diagram

```mermaid
stateDiagram-v2
    [*] --> Idle
  
    Idle --> Advance : เลือกเส้นทาง (Choose Node)
    Advance --> Idle : เริ่ม Encounter/Event
  
    Idle --> Attack : ใช้ card
    Attack --> calculate : คำนวณ damage รับ
  
    calculate --> Idle : Enemy Dies
    calculate --> EnemyAction : Enemy Alive
  
    EnemyAction --> Idle : จบ Turn  
```

## Rules

| State   | เข้าเงื่อนไข                        | ออกเงื่อนไข | Note                              |
| ------- | ----------------------------------------------- | ---------------------- | --------------------------------- |
| Idle    | เริ่มเกม / หยุดเคลื่อนที่ | กด input ใดๆ      | Animation loop                    |
| Attack  | โจมตีกัน                               | ใช้ card โจมตี | Damage Monster Recieve = X Damage |
| Adcance | Enemy Defeated + Idle State                     | อยุ่ note ไหม่ | Advance1 Step                     |
