// -----------------------------------------------------------------------
// <copyright file="EventHandlers.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace ShootBang
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using InventorySystem.Items.ThrowableProjectiles;

    /// <summary>
    /// Handles events derived from <see cref="Exiled.Events.Handlers"/>.
    /// </summary>
    public class EventHandlers
    {
        private readonly Plugin plugin;
        private readonly List<Player> targets = new List<Player>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlers"/> class.
        /// </summary>
        /// <param name="plugin">An instance of the <see cref="Plugin"/> class.</param>
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        /// <inheritdoc cref="Exiled.Events.Handlers.Map.OnExplodingGrenade(ExplodingGrenadeEventArgs)"/>
        public void OnExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (Methods.TrackedGrenades.Count == 0)
                return;

            ThrownProjectile projectile = Methods.TrackedGrenades.FirstOrDefault(thrownProjectile => thrownProjectile == ev.Grenade);
            if (projectile == null)
                return;

            targets.AddRange(ev.TargetsToAffect);
            Methods.TrackedGrenades.Remove(projectile);
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.OnHurting(HurtingEventArgs)"/>
        public void OnHurting(HurtingEventArgs ev)
        {
            if (targets.Contains(ev.Target))
            {
                ev.Amount *= plugin.Config.DamageMultiplier;
                targets.Remove(ev.Target);
            }
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Player.OnShooting(ShootingEventArgs)"/>
        public void OnShooting(ShootingEventArgs ev)
        {
            if (!plugin.Methods.CheckPickup(ev))
                plugin.Methods.CheckAir(ev);
        }

        /// <inheritdoc cref="Exiled.Events.Handlers.Server.OnWaitingForPlayers"/>
        public void OnWaitingForPlayers()
        {
            Methods.TrackedGrenades.Clear();
        }
    }
}