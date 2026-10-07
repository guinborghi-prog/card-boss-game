using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBoss
{
    // Builds the whole battle screen from code, so the scene only needs one GameObject
    // with this component on it. Swap pieces for prefabs and art later as the game grows.
    [RequireComponent(typeof(BattleManager))]
    public class BattleUI : MonoBehaviour
    {
        static readonly Color BackgroundColor = new Color(0.10f, 0.11f, 0.15f);
        static readonly Color PanelColor = new Color(0.17f, 0.19f, 0.25f);
        static readonly Color AttackCardColor = new Color(0.62f, 0.22f, 0.22f);
        static readonly Color SkillCardColor = new Color(0.22f, 0.38f, 0.62f);
        static readonly Color MixedCardColor = new Color(0.45f, 0.30f, 0.60f);
        static readonly Color ButtonColor = new Color(0.85f, 0.65f, 0.20f);
        static readonly Color IntentColor = new Color(1f, 0.85f, 0.45f);
        static readonly Vector2 CardSize = new Vector2(180, 260);

        BattleManager battle;
        Font font;

        Text bossNameText;
        Text bossStatsText;
        Text bossIntentText;
        Text playerStatsText;
        Text pileText;
        Text logText;
        RectTransform handArea;
        Button endTurnButton;
        GameObject overlay;
        Text overlayTitle;
        RectTransform overlayButtons;

        void Awake()
        {
            battle = GetComponent<BattleManager>();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            BuildLayout();
            battle.Changed += Refresh;
        }

        void OnDestroy()
        {
            if (battle != null) battle.Changed -= Refresh;
        }

        void Refresh()
        {
            var boss = battle.Boss;
            var player = battle.Player;

            bossNameText.text = $"{boss.Name}   (boss {battle.BossNumber} of {battle.BossCount})";
            bossStatsText.text = $"HP {boss.Hp}/{boss.MaxHp}" + (boss.Block > 0 ? $"     Block {boss.Block}" : "");
            bossIntentText.text = battle.Phase == BattlePhase.PlayerTurn
                ? $"Next move: {battle.BossIntent.Describe()}"
                : "";

            playerStatsText.text = $"HP {player.Hp}/{player.MaxHp}     Block {player.Block}     Mana {battle.Mana}/{battle.MaxMana}";
            pileText.text = $"Turn {battle.Turn}     Draw pile {battle.DrawPileCount}     Discard {battle.DiscardPileCount}";
            logText.text = string.Join("\n", battle.Log);

            ClearChildren(handArea);
            for (int i = 0; i < battle.Hand.Count; i++)
            {
                int index = i;
                var card = battle.Hand[i];
                var button = CreateCardButton(handArea, card, () => battle.PlayCard(index));
                button.interactable = battle.CanPlay(card);
            }

            endTurnButton.interactable = battle.Phase == BattlePhase.PlayerTurn;
            RefreshOverlay();
        }

        void RefreshOverlay()
        {
            ClearChildren(overlayButtons);
            overlay.SetActive(battle.Phase != BattlePhase.PlayerTurn);

            switch (battle.Phase)
            {
                case BattlePhase.Reward:
                    overlayTitle.text = $"{battle.Boss.Name} defeated!\nChoose a card to add to your deck";
                    for (int i = 0; i < battle.Rewards.Count; i++)
                    {
                        int index = i;
                        CreateCardButton(overlayButtons, battle.Rewards[i], () => battle.ChooseReward(index));
                    }
                    CreateOverlayButton("Skip", () => battle.ChooseReward(-1));
                    break;
                case BattlePhase.Victory:
                    overlayTitle.text = "You defeated every boss!";
                    CreateOverlayButton("Play Again", battle.Restart);
                    break;
                case BattlePhase.Defeat:
                    overlayTitle.text = $"You were defeated by {battle.Boss.Name}";
                    CreateOverlayButton("Try Again", battle.Restart);
                    break;
            }
        }

        void BuildLayout()
        {
            var canvasObject = new GameObject("BattleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var root = canvasObject.transform;

            CreateImage("Background", root, BackgroundColor, Vector2.zero, Vector2.one);

            // Boss panel across the top.
            var bossPanel = CreateImage("BossPanel", root, PanelColor, new Vector2(0.25f, 0.68f), new Vector2(0.75f, 0.96f)).transform;
            bossNameText = CreateText("BossName", bossPanel, 48, TextAnchor.MiddleCenter, new Vector2(0f, 0.64f), new Vector2(1f, 0.95f));
            bossStatsText = CreateText("BossStats", bossPanel, 36, TextAnchor.MiddleCenter, new Vector2(0f, 0.36f), new Vector2(1f, 0.64f));
            bossIntentText = CreateText("BossIntent", bossPanel, 32, TextAnchor.MiddleCenter, new Vector2(0f, 0.05f), new Vector2(1f, 0.36f));
            bossIntentText.color = IntentColor;

            // Combat log in the middle.
            logText = CreateText("Log", root, 24, TextAnchor.UpperCenter, new Vector2(0.2f, 0.45f), new Vector2(0.8f, 0.66f));
            logText.color = new Color(0.8f, 0.8f, 0.85f);

            // Player stats just above the hand.
            playerStatsText = CreateText("PlayerStats", root, 34, TextAnchor.MiddleLeft, new Vector2(0.02f, 0.36f), new Vector2(0.55f, 0.44f));
            pileText = CreateText("Piles", root, 26, TextAnchor.MiddleRight, new Vector2(0.55f, 0.36f), new Vector2(0.98f, 0.44f));

            handArea = CreateRect("Hand", root, new Vector2(0.02f, 0.02f), new Vector2(0.84f, 0.35f));
            AddRowLayout(handArea, 12);

            endTurnButton = CreateButton("EndTurn", root, "End Turn", ButtonColor, battle.EndTurn);
            SetAnchors((RectTransform)endTurnButton.transform, new Vector2(0.86f, 0.12f), new Vector2(0.98f, 0.24f));

            // Full-screen overlay for rewards, victory and defeat. Created last so it draws on top.
            overlay = CreateImage("Overlay", root, new Color(0f, 0f, 0f, 0.85f), Vector2.zero, Vector2.one).gameObject;
            overlayTitle = CreateText("Title", overlay.transform, 56, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.88f));
            overlayButtons = CreateRect("Buttons", overlay.transform, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.6f));
            AddRowLayout(overlayButtons, 24);
            overlay.SetActive(false);
        }

        Button CreateCardButton(Transform parent, CardData card, UnityAction onClick)
        {
            var button = CreateButton($"Card {card.cardName}", parent, null, CardColor(card), onClick);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = CardSize;

            CreateText("Cost", button.transform, 44, TextAnchor.UpperLeft, new Vector2(0.08f, 0.78f), new Vector2(0.5f, 0.97f))
                .text = card.manaCost.ToString();
            CreateText("Name", button.transform, 26, TextAnchor.MiddleCenter, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.78f))
                .text = card.cardName;
            CreateText("Description", button.transform, 22, TextAnchor.MiddleCenter, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.56f))
                .text = card.Description;
            return button;
        }

        void CreateOverlayButton(string label, UnityAction onClick)
        {
            var button = CreateButton(label, overlayButtons, label, ButtonColor, onClick);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(240, 90);
        }

        static Color CardColor(CardData card)
        {
            if (card.damage > 0 && card.block > 0) return MixedCardColor;
            return card.damage > 0 ? AttackCardColor : SkillCardColor;
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            // New Unity 6 projects use the Input System package by default.
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        static void ClearChildren(Transform parent)
        {
            // Detach before destroying so the layout group ignores them right away.
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                child.SetParent(null, false);
                Destroy(child.gameObject);
            }
        }

        static void AddRowLayout(RectTransform rect, float spacing)
        {
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchors(rect, anchorMin, anchorMax);
            return rect;
        }

        static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static Image CreateImage(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var image = CreateRect(name, parent, anchorMin, anchorMax).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        Text CreateText(string name, Transform parent, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax)
        {
            var text = CreateRect(name, parent, anchorMin, anchorMax).gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        Button CreateButton(string name, Transform parent, string label, Color color, UnityAction onClick)
        {
            var image = CreateImage(name, parent, color, Vector2.zero, Vector2.one);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            if (label != null)
            {
                var text = CreateText("Label", image.transform, 34, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
                text.text = label;
                text.color = Color.black;
            }
            return button;
        }
    }
}
