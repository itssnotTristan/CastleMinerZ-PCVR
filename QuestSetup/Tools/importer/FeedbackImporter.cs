// Local presentation export from the player's owned installation.
using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DNA.Drawing.Particles;
partial class ImportGame {
    static void FeedbackPresentation(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            var instance=(Game)FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));instance.Content=content;
            T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
            T("DNA.CastleMinerZ.Inventory.InventoryItem").GetMethod("Initalize",fields).Invoke(null,new object[]{content});
            T("DNA.CastleMinerZ.GunEntity").GetMethod("Init",fields).Invoke(null,null);
            HudPresentation(content,device,instance,services);
            Texture(device,content.Load<Texture2D>("UI/Screens/LoadScreen"),"intro-digitaldna");
            var sprite=content.Load<DNA.Drawing.SpriteManager>("UI/SpriteSheet")["Bar"];
            using(var target=new RenderTarget2D(device,sprite.Width,sprite.Height,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
                device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
                sprite.Draw(batch,new Rectangle(0,0,sprite.Width,sprite.Height),Color.White);batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"loading-bar",false);
            }
            var effect=content.Load<ParticleEffect>("ParticleEffects/BlasterFlash");
            var texture=effect.Texture??content.Load<Texture2D>("ParticleEffects/"+Path.GetFileNameWithoutExtension(effect.TexturePath)+"_0");
            Texture(device,texture,"laser-impact-flash");
            using(var writer=new BinaryWriter(File.Create(Path.Combine(output,"laser-impact.bin")))){writer.Write(0x314c4643);WriteImpact(writer,effect);}
            string[] names={"FireTrail","SmokeTrail","FireGlow","FireBallEffect","FlashEffect","FirePuff","BigSmokePuff","BigRockBlast","IceTrail","PuffTrail","IceGlow","IceBallEffect","IceFlash","IcePuff","BigPuff","BigSnowBlast","BlasterFlash"};
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"particle-timing.bin")))) {
                w.Write(0x31545043);w.Write(names.Length);
                foreach(var name in names) {var e=content.Load<ParticleEffect>("ParticleEffects/"+name);w.Write((float)e.EmmissionTime.TotalSeconds);w.Write(e.EmitterVelocitySensitivity);w.Write((e.LocalSpace?1:0)|(e.FadeOut?2:0)|(e.RandomizeRotations?4:0));Console.WriteLine("ORIGINAL_PARTICLE_TIMING "+name+" emission="+e.EmmissionTime+" local="+e.LocalSpace+" fade="+e.FadeOut);}
            }
            Console.WriteLine("ORIGINAL_LASER_IMPACT effect=BlasterFlash scale=0.01 life="+effect.ParticleLifeTime);
        }
    }
}
