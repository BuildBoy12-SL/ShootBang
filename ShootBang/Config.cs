// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace ShootBang
{
    using Exiled.API.Interfaces;

    /// <inheritdoc />
    public class Config : IConfig
    {
        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the fuse duration for shot grenades. Does not apply to shot Scp018 instances.
        /// </summary>
        public float FuseDuration { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets the amount to multiply inflicted damage of shot grenades by.
        /// </summary>
        public float DamageMultiplier { get; set; } = 0.25f;
    }
}