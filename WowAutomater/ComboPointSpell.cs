﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace ClassicWowNeuralParasite
{
    public class ComboPointSpell : Spell
    {
        public ushort MinimumComboPointsCost;
        public ushort MaximumComboPointsCost;

        public ComboPointSpell(VirtualKeyCode hotKey,
                                ushort minimumComboPointsCost,
                                ushort maximumComboPointsCost,
                                ushort manaCost = 0,
                                double cooldownTime = 0,
                                double healthPercentage = 100,
                                ushort level = 1,
                                bool useOnce = false) : base(hotKey, manaCost, cooldownTime, healthPercentage, level, useOnce)
        {
            MinimumComboPointsCost = minimumComboPointsCost;
            MaximumComboPointsCost = maximumComboPointsCost;
        }

        public override bool CanCastSpell
        {
            get
            {
                if (WowApi.PlayerData.PlayerMana >= ManaCost &&
                   (WowApi.PlayerData.TargetComboPoints >= MinimumComboPointsCost && WowApi.PlayerData.TargetComboPoints <= MaximumComboPointsCost) &&
                   !WowApi.PlayerData.Casting &&
                    WowApi.PlayerData.CanUseSkill &&
                    WowApi.PlayerData.PlayerHealthPercentage <= PlayerHealthPercentage &&
                    WowApi.PlayerData.PlayerLevel >= Level &&
                    !Used &&
                    CooledDown)
                    return true;
                else
                    return false;
            }
        }
    }
}
