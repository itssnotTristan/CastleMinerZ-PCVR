// Magazine surfaces are derived locally from the owner's original gun mesh.
// V2 adds only closure faces at openings hidden inside the assembled weapon.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
static class MagazineGeometry {
    struct Point { public Vector3 p; public float u,v; }
    class Edge { public int a,b,count; public Edge(int x,int y){a=x;b=y;count=1;} }
    static string Key(Vector3 p){return string.Join(",",new[]{p.X,p.Y,p.Z}.Select(v=>Math.Round(v,5).ToString("F5",CultureInfo.InvariantCulture)).ToArray());}
    static int Root(int[] roots,int i){while(roots[i]!=i){roots[i]=roots[roots[i]];i=roots[i];}return i;}
    static void Join(int[] roots,int a,int b){roots[Root(roots,a)]=Root(roots,b);}
    static string EdgeKey(int a,int b){return Math.Min(a,b)+":"+Math.Max(a,b);}
    static bool AkSurface(Vector3 p){return p.X>-.30f&&p.X<-.140f&&p.Z<=.1f*p.X+.0615f+.02f*(p.Y-.13f)+.00002f;}
    static byte[] Select(Point[] points,int[] tri,string family,string asset){
        int[] weld=new int[points.Length];var keys=new Dictionary<string,int>();
        for(int i=0;i<points.Length;i++){string key=Key(points[i].p);int id;if(!keys.TryGetValue(key,out id)){id=keys.Count;keys.Add(key,id);}weld[i]=id;}
        int[] roots=Enumerable.Range(0,keys.Count).ToArray();bool ak=family=="Conventional/AR";
        var allowed=new bool[tri.Length/3];
        for(int t=0;t<allowed.Length;t++){allowed[t]=!ak||(AkSurface(points[tri[t*3]].p)&&AkSurface(points[tri[t*3+1]].p)&&AkSurface(points[tri[t*3+2]].p));if(allowed[t]){Join(roots,weld[tri[t*3]],weld[tri[t*3+1]]);Join(roots,weld[tri[t*3+1]],weld[tri[t*3+2]]);}}
        if(ak){var triangleRoots=Enumerable.Range(0,allowed.Length).ToArray();var edges=new Dictionary<string,int>();for(int t=0;t<allowed.Length;t++)if(allowed[t])for(int j=0;j<3;j++){string key=EdgeKey(weld[tri[t*3+j]],weld[tri[t*3+(j+1)%3]]);int other;if(edges.TryGetValue(key,out other))Join(triangleRoots,t,other);else edges.Add(key,t);}var selected=new HashSet<int>();for(int t=0;t<allowed.Length;t++)if(allowed[t]&&Enumerable.Range(0,3).Any(j=>points[tri[t*3+j]].p.Z<-.12f))selected.Add(Root(triangleRoots,t));return Enumerable.Range(0,allowed.Length).Select(t=>(byte)(allowed[t]&&selected.Contains(Root(triangleRoots,t))?1:0)).ToArray();}
        var groups=new Dictionary<int,List<int>>();for(int t=0;t<allowed.Length;t++)if(allowed[t]){int root=Root(roots,weld[tri[t*3]]);if(!groups.ContainsKey(root))groups[root]=new List<int>();groups[root].Add(t);}
        var mask=new byte[allowed.Length];foreach(var group in groups.Values){Vector3 lo=new Vector3(float.MaxValue),hi=new Vector3(float.MinValue);foreach(int t in group)for(int j=0;j<3;j++){var p=points[tri[t*3+j]].p;lo=Vector3.Min(lo,p);hi=Vector3.Max(hi,p);}bool select=false;
            if(ak)select=lo.Z<-.12f;
            else if(family=="Conventional/Pistol")select=lo.Z<-.065f&&hi.Z<.08f;
            else if(family=="Conventional/SMG")select=lo.Z<-.15f;
            else if(family=="Conventional/LMG")select=lo.Z<-.11f&&hi.Z<.065f;
            else if(family=="Space/Pistol"||family=="Space/SMG")select=hi.Z<-.03f;
            else if(family=="Space/AR"||asset=="Props/Tools/Drill/Model")select=lo.Z<-.06f&&hi.Z<.07f;
            else if(family=="Space/Rifle")select=lo.Z<-.04f&&hi.Z<.04f;
            if(select)foreach(int t in group)mask[t]=1;
        }return mask;
    }
    static float Cross(Vector2 a,Vector2 b){return a.X*b.Y-a.Y*b.X;}
    static Vector2 Project(Vector3 p,int axis){return axis==0?new Vector2(p.Y,p.Z):axis==1?new Vector2(p.X,p.Z):new Vector2(p.X,p.Y);}
    static List<Point> Close(Point[] points,int[] tri,byte[] mask,bool curved){
        var vertices=new List<Point>();var keys=new Dictionary<string,int>();var weld=new int[points.Length];
        for(int i=0;i<points.Length;i++){string key=Key(points[i].p);int id;if(!keys.TryGetValue(key,out id)){id=vertices.Count;vertices.Add(points[i]);keys.Add(key,id);}weld[i]=id;}
        var edges=new Dictionary<string,Edge>();for(int t=0;t<mask.Length;t++)if(mask[t]!=0)for(int j=0;j<3;j++){int a=weld[tri[t*3+j]],b=weld[tri[t*3+(j+1)%3]];if(a==b)continue;string key=EdgeKey(a,b);Edge e;if(edges.TryGetValue(key,out e))e.count++;else edges[key]=new Edge(a,b);}
        if(edges.Values.Any(e=>e.count>2))throw new Exception("Nonmanifold original magazine");
        var neighbors=new Dictionary<int,List<int>>();var boundary=new HashSet<string>();foreach(var edge in edges.Values.Where(e=>e.count==1)){foreach(int p in new[]{edge.a,edge.b})if(!neighbors.ContainsKey(p))neighbors[p]=new List<int>();neighbors[edge.a].Add(edge.b);neighbors[edge.b].Add(edge.a);boundary.Add(EdgeKey(edge.a,edge.b));}
        if(neighbors.Values.Any(n=>n.Count!=2))throw new Exception("Magazine boundary branches: "+string.Join(";",neighbors.Where(n=>n.Value.Count!=2).Select(n=>Key(vertices[n.Key].p)+" degree="+n.Value.Count).ToArray()));
        var caps=new List<Point>();while(boundary.Count>0){var initial=edges[boundary.First()];int first=initial.b,at=first,previous=-1;var loop=new List<int>();do{loop.Add(at);int to=neighbors[at].First(p=>p!=previous&&boundary.Contains(EdgeKey(at,p)));boundary.Remove(EdgeKey(at,to));previous=at;at=to;if(loop.Count>vertices.Count)throw new Exception("Invalid magazine boundary");}while(at!=first);
            int orientation=0;for(int i=0;i<loop.Count;i++){int a=loop[i],b=loop[(i+1)%loop.Count];orientation+=edges[EdgeKey(a,b)].a==a?1:-1;}if(orientation>0)loop.Reverse();
            Vector3 normal=Vector3.Zero;for(int i=0;i<loop.Count;i++)normal+=Vector3.Cross(vertices[loop[i]].p,vertices[loop[(i+1)%loop.Count]].p);
            int axis=Math.Abs(normal.X)>Math.Abs(normal.Y)?0:1;if(Math.Abs(normal.Z)>Math.Abs(axis==0?normal.X:normal.Y))axis=2;
            float area=0;for(int i=0;i<loop.Count;i++)area+=Cross(Project(vertices[loop[i]].p,axis),Project(vertices[loop[(i+1)%loop.Count]].p,axis));if(Math.Abs(area)<1e-10f)throw new Exception("Degenerate magazine closure");float sign=Math.Sign(area);
            // The AK opening is bent. A centre fan avoids coinciding with its
            // existing folded panel edges. Verify the centre lies in its kernel.
            if(curved){Point centre=new Point();foreach(int p in loop){centre.p+=vertices[p].p;centre.u+=vertices[p].u;centre.v+=vertices[p].v;}centre.p/=loop.Count;centre.u/=loop.Count;centre.v/=loop.Count;var pc=Project(centre.p,axis);
                if(Enumerable.Range(0,loop.Count).Any(i=>Cross(Project(vertices[loop[(i+1)%loop.Count]].p,axis)-Project(vertices[loop[i]].p,axis),pc-Project(vertices[loop[i]].p,axis))*sign<=0))throw new Exception("Curved magazine closure centre leaves opening");
                for(int i=0;i<loop.Count;i++){caps.Add(vertices[loop[i]]);caps.Add(vertices[loop[(i+1)%loop.Count]]);caps.Add(centre);}continue;}

            while(loop.Count>3){bool cut=false;for(int i=0;i<loop.Count;i++){int a=loop[(i+loop.Count-1)%loop.Count],b=loop[i],c=loop[(i+1)%loop.Count];Edge originalDiagonal;if(edges.TryGetValue(EdgeKey(a,c),out originalDiagonal)&&originalDiagonal.count==2)continue;Vector2 pa=Project(vertices[a].p,axis),pb=Project(vertices[b].p,axis),pc=Project(vertices[c].p,axis);if(Cross(pb-pa,pc-pb)*sign<=1e-12f)continue;bool inside=false;foreach(int p in loop){if(p==a||p==b||p==c)continue;var q=Project(vertices[p].p,axis);if(Cross(pb-pa,q-pa)*sign>=-1e-12f&&Cross(pc-pb,q-pb)*sign>=-1e-12f&&Cross(pa-pc,q-pc)*sign>=-1e-12f){inside=true;break;}}if(inside)continue;caps.Add(vertices[a]);caps.Add(vertices[b]);caps.Add(vertices[c]);loop.RemoveAt(i);cut=true;break;}if(!cut)throw new Exception("Cannot triangulate original magazine opening");}
            foreach(int p in loop)caps.Add(vertices[p]);
        }return caps;
    }
    public static void Export(string source,string target,string family,string asset){
        var masks=new List<byte[]>();var caps=new List<List<Point>>();int total=0,closed=0;
        using(var r=new BinaryReader(File.OpenRead(source))){if(r.ReadInt32()!=0x314d4d43)throw new Exception("Invalid converted gun");int parts=r.ReadInt32();for(int part=0;part<parts;part++){r.ReadString();int count=r.ReadInt32(),indices=r.ReadInt32();var points=new Point[count];for(int i=0;i<count;i++)points[i]=new Point{p=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle()),u=r.ReadSingle(),v=r.ReadSingle()};int[] triangles=new int[indices];for(int i=0;i<indices;i++)triangles[i]=r.ReadInt32();byte[] mask=part==0?Select(points,triangles,family,asset):new byte[indices/3];var cap=Close(points,triangles,mask,family=="Conventional/AR");total+=mask.Count(v=>v!=0);closed+=cap.Count/3;masks.Add(mask);caps.Add(cap);}}
        if(total<10)throw new Exception("Original magazine selector found no geometry: "+asset);
        using(var w=new BinaryWriter(File.Create(target))){w.Write(0x324d4756);w.Write(masks.Count);for(int part=0;part<masks.Count;part++){w.Write(masks[part].Length);w.Write(masks[part]);w.Write(caps[part].Count);foreach(var p in caps[part]){w.Write(p.p.X);w.Write(p.p.Y);w.Write(p.p.Z);w.Write(p.u);w.Write(p.v);}}}
        Console.WriteLine("MAGAZINE_GEOMETRY asset="+asset+" triangles="+total+" closure_triangles="+closed);
    }
}
