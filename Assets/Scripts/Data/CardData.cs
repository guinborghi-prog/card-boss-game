using System.Collections.Generic;
using UnityEngine;

namespace CardBoss
{
    // One kind of player card. Create new ones with Assets > Create > Card Boss > Card.
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card Boss/Card")]
    public class CardData : ScriptableObject
    {
        public string cardName = "New Card";
        [Min(0)] public int manaCost = 1;
        [Min(0)] public int damage;
        [Min(0)] public int block;
        [Tooltip("Extra cards drawn when this card is played.")]
        [Min(0)] public int draw;

        public string Description
        {
            get
            {
                var lines = new List<string>();
                if (damage > 0) lines.Add($"Deal {damage} damage.");
                if (block > 0) lines.Add($"Gain {block} block.");
                if (draw > 0) lines.Add(draw == 1 ? "Draw 1 card." : $"Draw {draw} cards.");
                return string.Join("\n", lines);
            }
        }
    }
}
