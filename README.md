# Mods Plus

## Cleaner card creation with `SimpleCard`
Make a card with fewer lines by extending `SimpleCard` instead of `CustomCard`!

```cs
using UnityEngine;
using ModsPlus;

public class ExampleCard : SimpleCard
{
	public override CardDetails Details => new CardDetails
	{
		Title       = "Example Card",
		Description = "Your first effect card",
		ModName     = "<Your Mod ID>",
		Art         = Assets.YourCoolArt,
		Rarity      = CardInfo.Rarity.Common,
		Theme       = CardThemeColor.CardThemeColorType.TechWhite
	};

	public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
	{
		statModifiers.health = 0.5f;
		gun.damage = 2f;
	}

	protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
	{
		Debug.Log("Card added to the player!");
	}

	protected override void Removed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
	{
		Debug.Log("Card removed from the player!");
	}
}
```

## Streamlined (and safe) `CardEffect` wrapper
Do you find yourself writing the same boilerplate over and over when defining cards that attach a `MonoBehaviour` to the player?
Are you tired of writing the same code for registering and deregistering actions (such as `Block::BlockAction` or `Gun::ShootProjectileAction`) repeatedly?
If so, then this is the library for you.

### Creating an on block/attack effect
All actions will be automatically registered and deregistered when the card is added or removed, so you can focus on your cool features!

```cs
using UnityEngine;
using ModsPlus;

public class ExampleCard : CustomEffectCard<ExampleEffect>
{
	public override CardDetails Details => new CardDetails
	{
		Title       = "Example Card",
		Description = "Your first effect card",
		ModName     = "<Your Mod ID>",
		Art         = Assets.YourCoolArt,
		Rarity      = CardInfo.Rarity.Common,
		Theme       = CardThemeColor.CardThemeColorType.TechWhite
	};
}

public class ExampleEffect : CardEffect
{
	public override void OnBlock(BlockTrigger.BlockTriggerType trigger)
	{
		Debug.Log("[ExampleEffect] Player blocked!");
	}

	public override void OnShoot(GameObject projectile)
	{
		Debug.Log("[ExampleEffect] Player fired a shot!");
	}
}
``` 

Once defined you can register this card as you normally would:
```cs
CustomCard.BuildCard<ExampleCard>();
```

*For additional overrides check your IDE's auto-complete, or view the source code*

## PlayerSelector tools
This interface allows you to easily create selectors on players and react when the user clicks one (think Cards+ "Adware" or "Quickhack" abilities for reference)

### Creating selectors and responding to a selection event
```cs
/* make selectors on a specific player */
int playerID;	// ID of any specific player
var targets = PlayerManager.instance.players.Where(p => p.playerID == playerID);
PlayerSelector.InstantiateOn(Assets.SingleTargetSelector, targets, selectedPlayer =>
{
	Debug.Log($"Friendly with ID {selectedPlayer.playerID} selected!");
});

/* make selectors on friends */
PlayerSelector.InstantiateOnFriendlies(Assets.FriendlySelector, selectedPlayer =>
{
	Debug.Log($"Friendly with ID {selectedPlayer.playerID} selected!");
});

/* make selectors on enemies */
PlayerSelector.InstantiateOnEnemies(Assets.EnemySelector, selectedPlayer =>
{
	Debug.Log($"Enemy with ID {selectedPlayer.playerID} selected!");
});
```

*The `GameObject` provided to the utility will be instantiated as a child of each target. It can contain any effects (particles, etc.) you wish, but must have some type of `Collider3D` on it for clicks to be detected*

## Custom HP Bar
This component provides a wrapper around the base game's `HealthBar` class, giving you full control of the values to display and when to update them.

```cs
GameObject target; // any GameObject you wish to add a health bar to
float startingHp = 100;
float startingMaxHp = 100;

// create the health bar
var healthBar = target.AddComponent<CustomHealthBar>();

// set initial values
healthBar.SetValues(startingHp, startingMaxHp);

// take 10 damage
healthBar.CurrentHealth -= 10;
```

# Patch Notes

### 1.3.0
- Added Game Mode hooks

### 1.2.0
- Added `SimpleCard`

### 0.0.1
- Initial release