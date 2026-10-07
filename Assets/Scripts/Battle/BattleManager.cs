using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardBoss
{
    public enum BattlePhase { PlayerTurn, Reward, Victory, Defeat }

    // Runs the game: a series of boss fights, with a card reward after each win.
    // The UI only reads state from here and calls PlayCard / EndTurn / ChooseReward / Restart.
    public class BattleManager : MonoBehaviour
    {
        const int MaxHandSize = 8;
        const int MaxLogLines = 6;

        [Header("Player")]
        [Min(1)] public int playerMaxHp = 60;
        [Min(1)] public int maxMana = 3;
        [Min(1)] public int handSize = 5;
        [Tooltip("HP restored after each boss is defeated.")]
        [Min(0)] public int healAfterBoss = 15;
        [Min(1)] public int rewardChoiceCount = 3;

        [Header("Content (leave empty to use the built-in defaults)")]
        public List<CardData> starterDeck = new List<CardData>();
        public List<CardData> rewardPool = new List<CardData>();
        public List<BossData> bosses = new List<BossData>();

        // Raised whenever anything the UI shows has changed.
        public event Action Changed;

        public BattlePhase Phase { get; private set; }
        public Combatant Player { get; private set; }
        public Combatant Boss { get; private set; }
        public BossAction BossIntent { get; private set; }
        public int Mana { get; private set; }
        public int MaxMana => maxMana;
        public int Turn { get; private set; }
        public int BossNumber => bossIndex + 1;
        public int BossCount => bosses.Count;
        public int DrawPileCount => drawDeck.DrawCount;
        public int DiscardPileCount => drawDeck.DiscardCount;
        public IReadOnlyList<CardData> Hand => hand;
        public IReadOnlyList<CardData> Rewards => rewards;
        public IReadOnlyList<string> Log => log;

        // Every card the player owns this run. Each battle starts with a fresh shuffle of it.
        readonly List<CardData> runDeck = new List<CardData>();
        readonly List<CardData> hand = new List<CardData>();
        readonly List<CardData> rewards = new List<CardData>();
        readonly List<string> log = new List<string>();
        Deck<CardData> drawDeck;
        Deck<BossAction> bossDeck;
        int bossIndex;

        void Awake()
        {
            starterDeck.RemoveAll(card => card == null);
            rewardPool.RemoveAll(card => card == null);
            bosses.RemoveAll(boss => boss == null);

            if (starterDeck.Count == 0) starterDeck = DefaultContent.StarterDeck();
            if (rewardPool.Count == 0) rewardPool = DefaultContent.RewardPool();
            if (bosses.Count == 0) bosses = DefaultContent.Bosses();
        }

        void Start() => Restart();

        public void Restart()
        {
            Player = new Combatant("You", playerMaxHp);
            runDeck.Clear();
            runDeck.AddRange(starterDeck);
            log.Clear();
            StartBattle(0);
        }

        public bool CanPlay(CardData card) => Phase == BattlePhase.PlayerTurn && card.manaCost <= Mana;

        public void PlayCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= hand.Count) return;
            var card = hand[handIndex];
            if (!CanPlay(card)) return;

            Mana -= card.manaCost;
            hand.RemoveAt(handIndex);
            drawDeck.Discard(card);

            var effects = new List<string>();
            if (card.damage > 0)
            {
                int dealt = Boss.TakeDamage(card.damage);
                effects.Add($"{dealt} damage");
            }
            if (card.block > 0)
            {
                Player.GainBlock(card.block);
                effects.Add($"+{card.block} block");
            }
            if (card.draw > 0)
            {
                DrawCards(card.draw);
                effects.Add($"drew {card.draw}");
            }
            AddLog($"You play {card.cardName}: {string.Join(", ", effects)}.");

            if (Boss.IsDead) WinBattle();
            Changed?.Invoke();
        }

        public void EndTurn()
        {
            if (Phase != BattlePhase.PlayerTurn) return;

            foreach (var card in hand) drawDeck.Discard(card);
            hand.Clear();

            BossActs();
            if (Player.IsDead)
            {
                Phase = BattlePhase.Defeat;
                AddLog($"You were defeated by {Boss.Name}.");
            }
            else
            {
                StartPlayerTurn();
            }
            Changed?.Invoke();
        }

        // index -1 skips the reward.
        public void ChooseReward(int index)
        {
            if (Phase != BattlePhase.Reward) return;
            if (index >= 0 && index < rewards.Count)
            {
                runDeck.Add(rewards[index]);
                AddLog($"Added {rewards[index].cardName} to your deck.");
            }
            StartBattle(bossIndex + 1);
        }

        void StartBattle(int index)
        {
            bossIndex = index;
            var data = bosses[index];
            Boss = new Combatant(data.bossName, data.maxHp);
            var actions = data.deck.Count > 0 ? data.deck : new List<BossAction> { new BossAction() };
            bossDeck = new Deck<BossAction>(actions);
            drawDeck = new Deck<CardData>(runDeck);
            hand.Clear();
            Turn = 0;
            AddLog($"{data.bossName} appears!");
            StartPlayerTurn();
            Changed?.Invoke();
        }

        void StartPlayerTurn()
        {
            Phase = BattlePhase.PlayerTurn;
            Turn++;
            Player.Block = 0;
            Mana = maxMana;
            bossDeck.TryDraw(out var intent);
            BossIntent = intent;
            DrawCards(handSize);
        }

        void BossActs()
        {
            var action = BossIntent;
            // Like the player, the boss's block only lasts until its next turn.
            Boss.Block = 0;

            var effects = new List<string>();
            if (action.damage > 0)
            {
                int total = 0;
                for (int i = 0; i < action.hits && !Player.IsDead; i++)
                    total += Player.TakeDamage(action.damage);
                effects.Add($"you take {total} damage");
            }
            if (action.block > 0)
            {
                Boss.GainBlock(action.block);
                effects.Add($"it gains {action.block} block");
            }
            string result = effects.Count > 0 ? string.Join(", ", effects) : "nothing happens";
            AddLog($"{Boss.Name} uses {action.actionName}: {result}.");

            bossDeck.Discard(action);
        }

        void WinBattle()
        {
            AddLog($"{Boss.Name} is defeated!");
            if (bossIndex >= bosses.Count - 1)
            {
                Phase = BattlePhase.Victory;
                return;
            }

            Player.Heal(healAfterBoss);
            PickRewards();
            Phase = BattlePhase.Reward;
        }

        void PickRewards()
        {
            rewards.Clear();
            var pool = new List<CardData>(rewardPool);
            while (rewards.Count < rewardChoiceCount && pool.Count > 0)
            {
                int i = UnityEngine.Random.Range(0, pool.Count);
                rewards.Add(pool[i]);
                pool.RemoveAt(i);
            }
        }

        void DrawCards(int count)
        {
            for (int i = 0; i < count && hand.Count < MaxHandSize; i++)
            {
                if (!drawDeck.TryDraw(out var card)) break;
                hand.Add(card);
            }
        }

        void AddLog(string message)
        {
            log.Add(message);
            if (log.Count > MaxLogLines) log.RemoveAt(0);
        }
    }
}
