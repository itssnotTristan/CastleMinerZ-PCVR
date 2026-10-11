using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
partial class ImportGame {
    static void ModeGeneratorOracle(){var instance=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);var world=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.WorldInfo"));Set(world,"_seed",271828);Set(world,"MaxHellBossSpawns",50);Set(instance,"CurrentWorld",world);var terrain=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.Terrain.BlockTerrain"));var blocks=new int[384*128*384];Set(terrain,"_blocks",blocks);var biome=T("DNA.CastleMinerZ.Terrain.WorldBuilders.Biome");int uninitialized=(int)biome.GetField("uninitblock",fields).GetValue(null);var builderType=T("DNA.CastleMinerZ.Terrain.WorldBuilders.CastleMinerZBuilder");int[] positions={0,288,1600,3408,8192,2000,2016,2032};
        using(var writer=new BinaryWriter(File.Create(Path.Combine(output,"generator-modes.oracle")))){writer.Write(0x344f4d43);writer.Write(48);for(int mode=0;mode<6;mode++){Set(instance,"GameMode",Enum.ToObject(T("DNA.CastleMinerZ.UI.GameModeTypes"),mode));object builder=null;for(int index=0;index<positions.Length;index++){bool reset=index<=5;int x=positions[index],z=index>=5?48:32;if(reset){Set(world,"HellBossesSpawned",0);DNA.MathTools.Rnd=new Random(541);builder=Activator.CreateInstance(builderType,fields,null,new[]{world},null);}Set(terrain,"_worldMin",new DNA.IntVector3(x,-64,z));for(int row=0;row<16;row++)for(int col=0;col<16;col++)for(int y=0;y<128;y++)blocks[row*384*128+col*128+y]=uninitialized;builderType.GetMethod("BuildWorldChunk").Invoke(builder,new object[]{terrain,new DNA.IntVector3(x,-64,z)});writer.Write(mode);writer.Write(reset);writer.Write(x);writer.Write(z);writer.Write((int)Get(Get(builder,"caves"),"emptyBlockCount"));for(int row=0;row<16;row++)for(int col=0;col<16;col++)for(int y=0;y<128;y++){int type=(blocks[row*384*128+col*128+y]>>12)&0x7ffff;writer.Write((byte)(type==94?0:type));}Console.WriteLine("MODE_GENERATOR_ORACLE mode="+mode+" x="+x+" z="+z+" reset="+reset);}}}
    }
    static void SeedGeneratorOracle(){var instance=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);var world=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.WorldInfo"));Set(world,"_seed",271828);Set(world,"MaxHellBossSpawns",50);Set(instance,"CurrentWorld",world);var terrain=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.Terrain.BlockTerrain"));var blocks=new int[384*128*384];Set(terrain,"_blocks",blocks);var biome=T("DNA.CastleMinerZ.Terrain.WorldBuilders.Biome");int uninitialized=(int)biome.GetField("uninitblock",fields).GetValue(null);var builderType=T("DNA.CastleMinerZ.Terrain.WorldBuilders.CastleMinerZBuilder");int[] positions={0,288,1600,3408,8192,2000,2016,2032};
        using(var writer=new BinaryWriter(File.Create(Path.Combine(output,"generator-seeds.oracle")))){writer.Write(0x354f4d43);writer.Write(192);foreach(int seed in new[]{1,12345,19088743,2147483000})for(int mode=0;mode<6;mode++){Set(world,"_seed",seed);Set(instance,"GameMode",Enum.ToObject(T("DNA.CastleMinerZ.UI.GameModeTypes"),mode));object builder=null;for(int index=0;index<positions.Length;index++){bool reset=index<=5;int x=positions[index],z=index>=5?48:32;if(reset){Set(world,"HellBossesSpawned",0);DNA.MathTools.Rnd=new Random(541);builder=Activator.CreateInstance(builderType,fields,null,new[]{world},null);}Set(terrain,"_worldMin",new DNA.IntVector3(x,-64,z));for(int row=0;row<16;row++)for(int col=0;col<16;col++)for(int y=0;y<128;y++)blocks[row*384*128+col*128+y]=uninitialized;builderType.GetMethod("BuildWorldChunk").Invoke(builder,new object[]{terrain,new DNA.IntVector3(x,-64,z)});writer.Write(seed);writer.Write(mode);writer.Write(reset);writer.Write(x);writer.Write(z);writer.Write((int)Get(Get(builder,"caves"),"emptyBlockCount"));for(int row=0;row<16;row++)for(int col=0;col<16;col++)for(int y=0;y<128;y++){int type=(blocks[row*384*128+col*128+y]>>12)&0x7ffff;writer.Write((byte)(type==94?0:type));}Console.WriteLine("SEED_GENERATOR_ORACLE seed="+seed+" mode="+mode+" x="+x+" z="+z+" reset="+reset);}}}
    }
    // Export only private tables and original oracle output; no game code is copied.
    static void GeneratorData() {
        var instance=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
        Set(instance,"GameMode",Enum.Parse(T("DNA.CastleMinerZ.UI.GameModeTypes"),"Survival"));
        var world=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.WorldInfo"));Set(world,"_seed",271828);Set(world,"MaxHellBossSpawns",50);Set(instance,"CurrentWorld",world);
        var terrain=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.Terrain.BlockTerrain"));var blocks=new int[384*128*384];Set(terrain,"_blocks",blocks);
        var builderType=T("DNA.CastleMinerZ.Terrain.WorldBuilders.CastleMinerZBuilder");
        var builder=Activator.CreateInstance(builderType,fields,null,new[]{world},null);
        var biome=T("DNA.CastleMinerZ.Terrain.WorldBuilders.Biome");foreach(string name in new[]{"emptyblock","uninitblock","rockblock","grassblock","sandBlock"})Console.WriteLine("RAW_BLOCK "+name+"="+biome.GetField(name,fields).GetValue(null));
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,"generator.tables")))) {
            w.Write(0x31474d43);w.Write(271828);
            foreach(int p in (int[])typeof(DNA.Drawing.Noise.PerlinNoise).GetField("_permute",fields).GetValue(null))w.Write(p);
            foreach(int p in (int[])typeof(DNA.Drawing.Noise.IntNoise).GetField("_permute",fields).GetValue(null))w.Write(p);
            var gradients=(int[][])typeof(DNA.Drawing.Noise.PerlinNoise).GetField("s_gradientVectors",fields).GetValue(null);for(int i=0;i<16;i++)for(int c=0;c<3;c++)w.Write(gradients[i][c]);
            var random=new Random(541);foreach(int p in (int[])typeof(Random).GetField("SeedArray",fields).GetValue(random))w.Write(p);
        }
        int[] distances={0,192,288,896,992,1600,1696,2304,2400,2992,3104,3408,3600,4400,4496,8192,10000,15008};
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,"generator.oracle")))) {
            w.Write(0x334f4d43);w.Write(distances.Length*2+64+16);
            Action<int,int,bool> check=(x,z,reset)=>{
                if(reset){Set(world,"HellBossesSpawned",0);DNA.MathTools.Rnd=new Random(541);builder=Activator.CreateInstance(builderType,fields,null,new[]{world},null);}
                Set(terrain,"_worldMin",new DNA.IntVector3(x,-64,z));Array.Clear(blocks,0,blocks.Length);int uninitialized=(int)biome.GetField("uninitblock",fields).GetValue(null);
                for(int row=0;row<16;row++)for(int col=0;col<16;col++)for(int y=0;y<128;y++)blocks[row*384*128+col*128+y]=uninitialized;
                builderType.GetMethod("BuildWorldChunk").Invoke(builder,new object[]{terrain,new DNA.IntVector3(x,-64,z)});
                w.Write(reset);w.Write(x);w.Write(z);w.Write((int)Get(Get(builder,"caves"),"emptyBlockCount"));for(int dz=0;dz<16;dz++)for(int dx=0;dx<16;dx++)for(int y=0;y<128;y++){int type=(blocks[dz*384*128+dx*128+y]>>12)&0x7ffff;w.Write((byte)(type==94?0:type));}
                Console.WriteLine("GENERATOR_ORACLE x="+x+" z="+z+" reset="+reset);
            };
            foreach(int distance in distances)foreach(int sign in new[]{1,-1}) {
                check(distance*sign,distance==0?-16:32,true);
            }
            var samples=new Random(42);for(int i=0;i<64;i++)check(samples.Next(-1000,1000)*16,samples.Next(-1000,1000)*16,true);
            for(int i=0;i<16;i++)check(2000+i*16,48,i==0);
        }
    }
}
