using UnityEngine;

namespace CardBoss
{
    // HP and block for either the player or a boss.
    public class Combatant
    {
        public string Name { get; }
        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Block { get; set; }

        public bool IsDead => Hp <= 0;

        public Combatant(string name, int maxHp)
        {
            Name = name;
            MaxHp = maxHp;
            Hp = maxHp;
        }

        // Block absorbs damage first. Returns the damage that got through to HP.
        public int TakeDamage(int amount)
        {
            int absorbed = Mathf.Min(Block, amount);
            Block -= absorbed;
            int dealt = amount - absorbed;
            Hp = Mathf.Max(0, Hp - dealt);
            return dealt;
        }

        public void GainBlock(int amount) => Block += amount;

        public void Heal(int amount) => Hp = Mathf.Min(MaxHp, Hp + amount);
    }
}
