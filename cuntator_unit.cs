using System;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using Mohawk.SystemCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;

namespace cunctator
{
    internal class CunctatorUnit : Unit
    {
        public CunctatorUnit()
        {
            return;
        }


        public override int attackUnitDamage(Tile pFromTile, Unit pToUnit, int iPercent = 100, int iExistingDamage = -1, bool bCheckOurUnits = true, int iExtraModifier = 0)
        {
            Debug.Log("that's my function yo attack unit damage");
            Tile pToTile = pToUnit.tile();
            int iToUnitHP = iExistingDamage == -1 ? pToUnit.getHP() : Math.Max(0, pToUnit.getHPMax() - iExistingDamage);
            if (iPercent == 0)
            {
                return ((iToUnitHP > 1) ? 1 : 0);
            }
            else
            {
                int iDamage = infos().Helpers.getAttackDamage(attackUnitStrength(pFromTile, pToTile, pToUnit, bCheckOurUnits, iExtraModifier), pToUnit.defendUnitStrength(pToTile, this), iPercent);


                Debug.Log("hata nerede bip");
                if (game().isGameOption(infos().Globals.GAMEOPTION_CRITICAL_HIT_PREVIEW))
                {
                    int iCriticalChance = criticalChanceVs(pToUnit);
                    // might do triple damage, if there's more than 100% chance of critical
                    Debug.Log("hata nerede bop");
                    if (iCriticalChance > 0 && isCriticalHit())
                    {
                        if (iCriticalChance > 100 && randomPercent(iCriticalChance-100))
                        {
                            iDamage *= 3;
                        }
                        else{
                            iDamage *= 2;
                        }
                    }
                }
                Debug.Log("hata burda");

                if (pToUnit.hasLastStand() && (iToUnitHP > 1))
                {
                    if (iDamage >= iToUnitHP)
                    {
                        iDamage = (iToUnitHP - 1);
                    }
                }

                return Math.Min(iDamage, iToUnitHP);
            }
        }

        protected virtual int attackUnit(CunctatorUnit pToUnit, Tile pFromTile, bool bTargetTile, int iPercent, ref List<Triple<string, int, TeamType>> azTileTexts, out AttackOutcome eOutcome, ref bool bEvent)
        {
            Debug.Log("that's my function yo attack unit");
            if (azTileTexts == null)
            {
                azTileTexts = new List<Triple<string, int, TeamType>>();
            }

            Character pToGeneral = pToUnit.general();

            bool bEnlisted = false;

            int iDamage = attackUnitDamage(pFromTile, pToUnit, (iPercent * 1));

            eOutcome = AttackOutcome.NORMAL;

            if (hasPlayer() && (iPercent > 0))
            {
                if (game().isGameOption(infos().Globals.GAMEOPTION_CRITICAL_HIT_PREVIEW))
                {
                    if (isCriticalHit())
                    {
                        eOutcome = AttackOutcome.CRITICAL;
                    }
                    resetCriticalHit();
                }
                else if (iDamage < pToUnit.getHP())
                {
                    int iCriticalChance = criticalChanceVs(pToUnit);
                    // might do triple damage, if there's more than 100% chance of critical
                    
                    int iDamageMultiplier = 1;
                    
                    if (iCriticalChance > 100 && randomPercent(iCriticalChance-100))
                    {
                        iDamageMultiplier = 3;
                    }
                    else if (iCriticalChance > 100 || randomPercent(iCriticalChance))
                    {
                        iDamageMultiplier = 2;
                    }

                    if (iDamageMultiplier > 1)
                    {
                        iDamage = Math.Max(2, attackUnitDamage(pFromTile, pToUnit, (iPercent * iDamageMultiplier)));
                        eOutcome = AttackOutcome.CRITICAL;
                    }
                }
            }

            if (eOutcome == AttackOutcome.CRITICAL)
            {
                azTileTexts.Add(new Triple<string, int, TeamType>(TextManager.TEXT("TEXT_GAME_UNIT_ATTACK_CRITICAL"), pToUnit.getTileID(), TeamType.NONE));
            }

            if (iDamage >= pToUnit.getHP())
            {
                if (bTargetTile && game().randomPercent(getEnlistOnKillChance(pToUnit.tile())))
                {
                    pToUnit.convert(getPlayer(), getTribe());
                    bEnlisted = true;
                }
                else
                {
                    pToUnit.changeDamage(iDamage);
                }
            }
            else
            {
                pToUnit.changeDamage(iDamage);
            }

            if (bEnlisted)
            {
                azTileTexts.Add(new Triple<string, int, TeamType>(TextManager.TEXT("TEXT_GAME_UNIT_ATTACK_ENLISTED"), pToUnit.getTileID(), TeamType.NONE));
            }
            else
            {
                azTileTexts.Add(new Triple<string, int, TeamType>(TextManager.TEXT("TEXT_GAME_UNIT_ATTACK_DAMAGE", TEXTVAR(iDamage)), pToUnit.getTileID(), TeamType.NONE));
            }

            pToUnit.doInjury(this, ref azTileTexts);

            pToUnit.wake();
            pToUnit.clearQueue();

            if (hasPlayer() && pToUnit.hasPlayer())
            {
                if (!(pToUnit.hasCooldown()) && !(game().isTeamTurn(pToUnit.getTeam())))
                {
                    pToUnit.doCooldown(infos().Globals.ATTACKED_COOLDOWN);
                }
            }

            if (pToUnit.isAlive())
            {
                // not killed
                if (hasPlayer() && hasGeneral())
                {
                    if (!bEvent && pToUnit.hasGeneral())
                    {
                        bEvent = player().doEventTrigger(((isWaterAttack(pToUnit)) ? infos().Globals.GENERAL_DUEL_WATER_EVENTTRIGGER : infos().Globals.GENERAL_DUEL_EVENTTRIGGER), general(), pToUnit.general());
                    }

                    if (!bEvent)
                    {
                        bEvent = player().doEventTrigger(((isWaterAttack(pToUnit)) ? infos().Globals.GENERAL_ATTACK_WATER_EVENTTRIGGER : infos().Globals.GENERAL_ATTACK_EVENTTRIGGER), general(), pToUnit.getPlayer());
                    }

                    if (!bEvent && eOutcome == AttackOutcome.CRITICAL)
                    {
                        bEvent = player().doEventTrigger(((isWaterAttack(pToUnit)) ? infos().Globals.GENERAL_CRITICAL_WATER_EVENTTRIGGER : infos().Globals.GENERAL_CRITICAL_EVENTTRIGGER), general(), pToUnit.getPlayer());
                    }
                }
            }

            if (pToUnit.isAlive())
            {
                return 0;
            }
            else
            {
                if (bEnlisted)
                {
                    eOutcome = AttackOutcome.CAPTURED;
                }
                else
                {
                    eOutcome = AttackOutcome.KILL;
                }

                if (hasPlayer())
                {
                    if (!bEvent)
                    {
                        bEvent = player().doEventTrigger(((isWaterAttack(pToUnit)) ? infos().Globals.UNIT_KILL_WATER_EVENTTRIGGER : infos().Globals.UNIT_KILL_EVENTTRIGGER), this, pToUnit.getPlayer());
                    }
                    if (!bEvent && hasGeneral())
                    {
                        bEvent = player().doEventTrigger(((isWaterAttack(pToUnit)) ? infos().Globals.GENERAL_KILL_WATER_EVENTTRIGGER : infos().Globals.GENERAL_KILL_EVENTTRIGGER), general(), pToUnit.getPlayer());
                    }
                }
                if (pToUnit.hasPlayer())
                {
                    if (pToGeneral != null)
                    {
                        pToUnit.player().doEventTrigger(((isWaterAttack(pToUnit)) ? infos().Globals.GENERAL_UNIT_KILLED_WATER_EVENTTRIGGER : infos().Globals.GENERAL_UNIT_KILLED_EVENTTRIGGER), pToGeneral, getPlayer());
                    }
                }

                if (hasPlayer() && pToUnit.canDamage())
                {
                    player().incrementLeaderStat(infos().Globals.UNIT_MILITARY_KILLED_STAT);

                    if (isLeaderGeneral())
                    {
                        player().incrementLeaderStat(infos().Globals.UNIT_MILITARY_KILLED_GENERAL_STAT);
                    }

                    if (pToUnit.isTribe())
                    {
                        player().incrementGoalTribeKilled(pToUnit.getTribe());
                    }
                    else
                    {
                        player().incrementGoalPlayerKilled(pToUnit.getPlayer());
                    }

                    using (var effectUnitListScoped = CollectionCache.GetListScoped<EffectUnitType>())
                    {
                        // copy existing effectUnits because they can change in the loop below
                        foreach (EffectUnitType eLoopEffectUnit in getEffectUnits())
                        {
                            effectUnitListScoped.Value.Add(eLoopEffectUnit);
                        }

                        foreach (EffectUnitType eLoopEffectUnit in effectUnitListScoped.Value)
                        {
                            HelpText.CommaListVariableGenerator victoryTextList = new HelpText.CommaListVariableGenerator(HelpText.CommaListVariableGenerator.ListType.NONE, TextManager);

                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iValue = infos().effectUnit(eLoopEffectUnit).maiMilitaryKillYield[(int)eLoopYield];
                                if (iValue != 0)
                                {
                                    if (player().processYieldWholeTile(eLoopYield, iValue, pFromTile, false))
                                    {
                                        victoryTextList.AddItem(HelpText.buildYieldIconTextVariable(eLoopYield, iValue, true));
                                    }
                                }
                            }

                            if (victoryTextList.Count > 0)
                            {
                                using (var scope = CollectionCache.GetStringBuilderScoped())
                                {
                                    azTileTexts.Add(new Triple<string, int, TeamType>(TextManager.TEXT(scope.Value, victoryTextList.Finalize()).ProfiledToString(), getTileID(), getTeam()));
                                }
                            }
                        }
                    }
                }

                if (pToUnit.isTribe())
                {
                    if (hasPlayer())
                    {
                        game().changeTribeWarScore(pToUnit.getTribe(), getTeam(), -10);
                    }
                }
                else
                {
                    pToUnit.player().incrementLeaderStat(infos().Globals.UNIT_LOST_STAT);

                    if (isTribe())
                    {
                        game().changeTribeWarScore(getTribe(), pToUnit.getTeam(), 20);
                    }
                    else
                    {
                        game().changeTeamWarScore(getTeam(), pToUnit.getTeam(), 10);
                    }
                }

                return 1;
            }
        }
    }
}
