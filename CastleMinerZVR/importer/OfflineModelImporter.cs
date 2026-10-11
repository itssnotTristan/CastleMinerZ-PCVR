// Read owned XNB mesh buffers without creating a Windows graphics device.
// The installed XNA runtime performs decompression and the game's data readers.
using System;using System.IO;using System.Reflection;using System.Collections.Generic;using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Content;using Microsoft.Xna.Framework.Graphics;using DNA.Drawing.Effects;
partial class ImportGame {
    static Dictionary<VertexBuffer,byte[]> offlineVertices=new Dictionary<VertexBuffer,byte[]>();
    static Dictionary<IndexBuffer,uint[]> offlineIndices=new Dictionary<IndexBuffer,uint[]>();
    class OfflineVertexReader:ContentTypeReader<VertexBuffer> {
        protected override VertexBuffer Read(ContentReader input,VertexBuffer existing){var decl=input.ReadRawObject<VertexDeclaration>();int count=input.ReadInt32();if(count<1||count>1000000)throw new Exception("Invalid vertex count");var raw=input.ReadBytes(checked(count*decl.VertexStride));var buffer=(VertexBuffer)FormatterServices.GetUninitializedObject(typeof(VertexBuffer));typeof(VertexBuffer).GetField("_vertexCount",fields).SetValue(buffer,(uint)count);typeof(VertexBuffer).GetField("_vertexDeclaration",fields).SetValue(buffer,decl);offlineVertices.Add(buffer,raw);return buffer;}
    }
    class OfflineIndexReader:ContentTypeReader<IndexBuffer> {
        protected override IndexBuffer Read(ContentReader input,IndexBuffer existing){bool shortIndex=input.ReadBoolean();int length=input.ReadInt32();if(length<1||length>4000000)throw new Exception("Invalid index length");var raw=input.ReadBytes(length);var indices=new uint[length/(shortIndex?2:4)];for(int n=0;n<indices.Length;n++)indices[n]=shortIndex?BitConverter.ToUInt16(raw,n*2):BitConverter.ToUInt32(raw,n*4);var buffer=(IndexBuffer)FormatterServices.GetUninitializedObject(typeof(IndexBuffer));offlineIndices.Add(buffer,indices);return buffer;}
    }
    class OfflineEffectReader:ContentTypeReader<DNAEffect> {
        protected override DNAEffect Read(ContentReader input,DNAEffect existing){input.ReadString();int textures=input.ReadInt32();if(textures<0||textures>128)throw new Exception("Invalid material texture count");for(int i=0;i<textures;i++){input.ReadString();input.ReadString();}int parameters=input.ReadInt32();if(parameters<0||parameters>256)throw new Exception("Invalid material parameter count");for(int i=0;i<parameters;i++){input.ReadString();int type=input.ReadByte(),count=input.ReadInt32();if(count<0||count>100000)throw new Exception("Invalid material array size");count=Math.Max(1,count);for(int n=0;n<count;n++){if(type==1)input.ReadString();else if(type==2)input.ReadBoolean();else {int width=type==4?2:type==5?3:type==6?4:type==7?16:1;if(type<0||type>7)throw new Exception("Invalid material parameter type");input.ReadBytes(width*4);}}}return (DNAEffect)FormatterServices.GetUninitializedObject(typeof(DNAEffect));}
    }
    static Model ReadOfflineModel(ContentManager content,string asset){string file=null;foreach(string root in new[]{"","HiDefContent","ReachContent"}){string path=Path.Combine(install,"Content",root,asset+".xnb");if(File.Exists(path)){file=path;break;}}if(file==null)throw new FileNotFoundException(asset);
        using(var stream=File.OpenRead(file)){var create=typeof(ContentReader).GetMethod("Create",fields);using(var input=(ContentReader)create.Invoke(null,new object[]{content,stream,asset,(Action<IDisposable>)(x=>{})})){int shared=(int)typeof(ContentReader).GetMethod("ReadHeader",fields).Invoke(input,null);var readers=(ContentTypeReader[])typeof(ContentReader).GetField("typeReaders",fields).GetValue(input);for(int i=0;i<readers.Length;i++){string name=readers[i].GetType().FullName;if(name=="Microsoft.Xna.Framework.Content.VertexBufferReader")readers[i]=new OfflineVertexReader();else if(name=="Microsoft.Xna.Framework.Content.IndexBufferReader")readers[i]=new OfflineIndexReader();else if(name=="DNA.Drawing.Effects.DNAEffect+Reader")readers[i]=new OfflineEffectReader();}var model=input.ReadObject<Model>();typeof(ContentReader).GetMethod("ReadSharedResources",fields).Invoke(input,new object[]{shared});Console.WriteLine("OFFLINE_ORIGINAL_MODEL "+asset+" buffers="+offlineVertices.Count+" meshes="+model.Meshes.Count);return model;}}
    }
    class OfflineContent:OracleContent {
        public OfflineContent(IServiceProvider services):base(services){}
        public override A Load<A>(string asset){if(typeof(A)==typeof(Model))return (A)(object)ReadOfflineModel(this,asset);return base.Load<A>(asset);}
    }
}
