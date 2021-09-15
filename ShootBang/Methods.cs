// -----------------------------------------------------------------------
// <copyright file="Methods.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace ShootBang
{
    using System.Collections.Generic;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs;
    using Footprinting;
    using InventorySystem.Items;
    using InventorySystem.Items.Pickups;
    using InventorySystem.Items.ThrowableProjectiles;
    using Mirror;
    using UnityEngine;

    /// <summary>
    /// Handles miscellaneous methods and tracking required by the <see cref="EventHandlers"/>.
    /// </summary>
    public class Methods
    {
        private static readonly int PickupMask = LayerMask.GetMask("Pickup");
        private static readonly int GrenadeMask = LayerMask.GetMask("Grenade");
        private readonly Plugin plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="Methods"/> class.
        /// </summary>
        /// <param name="plugin">An instance of the <see cref="Plugin"/> class.</param>
        public Methods(Plugin plugin) => this.plugin = plugin;

        /// <summary>
        /// Gets a collection of all grenades to modify the explosion of.
        /// </summary>
        public static List<ThrownProjectile> TrackedGrenades { get; } = new List<ThrownProjectile>();

        /// <summary>
        /// Checks a players shot to see if a grenade item was hit.
        /// </summary>
        /// <param name="ev">The produced <see cref="ShootingEventArgs"/>.</param>
        /// <returns>A value indicating whether a pickup was detected with a linecast.</returns>
        public bool CheckPickup(ShootingEventArgs ev)
        {
            if (!Physics.Linecast(ev.Shooter.CameraTransform.transform.position, ev.ShotPosition, out RaycastHit raycastHit, PickupMask))
                return false;

            var item = raycastHit.transform.GetComponent<ItemPickupBase>();
            if (item == null)
                return true;

            Pickup pickup = Pickup.Get(item);
            if (pickup.Type.IsThrowable())
            {
                NetworkServer.Destroy(pickup.Base.gameObject);
                SpawnGrenade(ev.Shooter, ev.ShotPosition, pickup.Type);
            }

            return true;
        }

        /// <summary>
        /// Checks a players shot to see if a thrown grenade was hit.
        /// </summary>
        /// <param name="ev">The produced <see cref="ShootingEventArgs"/>.</param>
        public void CheckAir(ShootingEventArgs ev)
        {
            if (!Physics.Linecast(ev.Shooter.CameraTransform.transform.position, ev.ShotPosition, out RaycastHit raycastHit, GrenadeMask))
                return;

            ExplosionGrenade explosionGrenade = raycastHit.transform.GetComponent<ExplosionGrenade>();
            if (explosionGrenade != null && !explosionGrenade.DamageType.Equals(DamageTypes.Scp018))
            {
                NetworkServer.Destroy(explosionGrenade.gameObject);
                SpawnGrenade(ev.Shooter, ev.ShotPosition, ItemType.GrenadeHE);
                return;
            }

            var flash = raycastHit.transform.GetComponent<FlashbangGrenade>();
            if (flash != null)
            {
                NetworkServer.Destroy(flash.gameObject);
                SpawnGrenade(ev.Shooter, ev.ShotPosition, ItemType.GrenadeFlash);
            }
        }

        // https://github.com/Exiled-Team/EXILED/blob/dev/Exiled.CustomItems/API/Features/CustomGrenade.cs#L84 yoink
        private void SpawnGrenade(Player player, Vector3 position, ItemType itemType)
        {
            Throwable throwable = new Throwable(itemType, player);
            throwable.Throw();

            ThrownProjectile thrownProjectile = Object.Instantiate(throwable.Base.Projectile, position, throwable.Owner.CameraTransform.rotation);
            Transform transform = thrownProjectile.transform;
            PickupSyncInfo newInfo = new PickupSyncInfo()
            {
                ItemId = throwable.Type,
                Locked = !throwable.Base._repickupable,
                Serial = ItemSerialGenerator.GenerateNext(),
                Position = transform.position,
                Rotation = new LowPrecisionQuaternion(transform.rotation),
            };

            if (thrownProjectile is TimeGrenade time)
                time._fuseTime = plugin.Config.FuseDuration;

            thrownProjectile.NetworkInfo = newInfo;
            thrownProjectile.PreviousOwner = new Footprint(throwable.Owner.ReferenceHub);
            NetworkServer.Spawn(thrownProjectile.gameObject);
            thrownProjectile.InfoReceived(default, newInfo);
            if (thrownProjectile.TryGetComponent(out Rigidbody component))
                throwable.Base.PropelBody(component, throwable.Base.FullThrowSettings.StartTorque, throwable.Base.FullThrowSettings.StartVelocity, throwable.Base.FullThrowSettings.UpwardsFactor);
            thrownProjectile.ServerActivate();
            TrackedGrenades.Add(thrownProjectile);
        }
    }
}