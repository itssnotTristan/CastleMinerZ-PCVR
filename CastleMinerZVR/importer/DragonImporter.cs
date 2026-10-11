// Locally export the owned executable's initialized dragon definitions and original assets.
using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DNA.Drawing.Animation;
partial class ImportGame {
    static void Dragons(IServiceProvider services,GraphicsDevice device){using(var content=new OracleContent(services)){
        var instance=(Game)FormatterServices.GetUninitializedObject(T("DNA.CastleMinerZ.CastleMinerZGame"));instance.Content=content;T("DNA.CastleMinerZ.CastleMinerZGame").GetField("Instance",fields).SetValue(null,instance);
        T("DNA.CastleMinerZ.AI.DragonType").GetMethod("Init",fields).Invoke(null,null);
        var types=(Array)T("DNA.CastleMinerZ.AI.DragonType").GetField("Types",fields).GetValue(null);var breaks=(bool[,])T("DNA.CastleMinerZ.AI.DragonType").GetField("BreakLookup",fields).GetValue(null);
        var body=content.Load<Model>("Enemies/Dragon/DragonBodyHigh");var feet=content.Load<Model>("Enemies/Dragon/DragonFeetHigh");SkinnedModel(body,"dragon-body");SkinnedModel(feet,"dragon-feet");
        var skin=(SkinedAnimationData)body.Tag;int mouth=skin.Skeleton["Bip01 Ponytail1"].Index;
        // Skin matrices contain inverse-bind * animated-absolute. Undo the actual
        // inverse bind to locate the socket in mesh coordinates, including units.
        var mouthPoint=Matrix.Invert(skin.InverseBindPose[mouth]).Translation;
        using(var writer=new BinaryWriter(File.Create(Path.Combine(output,"dragons.bin")))){writer.Write(0x31444d43);writer.Write(types.Length);writer.Write(mouth);writer.Write(mouthPoint.X);writer.Write(mouthPoint.Y);writer.Write(mouthPoint.Z);
            foreach(var type in types){writer.Write(I(type,"EType"));writer.Write((string)type.GetType().GetMethod("GetDragonName").Invoke(type,null));
                foreach(var name in new[]{"StartingHealth","Speed","MaxAccel","YawRate","RollRate","MaxRoll","PitchRate","MaxPitch","MaxViewDistance","HoverDistance","MaxAttackDistance","MinAttackDistance","BreakOffStrafeDistance","SpawnDistance","CruisingAltitude","LoiterAltitude","HuntingAltitude","StrafeFireRate","HoverFireRate","MinLoiterTime","MaxLoiterTime","LoiterDistance","ChanceOfHoverAttack","FireballVelocity","FireballDamage"})writer.Write(F(type,name));
                writer.Write(I(type,"MinHoverShots"));writer.Write(I(type,"MaxHoverShots"));writer.Write(I(type,"MaxLoiters"));writer.Write(I(type,"DamageType"));for(int block=0;block<95;block++)writer.Write(breaks[I(type,"EType"),block]);Texture(device,(Texture2D)Value(type,"Texture"),"dragon-texture-"+I(type,"EType"));
                Console.WriteLine("ORIGINAL_DRAGON type="+I(type,"EType")+" health="+F(type,"StartingHealth")+" speed="+F(type,"Speed")+" fireball="+F(type,"FireballVelocity"));}}
        Mesh(device,content.Load<Model>("Props/Projectiles/FireBall/FireBall"),"dragon-fireball");Mesh(device,content.Load<Model>("Props/Projectiles/SnowBall/Snowball"),"dragon-iceball");
        Console.WriteLine("DRAGON_IMPORT_COMPLETE mouth_bone="+mouth+" bind="+mouthPoint);
    }}
}
