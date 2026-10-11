// Export the installed game's own initialized catalog, recipes and enemy data.
// This is a private data oracle, not copied game source or a modified assembly.
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
partial class ImportGame {
    class OracleContent : ContentManager {
        public Dictionary<Model,string> Models=new Dictionary<Model,string>();
        public Dictionary<Texture2D,string> Textures=new Dictionary<Texture2D,string>();
        public OracleContent(IServiceProvider services):base(services,Path.Combine(install,"Content")){}
        protected override Stream OpenStream(string asset) {
            foreach(var root in new[]{"","HiDefContent","ReachContent"}){var file=Path.Combine(install,"Content",root,asset+".xnb");if(File.Exists(file))return File.OpenRead(file);}
            throw new ContentLoadException("Original asset not found: "+asset);
        }
        public override A Load<A>(string asset) {
            A result=base.Load<A>(asset);
                if(result is Model)Models[(Model)(object)result]=asset;
                if(result is Texture2D)Textures[(Texture2D)(object)result]=asset;return result;
        }
    }
    static object Value(object item,string name) {
        if(item==null)return null;
        for(Type t=item.GetType();t!=null;t=t.BaseType){var f=t.GetField(name,fields|BindingFlags.DeclaredOnly);if(f!=null)return f.GetValue(item);
            var p=t.GetProperty(name,fields|BindingFlags.DeclaredOnly);if(p!=null)return p.GetValue(item,null);}
        return null;
    }
    static int I(object item,string name,int fallback=0){var value=Value(item,name);return value==null?fallback:Convert.ToInt32(value);}
    static float F(object item,string name,float fallback=0){var value=Value(item,name);if(value==null)return fallback;return value is TimeSpan?(float)((TimeSpan)value).TotalSeconds:Convert.ToSingle(value);}
    static string S(object item,string name){return Value(item,name) as string ?? "";}
    static void Catalog(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            var instance=(Game)FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));instance.Content=content;
            T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
            var inventory=T("DNA.CastleMinerZ.Inventory.InventoryItem");inventory.GetMethod("Initalize",fields).Invoke(null,new object[]{content});
            var all=(IDictionary)inventory.GetField("AllItems",fields).GetValue(null);
            var itemObjects=new List<object>();foreach(DictionaryEntry entry in all)itemObjects.Add(entry.Value);
            itemObjects.Sort((a,b)=>I(a,"ID").CompareTo(I(b,"ID")));
            var enemy=T("DNA.CastleMinerZ.AI.EnemyType");enemy.GetMethod("Init",fields).Invoke(null,null);
            var modelNumbers=new Dictionary<Model,int>();int modelCount=0;foreach(var pair in content.Models)modelNumbers[pair.Key]=modelCount++;
            using(var writer=new BinaryWriter(File.Create(Path.Combine(output,"catalog.bin")))) {
                writer.Write(0x31434d43);writer.Write(itemObjects.Count);
                foreach(var cls in itemObjects) {
                    writer.Write(I(cls,"ID"));writer.Write(S(cls,"Name"));writer.Write(S(cls,"Description"));writer.Write(cls.GetType().Name);
                    writer.Write(S(cls,"UseSound"));writer.Write(S(cls,"ReloadSound"));var block=Value(cls,"BlockType");writer.Write(block==null?-1:I(block,"_type"));
                    var model=Value(cls,"_model") as Model;writer.Write(model!=null&&modelNumbers.ContainsKey(model)?modelNumbers[model]:-1);
                    writer.Write(I(cls,"Material",-1));writer.Write(I(cls,"MaxStackCount"));writer.Write(F(cls,"CoolDownTime"));writer.Write(F(cls,"EnemyDamage"));
                    writer.Write(I(cls,"EnemyDamageType"));writer.Write(F(cls,"ItemSelfDamagePerUse"));writer.Write(I(cls,"ClipCapacity"));var ammo=Value(cls,"AmmoType");writer.Write(ammo==null?-1:I(ammo,"ID"));
                    writer.Write(F(cls,"ReloadTime"));writer.Write(Value(cls,"Automatic") is bool&&(bool)Value(cls,"Automatic"));writer.Write(F(cls,"Velocity"));writer.Write(F(cls,"FlightTime"));
                    var color=Value(cls,"ToolColor");var tint=color is Color?((Color)color).ToVector4():color is Vector3?new Vector4((Vector3)color,1):Vector4.One;
                    writer.Write(tint.X);writer.Write(tint.Y);writer.Write(tint.Z);writer.Write(tint.W);
                    object item=cls.GetType().GetMethod("CreateItem",fields).Invoke(cls,new object[]{1});
                    for(int b=0;b<94;b++) {var btype=Enum.ToObject(T("DNA.CastleMinerZ.Terrain.BlockTypeEnum"),b);float time=float.MaxValue;int drop=-1,count=0;
                        try {time=(float)((TimeSpan)item.GetType().GetMethod("TimeToDig",fields).Invoke(item,new[]{btype})).TotalSeconds;}catch{}
                        try {var produced=item.GetType().GetMethod("CreatesWhenDug",fields).Invoke(item,new object[]{btype,new DNA.IntVector3(0,0,0)});
                            if(produced!=null){drop=I(Value(produced,"ItemClass"),"ID",-1);count=I(produced,"StackCount");}}
                        catch{}
                        writer.Write(time);writer.Write(drop);writer.Write(count);
                    }
                    Console.WriteLine("ITEM "+I(cls,"ID")+" "+S(cls,"Name")+" "+cls.GetType().Name);
                }
                var recipes=(IList)T("DNA.CastleMinerZ.Inventory.Recipe").GetField("CookBook",fields).GetValue(null);writer.Write(recipes.Count);
                foreach(var recipe in recipes){writer.Write(I(recipe,"Type"));writer.Write(Value(recipe,"Type").ToString());var result=Value(recipe,"Result");
                    writer.Write(I(Value(result,"ItemClass"),"ID"));writer.Write(I(result,"StackCount"));var ingredients=(IList)Value(recipe,"Ingredients");writer.Write(ingredients.Count);
                    foreach(var ingredient in ingredients){writer.Write(I(Value(ingredient,"ItemClass"),"ID"));writer.Write(I(ingredient,"StackCount"));}}
                Console.WriteLine("RECIPES "+recipes.Count);
            }
            using(var writer=new BinaryWriter(File.Create(Path.Combine(output,"weapon-details.bin")))) {
                writer.Write(0x31474d43);writer.Write(itemObjects.Count);
                foreach(var cls in itemObjects){writer.Write(I(cls,"ID"));writer.Write(I(cls,"RoundsPerReload"));writer.Write(F(Value(cls,"MinInnaccuracy"),"Radians"));writer.Write(F(Value(cls,"MaxInnaccuracy"),"Radians"));
                    writer.Write(Value(cls,"NeedsDropCompensation") is bool&&(bool)Value(cls,"NeedsDropCompensation"));var color=Value(cls,"TracerColor");var tint=color is Color?((Color)color).ToVector4():color is Vector4?(Vector4)color:Vector4.One;
                    writer.Write(tint.X);writer.Write(tint.Y);writer.Write(tint.Z);writer.Write(tint.W);}
            }
            foreach(var pair in modelNumbers){Console.WriteLine("CATALOG_MODEL "+pair.Value+" "+content.Models[pair.Key]);Mesh(device,pair.Key,"item-model-"+pair.Value);
                if(content.Models[pair.Key].StartsWith("Enemies"))SkinnedModel(pair.Key,"item-model-"+pair.Value);}
            // Historical imports confirm these five launcher families contain
            // a separate original round named weap_m1_grenade_mesh. Export only
            // that part, preserving its source material and baked bone frame.
            foreach(var cls in itemObjects){string kind=cls.GetType().Name;if(!kind.Contains("RocketLauncher")&&!kind.Contains("GrenadeLauncher"))continue;
                int id=I(cls,"ID");var launcher=Value(cls,"_model") as Model;bool partFound=false;if(launcher!=null)foreach(var mesh in launcher.Meshes)partFound|=mesh.Name=="weap_m1_grenade_mesh";
                if(partFound){Mesh(device,launcher,"launcher-projectile-"+id,mesh=>mesh.Name=="weap_m1_grenade_mesh");Console.WriteLine("ORIGINAL_LAUNCHER_PROJECTILE item="+id+" source="+content.Models[launcher]+" part=weap_m1_grenade_mesh");}
                else Console.WriteLine("WARNING_MISSING_ORIGINAL_PROJECTILE_MODEL item="+id+"; original launcher has no known separate round; runtime checks ammunition geometry before explicit VR fallback");
            }
            Mesh(device,content.Load<Model>("Props\\Items\\Torch\\Model"),"torch");Gameplay();
            var enemies=(Array)enemy.GetField("Types",fields).GetValue(null);
            using(var writer=new BinaryWriter(File.Create(Path.Combine(output,"enemies.bin")))) {
                writer.Write(0x32454d43);writer.Write(enemies.Length);
                foreach(var type in enemies) {writer.Write(I(type,"EType"));writer.Write(type.GetType().Name);writer.Write(I(type,"FoundIn"));writer.Write(modelNumbers[(Model)Value(type,"Model")]);
                    int tex=I(type,"TextureIndex");writer.Write(tex);Texture(device,(Texture2D)Value(type,"EnemyTexture"),"enemy-texture-"+tex);
                    foreach(var name in new[]{"StartingHealth","Scale","Facing","BaseSlowSpeed","RandomSlowSpeed","BaseFastSpeed","BaseRunActivationTime","RandomRunActivationTime","BaseNormalActivationTime","FastJumpSpeed","DiggingMultiplier","AttackAnimationSpeed","DieAnimationSpeed","HitAnimationSpeed","SpawnAnimationSpeed"})writer.Write(F(type,name));
                    writer.Write(I(type,"SpawnRadius"));writer.Write(I(type,"StartingDistanceLimit"));writer.Write(I(type,"HardestBlockThatCanBeDug"));writer.Write((bool)Value(type,"HasRunFast"));
                    for(int damage=0;damage<5;damage++)for(int head=0;head<2;head++)writer.Write((float)type.GetType().GetMethod("GetDamageTypeMultiplier").Invoke(type,new object[]{Enum.ToObject(T("DNA.CastleMinerZ.AI.DamageType"),1<<damage),head!=0}));
                    var state=type.GetType().GetMethod("GetAttackState").Invoke(type,new object[]{null});var attacks=Value(state,"AttackArray") as string[];
                    var times=Value(state,"HitTimes") as float[][];var damages=Value(state,"HitDamages") as float[];var ranges=Value(state,"HitRanges") as float[];
                    writer.Write(attacks==null?0:attacks.Length);
                    if(attacks!=null)for(int a=0;a<attacks.Length;a++){writer.Write(attacks[a]);writer.Write(damages[a]);writer.Write(ranges[a]);writer.Write(times[a].Length);foreach(var time in times[a])writer.Write(time);}
                }
                Console.WriteLine("ENEMIES "+enemies.Length);
            }
            Console.WriteLine("CATALOG_COMPLETE items="+itemObjects.Count+" models="+modelNumbers.Count);
        }
    }
}
