using System;
using System.Collections.Generic;
using DreamSlayerV2.Core;

namespace DreamSlayerV2.Scenes
{
    public enum EnemyActionType
    {
        Attack,
        Defend,
        EldritchDebuff,
        DoNothing
    }

    public sealed class EnemyAction
    {
        public EnemyActionType Type { get; }
        public int Value { get; }

        public EnemyAction(EnemyActionType type, int value = 0)
        {
            Type = type;
            Value = value;
        }
    }

    public abstract class BaseEnemy
    {
        private IReadOnlyList<EnemyAction> _nextActions = Array.Empty<EnemyAction>();

        public string Name { get; protected set; }
        public int HP { get; set; }
        public int MaxHP { get; protected set; }
        public int Defense { get; set; }
        public IReadOnlyList<EnemyAction> NextActions => _nextActions;

        public abstract void RollNextAction(bool isSanityLow);

        protected void SetNextActions(params EnemyAction[] actions)
        {
            _nextActions = actions;
        }

        public virtual void ExecuteTurn(PlayerController player)
        {
            foreach (EnemyAction action in _nextActions)
            {
                switch (action.Type)
                {
                    case EnemyActionType.Attack:
                        int damage = Math.Max(0, action.Value - player.Defense);
                        player.Defense = Math.Max(0, player.Defense - action.Value);
                        player.PlayerHP = Math.Max(0, player.PlayerHP - damage);
                        break;
                    case EnemyActionType.Defend:
                        Defense += action.Value;
                        break;
                    case EnemyActionType.EldritchDebuff:
                        player.Sanity = Math.Max(0, player.Sanity - action.Value);
                        break;
                }
            }
        }
    }

    public class MonsterBattle : BaseEnemy
    {
        private readonly bool _isElite;
        private readonly Random _rng = new Random();

        public MonsterBattle(bool isElite = false)
        {
            _isElite = isElite;
            Name = isElite ? "Unreal Wolf (Elite)" : "Unreal Wolf";
            MaxHP = isElite ? 90 : 30;
            HP = MaxHP;
        }

        public override void RollNextAction(bool isSanityLow)
        {
            int roll = _rng.Next(0, 100);

            if (roll < 60)
            {
                int damage = _isElite ? _rng.Next(12, 20) : _rng.Next(6, 12);
                SetNextActions(new EnemyAction(EnemyActionType.Attack, damage));
            }
            else if (roll < 85)
            {
                SetNextActions(new EnemyAction(EnemyActionType.Defend, _isElite ? 12 : 8));
            }
            else
            {
                SetNextActions(new EnemyAction(EnemyActionType.EldritchDebuff, 15));
            }
        }
    }

    public class BossBattle : BaseEnemy
    {
        private readonly Random _rng = new Random();

        public int CurrentPhase { get; private set; } = 1;

        public BossBattle()
        {
            Name = "Cthulhu";
            MaxHP = 100;
            HP = MaxHP;
            Defense = 12;
        }

        public void TransitionToPhase2()
        {
            CurrentPhase = 2;
            MaxHP = 125;
            HP = MaxHP;
            Defense = 20;
        }

        public override void RollNextAction(bool isSanityLow)
        {
            if (CurrentPhase == 1)
            {
                EnemyAction action = _rng.Next(4) switch
                {
                    0 => new EnemyAction(EnemyActionType.Attack, _rng.Next(8, 41)),
                    1 => new EnemyAction(EnemyActionType.Defend, 12),
                    2 => new EnemyAction(EnemyActionType.EldritchDebuff, 25),
                    _ => new EnemyAction(EnemyActionType.DoNothing)
                };

                SetNextActions(action);
                return;
            }

            var phaseTwoActions = new List<EnemyActionType>
            {
                EnemyActionType.Attack,
                EnemyActionType.Defend,
                EnemyActionType.EldritchDebuff,
                EnemyActionType.DoNothing
            };

            EnemyAction firstAction = CreatePhaseTwoAction(phaseTwoActions[_rng.Next(phaseTwoActions.Count)]);
            if (_rng.Next(100) >= 25)
            {
                SetNextActions(firstAction);
                return;
            }

            phaseTwoActions.Remove(firstAction.Type);
            EnemyAction secondAction = CreatePhaseTwoAction(phaseTwoActions[_rng.Next(phaseTwoActions.Count)]);
            SetNextActions(firstAction, secondAction);
        }

        private EnemyAction CreatePhaseTwoAction(EnemyActionType type)
        {
            return type switch
            {
                EnemyActionType.Attack => new EnemyAction(type, _rng.Next(15, 29)),
                EnemyActionType.Defend => new EnemyAction(type, 18),
                EnemyActionType.EldritchDebuff => new EnemyAction(type, 35),
                _ => new EnemyAction(EnemyActionType.DoNothing)
            };
        }
    }
}
