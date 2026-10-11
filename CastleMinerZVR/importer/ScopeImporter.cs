// Private import of the owned game's scoped weapons, reticle and bolt geometry.
using System;using System.IO;using System.Collections;using System.Runtime.Serialization;
using Microsoft.Xna.Framework;using Microsoft.Xna.Framework.Graphics;
partial class ImportGame {
    static void ScopePresentation(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            var instance=(Game)FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));instance.Content=content;
            T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
            var inventory=T("DNA.CastleMinerZ.Inventory.InventoryItem");inventory.GetMethod("Initalize",fields).Invoke(null,new object[]{content});
            var all=(IDictionary)inventory.GetField("AllItems",fields).GetValue(null);
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"weapon-scopes.bin")))) {
                var scoped=new System.Collections.Generic.List<object>();foreach(DictionaryEntry item in all)if(I(item.Value,"Scoped")!=0)scoped.Add(item.Value);
                scoped.Sort((a,b)=>I(a,"ID").CompareTo(I(b,"ID")));w.Write(0x31534d56);w.Write(scoped.Count);
                foreach(var item in scoped){w.Write(I(item,"ID"));w.Write(F(item,"ShoulderMagnification",1));Console.WriteLine("ORIGINAL_SCOPE id="+I(item,"ID")+" kind="+item.GetType().Name+" magnification="+F(item,"ShoulderMagnification",1));}
            }
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"item-visibility.bin")))) {w.Write(0x31564d56);w.Write(all.Count);foreach(DictionaryEntry item in all){var cls=item.Value;var model=Value(cls,"_model") as Model;w.Write(I(cls,"ID"));int parts=0;if(model!=null)foreach(var mesh in model.Meshes)parts+=mesh.MeshParts.Count;w.Write(parts);if(model!=null)foreach(var mesh in model.Meshes)foreach(var part in mesh.MeshParts)w.Write((byte)(mesh.Name.Contains("recolor_")?1:mesh.Name.Contains("recolor2_")?2:0));}}
            var sprites=content.Load<DNA.Drawing.SpriteManager>("UI/SpriteSheet");var sprite=sprites["SniperScope"];
            using(var target=new RenderTarget2D(device,sprite.Width,sprite.Height,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
                device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
                sprite.Draw(batch,new Rectangle(0,0,sprite.Width,sprite.Height),Color.White);batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"scope-reticle",false);
                Console.WriteLine("ORIGINAL_SCOPE_RETICLE size="+sprite.Width+"x"+sprite.Height);
            }
            var rifle=content.Load<Model>("Props/Weapons/Space/Rifle/Model");var matrices=new Matrix[rifle.Bones.Count];rifle.CopyAbsoluteBoneTransformsTo(matrices);
            foreach(var bone in rifle.Bones)Console.WriteLine("SCOPE_BONE "+bone.Name+" pos="+matrices[bone.Index].Translation);
            var bolt=content.Load<Model>("Props/Projectiles/Laser/Tracer_Bolt");Mesh(device,bolt,"scope-laser-bolt");
            Vector3 min=new Vector3(float.MaxValue),max=new Vector3(float.MinValue);var absolute=new Matrix[bolt.Bones.Count];bolt.CopyAbsoluteBoneTransformsTo(absolute);
            foreach(var mesh in bolt.Meshes)foreach(var part in mesh.MeshParts){int stride=part.VertexBuffer.VertexDeclaration.VertexStride,offset=0;foreach(var e in part.VertexBuffer.VertexDeclaration.GetVertexElements())if(e.VertexElementUsage==VertexElementUsage.Position)offset=e.Offset;byte[] bytes=new byte[part.VertexBuffer.VertexCount*stride];part.VertexBuffer.GetData(bytes);for(int i=part.VertexOffset;i<part.VertexOffset+part.NumVertices;i++){int at=i*stride+offset;var p=Vector3.Transform(new Vector3(BitConverter.ToSingle(bytes,at),BitConverter.ToSingle(bytes,at+4),BitConverter.ToSingle(bytes,at+8)),absolute[mesh.ParentBone.Index]);min=Vector3.Min(min,p);max=Vector3.Max(max,p);}}
            Console.WriteLine("ORIGINAL_LASER_BOLT_BOUNDS min="+min+" max="+max);
        }
    }
}
