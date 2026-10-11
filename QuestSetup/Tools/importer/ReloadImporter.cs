// Original reload wrist motion kept separate from controller-normalized clips.
using System;using System.IO;using Microsoft.Xna.Framework;using Microsoft.Xna.Framework.Graphics;
using DNA.Drawing.Animation;using DNA.Avatars;using DNA.Net.GamerServices;
partial class ImportGame {
    static void ReloadPresentation(IServiceProvider services) {
        string[][] families={new[]{"Pistol","Conventional/Pistol"},new[]{"Assault","Conventional/AR"},new[]{"BoltRifle","Conventional/Rifle"},new[]{"PumpShotgun","Conventional/Shotgun"},new[]{"SMG","Conventional/SMG"},new[]{"LMG","Conventional/LMG"},new[]{"SpacePistol","Space/Pistol"},new[]{"SpaceAssault","Space/AR"},new[]{"SpaceBoltRifle","Space/Rifle"},new[]{"SpacePumpShotgun","Space/Shotgun"},new[]{"SpaceSMG","Space/SMG"},new[]{"RPG","Conventional/RPG"}};
        using(var content=new OfflineContent(services))using(var w=new BinaryWriter(File.Create(Path.Combine(output,"reload-motion.bin")))) {
            var model=content.Load<Model>("Character/SWATMale");var skin=(SkinedAnimationData)model.Tag;var skeleton=skin.Skeleton;
            var bind=new Matrix[skeleton.Count];skeleton.CopyTransformsTo(bind);int wrist=skeleton.IndexOf("RT_W__Skeleton");
            var positions=new Vector3[71];var rotations=new Quaternion[71];var scales=new Vector3[71];var absolute=new Matrix[skeleton.Count];var avatar=new Matrix[71];var local=new Matrix[skeleton.Count];
            var root=Matrix.CreateRotationY(MathHelper.Pi);root.Translation=new Vector3(0,.7769f,-.008664f);
            Action<AnimationClip,double> sample=(clip,time)=>{
                clip.CopyTransforms(positions,rotations,scales,TimeSpan.FromSeconds(time),null);
                var source=new Matrix[71];for(int b=0;b<71;b++)source[b]=Matrix.CreateScale(scales[b])*Matrix.CreateFromQuaternion(rotations[b])*Matrix.CreateTranslation(positions[b]);
                avatar[0]=source[0]*Avatar.DefaultBindPose[0];for(int b=1;b<71;b++){var m=source[b];m.Translation=Avatar.DefaultBindPose[b].Translation;avatar[b]=m*avatar[Avatar.DefaultParentBones[b]];}
                for(int b=0;b<skeleton.Count;b++){int index=b==0?0:Avatar.boneNameLookup[skeleton[b].Name];local[b]=source[index];if(b==0)local[b]=local[b]*root;else {var m=local[b];m.Translation=bind[b].Translation;local[b]=m;}}
                skeleton.CopyTransformsFrom(local);skeleton.CopyAbsoluteBoneTransformsTo(absolute,Matrix.Identity);
            };
            // The original RPG has no Reload clip; PC completes its timer directly.
            w.Write(0x31524d56);w.Write(families.Length-1);
            foreach(var family in families){if(family[0]=="RPG"){Console.WriteLine("ORIGINAL_RELOAD_ABSENT RPG");continue;}string idlePath=family[0]=="SpacePumpShotgun"?"Space/AR":family[1];var idle=content.Load<AnimationClip>("Props/Weapons/"+idlePath+"/Animation/Idle");var reload=content.Load<AnimationClip>("Props/Weapons/"+family[1]+"/Animation/Reload");
                sample(idle,0);var referenceWrist=absolute[wrist];var referenceProp=avatar[(int)AvatarBone.PropRight];var prefix=referenceProp*Matrix.Invert(referenceWrist);var inverseProp=Matrix.Invert(referenceProp);
                int frames=(int)Math.Ceiling(reload.Duration.TotalSeconds*30)+1;w.Write(family[0]);w.Write((float)reload.Duration.TotalSeconds);w.Write(30);w.Write(frames);float distance=0;
                for(int frame=0;frame<frames;frame++){sample(reload,Math.Min(reload.Duration.TotalSeconds,(double)frame/30));var delta=prefix*absolute[wrist]*inverseProp;MatrixData(w,delta);distance=Math.Max(distance,delta.Translation.Length());}
                Console.WriteLine("ORIGINAL_RELOAD_PATH "+family[0]+" frames="+frames+" seconds="+reload.Duration.TotalSeconds+" max_translation="+distance);
            }
            skeleton.CopyTransformsFrom(bind);
        }
    }
}
