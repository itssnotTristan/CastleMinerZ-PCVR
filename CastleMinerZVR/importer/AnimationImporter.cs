using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DNA.Drawing.Animation;
partial class ImportGame {
    static void MatrixData(BinaryWriter w,Matrix m){w.Write(m.M11);w.Write(m.M12);w.Write(m.M13);w.Write(m.M14);w.Write(m.M21);w.Write(m.M22);w.Write(m.M23);w.Write(m.M24);w.Write(m.M31);w.Write(m.M32);w.Write(m.M33);w.Write(m.M34);w.Write(m.M41);w.Write(m.M42);w.Write(m.M43);w.Write(m.M44);}
    static void SkinnedModel(Model model,string name){var skin=model.Tag as SkinedAnimationData;if(skin==null)return;var skeleton=skin.Skeleton;
        var bind=new Matrix[skeleton.Count];skeleton.CopyTransformsTo(bind);
        using(var w=new BinaryWriter(File.Create(Path.Combine(output,name+".skin")))) {
            w.Write(0x314b5343);w.Write(skeleton.Count);int parts=0;foreach(var mesh in model.Meshes)parts+=mesh.MeshParts.Count;w.Write(parts);int serial=0;
            foreach(var mesh in model.Meshes)foreach(var part in mesh.MeshParts){int stride=part.VertexBuffer.VertexDeclaration.VertexStride;
                byte[] bytes=new byte[part.VertexBuffer.VertexCount*stride];part.VertexBuffer.GetData(bytes);int pos=0,normal=-1,uv=-1,joints=-1,weights=-1;
                foreach(var e in part.VertexBuffer.VertexDeclaration.GetVertexElements()){if(e.VertexElementUsage==VertexElementUsage.Position)pos=e.Offset;if(e.VertexElementUsage==VertexElementUsage.Normal)normal=e.Offset;
                    if(e.VertexElementUsage==VertexElementUsage.TextureCoordinate&&e.UsageIndex==0)uv=e.Offset;if(e.VertexElementUsage==VertexElementUsage.BlendIndices)joints=e.Offset;if(e.VertexElementUsage==VertexElementUsage.BlendWeight)weights=e.Offset;}
                if(joints<0||weights<0)throw new Exception("Skinned mesh lacks bone weights: "+name);w.Write(name+"-"+serial++);w.Write(part.NumVertices);w.Write(part.PrimitiveCount*3);
                for(int i=0;i<part.NumVertices;i++){int o=(i+part.VertexOffset)*stride;for(int c=0;c<3;c++)w.Write(BitConverter.ToSingle(bytes,o+pos+c*4));
                    for(int c=0;c<3;c++)w.Write(normal<0?(c==1?1f:0f):BitConverter.ToSingle(bytes,o+normal+c*4));
                    for(int c=0;c<2;c++)w.Write(uv<0?0f:BitConverter.ToSingle(bytes,o+uv+c*4));for(int c=0;c<4;c++)w.Write(bytes[o+joints+c]);
                    float a=BitConverter.ToSingle(bytes,o+weights),b=BitConverter.ToSingle(bytes,o+weights+4),c0=BitConverter.ToSingle(bytes,o+weights+8);
                    w.Write(a);w.Write(b);w.Write(c0);w.Write(Math.Max(0,1-a-b-c0));}
                if(part.IndexBuffer.IndexElementSize==IndexElementSize.SixteenBits){ushort[] indices=new ushort[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(indices);for(int i=0;i<part.PrimitiveCount*3;i++)w.Write((uint)indices[part.StartIndex+i]);}
                else{uint[] indices=new uint[part.IndexBuffer.IndexCount];part.IndexBuffer.GetData(indices);for(int i=0;i<part.PrimitiveCount*3;i++)w.Write(indices[part.StartIndex+i]);}
            }
            w.Write(skin.AnimationClips.Count);var positions=new Vector3[skeleton.Count];var rotations=new Quaternion[skeleton.Count];var scales=new Vector3[skeleton.Count];var local=new Matrix[skeleton.Count];var absolute=new Matrix[skeleton.Count];
            foreach(var pair in skin.AnimationClips){var clip=pair.Value;const int rate=30;int frames=(int)Math.Ceiling(clip.Duration.TotalSeconds*rate)+1;
                w.Write(pair.Key);w.Write((float)clip.Duration.TotalSeconds);w.Write(rate);w.Write(frames);
                for(int frame=0;frame<frames;frame++){clip.CopyTransforms(positions,rotations,scales,TimeSpan.FromSeconds(Math.Min(clip.Duration.TotalSeconds,(double)frame/rate)),null);
                    for(int bone=0;bone<skeleton.Count;bone++)local[bone]=Matrix.CreateScale(scales[bone])*Matrix.CreateFromQuaternion(rotations[bone])*Matrix.CreateTranslation(positions[bone]);
                    skeleton.CopyTransformsFrom(local);skeleton.CopyAbsoluteBoneTransformsTo(absolute,Matrix.Identity);
                    for(int bone=0;bone<skeleton.Count;bone++)MatrixData(w,skin.InverseBindPose[bone]*absolute[bone]);
                }
            }
            skeleton.CopyTransformsFrom(bind);Console.WriteLine("ANIMATED_MODEL "+name+" bones="+skeleton.Count+" clips="+skin.AnimationClips.Count);
        }
    }
}
