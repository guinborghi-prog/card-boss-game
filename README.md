# Card Boss Game

A small deck-building card game for Unity 6. You fight a series of bosses, and each boss has its own deck of moves.
Every turn you get mana and a hand of cards, and you spend the mana to attack or to block the boss's next move.

## How to play

- Each turn you have **3 mana** and draw **5 cards**. Click a card to play it, then click **End Turn**.
- Red cards deal damage, blue cards give block, and purple cards do both.
- **Block** absorbs damage. It wears off at the start of your next turn, so use it on the turn the boss attacks.
- The boss panel shows the boss's **next move** ahead of time. Bosses draw their moves from their own deck.
- After you beat a boss you heal 15 HP and **choose a new card** to add to your deck.
- Beat all three bosses to win: Slime King, Iron Golem and Shadow Duelist.

## Setup

1. In Unity Hub, create a new project with the **Universal 2D** template on Unity 6.
2. Copy this repo's `Assets/Scripts` folder into your project's `Assets` folder.
3. Open your scene, for example `Assets/Scenes/SampleScene`.
4. In the **Hierarchy**, right-click and choose **Create Empty**, then name the new object `Game`.
5. With `Game` selected, click **Add Component** in the **Inspector** and add **Battle UI**. This adds **Battle Manager** automatically.
6. Press **Play**.

The whole screen is built from code, so the steps above are all the scene needs.

## Making your own cards and bosses

- **Card:** in the Project window, right-click and choose **Create > Card Boss > Card**. Set its name, mana cost, damage, block and draw.
- **Boss:** choose **Create > Card Boss > Boss**. Set its HP and add moves to its **Deck** list. Each move can attack, hit several times, block, or do a mix of these.
- To use your assets, select `Game` and drag them into the **Starter Deck**, **Reward Pool** and **Bosses** lists on Battle Manager.
- Any list you leave empty uses the built-in defaults from `DefaultContent.cs`.
- Battle Manager also lets you tune player HP, mana, hand size and healing between bosses.

## Code layout

| File | What it does |
| --- | --- |
| `Data/CardData.cs` | A player card. |
| `Data/BossData.cs` | A boss and its deck of moves (`BossAction`). |
| `Data/DefaultContent.cs` | The built-in starter deck, reward cards and bosses. |
| `Battle/Combatant.cs` | HP and block, used by both the player and the boss. |
| `Battle/Deck.cs` | Draw and discard piles, with reshuffling. |
| `Battle/BattleManager.cs` | The game rules: turns, mana, playing cards, boss moves, rewards, winning and losing. |
| `UI/BattleUI.cs` | Builds the screen and updates it when the game state changes. |

## Ideas for next steps

- Card art, plus a hover effect and animation when cards are played.
- Status effects such as poison, weakness and strength.
- Boss phases, where a boss switches to a harder deck below half HP.
- A map between fights, with shops and rest sites.
