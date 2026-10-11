// Independent local importer. Calls the user's installed game as a terrain oracle.
// Does not run the original entry point, patch assemblies, or copy decompiled code.
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

class GraphicsService : IGraphicsDeviceService {
    public GraphicsDevice GraphicsDevice { get; private set; }
    public event EventHandler<EventArgs> DeviceCreated, DeviceDisposing, DeviceReset, DeviceResetting;
    public GraphicsService(IntPtr window) {
        var parameters=new PresentationParameters { DeviceWindowHandle=window, BackBufferWidth=32, BackBufferHeight=32,
                BackBufferFormat=SurfaceFormat.Color, IsFullScreen=false, PresentationInterval=PresentInterval.Immediate };
        GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef,parameters);
    }
}
partial class ImportGame {
    static Assembly game;
    static string install, output;
    static readonly BindingFlags fields=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static Type T(string name) { return game.GetType(name,true); }
    static void Set(object o,string name,object value) { o.GetType().GetField(name,fields).SetValue(o,value); }
    static object Get(object o,string name) { return o.GetType().GetField(name,fields).GetValue(o); }
    [STAThread] static int Main(string[] args) {
        try {
            install=Path.GetFullPath(args[0]);output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=> { var p=Path.Combine(install,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null; };
            game=Assembly.LoadFrom(Path.Combine(install,"CastleMinerZ.exe"));
            var common=Assembly.LoadFrom(Path.Combine(install,"DNA.Common.dll"));
            common.GetType("DNA.Reflection.CommonAssembly").GetMethod("Initalize",fields).Invoke(null,null);
            Console.WriteLine("source_version="+game.GetName().Version);
            if(args.Length>2 && args[2]=="storage-ui") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);StoragePresentation(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="reloads") {ReloadPresentation(new GameServiceContainer());return 0;}
            if(args.Length>2 && args[2]=="barrels") {BarrelPresentation(new GameServiceContainer());return 0;}
            if(args.Length>2 && args[2]=="avatar") {AvatarPresentation(new GameServiceContainer(),null);return 0;}
            if(args.Length>2 && args[2]=="scopes") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);ScopePresentation(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="crosshair") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);CrosshairPresentation(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="selector") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);BlockSelectionPresentation(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="seed-oracle") {SeedGeneratorOracle();return 0;}
            if(args.Length>2 && args[2]=="mode-oracle") {ModeGeneratorOracle();return 0;}
            if(args.Length>2 && args[2]=="dragons") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);Dragons(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="generator") {GeneratorData();return 0;}
            if(args.Length>2 && args[2]=="enemy-oracle") {EnemyOracle();return 0;}
            if(args.Length>2 && args[2]=="item-dynamics") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);ItemDynamics(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="weapon-effects") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);WeaponEffects(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="dragon-effects") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);DragonEffects(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="effects") {using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);Effects(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;}
            if(args.Length>2 && args[2]=="presentation") {
                using(var form=new Form()){form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);
                    Presentation(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}return 0;
            }
            if(args.Length>3 && args[2]=="terrain-pack") {TerrainPack(int.Parse(args[3]),args.Length>4?int.Parse(args[4]):0,args.Length>5?int.Parse(args[5]):1);return 0;}
            if(args.Length>2 && args[2]=="catalog") {
                using(var form=new Form()) {form.ShowInTaskbar=false;var service=new GraphicsService(form.Handle);var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);
                    Catalog(services,service.GraphicsDevice);service.GraphicsDevice.Dispose();}
                return 0;
            }
            if(args.Length<3 || args[2]!="models") { Blocks();Terrain(); }
            using(var form=new Form()) {
                form.ShowInTaskbar=false;form.Width=32;form.Height=32;
                var service=new GraphicsService(form.Handle);
                var services=new GameServiceContainer();services.AddService(typeof(IGraphicsDeviceService),service);
                using(var content=new ContentManager(services,Path.Combine(install,"Content"))) {
                    Texture2D atlas=null;
                    foreach(var root in new[]{"HiDefContent","ReachContent",""}) {
                        try { content.RootDirectory=Path.Combine(install,"Content",root);atlas=(Texture2D)content.Load<Texture[]>("Terrain/Textures")[0];break; }
                        catch(Exception ex){Console.WriteLine("atlas_root "+root+": "+ex.GetType().Name);}
                    }
                    if(atlas==null)throw new Exception("Cannot load original terrain atlas");
                    Texture(service.GraphicsDevice,atlas,"atlas");
                    content.Unload();content.RootDirectory=Path.Combine(install,"Content");
                    foreach(var pair in new[]{new[]{"pistol","Props/Weapons/Conventional/Pistol/Model"},new[]{"pickaxe","Props/Tools/PickAxe/Model"},new[]{"rifle","Props/Weapons/Conventional/AR/Model"},new[]{"zombie","Enemies/Zombies/Zombie"},new[]{"player","Character/SWATMale"}}) {
                        try {var path=pair[1];if(!File.Exists(Path.Combine(content.RootDirectory,path+".xnb")))path="HiDefContent/"+path;
                            if(!File.Exists(Path.Combine(content.RootDirectory,path+".xnb")))path="ReachContent/"+pair[1];
                            var model=content.Load<Model>(path);Mesh(service.GraphicsDevice,model,pair[0]);
                            if(pair[0]=="player")Hands(model);}
                        catch(Exception ex){Console.WriteLine("MODEL_FAILURE "+pair[1]+" "+ex);}
                    }
                    Texture(service.GraphicsDevice,content.Load<Texture2D>("Enemies/Zombies/Diffuse01_0"),"zombie-diffuse");
                    service.GraphicsDevice.Dispose();
                }
            }
            Console.WriteLine("IMPORT_COMPLETE");return 0;
        } catch(Exception ex) {Console.Error.WriteLine(ex);return 1;}
    }
    static void Blocks() {
        var type=T("DNA.CastleMinerZ.Terrain.BlockType");
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        var blocks=(Array)type.GetField("_blockTypes",fields).GetValue(null);
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,"blocks.bin")))) {
            w.Write(0x31424d43);w.Write(blocks.Length);
            foreach(var block in blocks) {
                w.Write(Convert.ToInt32(Get(block,"_type")));w.Write((int)Get(block,"Hardness"));
                w.Write((bool)Get(block,"BlockPlayer"));w.Write((bool)Get(block,"CanBeDug"));w.Write((bool)Get(block,"HasAlpha"));w.Write((byte)0);
                var tiles=(int[])Get(block,"TileIndices");for(int i=0;i<6;i++)w.Write(tiles[i%tiles.Length]);
                w.Write((string)Get(block,"Name")??"");
            }
        }
        Console.WriteLine("blocks="+blocks.Length);
    }
    static void Terrain() {
        var gameType=T("DNA.CastleMinerZ.CastleMinerZGame");
        var instance=FormatterServices.GetUninitializedObject(gameType);
        gameType.GetField("Instance",fields).SetValue(null,instance);
        Set(instance,"GameMode",Enum.Parse(T("DNA.CastleMinerZ.UI.GameModeTypes"),"Survival"));
        var world=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.WorldInfo"));
        Set(world,"_seed",271828);Set(world,"MaxHellBossSpawns",0);Set(instance,"CurrentWorld",world);
        var terrain=FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.Terrain.BlockTerrain"));
        int[] blocks=new int[384*128*384];Set(terrain,"_blocks",blocks);
        Type iv=typeof(DNA.IntVector3);var min=new DNA.IntVector3(-192,-64,-192);Set(terrain,"_worldMin",min);
        DNA.MathTools.Rnd=new Random(541);
        var builder=Activator.CreateInstance(T("DNA.CastleMinerZ.Terrain.WorldBuilders.CastleMinerZBuilder"),fields,null,new[]{world},null);
        var build=builder.GetType().GetMethod("BuildWorldChunk");
        // Complete original rolling terrain window, including caves and ores.
        const int size=384,first=-192;
        for(int z=first;z<first+size;z+=16) {
            for(int x=first;x<first+size;x+=16)build.Invoke(builder,new object[]{terrain,new DNA.IntVector3(x,-64,z)});
            Console.WriteLine("terrain_rows="+(z-first+16));
        }
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,"terrain.bin")))) {
            w.Write(0x31574d43);w.Write(size);w.Write(128);w.Write(size);w.Write(first);w.Write(-64);w.Write(first);w.Write(271828);
            for(int z=first;z<first+size;z++)for(int x=first;x<first+size;x++)for(int y=0;y<128;y++) {
                int b=blocks[y+(x+192)*128+(z+192)*128*384];w.Write((byte)((b>>12)&0x7ffff));
            }
        }
    }
    static void Texture(GraphicsDevice device,Texture2D tex,string name) {
        Console.WriteLine("texture_format "+name+" "+tex.Format);
        var rt=new RenderTarget2D(device,tex.Width,tex.Height,false,SurfaceFormat.Color,DepthFormat.None);
        var batch=new SpriteBatch(device);device.SetRenderTarget(rt);device.Clear(Color.Transparent);
        batch.Begin(SpriteSortMode.Immediate,BlendState.Opaque,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone);
        batch.Draw(tex,new Rectangle(0,0,tex.Width,tex.Height),Color.White);batch.End();device.SetRenderTarget(null);
        Color[] pixels=new Color[tex.Width*tex.Height];rt.GetData(pixels);
        if(tex.Format==SurfaceFormat.Color)tex.GetData(pixels);
        else if(tex.Format==SurfaceFormat.Dxt1||tex.Format==SurfaceFormat.Dxt3||tex.Format==SurfaceFormat.Dxt5)pixels=DecodeBlocks(tex);
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,name+".rgba")))) {
            w.Write(tex.Width);w.Write(tex.Height);foreach(var c in pixels){w.Write(c.R);w.Write(c.G);w.Write(c.B);w.Write(c.A);}
        }
        using(var f=File.Create(Path.Combine(output,name+".png")))rt.SaveAsPng(f,tex.Width,tex.Height);
        batch.Dispose();rt.Dispose();Console.WriteLine("texture "+name+" "+tex.Width+"x"+tex.Height);
    }
    static Color[] DecodeBlocks(Texture2D texture) {
        int stride=texture.Format==SurfaceFormat.Dxt1?8:16,nx=(texture.Width+3)/4,nz=(texture.Height+3)/4;byte[] source=new byte[nx*nz*stride];texture.GetData(source);Color[] pixels=new Color[texture.Width*texture.Height];
        for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int start=(z*nx+x)*stride,color=start+(stride==16?8:0);ushort a=BitConverter.ToUInt16(source,color),b=BitConverter.ToUInt16(source,color+2);Color[] palette=new Color[4];
            palette[0]=new Color((byte)(((a>>11)&31)*255/31),(byte)(((a>>5)&63)*255/63),(byte)((a&31)*255/31),255);palette[1]=new Color((byte)(((b>>11)&31)*255/31),(byte)(((b>>5)&63)*255/63),(byte)((b&31)*255/31),255);
            bool transparent=stride==8&&a<=b;for(int i=0;i<3;i++){int ca=i==0?palette[0].R:i==1?palette[0].G:palette[0].B,cb=i==0?palette[1].R:i==1?palette[1].G:palette[1].B;
                byte c2=(byte)(transparent?(ca+cb)/2:(2*ca+cb)/3),c3=(byte)(transparent?0:(ca+2*cb)/3);if(i==0){palette[2].R=c2;palette[3].R=c3;}else if(i==1){palette[2].G=c2;palette[3].G=c3;}else{palette[2].B=c2;palette[3].B=c3;}}
            palette[2].A=255;palette[3].A=(byte)(transparent?0:255);uint indices=BitConverter.ToUInt32(source,color+4);byte[] alpha=new byte[8];ulong bits=0;
            if(texture.Format==SurfaceFormat.Dxt5){alpha[0]=source[start];alpha[1]=source[start+1];if(alpha[0]>alpha[1])for(int i=2;i<8;i++)alpha[i]=(byte)(((8-i)*alpha[0]+(i-1)*alpha[1])/7);
                else{for(int i=2;i<6;i++)alpha[i]=(byte)(((6-i)*alpha[0]+(i-1)*alpha[1])/5);alpha[6]=0;alpha[7]=255;}for(int i=0;i<6;i++)bits|=(ulong)source[start+2+i]<<(i*8);}
            for(int p=0;p<16;p++){int px=x*4+p%4,pz=z*4+p/4;if(px>=texture.Width||pz>=texture.Height)continue;Color c=palette[(indices>>(p*2))&3];
                if(texture.Format==SurfaceFormat.Dxt5)c.A=alpha[(bits>>(p*3))&7];else if(texture.Format==SurfaceFormat.Dxt3)c.A=(byte)(((source[start+p/2]>>((p%2)*4))&15)*17);pixels[pz*texture.Width+px]=c;}
        }return pixels;
    }
    static void Mesh(GraphicsDevice device,Model model,string name,Func<ModelMesh,bool> selectMesh=null) {
        var transforms=new Matrix[model.Bones.Count];model.CopyAbsoluteBoneTransformsTo(transforms);
        foreach(var bone in model.Bones) Console.WriteLine("bone="+name+" "+bone.Index+" "+bone.Name+" parent="+(bone.Parent==null?"none":bone.Parent.Name)+" local="+bone.Transform+" absolute="+transforms[bone.Index]);
        if(model.Tag!=null) {
            Console.WriteLine("tag="+name+" "+model.Tag.GetType());
            foreach(var prop in model.Tag.GetType().GetProperties()) Console.WriteLine("tag_property="+prop.Name+" "+prop.PropertyType);
            var skin=model.Tag as DNA.Drawing.Animation.SkinedAnimationData;
            if(skin!=null) {
                var pose=new Matrix[skin.Skeleton.Count];skin.Skeleton.CopyAbsoluteBoneTransformsTo(pose,Matrix.Identity);
                foreach(var bone in skin.Skeleton) if(bone.Name.Contains("_W_")||bone.Name.Contains("PROP")||bone.Index<2)
                    Console.WriteLine("skin_bone="+name+" "+bone.Index+" "+bone.Name+" pose="+pose[bone.Index]+" inverse="+skin.InverseBindPose[bone.Index]);
                foreach(var clip in skin.AnimationClips)Console.WriteLine("animation="+name+" "+clip.Key+" duration="+clip.Value.Duration+" count="+clip.Value.BoneCount);
            }
        }
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,name+".mesh")))) {
            w.Write(0x314d4d43);int count=0;foreach(var mesh in model.Meshes)if(selectMesh==null||selectMesh(mesh))foreach(var part in mesh.MeshParts)count++;
            w.Write(count);int serial=0;
            foreach(var mesh in model.Meshes)if(selectMesh==null||selectMesh(mesh))foreach(var part in mesh.MeshParts) {
                Console.WriteLine("mesh_part="+name+" "+mesh.Name+" bone_determinant="+transforms[mesh.ParentBone.Index].Determinant());
                int stride=part.VertexBuffer.VertexDeclaration.VertexStride;
                byte[] bytes=new byte[part.VertexBuffer.VertexCount*stride];part.VertexBuffer.GetData(bytes);
                int pos=0,uv=-1;
                foreach(var e in part.VertexBuffer.VertexDeclaration.GetVertexElements()) {
                    Console.WriteLine("vertex_element="+e.VertexElementUsage+" "+e.VertexElementFormat+" "+e.Offset);
                    if(e.VertexElementUsage==VertexElementUsage.Position)pos=e.Offset;
                    if(e.VertexElementUsage==VertexElementUsage.TextureCoordinate && e.UsageIndex==0)uv=e.Offset;
                }
                Texture2D tex=null;var basic=part.Effect as BasicEffect;
                Console.WriteLine("effect="+part.Effect.GetType().FullName);
                if(basic!=null)tex=basic.Texture;
                if(tex==null){var property=part.Effect.GetType().GetProperty("DiffuseMap");if(property!=null)tex=property.GetValue(part.Effect,null) as Texture2D;}
                if(tex==null)foreach(var param in part.Effect.Parameters) {
                    if(param.ParameterType==EffectParameterType.Texture2D)Console.WriteLine("texture_parameter="+param.Name);
                    if(param.ParameterType==EffectParameterType.Texture2D) {try {tex=param.GetValueTexture2D();}catch{}if(tex!=null)break;}
                }
                string textureName=name+"-"+serial++;w.Write(textureName);
                if(tex!=null)Texture(device,tex,textureName);
                w.Write(part.NumVertices);w.Write(part.PrimitiveCount*3);
                for(int i=0;i<part.NumVertices;i++) {
                    int offset=(i+part.VertexOffset)*stride;
                    var p=new Vector3(BitConverter.ToSingle(bytes,offset+pos),BitConverter.ToSingle(bytes,offset+pos+4),BitConverter.ToSingle(bytes,offset+pos+8));
                    var transform=transforms[mesh.ParentBone.Index];
                    if(Math.Abs(transform.Determinant())>0.000001f)p=Vector3.Transform(p,transform);
                    w.Write(p.X);w.Write(p.Y);w.Write(p.Z);
                    w.Write(uv<0?0:BitConverter.ToSingle(bytes,offset+uv));w.Write(uv<0?0:BitConverter.ToSingle(bytes,offset+uv+4));
                }
                Vector3 low=new Vector3(float.MaxValue),high=new Vector3(float.MinValue);
                for(int i=0;i<part.NumVertices;i++) {int o=(i+part.VertexOffset)*stride+pos;var p=new Vector3(BitConverter.ToSingle(bytes,o),BitConverter.ToSingle(bytes,o+4),BitConverter.ToSingle(bytes,o+8));low=Vector3.Min(low,p);high=Vector3.Max(high,p);}
                Console.WriteLine("raw_bounds="+name+" "+low+" "+high);
                if(part.IndexBuffer.IndexElementSize==IndexElementSize.SixteenBits) {
                    ushort[] indices=new ushort[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(indices);
                    for(int i=0;i<part.PrimitiveCount*3;i++)w.Write((uint)indices[part.StartIndex+i]);
                }else {uint[] indices=new uint[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(indices);
                    for(int i=0;i<part.PrimitiveCount*3;i++)w.Write(indices[part.StartIndex+i]);}
            }
        }
        Console.WriteLine("mesh="+name);
    }
    // Bake original skinned hand geometry into three anatomically jointed poses.
    // The runtime blends these poses using each controller's analog grip/trigger.
    static void Hands(Model model) {
        var skin=(DNA.Drawing.Animation.SkinedAnimationData)model.Tag;
        var skeleton=skin.Skeleton;var bind=new Matrix[skeleton.Count];skeleton.CopyTransformsTo(bind);
        for(int side=0;side<2;side++) {
            string prefix=side==0?"LF_":"RT_";int wrist=skeleton.IndexOf(prefix+"W__Skeleton");
            bool[] selected=new bool[skeleton.Count];
            foreach(var bone in skeleton) {var ancestor=bone;while(ancestor!=null){if(ancestor.Index==wrist){selected[bone.Index]=true;break;}ancestor=ancestor.Parent;}}
            Matrix[][] pose=new Matrix[3][];Matrix wristInverse=Matrix.Identity;
            for(int frame=0;frame<3;frame++) {
                skeleton.CopyTransformsFrom(bind);
                foreach(var bone in skeleton)if(selected[bone.Index]) {
                    var id=bone.Name.Substring(prefix.Length);float degrees=0;
                    if(id.StartsWith("FING")) {
                        degrees=id.Contains("1__")?62:id.Contains("2__")?48:38;
                        if(frame==2&&id.StartsWith("FINGA"))degrees+=22;
                        if(frame==0)degrees=0;
                        bone.Rotation=Quaternion.CreateFromAxisAngle(Vector3.UnitZ,MathHelper.ToRadians(degrees)*(side==0?-1:1))*bone.Rotation;
                    } else if(id.StartsWith("THUMB")&&frame>0) {
                        bone.Rotation=Quaternion.CreateFromAxisAngle(Vector3.UnitX,MathHelper.ToRadians(id.Contains("1__")?35:20))*bone.Rotation;
                    }
                }
                var absolute=new Matrix[skeleton.Count];skeleton.CopyAbsoluteBoneTransformsTo(absolute,Matrix.Identity);
                if(frame==0)wristInverse=Matrix.Invert(absolute[wrist]);
                pose[frame]=new Matrix[skeleton.Count];for(int i=0;i<pose[frame].Length;i++)pose[frame][i]=skin.InverseBindPose[i]*absolute[i]*wristInverse;
            }
            var positions=new List<Vector3[]>();var coords=new List<Vector2>();
            foreach(var mesh in model.Meshes)foreach(var part in mesh.MeshParts) {
                int stride=part.VertexBuffer.VertexDeclaration.VertexStride;byte[] bytes=new byte[part.VertexBuffer.VertexCount*stride];part.VertexBuffer.GetData(bytes);
                int pos=0,uv=44,indicesOffset=12,weightsOffset=16;
                foreach(var e in part.VertexBuffer.VertexDeclaration.GetVertexElements()) {
                    if(e.VertexElementUsage==VertexElementUsage.Position)pos=e.Offset;
                    if(e.VertexElementUsage==VertexElementUsage.TextureCoordinate&&e.UsageIndex==0)uv=e.Offset;
                    if(e.VertexElementUsage==VertexElementUsage.BlendIndices)indicesOffset=e.Offset;
                    if(e.VertexElementUsage==VertexElementUsage.BlendWeight)weightsOffset=e.Offset;
                }
                uint[] indices=new uint[part.PrimitiveCount*3];
                if(part.IndexBuffer.IndexElementSize==IndexElementSize.SixteenBits) {ushort[] source=new ushort[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(source);for(int i=0;i<indices.Length;i++)indices[i]=source[part.StartIndex+i];}
                else {uint[] source=new uint[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(source);Array.Copy(source,part.StartIndex,indices,0,indices.Length);}
                for(int tri=0;tri<indices.Length;tri+=3) {
                    bool include=true;
                    for(int corner=0;corner<3;corner++){int at=((int)indices[tri+corner]+part.VertexOffset)*stride;float weight=0;
                        for(int k=0;k<4;k++)if(selected[bytes[at+indicesOffset+k]])weight+=BitConverter.ToSingle(bytes,at+weightsOffset+k*4);
                        if(weight<.35f)include=false;}
                    if(!include)continue;
                    for(int corner=0;corner<3;corner++) {
                        int at=((int)indices[tri+corner]+part.VertexOffset)*stride;var p=new Vector3(BitConverter.ToSingle(bytes,at+pos),BitConverter.ToSingle(bytes,at+pos+4),BitConverter.ToSingle(bytes,at+pos+8));
                        var frames=new Vector3[3];for(int frame=0;frame<3;frame++) {
                            Vector3 baked=Vector3.Zero;for(int k=0;k<4;k++)baked+=Vector3.Transform(p,pose[frame][bytes[at+indicesOffset+k]])*BitConverter.ToSingle(bytes,at+weightsOffset+k*4);
                            frames[frame]=side==0?new Vector3(baked.Z,baked.Y,-baked.X):new Vector3(-baked.Z,baked.Y,baked.X);
                        }
                        positions.Add(frames);coords.Add(new Vector2(BitConverter.ToSingle(bytes,at+uv),BitConverter.ToSingle(bytes,at+uv+4)));
                    }
                }
            }
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,side==0?"left.hand":"right.hand")))) {
                w.Write(0x31484d43);w.Write("player-0");w.Write(positions.Count);
                for(int i=0;i<positions.Count;i++){foreach(var p in positions[i]){w.Write(p.X);w.Write(p.Y);w.Write(p.Z);}w.Write(coords[i].X);w.Write(coords[i].Y);}
            }
            Console.WriteLine("HAND_EXPORT side="+side+" triangles="+(positions.Count/3));
        }
        skeleton.CopyTransformsFrom(bind);
    }
}
