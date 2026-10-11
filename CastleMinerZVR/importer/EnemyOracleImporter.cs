using System;
using System.IO;
using System.Reflection;
partial class ImportGame {
    class FixedRandom : Random {public float draw;public override double NextDouble(){return draw;}}
    static void EnemyOracle() {
        var original=DNA.MathTools.Rnd;var random=new FixedRandom();DNA.MathTools.Rnd=random;
        try {var method=T("DNA.CastleMinerZ.AI.EnemyType").GetMethod("FindEnemy",BindingFlags.NonPublic|BindingFlags.Static);var type=T("DNA.CastleMinerZ.AI.EnemyTypeEnum");
            using(var w=new BinaryWriter(File.Create(Path.Combine(output,"enemy-selection.oracle")))) {w.Write(0x314f4543);w.Write(4*62*101);
                foreach(var range in new[]{new[]{0,17},new[]{18,25},new[]{26,49},new[]{26,50}})for(int sample=0;sample<62;sample++)for(int draw=0;draw<=100;draw++) {
                    float step=range[0]==0?188.88889f:range[0]==18?425:141.66667f;float distance=sample*step*.5f;random.draw=draw/100f;
                    int selected=Convert.ToInt32(method.Invoke(null,new[]{(object)step,(object)distance,Enum.ToObject(type,range[0]),Enum.ToObject(type,range[1])}));
                    w.Write(range[0]);w.Write(range[1]);w.Write(step);w.Write(distance);w.Write(random.draw);w.Write(selected);
                }
            }Console.WriteLine("ORIGINAL_ENEMY_SELECTION_ORACLE cases=25048");
        } finally {DNA.MathTools.Rnd=original;}
    }
}
