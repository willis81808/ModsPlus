using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using UnboundLib;
using System.Reflection.Emit;
using System.Reflection;

namespace ModsPlus
{
    public class CustomHealthBar : MonoBehaviour
    {
        private HealthBar healthBar;

        /// <summary>
        /// Current health value, clamped to the range <c>[0, <see cref="MaxHealth"/>]</c>
        /// </summary>
        public float CurrentHealth { get => _currentHealth; set => SetCurrentHealth(value); }
        private float _currentHealth = 100;

        /// <summary>
        /// Max health value, clamped to the range <c>[0, <see cref="float.PositiveInfinity"/>]</c>
        /// </summary>
        public float MaxHealth { get => _maxHealth; set => SetMaxHealth(value); }
        private float _maxHealth = 100;

        /// <summary>
        /// Override the values of <c>CurrentHealth</c> and <c>MaxHealth</c>
        /// </summary>
        /// <param name="currentHealth"></param>
        /// <param name="maxHealth"></param>
        public void SetValues(float currentHealth, float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        private void Awake()
        {
            healthBar = Instantiate(Assets.BaseHealthBar.gameObject, transform).GetComponent<HealthBar>();
        }

        private void Start()
        {
            healthBar.transform.Find("Canvas/PlayerName").gameObject.SetActive(false);
        }

        private void SetCurrentHealth(float value)
        {
            _currentHealth = Math.Max(0, Math.Min(MaxHealth, value));
            UpdateHealthBar();
        }

        private void SetMaxHealth(float value)
        {
            _maxHealth = Math.Max(0, value);
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            healthBar.TakeDamage(Vector2.zero, false);
        }

        /// <summary>
        /// Returns the underlying <c>HealthBar</c> managed by this instance
        /// </summary>
        public HealthBar GetBaseHealthBar()
        {
            return healthBar;
        }
    }
}
