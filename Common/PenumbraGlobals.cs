﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PenumbraMod.Content.Buffs;
using PenumbraMod.Content.DamageClasses;
using PenumbraMod.Content.Items;
using PenumbraMod.Content.Items.Consumables;
using PenumbraMod.Content.Items.ReaperCrystals;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace PenumbraMod.Common
{
    // This file contains ALL global changes from vanilla and stuff
    public class PenumbraGlobals
    {

    }
    public class PenumbraGlobalPlayer : ModPlayer
    {
        public bool sandhuntereff;
        public bool absolutecamera;
        public Vector2 absolutepos;
        Vector2 scrcache;
        public Item item = new(); // initialize a new item so that NullReferenceException doesn't occur when creating a new player / loading a player that doesn't have an item already saved

        // UI updates item from base to something

        public override void Load()
        {
            base.Load();
            if (item == new Item())
            {
                item.SetDefaults(ModContent.ItemType<AmythestCrystal>()); // example
            }
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);

            tag.Add("item", item);
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);

            if (tag.TryGet("item", out Item savedItem))
            {
                item = savedItem;
                var mo = Mod as PenumbraMod;
                mo.Logger.Info(item);
            }
        }
        public override void ResetEffects()
        {
            sandhuntereff = false;
        }
        public override void ModifyScreenPosition()
        {
            Vector2 scrctr = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            if (absolutecamera)
            {
                scrcache = Vector2.Lerp(scrcache, new Vector2(absolutepos.X, absolutepos.Y) - scrctr, 0.1f);
                Main.screenPosition = scrcache;
            }
            else
            {
                scrcache = Main.screenPosition;
            }
            base.ModifyScreenPosition();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (sandhuntereff)
                if (Main.LocalPlayer.HeldItem.DamageType == GetInstance<ReaperClass>())
                    target.AddBuff(BuffID.Venom, 120);
            if (Main.LocalPlayer.HasBuff(BuffType<BloodstainedForce>()))
            {
                Main.LocalPlayer.Heal(1);
                Main.LocalPlayer.HealEffect(1);
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (sandhuntereff)
                if (proj.DamageType.CountsAsClass(GetInstance<ReaperClass>()))
                    target.AddBuff(BuffID.Venom, 120);
        }
    }

    public class PenumbraGlobalNPC : GlobalNPC
    {
        public static int eyeStorm = -1;
    }
    public class PenumbraGlobalTooltips : GlobalItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GetInstance<PenumbraConfig>().VanillaChanges;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // Here we add a tooltip to the gel to let the player know what will happen
            if (item.type == ItemID.DeathSickle)
            {
                tooltips.Add(new(Mod, "", "[c/3b12d3:Special Ability:] Increases the swing speed of the scythe, shots faster extra projectiles, at cost of reaper energy not increases"));
            }
            if (item.type == ItemID.IceSickle)
            {
                tooltips.Add(new(Mod, "", "[c/00a4ff:Special Ability:] Increases the swing speed of the scythe, and shoots additional ice sickles"));
            }
            if (item.type == ItemID.BeamSword)
            {
                tooltips.Add(new(Mod, "", "Right Click to stand a shield in your front, at enemy hits, you will gain holy protection to dodge the next attack and gain 10+ defense"));
            }
        }
    }

    public class PenumbraGlobalItem : GlobalItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GetInstance<PenumbraConfig>().VanillaChanges;
        }
        public override bool InstancePerEntity => true;
        /// <summary>
        /// Useful to check if item hit NPC
        /// </summary>
        public bool ItemhitNPC;

        public override void SetDefaults(Item item)
        {
            ItemhitNPC = false;
            if (item.type == ItemID.DeathSickle)
            {
                item.DamageType = GetInstance<ReaperClass>();
                item.useAnimation = 22;
                item.useTime = 22;
                item.shootSpeed = 11f;
            }
            if (item.type == ItemID.BottledWater)
            {
                ItemID.Sets.ShimmerTransformToItem[ItemID.BottledWater] = ItemType<BottledShimmer>(); 
            }
            if (item.type == ItemID.BeamSword)
            {
                item.useAnimation = 21;
                item.useTime = 21;
                item.shootSpeed = 14f;
                item.shoot = ProjectileType<BeamSwordProj>();
                item.noUseGraphic = false;
                item.noMelee = false;
            }
            if (item.type == ItemID.IceSickle)
            {
                item.DamageType = GetInstance<ReaperClass>();
                item.shoot = ProjectileType<IceSickle>();
                item.shootSpeed = 14f;
            }
            if (item.type == ItemID.CursedFlames)
            {
                item.shoot = ProjectileType<ShadowFlameProj>();
                item.shootSpeed = 27f;
                item.useTime = 4;
                item.useAnimation = 16;
                item.reuseDelay = 8;
            }
            if (item.type == ItemID.Gladius)
            {
                item.shoot = ProjectileType<GladiusRework>();
                item.shootSpeed = 7f;
                item.useTime = 6;
                item.useAnimation = 14;
                item.reuseDelay = 10;
            }
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ItemID.DeathSickle)
            {
                if (player.HasBuff(BuffType<ReaperControl>()))
                {
                    player.AddBuff(BuffType<DeathSpeed>(), 360);
                }
            }
            return true;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (player.HasBuff(BuffID.ShadowDodge))
            {
                if (player.HeldItem?.type == ItemID.BeamSword)
                {
                    player.statDefense += 10;
                }
            }
        }
        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ItemhitNPC = true;
            target.AddBuff(BuffType<hitef>(), 10);
        }
        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            ItemhitNPC = true;
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ItemID.BeamSword)
            {
                return true;
            }
            return false;
        }
        public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
        {
            if (player.HasBuff(BuffType<DeathSpeed>()))
            {
                if (item.type == ItemID.DeathSickle)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.PurpleTorch);
                    }
                }
            }

            if (item.type == ItemID.IceSickle)
            {
                if (Main.rand.NextBool(8))
                {
                    Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.BlueTorch);
                }
            }
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ItemID.IceSickle)
            {
                item.shoot = ProjectileType<IceSickle>();
                if (player.HasBuff(BuffType<ReaperControl>()))
                {
                    item.shootSpeed = 18f;
                    const int NumProjectiles = 3;
                    for (int i = 0; i < NumProjectiles; i++)
                    {
                        Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(100));
                        // Decrease velocity randomly for nicer visuals.
                        newVelocity *= 1f - Main.rand.NextFloat(0.1f);
                        // Create a projectile.
                        Projectile.NewProjectileDirect(source, position, newVelocity, ProjectileType<IceSickle>(), damage, knockback, player.whoAmI);

                    }
                    return false;
                }
            }
            if (item.type == ItemID.Gladius)
            {
                item.shoot = ProjectileType<GladiusRework>();
            }
            if (item.type == ItemID.BeamSword)
            {
                if (player.altFunctionUse == 2)
                {
                    const int NumProjectiles = 1;
                    for (int i = 0; i < NumProjectiles; i++)
                    {
                        Projectile.NewProjectileDirect(source, position, player.DirectionTo(Main.MouseWorld) * 2f, ProjectileType<BeamSwordShield>(), 100, knockback, player.whoAmI);

                    }

                    item.noUseGraphic = true;
                    item.noMelee = true;
                    item.useStyle = ItemUseStyleID.Shoot;
                }

                else
                {
                    Projectile.NewProjectileDirect(source, position, player.DirectionTo(Main.MouseWorld) * 14f, ProjectileType<BeamSwordProj>(), 70, knockback, player.whoAmI);
                    item.noUseGraphic = false;
                    item.noMelee = false;
                    item.useStyle = ItemUseStyleID.Swing;
                }
                return false;
            }
            if (player.HasBuff(BuffType<DeathSpeed>()))
            {
                if (item.type == ItemID.DeathSickle)
                {
                    const int NumProjectiles = 1;
                    for (int i = 0; i < NumProjectiles; i++)
                    {
                        Projectile.NewProjectileDirect(source, position, player.DirectionTo(Main.MouseWorld) * 16f, ProjectileType<SwordBeam>(), 100, knockback, player.whoAmI);
                        Projectile.NewProjectileDirect(source, position, player.DirectionTo(Main.MouseWorld) * 12f, ProjectileID.DeathSickle, damage, knockback, player.whoAmI);

                    }
                    return false;

                }
            }
            return true;

        }
        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (item.type == ItemID.CursedFlames)
            {
                spriteBatch.Draw(Request<Texture2D>("PenumbraMod/Content/Items/CursedFlameInv").Value, position, null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
                return false;
            }
            return true;
        }
        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (item.type == ItemID.CursedFlames)
            {
                spriteBatch.Draw(Request<Texture2D>("PenumbraMod/Content/Items/CursedFlameInv").Value, item.position - Main.screenPosition, null, lightColor, rotation, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                return false;
            }
            return true;
        }
    }
    public class HitEffect : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GetInstance<PenumbraConfig>().HitEffect;
        }
        /// <summary>
        /// Useful to check if item hit NPC
        /// </summary>
        public bool ItemhitNPC;
        public override void SetDefaults(Item item)
        {
            ItemhitNPC = false;
        }
        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            OnHit2(target, player);
            ItemhitNPC = true;
        }
        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            ItemhitNPC = true;
        }
        public void OnHit2(NPC npc, Player player)
        {
            if (ItemhitNPC)
            {
                Vector2 velocity = GetInstance<PenumbraConfig>().HitEffectVelocity;
                int NumProjectiles = GetInstance<PenumbraConfig>().HitEffectCount;
                for (int i = 0; i < NumProjectiles; i++)
                {

                    Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(360));
                    // Decrease velocity randomly for nicer visuals.
                    newVelocity *= 4f - Main.rand.NextFloat(0.2f);
                    // Create a projectile.
                    Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, newVelocity, ProjectileType<HitEffectProj>(), 0, 0, player.whoAmI);

                }
            }
        }
    }
    public class UseTurn : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GetInstance<PenumbraConfig>().UseTurn;
        }
        // some weapons doenst look that good with useturn and some of the mod "whips" have some visual glitches with it, so, make them dont be affected with useturn config.
        public override void SetDefaults(Item item)
        {
            if (item.type != ItemType<ShockWave>() && item.type != ItemID.Gladius && item.type != ItemID.CopperShortsword && item.type != ItemID.GoldShortsword && item.type != ItemID.IronShortsword && item.type != ItemID.LeadShortsword
                && item.type != ItemID.PlatinumShortsword && item.type != ItemID.SilverShortsword && item.type != ItemID.TinShortsword && item.type != ItemID.TungstenShortsword && item.type != ItemType<CompositeSword>() &&
                item.type != ItemType<Kusarigama>() && item.type != ItemType<BloodReaper>() && item.type != ItemType<CorrosionCannon>())
            {
                item.useTurn = true;
            }

        }

    }
    public class AutoReuse : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GetInstance<PenumbraConfig>().Autouse;
        }
        public override void SetDefaults(Item item)
        {
            if (item.type != ItemType<Glock17>() && item.type != ItemType<NeutronReactor>())
            {
                item.autoReuse = true;
            }

        }

    }
    public class PenumbraGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GetInstance<PenumbraConfig>().VanillaChanges;
        }
        /// <summary>
        /// Useful to check if projectile hit NPC
        /// </summary>
        public bool ProjhitNPC;
        public override void SetDefaults(Projectile projectile)
        {
            ProjhitNPC = false;
            if (projectile.type == ProjectileID.DeathSickle)
            {
                projectile.DamageType = GetInstance<ReaperClass>();
            }
        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[projectile.owner];
            if (player.HeldItem?.type == ItemType<OmniStaff>())
            {
                if (projectile.type == ProjectileID.CultistBossLightningOrbArc)
                {
                    target.AddBuff(BuffType<HighVoltage>(), 120);
                }
            }
            if (player.HeldItem?.type == ItemID.InfluxWaver)
            {
                if (projectile.type == ProjectileID.InfluxWaver)
                {
                    target.AddBuff(BuffType<MediumVoltage>(), 120);
                }
            }
        }
    }
    public class HitEffectProjectile : PenumbraGlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GetInstance<PenumbraConfig>().HitEffect;
        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ProjhitNPC = true;
            target.AddBuff(BuffType<hitef>(), 10);
            Vector2 velocity = GetInstance<PenumbraConfig>().HitEffectVelocity;
            int NumProjectiles = GetInstance<PenumbraConfig>().HitEffectCount;
            Player player = Main.player[projectile.owner];
            for (int i = 0; i < NumProjectiles; i++)
            {

                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(360));
                // Decrease velocity randomly for nicer visuals.
                newVelocity *= 4f - Main.rand.NextFloat(0.2f);
                // Create a projectile.
                Projectile.NewProjectileDirect(Projectile.InheritSource(projectile), target.Center, newVelocity, ProjectileType<HitEffectProj>(), 0, 0, player.whoAmI);

            }
        }

    }
}
