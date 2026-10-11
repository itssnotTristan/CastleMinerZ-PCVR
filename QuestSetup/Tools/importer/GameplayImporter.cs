using System;
using System.IO;
partial class ImportGame {
    static void Gameplay() {
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,"gameplay.bin")))) {
            w.Write(0x314c4d43);var loot=T("DNA.CastleMinerZ.Inventory.PossibleLootType");
            foreach(string method in new[]{"GetLootSource","GetEnemySource"}){var choices=(Array)loot.GetMethod(method,fields).Invoke(null,null);w.Write(choices.Length);
                foreach(var choice in choices)foreach(string name in new[]{"itemID","enemyType","weight","lootTier","minWorldLevel","maxWorldLevel","minQuantity","maxQuantity","worth","packageFlags","spawnType"})w.Write(I(choice,name));}
            var explosive=T("DNA.CastleMinerZ.Inventory.Explosive");var radii=(int[])explosive.GetField("cDestructionRanges",fields).GetValue(null);
            var enemy=(float[])explosive.GetField("cEnemyDamageRanges",fields).GetValue(null);var damage=(float[])explosive.GetField("cDamageRanges",fields).GetValue(null);var kill=(float[])explosive.GetField("cKillRanges",fields).GetValue(null);
            var immune=(bool[,])explosive.GetField("BreakLookup",fields).GetValue(null);var level1=(bool[])explosive.GetField("Level1Hardness",fields).GetValue(null);var level2=(bool[])explosive.GetField("Level2Hardness",fields).GetValue(null);
            w.Write(radii.Length);for(int i=0;i<radii.Length;i++){w.Write(radii[i]);w.Write(enemy[i]);w.Write(damage[i]);w.Write(kill[i]);for(int block=0;block<94;block++){w.Write(immune[i,block]);w.Write(level1[block]);w.Write(level2[block]);}}
        }Console.WriteLine("GAMEPLAY_DATA loot=49 spawnPackages=19 explosives=6");
    }
}
