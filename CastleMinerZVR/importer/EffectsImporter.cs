// Export effect settings and mesh roles from the owner's installation only.
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DNA.Drawing.Particles;
partial class ImportGame {
    static void Effects(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            var model=content.Load<Model>("Props/Items/Torch/Model");var matrices=new Matrix[model.Bones.Count];model.CopyAbsoluteBoneTransformsTo(matrices);
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"torch-effects.bin")))) {
                w.Write(0x31464d43);var flame=matrices[model.Bones["Flame"].Index].Translation;w.Write(flame.X);w.Write(flame.Y);w.Write(flame.Z);
                foreach(string name in new[]{"TorchFire","TorchSmoke"}) {
                    var effect=content.Load<ParticleEffect>("ParticleEffects/"+name);var texture=effect.Texture ?? content.Load<Texture2D>("ParticleEffects/"+Path.GetFileNameWithoutExtension(effect.TexturePath)+"_0");Texture(device,texture,"effect-"+name);
                    w.Write(effect.NumTilesWide);w.Write(effect.NumTilesHigh);w.Write(effect.FirstTileToInclude);w.Write(effect.LastTileToInclude);w.Write((int)effect.BlendMode);
                    w.Write(effect.ParticlesPerSecond);w.Write((float)effect.ParticleLifeTime.TotalSeconds);w.Write(effect.LifetimeVariation);
                    w.Write(effect.HorizontalVelocityMin);w.Write(effect.HorizontalVelocityMax);w.Write(effect.VerticalVelocityMin);w.Write(effect.VerticalVelocityMax);
                    w.Write(effect.Gravity.X);w.Write(effect.Gravity.Y);w.Write(effect.Gravity.Z);w.Write(effect.VelocityEnd);
                    var a=effect.ColorMin.ToVector4();var b=effect.ColorMax.ToVector4();foreach(float c in new[]{a.X,a.Y,a.Z,a.W,b.X,b.Y,b.Z,b.W})w.Write(c);
                    foreach(float f in new[]{effect.RotateSpeedMin,effect.RotateSpeedMax,effect.StartSizeMin,effect.StartSizeMax,effect.EndSizeMin,effect.EndSizeMax})w.Write(f);
                    Console.WriteLine("ORIGINAL_EFFECT "+name+" sprite="+effect.TexturePath+" life="+effect.ParticleLifeTime+" rate="+effect.ParticlesPerSecond);
                }
            }
            var instance=(Game)FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));instance.Content=content;T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
            var inventory=T("DNA.CastleMinerZ.Inventory.InventoryItem");inventory.GetMethod("Initalize",fields).Invoke(null,new object[]{content});var all=(IDictionary)inventory.GetField("AllItems",fields).GetValue(null);
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"weapon-recoil.bin")))) {
                var gunType=T("DNA.CastleMinerZ.Inventory.GunInventoryItemClass");int count=0;foreach(DictionaryEntry pair in all)if(gunType.IsAssignableFrom(pair.Value.GetType()))count++;
                w.Write(0x31524d43);w.Write(count);foreach(DictionaryEntry pair in all)if(gunType.IsAssignableFrom(pair.Value.GetType())){int id=I(pair.Value,"ID");float angle=F(Value(pair.Value,"Recoil"),"Radians");w.Write(id);w.Write(angle);Console.WriteLine("ORIGINAL_GUN_RECOIL id="+id+" radians="+angle);}
            }
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"saber-effects.bin")))) {
                w.Write(0x31534d45);int count=0;foreach(DictionaryEntry pair in all)if(pair.Value.GetType().Name=="SaberInventoryItemClass")count++;w.Write(count);
                foreach(DictionaryEntry pair in all) {var cls=pair.Value;if(cls.GetType().Name!="SaberInventoryItemClass")continue;w.Write(I(cls,"ID"));var color=((Color)Value(cls,"BeamColor")).ToVector4();w.Write(color.X);w.Write(color.Y);w.Write(color.Z);w.Write(color.W);
                    w.Write(S(cls,"_activeSound"));var weapon=(Model)Value(cls,"_model");int parts=0;foreach(var mesh in weapon.Meshes)parts+=mesh.MeshParts.Count;w.Write(parts);
                    foreach(var mesh in weapon.Meshes)foreach(var part in mesh.MeshParts)w.Write(mesh.Name.Contains("Beam"));Console.WriteLine("ORIGINAL_SABER id="+I(cls,"ID")+" color="+color+" hum="+S(cls,"_activeSound"));
                }
            }
            var sprites=content.Load<DNA.Drawing.SpriteManager>("UI/SpriteSheet");
            foreach(string name in new[]{"BlockUIBack","SingleGrid","Tier2Back","CraftSelector","MaterialsIcon","ToolsIcon","WeaponsIcon","StructuresIcon"}) {
                var sprite=sprites[name];using(var target=new RenderTarget2D(device,sprite.Width,sprite.Height,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
                    device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);sprite.Draw(batch,new Rectangle(0,0,sprite.Width,sprite.Height),Color.White);batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"ui-"+name,false);
                    Console.WriteLine("ORIGINAL_CRAFT_UI "+name+" size="+sprite.Width+"x"+sprite.Height);
                }
            }
        }
    }

    static void WriteImpact(BinaryWriter w,ParticleEffect e){w.Write(e.NumTilesWide);w.Write(e.NumTilesHigh);w.Write(e.FirstTileToInclude);w.Write(e.LastTileToInclude);w.Write((int)e.BlendMode);foreach(float f in new[]{e.ParticlesPerSecond,(float)e.ParticleLifeTime.TotalSeconds,e.LifetimeVariation,e.HorizontalVelocityMin,e.HorizontalVelocityMax,e.VerticalVelocityMin,e.VerticalVelocityMax,e.Gravity.X,e.Gravity.Y,e.Gravity.Z,e.VelocityEnd})w.Write(f);var a=e.ColorMin.ToVector4();var b=e.ColorMax.ToVector4();foreach(float f in new[]{a.X,a.Y,a.Z,a.W,b.X,b.Y,b.Z,b.W,e.RotateSpeedMin,e.RotateSpeedMax,e.StartSizeMin,e.StartSizeMax,e.EndSizeMin,e.EndSizeMax})w.Write(f);}
    static void WeaponEffects(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            Mesh(device,content.Load<Model>("MuzzleFlash"),"weapon-muzzle-flash");
            var e=content.Load<ParticleEffect>("ParticleEffects/SmokeEffect");
            var texture=e.Texture??content.Load<Texture2D>("ParticleEffects/"+Path.GetFileNameWithoutExtension(e.TexturePath)+"_0");Texture(device,texture,"weapon-smoke");
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"weapon-effects.bin")))) {
                w.Write(0x31574643);w.Write(e.NumTilesWide);w.Write(e.NumTilesHigh);w.Write(e.FirstTileToInclude);w.Write(e.LastTileToInclude);w.Write((int)e.BlendMode);
                w.Write(e.ParticlesPerSecond);w.Write((float)e.ParticleLifeTime.TotalSeconds);w.Write(e.LifetimeVariation);w.Write(e.HorizontalVelocityMin);w.Write(e.HorizontalVelocityMax);w.Write(e.VerticalVelocityMin);w.Write(e.VerticalVelocityMax);
                w.Write(e.Gravity.X);w.Write(e.Gravity.Y);w.Write(e.Gravity.Z);w.Write(e.VelocityEnd);var a=e.ColorMin.ToVector4();var b=e.ColorMax.ToVector4();foreach(float c in new[]{a.X,a.Y,a.Z,a.W,b.X,b.Y,b.Z,b.W})w.Write(c);
                foreach(float f in new[]{e.RotateSpeedMin,e.RotateSpeedMax,e.StartSizeMin,e.StartSizeMax,e.EndSizeMin,e.EndSizeMax})w.Write(f);
            }
            var sparks=content.Load<ParticleEffect>("ParticleEffects/SparksEffect");var sparkTexture=sparks.Texture??content.Load<Texture2D>("ParticleEffects/"+Path.GetFileNameWithoutExtension(sparks.TexturePath)+"_0");Texture(device,sparkTexture,"weapon-impact-sparks");using(var w=new BinaryWriter(File.Create(Path.Combine(output,"impact-effects.bin")))){w.Write(0x31494643);WriteImpact(w,sparks);WriteImpact(w,e);}
            Console.WriteLine("ORIGINAL_IMPACT_FX sparks="+sparks.TexturePath+" life="+sparks.ParticleLifeTime);
            Console.WriteLine("ORIGINAL_WEAPON_FX flash=MuzzleFlash smoke="+e.TexturePath+" life="+e.ParticleLifeTime);
        }
    }
}
