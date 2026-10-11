// Local export of original HUD sprites, font and the PC-rendered item icon atlas.
using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
partial class ImportGame {
    static void StoragePresentation(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            var sprites=content.Load<DNA.Drawing.SpriteManager>("UI/SpriteSheet");
            var sprite=sprites["InventoryGrid"];
            using(var target=new RenderTarget2D(device,sprite.Width,sprite.Height,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
                device.SetRenderTarget(target);device.Clear(Color.Transparent);
                batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
                sprite.Draw(batch,new Rectangle(0,0,sprite.Width,sprite.Height),Color.White);batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"ui-InventoryGrid",false);
                Console.WriteLine("ORIGINAL_STORAGE_GRID size="+sprite.Width+"x"+sprite.Height);
            }
            var font=content.Load<SpriteFont>("Fonts/SmallFont");const int cell=64;
            using(var target=new RenderTarget2D(device,cell*16,cell*8,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
                device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
                for(int c=32;c<127;c++)batch.DrawString(font,((char)c).ToString(),new Vector2(c%16*cell,c/16*cell),Color.White);
                batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"storage-font",true);
            }
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"storage.font")))){w.Write(0x31484d43);w.Write(cell);w.Write(font.LineSpacing);for(int c=0;c<128;c++)w.Write(c<32?0:font.MeasureString(((char)c).ToString()).X);}
            Console.WriteLine("ORIGINAL_STORAGE_FONT line="+font.LineSpacing);
        }
    }
    static void BlockSelectionPresentation(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            content.RootDirectory=Path.Combine(install,"Content");var model=content.Load<Model>("Selector");Mesh(device,model,"block-selector");Mesh(device,content.Load<Model>("CrackBox"),"block-cracks");
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"block-selector.bin")))) {
                w.Write(0x31534243);int count=0;foreach(var mesh in model.Meshes)count+=mesh.MeshParts.Count;w.Write(count);
                foreach(var mesh in model.Meshes)foreach(var part in mesh.MeshParts)w.Write(mesh.Name=="Box002"?-1f:mesh.Name=="Box001"?1f:0f);
            }
            Console.WriteLine("ORIGINAL_BLOCK_SELECTOR meshes="+model.Meshes.Count);
        }
    }
    static void CrosshairPresentation(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            content.RootDirectory=Path.Combine(install,"Content");
            var sprites=content.Load<DNA.Drawing.SpriteManager>("UI/SpriteSheet");
            foreach(string name in new[]{"CrossHair","CrossHairTick"}) {
                var sprite=sprites[name];
                using(var target=new RenderTarget2D(device,sprite.Width,sprite.Height,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
                    device.SetRenderTarget(target);device.Clear(Color.Transparent);
                    batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
                    sprite.Draw(batch,new Rectangle(0,0,sprite.Width,sprite.Height),Color.White);batch.End();device.SetRenderTarget(null);
                    WriteUiTexture(target,"hud-"+name,false);
                    Console.WriteLine("ORIGINAL_CROSSHAIR "+name+" size="+sprite.Width+"x"+sprite.Height);
                }
            }
        }
    }
    static void HudPresentation(OracleContent content,GraphicsDevice device,Game instance,IServiceProvider services) {
        BlockSelectionPresentation(services,device);
        var sprites=content.Load<DNA.Drawing.SpriteManager>("UI/SpriteSheet");
        foreach(string name in new[]{"HealthBarEmpty","StaminaBarEmpty","HealthBarFull","HudGrid","Selector","CrossHair","CrossHairTick"}) {
            var sprite=sprites[name];using(var target=new RenderTarget2D(device,sprite.Width,sprite.Height,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
                device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
                sprite.Draw(batch,new Rectangle(0,0,sprite.Width,sprite.Height),Color.White);batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"hud-"+name,false);
                Console.WriteLine("ORIGINAL_HUD_SPRITE "+name+" size="+sprite.Width+"x"+sprite.Height);
            }
        }
        var font=content.Load<SpriteFont>("Fonts/MedFont");const int cell=64;
        using(var target=new RenderTarget2D(device,cell*16,cell*8,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
            device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
            for(int c=32;c<127;c++)batch.DrawString(font,((char)c).ToString(),new Vector2(c%16*cell,c/16*cell),Color.White);
            batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"hud-font",true);
        }
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,"hud.font")))){w.Write(0x31484d43);w.Write(cell);w.Write(font.LineSpacing);for(int c=0;c<128;c++)w.Write(c<32?0:font.MeasureString(((char)c).ToString()).X);}
        // Supply the existing graphics service to the unstarted original Game object.
        foreach(var field in typeof(Game).GetFields(BindingFlags.Instance|BindingFlags.NonPublic)) {
            if(field.FieldType==typeof(GameServiceContainer))field.SetValue(instance,services);
            if(field.FieldType==typeof(IGraphicsDeviceService))field.SetValue(instance,services.GetService(typeof(IGraphicsDeviceService)));
        }
        Console.WriteLine("HUD_ICON_DEVICE "+instance.GraphicsDevice.GraphicsProfile);
        var argsType=T("DNA.CastleMinerZ.CastleMinerZArgs");argsType.GetField("Instance",fields).SetValue(null,System.Runtime.Serialization.FormatterServices.GetUninitializedObject(argsType));
        DNA.Drawing.GraphicsDeviceLocker.Instance=new DNA.Drawing.GraphicsDeviceLocker();
        var terrain=Activator.CreateInstance(T("DNA.CastleMinerZ.Terrain.BlockTerrain"),new object[]{content});
        T("DNA.CastleMinerZ.BlockEntity").GetMethod("Initialize",fields).Invoke(null,null);
        // Icon rendering also needs the original UI-only model registries.
        T("DNA.CastleMinerZ.TorchEntity").GetMethod("Init",fields).Invoke(null,null);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(T("DNA.CastleMinerZ.DoorEntity").TypeHandle);
        var inventory=T("DNA.CastleMinerZ.Inventory.InventoryItem");inventory.GetMethod("FinishInitialization",fields).Invoke(null,new object[]{device});
        var atlas=(Texture2D)inventory.GetField("_2DImages",fields).GetValue(null);WriteUiTexture(atlas,"hud-items",false);
        device.SetRenderTarget(null);Console.WriteLine("ORIGINAL_HUD_ICONS atlas="+atlas.Width+"x"+atlas.Height+" items=143");
    }
}
