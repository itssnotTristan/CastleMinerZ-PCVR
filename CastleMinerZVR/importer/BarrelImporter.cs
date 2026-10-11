// Export the owned weapon's muzzle frame without a desktop graphics device.
using System;using System.IO;using Microsoft.Xna.Framework;using Microsoft.Xna.Framework.Graphics;
partial class ImportGame {
    static void BarrelPresentation(IServiceProvider services) {
        string[][] families={new[]{"Pistol","Conventional/Pistol"},new[]{"Assault","Conventional/AR"},new[]{"BoltRifle","Conventional/Rifle"},new[]{"PumpShotgun","Conventional/Shotgun"},new[]{"SMG","Conventional/SMG"},new[]{"LMG","Conventional/LMG"},new[]{"SpacePistol","Space/Pistol"},new[]{"SpaceAssault","Space/AR"},new[]{"SpaceBoltRifle","Space/Rifle"},new[]{"SpacePumpShotgun","Space/Shotgun"},new[]{"SpaceSMG","Space/SMG"},new[]{"RPG","Conventional/RPG"}};
        using(var content=new OfflineContent(services))using(var w=new BinaryWriter(File.Create(Path.Combine(output,"barrel-frames.bin")))) {
            var list=new System.Collections.Generic.List<string[]>(families);list.Add(new[]{"Launcher","Conventional/Launcher"});list.Add(new[]{"Drill","Tools/Drill"});w.Write(0x32424d56);w.Write(list.Count);
            foreach(var family in list){var model=content.Load<Model>((family[1].StartsWith("Tools/")?"Props/":"Props/Weapons/")+family[1]+"/Model");var matrices=new Matrix[model.Bones.Count];model.CopyAbsoluteBoneTransformsTo(matrices);ModelBone tip=null;
                foreach(var bone in model.Bones)if(bone.Name=="BarrelTip")tip=bone;if(tip==null){foreach(var bone in model.Bones)Console.WriteLine("NOZZLE_BONE "+family[0]+" "+bone.Name+" position="+matrices[bone.Index].Translation);throw new Exception("Missing original BarrelTip: "+family[0]);}
                var frame=matrices[tip.Index];Vector3 center=Vector3.Zero;int vertices=0;
                foreach(var mesh in model.Meshes){if(mesh.Name.ToLowerInvariant().Contains("gem")||mesh.Name.ToLowerInvariant().Contains("grenade"))continue;foreach(var part in mesh.MeshParts){int stride=part.VertexBuffer.VertexDeclaration.VertexStride,pos=0;foreach(var e in part.VertexBuffer.VertexDeclaration.GetVertexElements())if(e.VertexElementUsage==VertexElementUsage.Position)pos=e.Offset;var bytes=offlineVertices[part.VertexBuffer];var transform=matrices[mesh.ParentBone.Index];for(int i=0;i<part.NumVertices;i++){int o=(i+part.VertexOffset)*stride+pos;var point=new Vector3(BitConverter.ToSingle(bytes,o),BitConverter.ToSingle(bytes,o+4),BitConverter.ToSingle(bytes,o+8));if(Math.Abs(transform.Determinant())>.000001f)point=Vector3.Transform(point,transform);center+=point;vertices++;}}}
                if(vertices==0)throw new Exception("Missing original barrel geometry");center/=vertices;var outward=Vector3.Normalize(frame.Translation-center);Vector3 direction=Vector3.Zero;float best=-1;
                // Markers are not uniformly oriented: the grenade launcher uses
                // +Y where the other weapons use +Z. Choose the marker axis
                // pointing outward from its own baked receiver geometry.
                foreach(var axis in new[]{frame.Right,frame.Up,frame.Backward})foreach(float sign in new[]{-1f,1f}){var candidate=Vector3.Normalize(axis)*sign;float score=Vector3.Dot(candidate,outward);if(score>best){best=score;direction=candidate;}}
                if(best<.6f)throw new Exception("Ambiguous original barrel axis: "+family[0]);w.Write(family[0]);MatrixData(w,frame);w.Write(direction.X);w.Write(direction.Y);w.Write(direction.Z);Console.WriteLine("BARREL_GEOMETRY_AXIS "+family[0]+" outward="+direction+" agreement="+best);
                Console.WriteLine("BARREL_FRAME "+family[0]+" tip="+frame.Translation+" forward="+frame.Forward+" right="+frame.Right+" up="+frame.Up);
                foreach(var mesh in model.Meshes)Console.WriteLine("BARREL_MESH "+family[0]+" "+mesh.Name+" parent="+mesh.ParentBone.Name);
            }
        }
    }
}
