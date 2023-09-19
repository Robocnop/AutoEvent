﻿using AutoEvent.Events.EventArgs;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System.Linq;
using AutoEvent.API.Enums;
using CustomPlayerEffects;
using PluginAPI.Core;
using UnityEngine;

namespace AutoEvent.Games.HideAndSeek
{
    public class EventHandler
    {
        private HideAndSeek.Plugin _plugin { get; set; }
        public EventHandler(HideAndSeek.Plugin ev)
        {
            _plugin = ev as HideAndSeek.Plugin;
        }    
    public void OnPlayerDamage(PlayerDamageArgs ev)
        {
            if (ev.DamageType == DeathTranslations.Falldown.Id)
            {
                ev.IsAllowed = false;
            }
            
            if (ev.Attacker != null)
            {
                bool isAttackerTagger = ev.Attacker.Items.FirstOrDefault(r => r.ItemTypeId == _plugin.Config.TaggerWeapon);
                bool isTargetTagger = ev.Target.Items.FirstOrDefault(r => r.ItemTypeId == _plugin.Config.TaggerWeapon);
                bool isAllowed = isAttackerTagger && !isTargetTagger;
                switch (ev.DamageType)
                {
                    case (byte)DamageType.GrenadeExplosion:
                        if (_plugin.Config.TaggerWeapon == ItemType.GrenadeHE)
                            ev.IsAllowed = false;
                        return; 
                    /*case DamageType.Jailbird:
                        if (_plugin.Config.TaggerWeapon == ItemType.Jailbird)
                            isAllowed = true;
                        break;*/
                    case (byte)DamageType.Scp018:
                        ev.IsAllowed = false;
                        if (_plugin.Config.TaggerWeapon == ItemType.SCP018)
                            isAllowed = true;
                        break;
                    default:
                        if (!(_plugin.Config.Range > 0 && Vector3.Distance(ev.Attacker.Position, ev.Target.Position) <=
                                _plugin.Config.Range))
                            isAllowed = false;
                        break;
                }

                if (isAllowed)
                {

                    ev.IsAllowed = false;
                    ev.Attacker.EffectsManager.EnableEffect<SpawnProtected>(_plugin.Config.NoTagBackDuration, false);
                    ev.Attacker.ClearInventory();
                    ev.Attacker.GiveLoadout(_plugin.Config.PlayerLoadouts, LoadoutFlags.IgnoreItems | LoadoutFlags.IgnoreWeapons | LoadoutFlags.IgnoreGodMode);

                    var weapon = ev.Target.AddItem(_plugin.Config.TaggerWeapon);
                    ev.Target.GiveLoadout(_plugin.Config.TaggerLoadouts, LoadoutFlags.IgnoreItems | LoadoutFlags.IgnoreWeapons | LoadoutFlags.IgnoreGodMode);
                    Timing.CallDelayed(0.1f, () =>
                    {
                        ev.Target.CurrentItem = weapon;
                    });
                }
            }
        }

    [PluginEvent(ServerEventType.GrenadeExploded)]
    public void OnGrenadeExplode(GrenadeExplodedEvent ev)
    {
        if (_plugin.Config.TaggerWeapon != ItemType.GrenadeHE)
            return;
        bool noRange = _plugin.Config.Range == 0;
        // Player.GetPlayers().Where(x => x.IsAlive && x.)

    }

    public void OnGrenadeThrown()
    {
        
    }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnJoin(PlayerJoinedEvent ev)
        {
            ev.Player.SetRole(RoleTypeId.Spectator);
        }

        public void OnTeamRespawn(TeamRespawnArgs ev) => ev.IsAllowed = false;
        public void OnSpawnRagdoll(SpawnRagdollArgs ev) => ev.IsAllowed = false;
        public void OnPlaceBullet(PlaceBulletArgs ev) => ev.IsAllowed = false;
        public void OnPlaceBlood(PlaceBloodArgs ev) => ev.IsAllowed = false;
        public void OnDropItem(DropItemArgs ev) => ev.IsAllowed = false;
        public void OnDropAmmo(DropAmmoArgs ev) => ev.IsAllowed = false;
    }
}
