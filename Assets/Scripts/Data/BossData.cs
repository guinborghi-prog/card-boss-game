using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardBoss
{
    // One card in a boss's deck. The boss draws one each turn and shows it as its next move.
    [Serializable]
    public class BossAction
    {
        public string actionName = "Attack";
        [Min(0)] public int damage = 5;
        [Min(1)] public int hits = 1;
        [Min(0)] public int block;

        public string Describe()
        {
            var parts = new List<string>();
            if (damage > 0) parts.Add(hits > 1 ? $"attack {damage} x{hits}" : $"attack {damage}");
            if (block > 0) parts.Add($"block {block}");
            string effect = parts.Count > 0 ? string.Join(" and ", parts) : "do nothing";
            return $"{actionName} ({effect})";
        }
    }

    // A boss and its deck of actions. Create new ones with Assets > Create > Card Boss > Boss.
    [CreateAssetMenu(fileName = "NewBoss", menuName = "Card Boss/Boss")]
    public class BossData : ScriptableObject
    {
        public string bossName = "New Boss";
        [Min(1)] public int maxHp = 40;
        public List<BossAction> deck = new List<BossAction>();
    }
}
