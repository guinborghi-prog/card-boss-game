using System.Collections.Generic;
using UnityEngine;

namespace CardBoss
{
    // Built-in cards and bosses, used when the BattleManager's lists are left empty.
    // Once you make your own Card and Boss assets, drag them into the BattleManager instead.
    public static class DefaultContent
    {
        public static List<CardData> StarterDeck()
        {
            var strike = Card("Strike", cost: 1, damage: 6);
            var defend = Card("Defend", cost: 1, block: 5);
            var heavyBlow = Card("Heavy Blow", cost: 2, damage: 12);

            var deck = new List<CardData>();
            for (int i = 0; i < 5; i++) deck.Add(strike);
            for (int i = 0; i < 4; i++) deck.Add(defend);
            deck.Add(heavyBlow);
            return deck;
        }

        public static List<CardData> RewardPool()
        {
            return new List<CardData>
            {
                Card("Quick Slash", cost: 0, damage: 4),
                Card("Iron Wave", cost: 1, damage: 5, block: 5),
                Card("Shield Wall", cost: 2, block: 13),
                Card("Cleave", cost: 2, damage: 15),
                Card("Insight", cost: 1, draw: 2),
                Card("Riposte", cost: 1, damage: 4, draw: 1),
                Card("Fortress", cost: 1, block: 8),
                Card("Execute", cost: 3, damage: 24),
            };
        }

        public static List<BossData> Bosses()
        {
            return new List<BossData>
            {
                Boss("Slime King", 40,
                    Action("Slam", damage: 6),
                    Action("Slam", damage: 6),
                    Action("Ooze Shield", damage: 0, block: 8),
                    Action("Belly Flop", damage: 10)),
                Boss("Iron Golem", 60,
                    Action("Punch", damage: 8),
                    Action("Punch", damage: 8),
                    Action("Fortify", damage: 0, block: 15),
                    Action("Crush", damage: 14)),
                Boss("Shadow Duelist", 70,
                    Action("Flurry", damage: 3, hits: 4),
                    Action("Stab", damage: 11),
                    Action("Parry", damage: 6, block: 8),
                    Action("Blade Dance", damage: 4, hits: 3)),
            };
        }

        static CardData Card(string name, int cost, int damage = 0, int block = 0, int draw = 0)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = name;
            card.cardName = name;
            card.manaCost = cost;
            card.damage = damage;
            card.block = block;
            card.draw = draw;
            return card;
        }

        static BossAction Action(string name, int damage, int hits = 1, int block = 0)
        {
            return new BossAction { actionName = name, damage = damage, hits = hits, block = block };
        }

        static BossData Boss(string name, int maxHp, params BossAction[] deck)
        {
            var boss = ScriptableObject.CreateInstance<BossData>();
            boss.name = name;
            boss.bossName = name;
            boss.maxHp = maxHp;
            boss.deck = new List<BossAction>(deck);
            return boss;
        }
    }
}
