using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
partial class ImportGame {
    // Precompute chunks with the real installed generator for standalone use.
    // The private resumable pack is kept outside this source project.
    static void TerrainPack(int size,int shard=0,int shards=1) {
        if(size<64||size>8192||size%32!=0)throw new Exception("Terrain pack edge must be 64..8192 and divisible by 32");
        var instance=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
        Set(instance,"GameMode",Enum.Parse(T("DNA.CastleMinerZ.UI.GameModeTypes"),"Survival"));var world=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.WorldInfo"));
        Set(world,"_seed",271828);Set(world,"MaxHellBossSpawns",0);Set(instance,"CurrentWorld",world);
        var terrain=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.Terrain.BlockTerrain"));int[] blocks=new int[384*128*384];Set(terrain,"_blocks",blocks);
        var builder=Activator.CreateInstance(T("DNA.CastleMinerZ.Terrain.WorldBuilders.CastleMinerZBuilder"),fields,null,new[]{world},null);var build=builder.GetType().GetMethod("BuildWorldChunk");
        if(shards<1||shards>16||size/16%shards!=0||shard<0||shard>=shards)throw new Exception("Invalid terrain pack shard");
        int columns=size/16,rows=columns/shards,first=-size/2,firstZ=first+shard*rows*16,total=columns*rows;long dataStart=24+(long)total*12;string path=Path.Combine(output,"terrain-pack-"+size+(shards>1?"-"+shard:"")+".bin");
        using(var file=new FileStream(path,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.Read))using(var writer=new BinaryWriter(file)) {
            if(file.Length==0){writer.Write(0x31504d43);writer.Write(columns);writer.Write(rows);writer.Write(first);writer.Write(firstZ);writer.Write(271828);file.SetLength(dataStart);}
            using(var reader=new BinaryReader(file,System.Text.Encoding.UTF8,true)) {file.Position=0;if(reader.ReadInt32()!=0x31504d43||reader.ReadInt32()!=columns||reader.ReadInt32()!=rows||reader.ReadInt32()!=first||reader.ReadInt32()!=firstZ||reader.ReadInt32()!=271828)throw new Exception("Existing terrain pack header does not match request");
                var watch=System.Diagnostics.Stopwatch.StartNew();int generated=0;
                for(int z=0;z<rows;z++)for(int x=0;x<columns;x++){int index=z*columns+x;file.Position=24+(long)index*12;long existing=reader.ReadInt64();int length=reader.ReadInt32();if(length>0&&existing>=dataStart&&existing+length<=file.Length)continue;
                    int cx=first+x*16,cz=firstZ+z*16;Set(terrain,"_worldMin",new DNA.IntVector3(cx,-64,cz));for(int row=0;row<16;row++)Array.Clear(blocks,row*128*384,16*128);
                    build.Invoke(builder,new object[]{terrain,new DNA.IntVector3(cx,-64,cz)});byte[] data=new byte[16*16*128];for(int row=0;row<16;row++)for(int col=0;col<16;col++)for(int y=0;y<128;y++){int type=(blocks[row*384*128+col*128+y]>>12)&0x7ffff;data[(row*16+col)*128+y]=type<94?(byte)type:(byte)0;}
                    using(var encoded=new MemoryStream()){using(var gzip=new GZipStream(encoded,CompressionLevel.Optimal,true))gzip.Write(data,0,data.Length);
                        byte[] bytes=encoded.ToArray();file.Position=file.Length;long offset=file.Position;writer.Write(bytes);file.Position=24+(long)index*12;writer.Write(offset);writer.Write(bytes.Length);}
                    generated++;if(generated%512==0){writer.Flush();Console.WriteLine("TERRAIN_PACK chunks="+(index+1)+"/"+total+" generated="+generated+" seconds="+watch.Elapsed.TotalSeconds.ToString("F1")+" bytes="+file.Length);}
                }
                writer.Flush();Console.WriteLine("TERRAIN_PACK_COMPLETE size="+size+" chunks="+total+" seconds="+watch.Elapsed.TotalSeconds.ToString("F2")+" bytes="+file.Length);
            }
        }
    }
}
