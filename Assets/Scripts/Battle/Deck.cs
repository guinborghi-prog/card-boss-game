using System.Collections.Generic;

namespace CardBoss
{
    // A draw pile and a discard pile. Used for both the player's cards and the boss's actions.
    public class Deck<T>
    {
        readonly List<T> drawPile = new List<T>();
        readonly List<T> discardPile = new List<T>();

        public int DrawCount => drawPile.Count;
        public int DiscardCount => discardPile.Count;

        public Deck(IEnumerable<T> cards)
        {
            drawPile.AddRange(cards);
            Shuffle(drawPile);
        }

        // Draws the top card, shuffling the discard pile back in when the draw pile is empty.
        // Returns false only when both piles are empty.
        public bool TryDraw(out T card)
        {
            if (drawPile.Count == 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
            }

            if (drawPile.Count == 0)
            {
                card = default;
                return false;
            }

            int top = drawPile.Count - 1;
            card = drawPile[top];
            drawPile.RemoveAt(top);
            return true;
        }

        public void Discard(T card) => discardPile.Add(card);

        static void Shuffle(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
