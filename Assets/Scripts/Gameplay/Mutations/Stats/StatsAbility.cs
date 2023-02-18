﻿using System;
using Player;
using UnityEngine;

namespace Gameplay.Abilities.Base
{
    public class StatsAbility : BasicAbility
    {
        [SerializeField] private Manager playerManager;
        [SerializeField] private PlayerStats statsLvl1;
        [SerializeField] private PlayerStats statsLvl10;

        [SerializeField] private PlayerStats current = PlayerStats.Zero;

        public override void OnLevelChanged(int lvl)
        {
            base.OnLevelChanged(lvl);
            if(!current.Equals(PlayerStats.Zero) && Application.isPlaying) playerManager.AddStats(current.Negated());
            current = PlayerStats.LerpLevel(statsLvl1, statsLvl10, lvl);
            if(Application.isPlaying) playerManager.AddStats(current);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if(current.Equals(PlayerStats.Zero)) return;
            playerManager.AddStats(current);
        }
        
        protected override void OnDisable()
        {
            playerManager.AddStats(current.Negated());
            base.OnDisable();
        }
    }
}