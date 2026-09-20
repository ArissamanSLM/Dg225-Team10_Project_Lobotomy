using System;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace DreamSlayerV2.Scenes;

public interface IEvent : IScene
{

public enum EventTag
{
    Lucky,
    Normal,
    Nightmare
}

public interface IEvent : IScene
{
    EventTag Tag { get; }
    string DialogText { get; }
    void ExecuteConsequence(int choiceIndex);
}
}

