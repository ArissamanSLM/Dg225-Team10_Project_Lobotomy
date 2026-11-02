using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;

namespace DreamSlayerV2.Core
{
    public static class GlobalContentManager
    {
        private static ContentManager _content;
        private static GraphicsDevice _graphicsDevice;
        private static SpriteFont _defaultFont;
        private static SpriteFont _battleFont;
        private static SpriteFont _natureFont;
        private static Song _nightmareSong;
        private static Song _nodeSong;
        private static Texture2D _menuBackground;
        private static Texture2D _forestBackground;
        private static Texture2D _caveBackground;
        private static Texture2D _mapLeft;
        private static Texture2D _mapRight;
        private static Texture2D _enemy;
        private static Texture2D _cardSheet;
        private static Texture2D _healthBar;
        private static Texture2D _sanityBar;
        private static Texture2D _shieldBar;
        private static Texture2D _enemyHealthBar;
        private static Texture2D _healthEffect;
        private static Texture2D _shopIcon;
        private static Texture2D _eliteIcon;
        private static Texture2D _bossIcon;
        private static Texture2D _encounterIcon;
        private static Texture2D _plusCard;
        private static SoundEffect _drawCardsSound;
        private static SoundEffect _drawCardSound;
        private static SoundEffect _playerCardSound;
        private static SoundEffect _enemyAttackSound;
        private static SoundEffect _badEffectSound;
        private static SoundEffect _healSound;
        private static SoundEffect _titleMusicEffect;
        private static Texture2D _whitePixel;
        private static Texture2D _nodeCircle;
        private static Texture2D[] _battleBackgrounds;
        private static Texture2D[] _battleUiTextures;
        private static SoundEffect[] _drawCardSounds;
        private static SoundEffect[] _enemyAttackSounds;
        private static Texture2D[] _mapBackgrounds;
        private static Texture2D _firstMenu;
    private static Texture2D _secondMenu;
    private static Texture2D _startMenu;
    private static Texture2D _continueMenu;
    private static Texture2D _exitMenu;
    private static Texture2D _optionsMenu;
    private static Texture2D _upgradesMenu;
    private static Texture2D _hardResetMenu;
    private static SoundEffect _shieldBlockShortswordSound;
    private static Texture2D[] _mainMenuBackgroundArray;
    private static Texture2D _SceneStoryBackground1;
    private static Texture2D _SceneStoryBackground2;
    private static Texture2D[] _SceneStoryBackgroundArray;
    private static Texture2D _eliteEnemy;
    private static Texture2D _bossEnemy;
    private static Texture2D _portal;
    private static Texture2D _nextFloorLogo;
    private static Texture2D _rewardScreen;
    private static Texture2D _soulCoinIcon;

        public static GraphicsDevice GraphicsDevice =>
            _graphicsDevice ?? throw new InvalidOperationException("Global content has not been initialized.");

        public static SpriteFont DefaultFont => GetAsset(ref _defaultFont, "File");
        public static SpriteFont BattleFont => GetAsset(ref _battleFont, "BattleUIFont");
        public static SpriteFont NatureFont => GetAsset(ref _natureFont, "NatureFonts");
        public static Song NightmareSong => GetAsset(ref _nightmareSong, "leberch-nightmare-611631");
        public static Song NodeSong => GetAsset(ref _nodeSong, "Start_music (1)");
        public static Texture2D MenuBackground => GetAsset(ref _menuBackground, "Blankmenu");
        public static Texture2D ForestBackground => GetAsset(ref _forestBackground, "HorrorForestDreamV2");
        public static Texture2D CaveBackground => GetAsset(ref _caveBackground, "TheDarkCave");
        public static Texture2D MapLeft => GetAsset(ref _mapLeft, "MapLeft_DreamSlayer");
        public static Texture2D MapRight => GetAsset(ref _mapRight, "MapRight_DreamSlayer");
        public static Texture2D Enemy => GetAsset(ref _enemy, "UnrealWolf");
        public static Texture2D CardSheet => GetAsset(ref _cardSheet, "All16Card (1)");
        public static Texture2D HealthBar => GetAsset(ref _healthBar, "HealthBar");
        public static Texture2D SanityBar => GetAsset(ref _sanityBar, "Sanity_bar");
        public static Texture2D ShieldBar => GetAsset(ref _shieldBar, "Sheid_bar");
        public static Texture2D EnemyHealthBar => GetAsset(ref _enemyHealthBar, "Enemies_health_bar");
        public static Texture2D HealthEffect => GetAsset(ref _healthEffect, "HealthEffect");
        public static Texture2D ShopIcon => GetAsset(ref _shopIcon, "Shop_logo");
        public static Texture2D EliteIcon => GetAsset(ref _eliteIcon, "E_enemy2");
        public static Texture2D BossIcon => GetAsset(ref _bossIcon, "Boss_logo");
        public static Texture2D EncounterIcon => GetAsset(ref _encounterIcon, "enemy_logo");
        public static Texture2D PlusCard => GetAsset(ref _plusCard, "Pluscard");
        public static SoundEffect DrawCardsSound => GetAsset(ref _drawCardsSound, "Handing out cards");
        public static SoundEffect DrawCardSound => GetAsset(ref _drawCardSound, "Handing out cards(One Card)");
        public static SoundEffect EnemyAttackSound => GetAsset(ref _enemyAttackSound, "attack");
        public static SoundEffect AttackSound => EnemyAttackSound;
        public static SoundEffect ShieldBlockSound => GetAsset(ref _shieldBlockShortswordSound, "sectionsound-shield-block-shortsword-143940");
        public static SoundEffect PlayCardSound => GetAsset(ref _playerCardSound, "Play Card");
        public static SoundEffect BadEffectSound => GetAsset(ref _badEffectSound, "Bad_Effect_gain");
        public static SoundEffect HealSound => GetAsset(ref _healSound, "heal");
        public static SoundEffect TitleMusicEffect => GetAsset(ref _titleMusicEffect, "TitleMusic");
        public static Texture2D FirstMenu => GetAsset(ref _firstMenu, "BlankUI1");
    public static Texture2D SecondMenu => GetAsset(ref _secondMenu, "BlankUI2");
    public static Texture2D ThirdMenu => GetAsset(ref _startMenu, "START");
    public static Texture2D FourthMenu => GetAsset(ref _continueMenu, "CONTINUE");
    public static Texture2D FifthMenu => GetAsset(ref _exitMenu, "EXIT");
    public static Texture2D SixthMenu => GetAsset(ref _optionsMenu, "OPTIONS");
    public static Texture2D SeventhMenu => GetAsset(ref _upgradesMenu, "UPGRADES");
    public static Texture2D EighthMenu => GetAsset(ref _hardResetMenu, "HardReset");
    public static Texture2D SceneStoryBackground1 => GetAsset(ref _SceneStoryBackground1, "Scene_1_20261101212247");
    public static Texture2D SceneStoryBackground2 => GetAsset(ref _SceneStoryBackground2, "Scene_2_20261101212255");
    public static Texture2D EliteEnemy => GetAsset(ref _eliteEnemy, "Elite_Enemies_1");
    public static Texture2D BossEnemy => GetAsset(ref _bossEnemy, "Kraken-BossPhase1");
    public static Texture2D Portal => GetAsset(ref _portal, "Portal");
    public static Texture2D NextFloorLogo => GetAsset(ref _nextFloorLogo, "Next_Floor_logo");
    public static Texture2D RewardScreen => GetAsset(ref _rewardScreen, "reward_screen");
    public static Texture2D SoulCoinIcon => GetAsset(ref _soulCoinIcon, "Soul coin UI");


        public static Texture2D[] BattleBackgrounds => _battleBackgrounds ??= new[]
        {
            ForestBackground,
            ForestBackground,
            CaveBackground
        };
        public static Texture2D[] BattleUiTextures => _battleUiTextures ??= new[]
        {
            SanityBar,
            ShieldBar,
            EnemyHealthBar
        };
        public static SoundEffect[] DrawCardSounds => _drawCardSounds ??= new[]
        {
            DrawCardsSound,
            DrawCardSound,
            null
        };
        // public static SoundEffect[] PlayerCardSounds => _playerCardSounds ??= new[]
        // {
        //     null,
        //     null,
        //     null
        // };
        public static SoundEffect[] EnemyAttackSounds => _enemyAttackSounds ??= new[]
        {
            EnemyAttackSound,
            null,
            null
        };
        public static Texture2D[] MapBackgrounds => _mapBackgrounds ??= new[]
        {
            MapLeft,
            MapRight
        };
        public static Texture2D[] MainmenuBackground => _mainMenuBackgroundArray ??= new[]
    {
        FirstMenu,
        SecondMenu,
        ThirdMenu,
        FourthMenu,
        FifthMenu,
        SixthMenu,
        SeventhMenu,
        EighthMenu
    };
        public static Texture2D[] SceneStoryBackgrounds => _SceneStoryBackgroundArray ??= new[]
        {
            SceneStoryBackground1,
            SceneStoryBackground2
        };

        public static Texture2D WhitePixel
        {
            get
            {
                if (_whitePixel == null)
                {
                    _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
                    _whitePixel.SetData(new[] { Color.White });
                }

                return _whitePixel;
            }
        }

        public static Texture2D NodeCircle
        {
            get
            {
                if (_nodeCircle == null)
                {
                    const int diameter = 64;
                    _nodeCircle = new Texture2D(GraphicsDevice, diameter, diameter);
                    var pixels = new Color[diameter * diameter];
                    float center = (diameter - 1) / 2f;
                    float radius = diameter / 2f - 1f;
                    for (int y = 0; y < diameter; y++)
                    {
                        for (int x = 0; x < diameter; x++)
                        {
                            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                            byte coverage = (byte)(MathHelper.Clamp(radius + 0.5f - distance, 0f, 1f) * 255f);
                            pixels[y * diameter + x] = new Color(coverage, coverage, coverage, coverage);
                        }
                    }

                    _nodeCircle.SetData(pixels);
                }

                return _nodeCircle;
            }
        }

        public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }

        public static T Load<T>(string assetName)
        {
            if (_content == null)
            {
                throw new InvalidOperationException("Global content has not been initialized.");
            }

            return _content.Load<T>(assetName);
        }

        private static T GetAsset<T>(ref T asset, string assetName) where T : class
        {
            return asset ?? (asset = Load<T>(assetName));
        }
    }
}