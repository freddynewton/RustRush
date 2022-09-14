﻿// Copyright 2021, Infima Games. All Rights Reserved.

using Unity.Netcode;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Abstract movement class. Handles interactions with the main movement component.
    /// </summary>
    public abstract class MovementBehaviour : NetworkBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake(){}

        /// <summary>
        /// Start.
        /// </summary>
        protected virtual void Start(){}

        /// <summary>
        /// Update.
        /// </summary>
        protected virtual void Update(){}

        /// <summary>
        /// Fixed Update.
        /// </summary>
        protected virtual void FixedUpdate(){}

        /// <summary>
        /// Late Update.
        /// </summary>
        protected virtual void LateUpdate(){}

        #endregion
    }
}