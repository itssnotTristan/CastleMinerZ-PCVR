// Private export of the original SWAT body and PC locomotion clips.
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DNA.Drawing.Animation;
using DNA.Avatars;
partial class ImportGame {
    static void AvatarPresentation(IServiceProvider services,GraphicsDevice device) {
        using(var content=new OfflineContent(services)) {
            var model=content.Load<Model>("Character/SWATMale");var skin=(SkinedAnimationData)model.Tag;var skeleton=skin.Skeleton;
            var bind=new Matrix[skeleton.Count];skeleton.CopyTransformsTo(bind);var bindAbsolute=new Matrix[skeleton.Count];skeleton.CopyAbsoluteBoneTransformsTo(bindAbsolute,Matrix.Identity);
            using(var r=new BinaryWriter(File.Create(Path.Combine(output,"avatar-rig.bin")))) {r.Write(0x31524d43);r.Write(skeleton.Count);foreach(var bone in skeleton){r.Write(bone.Name);r.Write(bone.Parent==null?-1:bone.Parent.Index);MatrixData(r,bindAbsolute[bone.Index]);MatrixData(r,skin.InverseBindPose[bone.Index]);Console.WriteLine("AVATAR_BONE "+bone.Index+" "+bone.Name+" parent="+(bone.Parent==null?-1:bone.Parent.Index));}}
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"player-body.skin")))) {
                w.Write(0x314b5343);w.Write(skeleton.Count);int parts=0;foreach(var mesh in model.Meshes)parts+=mesh.MeshParts.Count;w.Write(parts);
                foreach(var mesh in model.Meshes)foreach(var part in mesh.MeshParts) {
                    int stride=part.VertexBuffer.VertexDeclaration.VertexStride;var bytes=offlineVertices[part.VertexBuffer];int pos=0,normal=-1,uv=-1,joints=-1,weights=-1;
                    foreach(var e in part.VertexBuffer.VertexDeclaration.GetVertexElements()){if(e.VertexElementUsage==VertexElementUsage.Position)pos=e.Offset;if(e.VertexElementUsage==VertexElementUsage.Normal)normal=e.Offset;if(e.VertexElementUsage==VertexElementUsage.TextureCoordinate&&e.UsageIndex==0)uv=e.Offset;if(e.VertexElementUsage==VertexElementUsage.BlendIndices)joints=e.Offset;if(e.VertexElementUsage==VertexElementUsage.BlendWeight)weights=e.Offset;}
                    if(joints<0||weights<0)throw new Exception("Original avatar has no skin weights");w.Write("player-0");w.Write(part.NumVertices);w.Write(part.PrimitiveCount*3);
                    for(int i=0;i<part.NumVertices;i++){int o=(i+part.VertexOffset)*stride;for(int c=0;c<3;c++)w.Write(BitConverter.ToSingle(bytes,o+pos+c*4));for(int c=0;c<3;c++)w.Write(normal<0?(c==1?1f:0f):BitConverter.ToSingle(bytes,o+normal+c*4));for(int c=0;c<2;c++)w.Write(uv<0?0f:BitConverter.ToSingle(bytes,o+uv+c*4));for(int c=0;c<4;c++)w.Write(bytes[o+joints+c]);float sum=0;for(int c=0;c<3;c++){float f=BitConverter.ToSingle(bytes,o+weights+c*4);sum+=f;w.Write(f);}w.Write(Math.Max(0,1-sum));}
                    var indices=new uint[part.PrimitiveCount*3];Array.Copy(offlineIndices[part.IndexBuffer],part.StartIndex,indices,0,indices.Length);foreach(uint index in indices)w.Write(index);
                }
                var clips=new Dictionary<string,AnimationClip>();clips.Add("Idle",content.Load<AnimationClip>("Character/Animation/Stand0"));clips.Add("Walk",content.Load<AnimationClip>("Character/Animation/Walk"));clips.Add("Run",content.Load<AnimationClip>("Character/Animation/Run"));clips.Add("Die",content.Load<AnimationClip>("Character/Animation/Faint"));w.Write(clips.Count);
                foreach(var pair in clips){var clip=pair.Value;int frames=(int)Math.Ceiling(clip.Duration.TotalSeconds*30)+1;w.Write(pair.Key);w.Write((float)clip.Duration.TotalSeconds);w.Write(30);w.Write(frames);var positions=new Vector3[71];var rotations=new Quaternion[71];var scales=new Vector3[71];var local=new Matrix[skeleton.Count];var absolute=new Matrix[skeleton.Count];var root=Matrix.CreateRotationY(MathHelper.Pi);root.Translation=new Vector3(0,.7769f,-.008664f);
                    for(int frame=0;frame<frames;frame++){clip.CopyTransforms(positions,rotations,scales,TimeSpan.FromSeconds(Math.Min(clip.Duration.TotalSeconds,(double)frame/30)),null);for(int b=0;b<skeleton.Count;b++){int index=b==0?0:Avatar.boneNameLookup[skeleton[b].Name];local[b]=Matrix.CreateScale(scales[index])*Matrix.CreateFromQuaternion(rotations[index])*Matrix.CreateTranslation(positions[index]);if(b==0)local[b]=local[b]*root;else {var m=local[b];m.Translation=bind[b].Translation;local[b]=m;}}skeleton.CopyTransformsFrom(local);skeleton.CopyAbsoluteBoneTransformsTo(absolute,Matrix.Identity);for(int b=0;b<skeleton.Count;b++)MatrixData(w,skin.InverseBindPose[b]*absolute[b]);}
                    Console.WriteLine("ORIGINAL_AVATAR_CLIP "+pair.Key+" frames="+frames);
                }
            }
            skeleton.CopyTransformsFrom(bind);Console.WriteLine("ORIGINAL_AVATAR_BODY parts="+model.Meshes.Count+" bones="+skeleton.Count);
        }
    }
}
