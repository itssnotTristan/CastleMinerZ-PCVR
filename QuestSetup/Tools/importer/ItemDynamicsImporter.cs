// Independently authored metadata conversion. Original meshes remain private.
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
partial class ImportGame {
    static void ItemDynamics(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            var instance=(Game)FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));instance.Content=content;
            T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
            var inventory=T("DNA.CastleMinerZ.Inventory.InventoryItem");inventory.GetMethod("Initalize",fields).Invoke(null,new object[]{content});
            var all=(IDictionary)inventory.GetField("AllItems",fields).GetValue(null);
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"compass-pivot.bin")))) {
                w.Write(0x31504d56);int id=39;var cls=all[Enum.ToObject(T("DNA.CastleMinerZ.Inventory.InventoryItemIDs"),id)];
                if(cls==null)foreach(DictionaryEntry entry in all)if(I(entry.Value,"ID")==id)cls=entry.Value;
                var model=(Model)Value(cls,"_model");var transforms=new Matrix[model.Bones.Count];model.CopyAbsoluteBoneTransformsTo(transforms);
                int index=0,count=0;foreach(var mesh in model.Meshes){if(mesh.Name.ToLowerInvariant().Contains("needle"))count+=mesh.MeshParts.Count;}w.Write(count);
                foreach(var mesh in model.Meshes)foreach(var part in mesh.MeshParts){if(mesh.Name.ToLowerInvariant().Contains("needle")){w.Write(index);MatrixData(w,transforms[mesh.ParentBone.Index]);Console.WriteLine("COMPASS_PIVOT part="+index+" parent="+mesh.ParentBone.Name+" matrix="+transforms[mesh.ParentBone.Index]);}index++;}
            }
            var seen=new HashSet<Model>();foreach(DictionaryEntry entry in all){var cls=entry.Value;var model=Value(cls,"_model") as Model;if(model==null||I(cls,"ClipCapacity")<=0||!seen.Add(model))continue;
                Console.WriteLine("DYNAMIC_GUN id="+I(cls,"ID")+" name="+S(cls,"Name")+" asset="+content.Models[model]+" clip="+I(cls,"ClipCapacity")+" rounds="+I(cls,"RoundsPerReload"));
                var transforms=new Matrix[model.Bones.Count];model.CopyAbsoluteBoneTransformsTo(transforms);foreach(var bone in model.Bones)Console.WriteLine("DYNAMIC_BONE "+bone.Name+" "+transforms[bone.Index]);
            }
            MagazineMasks(all,content);
        }
    }
    static void MagazineMasks(IDictionary all,OracleContent content) {
        var modelIds=new Dictionary<int,int>();using(var r=new BinaryReader(File.OpenRead(Path.Combine(output,"catalog.bin")))){if(r.ReadInt32()!=0x31434d43)throw new Exception("Import catalog before item dynamics");int n=r.ReadInt32();for(int i=0;i<n;i++){int id=r.ReadInt32();for(int s=0;s<5;s++)r.ReadString();r.ReadInt32();modelIds[id]=r.ReadInt32();r.ReadBytes(4*2+4*2+4+4+4+4+4+1+4+4+16+94*12);}}
        var guns=new List<object>();foreach(DictionaryEntry e in all)if(I(e.Value,"ClipCapacity")>0)guns.Add(e.Value);guns.Sort((a,b)=>I(a,"ID").CompareTo(I(b,"ID")));
        var converted=new Dictionary<int,bool>();using(var w=new BinaryWriter(File.Create(Path.Combine(output,"magazines.bin")))){w.Write(0x31474d56);w.Write(guns.Count);foreach(var gun in guns){int id=I(gun,"ID"),modelId=modelIds[id];bool has=false;if(!converted.TryGetValue(modelId,out has)){var model=Value(gun,"_model") as Model;string asset=model==null?"":content.Models[model].Replace('\\','/');has=ConvertMagazine(modelId,asset);converted[modelId]=has;}w.Write(id);w.Write(has);Console.WriteLine("ORIGINAL_MAGAZINE id="+id+" model="+modelId+" detachable="+has);}}
    }
    static int Root(int[] roots,int index){while(roots[index]!=index){roots[index]=roots[roots[index]];index=roots[index];}return index;}
    static bool ConvertMagazine(int model,string asset){
        string family=asset.Replace("Props/Weapons/","").Replace("/Model","");bool candidate=family=="Conventional/Pistol"||family=="Conventional/AR"||family=="Conventional/SMG"||family=="Conventional/LMG"||family=="Space/Pistol"||family=="Space/AR"||family=="Space/SMG"||family=="Space/Rifle"||asset=="Props/Tools/Drill/Model";
        string source=Path.Combine(output,"item-model-"+model+".mesh"),target=Path.ChangeExtension(source,".mag");if(!candidate){if(File.Exists(target))File.Delete(target);return false;}
        MagazineGeometry.Export(source,target,family,asset);return true;
    }
}

