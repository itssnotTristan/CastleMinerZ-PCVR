// Independently authored conversion of the user's original PC presentation data.
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DNA.Drawing.Animation;
using DNA.Avatars;
using DNA.Net.GamerServices;
partial class ImportGame {
    static void MenuPresentation(OracleContent content,GraphicsDevice device) {
        Texture(device,content.Load<Texture2D>("UI/Screens/MenuBack"),"menu-background");
        Texture(device,content.Load<Texture2D>("UI/CMZREAd"),"menu-ad");
        var logo=content.Load<DNA.Drawing.SpriteManager>("UI/SpriteSheet")["Logo"];
        using(var target=new RenderTarget2D(device,logo.Width,logo.Height,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
            device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
            logo.Draw(batch,new Rectangle(0,0,logo.Width,logo.Height),Color.White);batch.End();device.SetRenderTarget(null);
            WriteUiTexture(target,"menu-logo",false);
        }
        var font=content.Load<SpriteFont>("Fonts/LargeFont");const int cell=96;
        using(var target=new RenderTarget2D(device,cell*16,cell*8,false,SurfaceFormat.Color,DepthFormat.None))using(var batch=new SpriteBatch(device)) {
            device.SetRenderTarget(target);device.Clear(Color.Transparent);batch.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
            for(int c=32;c<127;c++)batch.DrawString(font,((char)c).ToString(),new Vector2(c%16*cell,c/16*cell),Color.White);
            batch.End();device.SetRenderTarget(null);WriteUiTexture(target,"menu-font",true);
        }
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,"menu.font")))) {
            w.Write(0x31464d43);w.Write(cell);w.Write(font.LineSpacing);w.Write((float)logo.Height/logo.Width);
            for(int c=0;c<128;c++)w.Write(c<32?0:font.MeasureString(((char)c).ToString()).X);
        }
        Console.WriteLine("ORIGINAL_MENU font_line="+font.LineSpacing+" logo="+logo.Width+"x"+logo.Height);
    }
    static void WriteUiTexture(Texture2D texture,string name,bool white) {
        var pixels=new Color[texture.Width*texture.Height];texture.GetData(pixels);
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,name+".rgba")))) {
            w.Write(texture.Width);w.Write(texture.Height);foreach(var p in pixels) {
                w.Write((byte)(p.A==0?0:white?255:Math.Min(255,p.R*255/p.A)));
                w.Write((byte)(p.A==0?0:white?255:Math.Min(255,p.G*255/p.A)));
                w.Write((byte)(p.A==0?0:white?255:Math.Min(255,p.B*255/p.A)));w.Write(p.A);
            }
        }
    }
    static void SkyReference(OracleContent content,GraphicsDevice device) {
        var effect=content.Load<Effect>("Shaders/TextureSky");
        effect.Parameters["Blender"].SetValue(0f);effect.Parameters["LerpColor"].SetValue(Vector3.Zero);effect.Parameters["LerpAmount"].SetValue(0f);
        effect.Parameters["CloudOffset"].SetValue(Matrix.Identity);effect.Parameters["Offset"].SetValue(new Vector3(0,-100,0));
        foreach(string name in new[]{"ClearSky","NightSky","DawnSky","SunSet"}) {
            var cube=content.Load<TextureCube>("Textures/Skys/"+name);effect.Parameters["Sky1Texture"].SetValue(cube);effect.Parameters["Sky2Texture"].SetValue(cube);
            var sphere=new DNA.Drawing.SkySphere(device,500,Vector3.Zero,20,effect,cube);
            using(var target=new RenderTarget2D(device,768,768,false,SurfaceFormat.Color,DepthFormat.Depth24)) {
                device.SetRenderTarget(target);device.Clear(Color.Black);device.RasterizerState=RasterizerState.CullNone;device.DepthStencilState=DepthStencilState.None;device.BlendState=BlendState.Opaque;
                sphere.Draw(device,new GameTime(),Matrix.Identity,Matrix.CreatePerspectiveFieldOfView(1.4f,1,.03f,1000));device.SetRenderTarget(null);
                using(var file=File.Create(Path.Combine(output,"oracle-sky-"+name+".png")))target.SaveAsPng(file,768,768);
            }
        }
    }
    class ViewPoint {public Vector3 p,n;public Vector2 uv;public byte[] joints;public float[] weights;}
    static void Presentation(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OracleContent(services)) {
            MenuPresentation(content,device);
            SkyReference(content,device);
            foreach(string name in new[]{"ClearSky","NightSky","SunSet","DawnSky"}) {
                var cube=content.Load<TextureCube>("Textures/Skys/"+name);
                using(var w=new BinaryWriter(File.Create(Path.Combine(output,name+".cube")))) {
                    w.Write(0x31435343);w.Write(cube.Size);
                    for(int face=0;face<6;face++) {
                        Color[] pixels;
                        if(cube.Format==SurfaceFormat.Color){pixels=new Color[cube.Size*cube.Size];cube.GetData((CubeMapFace)face,pixels);}
                        else {
                            int block=cube.Format==SurfaceFormat.Dxt1?8:16;
                            var raw=new byte[((cube.Size+3)/4)*((cube.Size+3)/4)*block];cube.GetData((CubeMapFace)face,raw);
                            using(var temp=new Texture2D(device,cube.Size,cube.Size,false,cube.Format)){temp.SetData(raw);pixels=DecodeBlocks(temp);}
                        }
                        foreach(var p in pixels){w.Write(p.R);w.Write(p.G);w.Write(p.B);w.Write(p.A);}
                    }
                }
                Console.WriteLine("ORIGINAL_SKY "+name+" size="+cube.Size+" format="+cube.Format);
            }
            var player=content.Load<Model>("Character/SWATMale");
            var skin=(SkinedAnimationData)player.Tag;var skeleton=skin.Skeleton;
            var bind=new Matrix[skeleton.Count];skeleton.CopyTransformsTo(bind);
            var points=new List<ViewPoint>[2];
            for(int side=0;side<2;side++) {
                points[side]=new List<ViewPoint>();string prefix=side==0?"LF_":"RT_";int wrist=skeleton.IndexOf(prefix+"W__Skeleton");
                var selected=new bool[skeleton.Count];foreach(var bone in skeleton){var ancestor=bone;while(ancestor!=null){if(ancestor.Index==wrist){selected[bone.Index]=true;break;}ancestor=ancestor.Parent;}}
                foreach(var mesh in player.Meshes)foreach(var part in mesh.MeshParts) {
                    int stride=part.VertexBuffer.VertexDeclaration.VertexStride;var bytes=new byte[part.VertexBuffer.VertexCount*stride];part.VertexBuffer.GetData(bytes);
                    int pos=0,normal=-1,uv=0,joints=0,weights=0;
                    foreach(var e in part.VertexBuffer.VertexDeclaration.GetVertexElements()){
                        if(e.VertexElementUsage==VertexElementUsage.Position)pos=e.Offset;if(e.VertexElementUsage==VertexElementUsage.Normal)normal=e.Offset;
                        if(e.VertexElementUsage==VertexElementUsage.TextureCoordinate&&e.UsageIndex==0)uv=e.Offset;
                        if(e.VertexElementUsage==VertexElementUsage.BlendIndices)joints=e.Offset;if(e.VertexElementUsage==VertexElementUsage.BlendWeight)weights=e.Offset;
                    }
                    var indices=new uint[part.PrimitiveCount*3];if(part.IndexBuffer.IndexElementSize==IndexElementSize.SixteenBits){var src=new ushort[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(src);for(int i=0;i<indices.Length;i++)indices[i]=src[part.StartIndex+i];}
                    else {var src=new uint[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(src);Array.Copy(src,part.StartIndex,indices,0,indices.Length);}
                    for(int tri=0;tri<indices.Length;tri+=3){var vertices=new ViewPoint[3];bool include=true;
                        for(int c=0;c<3;c++){int at=((int)indices[tri+c]+part.VertexOffset)*stride;var p=new ViewPoint();
                            p.p=new Vector3(BitConverter.ToSingle(bytes,at+pos),BitConverter.ToSingle(bytes,at+pos+4),BitConverter.ToSingle(bytes,at+pos+8));
                            p.n=normal<0?Vector3.Up:new Vector3(BitConverter.ToSingle(bytes,at+normal),BitConverter.ToSingle(bytes,at+normal+4),BitConverter.ToSingle(bytes,at+normal+8));
                            p.uv=new Vector2(BitConverter.ToSingle(bytes,at+uv),BitConverter.ToSingle(bytes,at+uv+4));p.joints=new byte[4];p.weights=new float[4];
                            for(int k=0;k<4;k++)p.joints[k]=bytes[at+joints+k];for(int k=0;k<3;k++)p.weights[k]=BitConverter.ToSingle(bytes,at+weights+k*4);p.weights[3]=Math.Max(0,1-p.weights[0]-p.weights[1]-p.weights[2]);
                            float total=0;for(int k=0;k<4;k++)if(selected[p.joints[k]])total+=p.weights[k];include&=total>=.35f;vertices[c]=p;
                        }
                        if(include)points[side].AddRange(vertices);
                    }
                }
            }
            string[][] families={new[]{"Pistol","Weapons/Conventional/Pistol"},new[]{"Assault","Weapons/Conventional/AR"},new[]{"BoltRifle","Weapons/Conventional/Rifle"},
                new[]{"PumpShotgun","Weapons/Conventional/Shotgun"},new[]{"SMG","Weapons/Conventional/SMG"},new[]{"LMG","Weapons/Conventional/LMG"},
                new[]{"SpacePistol","Weapons/Space/Pistol"},new[]{"SpaceAssault","Weapons/Space/AR"},new[]{"SpaceBoltRifle","Weapons/Space/Rifle"},
                new[]{"SpacePumpShotgun","Weapons/Space/Shotgun"},new[]{"SpaceSMG","Weapons/Space/SMG"},new[]{"RPG","Weapons/Conventional/RPG"},
                new[]{"Pick","Tools/PickAxe"},new[]{"Generic","Generic"}};
            var positions=new Vector3[71];var rotations=new Quaternion[71];var scales=new Vector3[71];
            var avatarAbsolute=new Matrix[71];var proxyAbsolute=new Matrix[skeleton.Count];var proxyLocal=new Matrix[skeleton.Count];
            int rightWrist=skeleton.IndexOf("RT_W__Skeleton");var root=Matrix.CreateRotationY(MathHelper.Pi);root.Translation=new Vector3(0,.7769f,-.008664f);
            Action<AnimationClip,double> sample=(clip,time)=>{
                if(clip.BoneCount!=71)throw new Exception("Unexpected PC avatar bone count");
                clip.CopyTransforms(positions,rotations,scales,TimeSpan.FromSeconds(time),null);
                var local=new Matrix[71];for(int b=0;b<71;b++)local[b]=Matrix.CreateScale(scales[b])*Matrix.CreateFromQuaternion(rotations[b])*Matrix.CreateTranslation(positions[b]);
                avatarAbsolute[0]=local[0]*Avatar.DefaultBindPose[0];
                for(int b=1;b<71;b++){var m=local[b];m.Translation=Avatar.DefaultBindPose[b].Translation;avatarAbsolute[b]=m*avatarAbsolute[Avatar.DefaultParentBones[b]];}
                proxyLocal[0]=local[0]*root;
                for(int b=1;b<skeleton.Count;b++){var m=local[Avatar.boneNameLookup[skeleton[b].Name]];m.Translation=bind[b].Translation;proxyLocal[b]=m;}
                skeleton.CopyTransformsFrom(proxyLocal);skeleton.CopyAbsoluteBoneTransformsTo(proxyAbsolute,Matrix.Identity);
            };
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"player.view")))) {
                w.Write(0x32564d43);w.Write(skeleton.Count);w.Write(rightWrist);w.Write("player-0");
                Console.WriteLine("PC_VIEW_WRIST bone="+rightWrist+" name="+skeleton[rightWrist].Name);
                for(int side=0;side<2;side++){w.Write(points[side].Count);foreach(var p in points[side]){w.Write(p.p.X);w.Write(p.p.Y);w.Write(p.p.Z);w.Write(p.n.X);w.Write(p.n.Y);w.Write(p.n.Z);w.Write(p.uv.X);w.Write(p.uv.Y);w.Write(p.joints);foreach(float f in p.weights)w.Write(f);}}
                w.Write(families.Length);int totalClips=0;
                foreach(var family in families) {
                    var clips=new Dictionary<string,AnimationClip>();
                    foreach(string action in new[]{"Idle","Walk","Shoot","Reload","Use"}) {
                        string asset=family[0]=="Generic"?"Character/Animation/Generic"+action:"Props/"+family[1]+"/Animation/"+action;
                        // PC laser shotgun shares the laser assault idle/walk clips.
                        if(family[0]=="SpacePumpShotgun"&&(action=="Idle"||action=="Walk"))asset="Props/Weapons/Space/AR/Animation/"+action;
                        if(File.Exists(Path.Combine(install,"Content",asset+".xnb"))||File.Exists(Path.Combine(install,"Content/HiDefContent",asset+".xnb"))||File.Exists(Path.Combine(install,"Content/ReachContent",asset+".xnb")))clips.Add(action,content.Load<AnimationClip>(asset));
                    }
                    if(!clips.ContainsKey("Idle"))throw new Exception("Original idle missing: "+family[0]);
                    sample(clips["Idle"],0);Matrix referenceWrist=proxyAbsolute[rightWrist],referenceProp=avatarAbsolute[(int)AvatarBone.PropRight],inverseProp=Matrix.Invert(referenceProp);
                    var grip=Vector3.Transform(referenceWrist.Translation,inverseProp);
                    w.Write(family[0]);w.Write(grip.X);w.Write(grip.Y);w.Write(grip.Z);w.Write(clips.Count);
                    foreach(var pair in clips){var clip=pair.Value;int frames=(int)Math.Ceiling(clip.Duration.TotalSeconds*30)+1;w.Write(pair.Key);w.Write((float)clip.Duration.TotalSeconds);w.Write(30);w.Write(frames);
                        for(int frame=0;frame<frames;frame++){
                            sample(clip,Math.Min(clip.Duration.TotalSeconds,(double)frame/30));
                            // Retain source finger/support-hand motion around a 6DOF right wrist.
                            Matrix reference=Matrix.Invert(proxyAbsolute[rightWrist])*referenceWrist*inverseProp;
                            MatrixData(w,avatarAbsolute[(int)AvatarBone.PropRight]*reference);
                            for(int b=0;b<skeleton.Count;b++)MatrixData(w,skin.InverseBindPose[b]*proxyAbsolute[b]*reference);
                        }
                        totalClips++;Console.WriteLine("PC_VIEW_CLIP "+family[0]+"/"+pair.Key+" seconds="+clip.Duration.TotalSeconds+" frames="+frames);
                    }
                    Console.WriteLine("PC_VIEW_GRIP "+family[0]+" wrist="+grip);
                }
                Console.WriteLine("PC_VIEW_EXPORT bones="+skeleton.Count+" hands="+points[0].Count+","+points[1].Count+" families="+families.Length+" clips="+totalClips);
            }
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"hand-tracking.bin")))) {w.Write(0x31484d56);int left=skeleton.IndexOf("LF_W__Skeleton");w.Write(left);MatrixData(w,Matrix.Invert(skin.InverseBindPose[left]));MatrixData(w,Matrix.Invert(skin.InverseBindPose[rightWrist]));}
            skeleton.CopyTransformsFrom(bind);
            var instance=(Game)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));instance.Content=content;
            T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
            var inventory=T("DNA.CastleMinerZ.Inventory.InventoryItem");inventory.GetMethod("Initalize",fields).Invoke(null,new object[]{content});
            T("DNA.CastleMinerZ.GunEntity").GetMethod("Init",fields).Invoke(null,null);
            var all=(IDictionary)inventory.GetField("AllItems",fields).GetValue(null);
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"view-items.bin")))) {
                w.Write(0x33564d43);w.Write(all.Count);foreach(DictionaryEntry entry in all){var cls=entry.Value;w.Write(I(cls,"ID"));w.Write(Convert.ToString(Value(cls,"PlayerAnimationMode")));var model=Value(cls,"_model") as Model;
                    Vector3 muzzle=Vector3.Zero;if(model!=null){var transforms=new Matrix[model.Bones.Count];model.CopyAbsoluteBoneTransformsTo(transforms);foreach(var bone in model.Bones)if(bone.Name=="BarrelTip")muzzle=transforms[bone.Index].Translation;}
                    w.Write(muzzle.X);w.Write(muzzle.Y);w.Write(muzzle.Z);Matrix attachment=Matrix.Identity;
                    if(model!=null){var entity=cls.GetType().GetMethod("CreateEntity",fields).Invoke(cls,new object[]{Enum.Parse(T("DNA.CastleMinerZ.Inventory.ItemUse"),"Hand"),false});var transform=Value(entity,"LocalToParent");if(transform is Matrix)attachment=(Matrix)transform;}
                    MatrixData(w,attachment);
                }
            }
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"item-mounts.bin")))) {
                w.Write(0x314d4d56);w.Write(all.Count);foreach(DictionaryEntry entry in all){var cls=entry.Value;int id=I(cls,"ID");w.Write(id);var model=Value(cls,"_model") as Model;int count=0;if(model!=null)foreach(var mesh in model.Meshes)count+=mesh.MeshParts.Count;w.Write(count);
                    if(model!=null)foreach(var mesh in model.Meshes)foreach(var part in mesh.MeshParts){string name=mesh.Name.ToLowerInvariant();byte role=(byte)(name.Contains("needle")?2:name.Contains("mag")||name.Contains("clip")?1:0);w.Write(role);if(I(cls,"ClipCapacity")>0||id==39||id==44)Console.WriteLine("ITEM_MESH_ROLE id="+id+" mesh="+mesh.Name+" role="+role);}
                }
            }
            HudPresentation(content,device,instance,services);
            ItemDynamics(services,device);
        }
    }
}
