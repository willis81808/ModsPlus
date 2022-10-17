using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace ModsPlus
{
    public abstract class CustomEffectCard<T> : CustomCard where T : CardEffect
    {
        public abstract CardDetails Details { get; }

        public class CardDetails
        {
            public string Title { get; set; } = typeof(T).Name;
            public string Description { get; set; } = $"{typeof(T).Name} description";
            public string ModName { get; set; } = "Modded";
            public bool OwnerOnly { get; set; } = false;
            public CardInfo.Rarity Rarity { get; set; } = CardInfo.Rarity.Common;
            public CardThemeColor.CardThemeColorType Theme { get; set; } = CardThemeColor.CardThemeColorType.TechWhite;
            public CardInfoStat[] Stats { get; set; }
            public GameObject Art { get; set; }
        }

        public sealed override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (Details.OwnerOnly && !player.data.view.IsMine) return;

            var effect = player.gameObject.GetComponent<T>();

            if (effect != null)
            {
                effect.OnUpgradeCardInternal();
            }
            else
            {
                player.gameObject.AddComponent<T>().Initialize(player, gun, gunAmmo, data, health, gravity, block, characterStats);
            }

            Added(player, gun, gunAmmo, data, health, gravity, block, characterStats);
        }

        public sealed override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (Details.OwnerOnly && !player.data.view.IsMine) return;

            var effect = player.gameObject.GetComponent<T>();

            if (effect != null)
            {
                Destroy(effect);
            }

            Removed(player, gun, gunAmmo, data, health, gravity, block, statModifiers);
        }

        public sealed override void OnRemoveCard()
        {
            base.OnRemoveCard();
        }

        protected sealed override string GetTitle() => Details.Title;
        protected sealed override string GetDescription() => Details.Description;
        public sealed override string GetModName() => Details.ModName;
        protected sealed override CardInfo.Rarity GetRarity() => Details.Rarity;
        protected sealed override CardThemeColor.CardThemeColorType GetTheme() => Details.Theme;
        protected sealed override CardInfoStat[] GetStats() => Details.Stats;
        protected sealed override GameObject GetCardArt() => Details.Art;

        protected virtual void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats) { }
        protected virtual void Removed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats) { }
    }

    public abstract class CardEffect : MonoBehaviour
    {
        protected Player player { get; private set; }
        protected Gun gun { get; private set; }
        protected GunAmmo gunAmmo { get; private set; }
        protected CharacterData data { get; private set; }
        protected HealthHandler health { get; private set; }
        protected Gravity gravity { get; private set; }
        protected Block block { get; private set; }
        protected CharacterStatModifiers characterStats { get; private set; }

        public virtual void Initialize(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            this.player = player;
            this.gun = gun;
            this.gunAmmo = gunAmmo;
            this.data = data;
            this.health = health;
            this.gravity = gravity;
            this.block = block;
            this.characterStats = characterStats;
        }

        protected virtual void Start()
        {
            gun.ShootPojectileAction += OnShootInternal;
            data.TouchGroundAction += OnTouchGroundInternal;
            data.TouchWallAction += OnTouchWallInternal;
            health.reviveAction += OnReviveInternal;
            health.delayedReviveAction += OnDelayedReviveInternal;
            block.BlockAction += OnBlockInternal;
            block.BlockActionEarly += OnBlockEarlyInternal;
            block.BlockProjectileAction += OnBlockProjectileInternal;
            block.BlockRechargeAction += OnBlockRechargeInternal;
            characterStats.DealtDamageAction += OnDealtDamageInternal;
            characterStats.WasDealtDamageAction += OnTakeDamageInternal;
            characterStats.OnReloadDoneAction += OnReloadDoneInternal;
            characterStats.OutOfAmmpAction += OnOutOfAmmoInternal;
        }

        protected virtual void OnDestroy()
        {
            gun.ShootPojectileAction -= OnShootInternal;
            data.TouchGroundAction -= OnTouchGroundInternal;
            data.TouchWallAction -= OnTouchWallInternal;
            health.reviveAction -= OnReviveInternal;
            health.delayedReviveAction -= OnDelayedReviveInternal;
            block.BlockAction -= OnBlockInternal;
            block.BlockActionEarly -= OnBlockEarlyInternal;
            block.BlockProjectileAction -= OnBlockProjectileInternal;
            block.BlockRechargeAction -= OnBlockRechargeInternal;
            characterStats.DealtDamageAction -= OnDealtDamageInternal;
            characterStats.WasDealtDamageAction -= OnTakeDamageInternal;
            characterStats.OnReloadDoneAction -= OnReloadDoneInternal;
            characterStats.OutOfAmmpAction -= OnOutOfAmmoInternal;

            StopAllCoroutines();
        }


        internal void OnUpgradeCardInternal()
        {
            OnUpgradeCard();
            StartCoroutine(OnUpgradeCardCoroutine());
        }
        internal void OnShootInternal(GameObject projectile)
        {
            OnShoot(projectile);
            StartCoroutine(OnShootCoroutine(projectile));
        }
        internal void OnTouchGroundInternal(float timeSinceGrounded, Vector3 position, Vector3 groundNormal, Transform groundTransform)
        {
            OnTouchGround(timeSinceGrounded, position, groundNormal, groundTransform);
            StartCoroutine(OnTouchGroundCoroutine(timeSinceGrounded, position, groundNormal, groundTransform));
        }
        internal void OnTouchWallInternal(float timeSinceLastGrab, Vector3 position, Vector3 wallNormal)
        {
            OnTouchWall(timeSinceLastGrab, position, wallNormal);
            StartCoroutine(OnTouchWallCoroutine(timeSinceLastGrab, position, wallNormal));
        }
        internal void OnReviveInternal()
        {
            OnRevive();
            StartCoroutine(OnReviveCoroutine());
        }
        internal void OnDelayedReviveInternal()
        {
            OnDelayedRevive();
            StartCoroutine(OnDelayedReviveCoroutine());
        }
        internal void OnBlockInternal(BlockTrigger.BlockTriggerType blockTriggerType)
        {
            OnBlock(blockTriggerType);
            StartCoroutine(OnBlockCoroutine(blockTriggerType));
        }
        internal void OnBlockEarlyInternal(BlockTrigger.BlockTriggerType blockTriggerType)
        {
            OnBlockEarly(blockTriggerType);
            StartCoroutine(OnBlockEarlyCoroutine(blockTriggerType));
        }
        internal void OnBlockProjectileInternal(GameObject projectile, Vector3 forward, Vector3 hitPosition)
        {
            OnBlockProjectile(projectile, forward, hitPosition);
            StartCoroutine(OnBlockProjectileCoroutine(projectile, forward, hitPosition));
        }
        internal void OnBlockRechargeInternal()
        {
            OnBlockRecharge();
            StartCoroutine(OnBlockRechargeCoroutine());
        }
        internal void OnDealtDamageInternal(Vector2 damage, bool selfDamage)
        {
            OnDealtDamage(damage, selfDamage);
            StartCoroutine(OnDealtDamageCoroutine(damage, selfDamage));
        }
        internal void OnTakeDamageInternal(Vector2 damage, bool selfDamage)
        {
            OnTakeDamage(damage, selfDamage);
            StartCoroutine(OnTakeDamageCoroutine(damage, selfDamage));
        }
        internal void OnReloadDoneInternal(int bulletsReloaded)
        {
            OnReloadDone(bulletsReloaded);
            StartCoroutine(OnReloadDoneCoroutine(bulletsReloaded));
        }
        internal void OnOutOfAmmoInternal(int bulletsReloaded)
        {
            OnOutOfAmmo(bulletsReloaded);
            StartCoroutine(OnOutOfAmmoCoroutine(bulletsReloaded));
        }


        public virtual void OnUpgradeCard() { }
        public virtual void OnShoot(GameObject projectile) { }
        public virtual void OnTouchGround(float timeSinceGrounded, Vector3 position, Vector3 groundNormal, Transform groundTransform) { }
        public virtual void OnTouchWall(float timeSinceLastGrab, Vector3 position, Vector3 wallNormal) { }
        public virtual void OnRevive() { }
        public virtual void OnDelayedRevive() { }
        public virtual void OnBlock(BlockTrigger.BlockTriggerType blockTriggerType) { }
        public virtual void OnBlockEarly(BlockTrigger.BlockTriggerType blockTriggerType) { }
        public virtual void OnBlockProjectile(GameObject projectile, Vector3 forward, Vector3 hitPosition) { }
        public virtual void OnBlockRecharge() { }
        public virtual void OnDealtDamage(Vector2 damage, bool selfDamage) { }
        public virtual void OnTakeDamage(Vector2 damage, bool selfDamage) { }
        public virtual void OnReloadDone(int bulletsReloaded) { }
        public virtual void OnOutOfAmmo(int bulletsReloaded) { }

        public virtual IEnumerator OnUpgradeCardCoroutine() { yield return null; }
        public virtual IEnumerator OnShootCoroutine(GameObject projectile) { yield return null; }
        public virtual IEnumerator OnTouchGroundCoroutine(float timeSinceGrounded, Vector3 position, Vector3 groundNormal, Transform groundTransform) { yield return null; }
        public virtual IEnumerator OnTouchWallCoroutine(float timeSinceLastGrab, Vector3 position, Vector3 wallNormal) { yield return null; }
        public virtual IEnumerator OnReviveCoroutine() { yield return null; }
        public virtual IEnumerator OnDelayedReviveCoroutine() { yield return null; }
        public virtual IEnumerator OnBlockCoroutine(BlockTrigger.BlockTriggerType blockTriggerType) { yield return null; }
        public virtual IEnumerator OnBlockEarlyCoroutine(BlockTrigger.BlockTriggerType blockTriggerType) { yield return null; }
        public virtual IEnumerator OnBlockProjectileCoroutine(GameObject projectile, Vector3 forward, Vector3 hitPosition) { yield return null; }
        public virtual IEnumerator OnBlockRechargeCoroutine() { yield return null; }
        public virtual IEnumerator OnDealtDamageCoroutine(Vector3 damage, bool selfDamage) { yield return null; }
        public virtual IEnumerator OnTakeDamageCoroutine(Vector3 damage, bool selfDamage) { yield return null; }
        public virtual IEnumerator OnReloadDoneCoroutine(int bulletsReloaded) { yield return null; }
        public virtual IEnumerator OnOutOfAmmoCoroutine(int bulletsReloaded) { yield return null; }
    }
}
