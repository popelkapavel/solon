using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;


namespace solon {
    public enum H {quad,hexa,tria,tria2,tria4,penta,cubes,delta,trap,deca};
    public enum D {play,free,line,rect,circ,circ2,fill,edge,color,color2};
    public enum Game {Sol,OnOff,Shift};

    public class ME {
      public float x,y;
      public bool shiftKey,ctrlKey;
      public int buttons;
      public ME(float x,float y,int b,bool sh,bool ct) {
        this.x=x;this.y=y;shiftKey=sh;ctrlKey=ct;buttons=b;
      }
    }
    public class U {
      public int[] ia,xy;
      public int t;
      public U(int typ) { t=typ;}
      public U(int typ,params int[] ia) {
        t=typ;this.ia=ia;
      }
      public static U Sol(params int[] ia) {
        var u=new U(0);
        u.ia=ia;
        return u;        
      }
      public static U OnOff(int[] xy,params int[] ia) {
        var u=new U(1);
        u.ia=ia;u.xy=xy;
        return u;        
      }
      public static U Shifter(int x,int y,int b,bool opt,int i) {
        var u=new U(2);
        u.ia=new int[] {x,y,b,pmap.b2i(opt),i};
        return u;
      }
    }
    public class V {
      public bool rou,peg,white,back;
      public string whiter="eeee";
      public int mono=2,grdm,grdm2=2,corn;
      public float grdx=15,grdy=16;
      public D design;
      public List<int> sele=new List<int>();
      public int dm9c,dm10c,onoff6;
      public fMain f;
      public List<U> undo=new List<U>();
      public int redo;
      public float[] mm,mp;
      public int[] mm1;
      public ME mm1e;
      public int moves;      
    }

    public struct C {
      public int ch; //x empty,o black,w white
      public int block,fore,back;
      public override string ToString() {
        return ch==1?"x":ch==2?"o":ch==3?"w":ch==0?"":""+ch;
      }
  }
    public partial class pmap {
      static float Cell;
      static float cell2,cell5,cell7,brd;
      static float cell6x,cell6y;
      static float cell8y,cell8x;
      static float cell3x,cell3y;
      static float cell9x,cell9y;

      public int Width,Height;
      public C[] Data; 
      public H H=H.quad; // 0 quad,1 hexa,2 tria,3 tria4,5 penta,6 cubes,7 delta,8 trap
      public Game Game;
      public string Whiter;
      public bool Diag,oox,oo3,ooc,ooo;
      public int White,Color;
      public V View=new V(); 

      public override string ToString() {
        return ""+Width+"x"+Height;
      }

      public pmap(fMain f) {
        View.f=f;
        Sizes(1);
      }
      public static void Sizes(float k) {
        Cell=32*k;
        cell2=36*k;cell5=48*k;cell7=48*k;brd=-8;
        cell6x=40*k;cell6y=(float)Math.Round(cell6x*Math.Sqrt(3)/2);
        cell8y=60*k;cell8x=(float)Math.Round(cell8y*Math.Sqrt(3)/2);
        cell3x=48*k;cell3y=(float)Math.Round(cell3x*Math.Sqrt(3)/2);
        cell9x=64*k;cell9y=(float)Math.Round(cell9x*Math.Sqrt(3)/2);
      }

      public pmap(int width,int height) {Alloc(width,height);}
      public pmap(pmap src) {Copy(src);}
      public pmap Clone() { return new pmap(this);}
      public void Copy(pmap src) {
        Data=src.Data.Clone() as C[];
        Width=src.Width;Height=src.Height;
        View=src.View;
      }
      public void Alloc(int width,int height) {
        Data=new C[width*height];
        Width=width;Height=height;
      }
      public void _cmd(string s) {
        s=Regex.Replace(s,@"^\s*//","");        
        foreach(var xx in s.Split(',')) {
          string x=xx.Trim();
          if(x==""||x[0]=='@') continue;
          else if(Regex.IsMatch(x,@"quad|hexa|tria|penta|cubes|delta|trap|deca")) 
            H=(H)(x=="hexa"?1:x=="tria"?2:x=="tria2"?3:x=="tria4"?4:x=="penta"?5:x=="cubes"?6:x=="delta"?7:x=="trap"?8:x=="deca"?9:0);
           else if(Regex.IsMatch(x,@"sol|shift|onoff")) {             
             if(x=="onoff") oox=oo3=ooc=ooo=B0;
             if(x=="onoffx") oox=B1;
             if(x=="onoffo") ooo=B1;
             if(x=="onoff3") oo3=B1;
             if(x=="onoffc") ooc=B1;
             Game=x=="sol"?Game.Sol:x=="shift"?Game.Shift:Game.OnOff;
           } else if(Regex.IsMatch(x,@"^white_")) Whiter=x.Substring(6);
           else if(Regex.IsMatch(x,@"(no)diag")) Diag=x=="diag";
           else if(Regex.IsMatch(x,@"col\d+$")) {
             int c=int.Parse(x.Substring(3)),c2;
             int cc=c==2?1:c==23?2:c==3?3:c==4?4:0;
             //SetColor(cc);
           }
        }
        View.f.UpdateControls();
      }
      public void Parse(string txt) {
        var cmd=Regex.Match(txt,@"^\s*//.*");
        if(cmd.Success) _cmd(cmd.Value);
        txt=Regex.Replace(txt,@"//.*(\n|$)","");
        txt=Regex.Replace(txt,@"([ \t]+)($|\r?\n)","$2");
        string[] ln=Regex.Split(txt,@"\r?\n");
        int x,y,m=0,l,e,i;
        string b;char c;
        for(y=0;y<ln.Length;y++) {
          if((l=ln[y].Length)>m) m=l;
        }
        Alloc(m+2,ln.Length+2);
        for(y=0;y<ln.Length;y++) {
          for(x=0,b=ln[y];x<b.Length;x++) {            
            c=b[x];
            c=_getblock1(c,out e);
            Data[i=Index(x+1,y+1)].ch=c=='X'||c=='x'||c=='.'||c=='+'?1:c=='O'||c=='o'||c=='0'?2:c=='W'||c=='w'||c=='A'?3:0;
            if(e!=0) {e=_txt2block(x+1,y+1,e,B0);Data[i].block=e;}
          }
        }
        _dublocka();
      }
      public bool _white() {
        for(int i=0;i<Data.Length;i++) if(Data[i].ch==3) return true;
        return false;
      }
      public string _game2txt(string name,string txt) {        
        var sa=new List<string>();int i,c,e,r,x,y,xi=-1,yi=-1,ya=0,n;bool wh=Whiter!="xxff"||_white();string hdr,l,s;
        hdr="//"+(name+""==""?"":"@"+name)+','+(H==H.deca?"deca":H==H.trap?"trap":H==H.delta?"delta":H==H.cubes?"cubes":H==H.penta?"penta":H==H.tria4?"tria4":H==H.tria2?"tria2":H==H.tria?"tria":H==H.hexa?"hexa":"quad")
          +(Game==Game.Shift?",shift":Game==Game.OnOff?",onoff"+(ooc?",onoffc":oo3?",onoff3":"")+(oox?",onoffx":"")+(ooo?",onoffo":""):",sol")+(wh?",white_"+Whiter:"")+(Diag?",diag":"");
        for(y=0;y<Height;y++) {
          l="";
          for(x=0;x<Width;x++) {
            c=Data[i=Index(x,y)].ch;
            e=Data[i].block;
            if(e!=0) {
              e=_txt2block(x,y,e,B1);
              c=i2b(e)?_setblock1(c,e):c==1?'.':c==2?'O':c==3?'W':' ';
            } else
              c=c==1?'.':c==2?'O':c==3?'W':' ';
            l+=(char)c;
            if(c!=' ') {
              if(xi<0) {xi=x;ya=yi=y;}
              else {
                if(x<xi) xi=x;
                ya=y;
              }
            }
         }
         sa.Add(l);
       }
       int sy=H==H.hexa||H==H.tria||H==H.tria4||H==H.penta?2:H==H.tria2?4:1,sx=H==H.penta?4:H==H.hexa?1:sy;
       if(yi>0) { yi=1+(((yi-1)/sy)|0)*sy;sa.RemoveRange(0,yi);ya-=yi;}
       if(ya+1<sa.Count) sa.RemoveRange(ya+1,sa.Count-ya-1);
       if(xi>0) { xi=1+(((xi-1)/sx)|0)*sx;for(y=0;y<sa.Count;y++) sa[y]=sa[y].Substring(xi); }
       s="";foreach(string si in sa) s+=(s==""?"":"\r\n")+si.TrimEnd();       
       return hdr+"\n"+s;
      }

      /*
       *       ce=_getblock1(c),c=ce[0],e=ce[1];
      b[1+x]=c=='X'||c=='x'||c=='.'||c=='+'?'x'
         :c=='O'||c=='o'||c=='0'?'o':c=='W'||c=='w'||c=='A'?'w':' ';
      if(e) e=_txt2block(1+x,1+y,e,0),_setblock(1+x,1+y,e);

      */
      public int Index(int x,int y) { return Width*y+x;}
      public bool _xo(int x,int y) { return Data[Width*y+x].ch>0;}
      public bool _xo2(int x,int y) { return In(x,y)&&Data[Width*y+x].ch>0;}
      public int Ch(int x,int y) { return x>=0&&x<Width&&y>=0&&y<Height?Data[Width*y+x].ch:-1;}
      public bool In(int x,int y) { return x>=0&&x<Width&&y>=0&&y<Height;}
      public int _getblock(int x,int y) { return In(x,y)?Data[Width*y+x].block:0;}
      public void _setblock(int x,int y,int e) { if(In(x,y)) Data[Width*y+x].block|=e;}
      public void _rstblock(int x,int y,int e) { if(In(x,y)) Data[Width*y+x].block&=~e;}
      public int _getuco(int x,int y) { return In(x,y)?Data[Width*y+x].back:0;}
      public void _setuco(int x,int y,int c) { if(In(x,y)) Data[Width*y+x].back=c;}
      public int _getpgc(int x,int y) { return In(x,y)?Data[Width*y+x].fore:0;}
      public void _setpgc(int x,int y,int c) { if(In(x,y)) Data[Width*y+x].fore=c;}
      public void ParseFile(string name) {
        using(var r=new StreamReader(name)) {
          string s=r.ReadToEnd();
          Parse(s);
        }
      }
      public void Save(string s) {}
      public void ExportPng(string s) {
        using(Bitmap bm=new Bitmap(BWidth(),BHeight())) {
         using(Graphics gr=Graphics.FromImage(bm)) {
          if(View.back) pmap.Back(gr,bm.Width,bm.Height);
          gr.SmoothingMode=SmoothingMode.HighQuality;
          Draw(gr);          
         }
         if(s+""=="") System.Windows.Forms.Clipboard.SetImage(bm);
         else bm.Save(s,ImageFormat.Png);
        }
      }
      public void Clear() {}
      public void Extent(int x,int y) {}
      public void Rotate90(bool counter) {}
      public void Invert() {
        for(int i=0;i<Data.Length;i++) Invert(ref Data[i].ch);
      }
      public static void Invert(ref int ch) {
        if(ch==3) ch=2;
        else if(ch==2) ch=3;
      }
      public bool Bounding(ref int x0,ref int y0,ref int x1,ref int y1) { return false;}
      public bool Copy(int dx,int dy,pmap src,int x0,int y0,int x1,int y1) {
        if(!src.Intersected(ref x0,ref y0,ref x1,ref y1,ref dx,ref dy,Width,Height)) return false;        
        for(;y0<=y1;y0++,dy++) {
          int g=(Width*dy+dx)*4,h=(src.Width*y0+x0)*4;
          Array.Copy(src.Data,h,Data,g,4*(x1-x0+1));
        }
        return true;
      }
      public static void Sort(ref int x0,ref int x1) {
        if(x0<=x1) return;
        int x;
        x=x0;x0=x1;x1=x;
      }
      public static void Sort(ref int x0,ref int y0,ref int x1,ref int y1) { Sort(ref x0,ref x1);Sort(ref y0,ref y1);}
      public static void MinMax(ref int x0,ref int x1,int x2,int x3) {
        int x;
        if(x0>x2) {x=x0;x0=x2;x2=x;}
        if(x3>x1) {x=x1;x1=x3;x3=x;}
        if(x0>x3) x0=x3;
        if(x2>x1) x1=x2;
      }
      public static void MinMax(ref int x0,ref int y0,ref int x1,ref int y1,int x2,int y2,int x3,int y3) { 
        MinMax(ref x0,ref x1,x2,x3);MinMax(ref y0,ref y1,y2,y3);
      }
      public bool Intersected(ref int x0,ref int y0,ref int x1,ref int y1) {
        Sort(ref x0,ref x1);Sort(ref y0,ref y1);
        if(x1<0||x0>=Width||y1<0||y0>=Height) return false;
        if(x0<0) x0=0;if(x1>=Width) x1=Width-1;
        if(y0<0) y0=0;if(y1>=Height) y1=Height-1;
        return true;
      }
      public bool Intersected(ref int x0,ref int y0,ref int x1,ref int y1,ref int sx,ref int sy,int width,int height) {
        Sort(ref x0,ref x1);Sort(ref y0,ref y1);
        if(x1<0||x0-(sx<0?sx:0)>=Width||y1<0||y0-(sy<0?sy:0)>=Height) return false;
        if(x0<0) {sx-=x0;x0=0;} if(x1>=Width) x1=Width-1;
        if(y0<0) {sy-=y0;y0=0;} if(y1>=Height) y1=Height-1;
        if(sx<0) {x0-=sx;sx=0;}
        if(sy<0) {y0-=sy;sy=0;}
        int i;
        if((i=sx+x1-x0-width+1)>0) x1-=i;
        if((i=sy+y1-y0-height+1)>0) y1-=i;
        if(y1<y0||x1<x0) return false;
        return true;
      }
      static int[] clr2=new int[] {0xffeebb,0xddbb99,0xaa8866};
      static int[] clr3=new int[] {0xbbbbff,0xffbbbb,0xffffbb};
      static int[] clr4=new int[] {0xffffcc,0xcc8888,0x8888cc,0xccffff};
      public static int i2(int a,int b) { return b;}
      public static int b2i(bool x) { return x?1:0;}
      public static bool i2b(int x) { return x!=0;}
      public static bool o2b(object x) { return x!=null;}
      public static bool f2b(float x) { return x!=0;}
      public static int f2i(float x) { return (int)x;}
      int rol(int n,int x,int c,bool r=false) {
        if(r) c=n-c;
        c=(c%n);if(c<0) c+=n;
        return ((x<<c)|(x>>(n-c)))&((1<<n)-1);
      }

      public int _whi(int c) {
        int r=(c>>16)&255,g=(c>>8)&255,b=c&255;
        r=255-(255-r)/3;
        g=255-(255-g)/3;
        b=255-(255-b)/3;
        c=(r<<16)|(g<<8)|b;
        return c;
      }
      public int _color(int x,int y) {
        int idx=Index(x,y),c=Data[idx].back,mono=View.mono;
        if(c<1) {
          switch(H) {
           case H.trap:c=mono==2?clr2[(x+y)&1]:mono==22?clr2[1&((x+1)>>1)]:mono==21?clr2[x&1]:mono==4?clr4[2*(y&1)+(x&1)]:mono==30?clr2[y&1]:mono==31?clr3[(x+2*(y&1))%3]:0xffffff;break;
           case H.delta:{
             int d=(x+2)%3,cx=(x+2-d)/3;
             c=mono==2?clr2[(x+y)&1]:mono==22?clr2[1&((x+1)>>1)]:mono==21?clr2[x&1]:mono==4?clr4[2*(y&1)+(x&1)]:mono==30?clr2[y&1]:mono==31?clr3[d]:0xffffff;
           } break;
           case H.deca:
           case H.cubes:{
             int d=(x+2)%3,cx=(x+2-d)/3;
             c=mono==2?clr2[(x+y)&1]:mono==22?clr2[1&((x+1)>>1)]:mono==21?clr2[x&1]:mono==4?clr4[2*(y&1)+(x&1)]:mono==30?clr2[y&1]:mono==31?clr3[d]:0xffffff;
           } break;
           case H.penta:{
              int cx=(x+1)/2|0,x1=x&1,v=(cx+y)&1;
              c=mono==2?clr2[(x+y)&1]:mono==22?clr2[1&((x+1)>>1)]:mono==21?clr2[x&1]:mono==4?clr4[2*v+x1]:mono==30?clr2[y&1]:mono==31?clr3[(x+2*(y&1))%3]:0xffffff;
            } break;
           case H.tria4:{
             int x2=(x-1)>>1,y2=(y-1)>>1,r=(1^x&1)|((1^y&1)<<1),tx=b2i(r==0||r==3);
             int dx=2*(x2-y2)+b2i(r==1||r==3),dy=2*(x2+y2)-b2i(r==2||r==3);
             c=mono==2?clr2[(x2^y2^tx)&1]:mono==22?clr2[1&((x+1)>>1)]:mono==21?clr2[1^y&1]:mono==4?clr4["0312"[r]]:mono==30?clr2[+b2i(!i2b(tx))]:mono==31?clr3[(3+dx%3+((dy>>1)&1))%3]:0xffffff;
            } break;
           case H.tria2:c=mono==2?clr2[(x+y)&1]:mono==22?clr2[1&((x+1)>>1)]:mono==21?clr2[x&1]:mono==4?clr4[(x&1)+2*(y&1)]:mono==30?clr2[y&1]:mono==31?clr3[(x+2*(y&1))%3]:0xffffff;break;
           case H.tria:c=mono==2?clr2[(x+y)&1]:mono==22?clr2[((x+y)/2)&1]:mono==21?clr2[x&1]:mono==4?clr4[(x&1)+2*(y&1)]:mono==30?clr2[y&1]:mono==31?clr3[(x+2*(y&1))%3]:0xffffff;break;
           case H.hexa:c=mono==2?clr2[(x+y)&1]:mono==22?clr2[(x+2-(y&1)-(y/2)&1)&1]:mono==21?clr2[(x+(y&1)+y/2)&1]:mono==4?clr4[(x&1)+2*(y&1)]:mono==30?clr2[y&1]:mono==31?clr3[(x+2*(y&1))%3]:0xffffff;break;
           default:c=mono==2?clr2[(x+y)&1]:mono==22?clr2[x&1]:mono==21?clr2[y&1]:mono==4?clr4[(x&1)+2*(y&1)]:mono==30?clr3[(x+9999-y)%3]:mono==31?clr3[(x+y)%3]:0xffffff;break;
          }
        }
        return c;
      }
      delegate void fDrawCell(Graphics gr,int x,int y);
      public void _drawcell(Graphics gr,int x,int y) {
        float sx=brd+x*Cell,sy=brd+y*Cell;
        int c=_color(x,y);        
  Brush b;
  if(View.grdm>0) {
    if(c==Pal.White) c=clr2[1];
    var xy=_points14(x,y);
    if(View.grdm>=5) {
      if(View.rou) {
        bool[] ba=_brdborder(x,y,null);
        _ground(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5,ba,(float)Math.Sqrt(3)/3); 
      } else _gpoly(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5);
      return;
    } else if(View.grdm>1) {float[] fa=_radial(View.grdx,View.grdy,xy);b=_radialbrush(fa[0],fa[1],fa[2],c,Pal.White);}
    else { float[] fa=_linear(View.grdx,View.grdy,xy);b=_linearbrush(fa[0],fa[1],fa[2],fa[3],c,Pal.White);}
  } else
    b=new SolidBrush(Pal.IntColor(c));    
  GraphicsPath gp=new GraphicsPath();
        if(View.rou) {
    float c2=Cell/2,cx=sx+c2,cy=sy+c2;var ba=new bool[]{_brd(x-1,y),_brd(x,y-1),_brd(x+1,y),_brd(x,y+1),false};
    ba[4]=ba[0];
    gr.FillEllipse(b,sx,sy,Cell,Cell);
    if(ba[0]||ba[1]) gr.FillRectangle(b,sx,sy,c2,c2);
    if(ba[1]||ba[2]) gr.FillRectangle(b,sx+c2,sy,c2,c2);
    if(ba[2]||ba[3]) gr.FillRectangle(b,sx+c2,sy+c2,c2,c2);
    if(ba[3]||ba[0]) gr.FillRectangle(b,sx,sy+c2,c2,c2);
    if(ba[0]) _rline(gr,Pen05,sx,sy,0,Cell);
    if(ba[1]) _rline(gr,Pen05,sx,sy,Cell,0);          
        } else {
          gr.FillRectangle(b,sx,sy,Cell,Cell);
          gr.DrawRectangle(Pen05,sx,sy,Cell,Cell);
        }
      }
bool[] _brdborder(int x,int y,int[] b) {
  if(b==null) b=_border(x,y);
  int i,i1;bool[] r=new bool[b.Length];
  for(i1=i=0;i<b.Length;i1++,i+=2) r[i1]=_brd(x+b[i],y+b[i+1]);
  return r;
}

void _poly(Graphics gr,Brush b,Pen p,params float[] xy) {
  var pa=new PointF[xy.Length/2];
  for(int i=0,i1=0;i<xy.Length;i+=2,i1++)
    pa[i1]=new PointF(xy[i],xy[i+1]);   
  if(b!=null) gr.FillPolygon(b,pa);
  if(p!=null) gr.DrawPolygon(p,pa);
}

void _gpoly(Graphics gr,float[] xy,int c0,int c1,Pen s,float sx,float sy,bool m) {
  int i;Brush g;float x=xy[xy.Length-2],y=xy[xy.Length-1];
  if(!(sx!=0||sy!=0)) sx=pmap._polyc(xy,out sy);
  _poly(gr,new SolidBrush(Pal.IntColor(c0)),null,xy);
  for(i=0;i<xy.Length;i+=2) {
    float x2=x,y2=y;x=xy[i];y=xy[i+1];float x1=xy[(i+2)%xy.Length],y1=xy[(i+3)%xy.Length];
    float[] pa;
    if(m) {
      pa=FA(x,y,x2,y2,sx,sy);
      //ctx.fillStyle=g=ctx.createLinearGradient((x+x2)/2,(y+y2)/2,sx,sy);
      x2=(x+x2)/2;y2=(y+y2)/2;g=_radialbrush(x2,y2,(float)Math.Sqrt(_sqr(x2-sx,y2-sy)),c1,c0);
    } else {
      pa=FA((x+x2)/2,(y+y2)/2,x,y,(x+x1)/2,(y+y1)/2,sx,sy);
      //ctx.fillStyle=g=ctx.createLinearGradient(x,y,sx,sy);
      g=_radialbrush(x,y,(float)Math.Sqrt(_sqr(x-sx,y-sy)),c1,c0);
    }
    _poly(gr,g,null,pa);
  }
  if(s!=null) _poly(gr,null,s,xy);
}

void _bez2(GraphicsPath gp,bool c,float x,float y,float x1,float y1,float x2,float y2,float r) { 
  if(c) {gp.AddLine(x,y,x1,y1);gp.AddLine(x1,y1,x2,y2);}
  else {
   float r1=1-r;
   gp.AddBezier(x,y,r1*x1+r*x,r1*y1+r*y,r1*x1+r*x2,r1*y1+r*y2,x2,y2);
  }
}

void _bez3(GraphicsPath gp,bool s,bool c,float x,float y,float x1,float y1,float x2,float y2,float r,bool m) {
  if(c) {
    if(s) {x=x1;y=y1;x1=x2;y1=y2;}
    gp.AddLine(x,y,x1,y1);
  } else {
    float r1=1-r;
    float[] xa=_bezsplit(0.5f,s,x,r1*x1+r*x,r1*x1+r*x2,x2),ya=_bezsplit(0.5f,s,y,r1*y1+r*y,r1*y1+r*y2,y2);
    gp.AddBezier(xa[0],ya[0],xa[1],ya[1],xa[2],ya[2],xa[3],ya[3]);
  }
}

GraphicsPath _round(float[] xy,bool[] b,float r) {
  int i1,i,l=xy.Length;float lx,ly,nx,ny;
  nx=(xy[0]+xy[l-2])/2;ny=(xy[1]+xy[l-1])/2;
  GraphicsPath gp=new GraphicsPath();
  for(i1=i=0;i<l;i1++,i+=2) {lx=nx;ly=ny;_bez2(gp,b[i1]||b[i1+1],xy[i],xy[i+1],lx,ly,nx=(xy[i]+xy[(i+2)%l])/2,ny=(xy[i+1]+xy[(i+3)%l])/2,r);}
  return gp;
}

void _ground(Graphics gr,float[] xy,int c0,int c1,Pen s,float sx,float sy,bool m,bool[] b,float r) {
  int i,i1,l=xy.Length;Brush g;float lx,ly,nx,ny,x,y;
  if(!(sx!=0||sy!=0)) sx=_polyc(xy,out sy);
  Push(ref b,b[0],b[1]);
  var gp=_round(xy,b,r);
  gr.FillPath(new SolidBrush(Pal.IntColor(c0)),gp);
  
  nx=(xy[0]+xy[l-2])/2;ny=(xy[1]+xy[l-1])/2;
  for(i1=i=0;i<l;i1++,i+=2) {
    gp.Reset();
    if(m) {
      float mx=xy[(i+2)%l],my=xy[(i+3)%l],px=xy[(i+4)%l],py=xy[(i+5)%l];
      lx=nx;ly=ny;nx=(xy[i]+mx)/2;ny=(xy[i+1]+my)/2;
      _bez3(gp,B1,b[i1]||b[i1+1],lx,ly,xy[i],xy[i+1],nx,ny,r,B1);
      _bez3(gp,B0,b[i1+1]||b[i1+2],nx,ny,mx,my,(mx+px)/2,(my+py)/2,r,B0);
      x=nx;y=ny;
    } else {      
      lx=nx;ly=ny;_bez2(gp,b[i1]||b[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[(i+2)%l])/2,ny=(xy[i+1]+xy[(i+3)%l])/2,r);
      x=xy[i];y=xy[i+1];
    }
    gp.AddLine(nx,ny,sx,sy);
    g=_radialbrush(x,y,(float)Math.Sqrt(_sqr(x-sx,y-sy)),c1,c0);
    gr.FillPath(g,gp);
  }
  if(s!=null) gr.DrawPath(s,gp);
}



void _drawcell16(Graphics gr,int x,int y) {
  var xy=_points16(x,y);
  //var sx=brd+(x+(y&1)/2)*cell6y,sy=brd+((1+y*3)/4)*cell6x,x2=sx+cell6y/2,x3=sx+cell6y;
  int c=_color(x,y);        
  Brush b;
  if(View.grdm>0) {
    if(c==Pal.White) c=clr2[1];
    if(View.grdm>=5) {
      if(View.rou) {
        bool[] ba=_brdborder(x,y,null);
        _ground(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5,ba,(float)Math.Sqrt(3)/3); 
      } else _gpoly(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5);
      return;
    } else if(View.grdm>1) {float[] fa=_radial(View.grdx,View.grdy,xy);b=_radialbrush(fa[0],fa[1],fa[2],c,Pal.White);}
    else { float[] fa=_linear(View.grdx,View.grdy,xy);b=_linearbrush(fa[0],fa[1],fa[2],fa[3],c,Pal.White);}
  } else
    b=new SolidBrush(Pal.IntColor(c));    
  GraphicsPath gp=new GraphicsPath();
  if(View.rou) {
    int i,i1;float cx=(xy[0]+xy[6])/2,cy=(xy[1]+xy[7])/2,r=(float)Math.Sqrt(_sqr(xy[0]-cx,xy[1]-cy));int[] b2=_border16(x,y);bool[] ba=new bool[8];
    for(i1=i=0;i<12;i1++,i+=2) ba[i1]=_brd(x+b2[i],y+b2[i+1]);
    ba[6]=ba[0];
    Push(ref xy,xy[0],xy[1]);
    float lx=(xy[0]+xy[10])/2,ly=(xy[1]+xy[11])/2,nx,ny;
    for(i1=i=0;i<12;i1++,i+=2) {_arc2(gp,ba[i1]||ba[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,r);lx=nx;ly=ny;}
    gp.CloseFigure();
    gr.FillPath(b,gp);
    ShiftL(ba);
    for(i1=i=0;i<12;i1++,i+=2) if(ba[i1]) gr.DrawLine(Pen05,xy[i],xy[i+1],xy[i+2],xy[i+3]);
  } else {
    gp.AddLine(xy[0],xy[1],xy[2],xy[3]);
    gp.AddLine(xy[2],xy[3],xy[4],xy[5]);
    gp.AddLine(xy[4],xy[5],xy[6],xy[7]);
    gp.AddLine(xy[6],xy[7],xy[8],xy[9]);
    gp.AddLine(xy[8],xy[9],xy[10],xy[11]);
    gp.AddLine(xy[10],xy[11],xy[0],xy[1]);
    gr.FillPath(b,gp);
    gr.DrawPath(Pen05,gp);  
  }
}

void _drawcell3x(Graphics gr,int x,int y,float[] xy,int c,fBorder border) {
  Brush b;int n=xy.Length;
  if(View.grdm>0) {
    if(c==Pal.White) c=clr2[1];
    if(View.grdm>=5) {
      if(View.rou) {
        int[] b2=border(x,y);bool[] ba=_brdborder(x,y,null);
        _ground(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5,ba,H==H.tria?(float)Math.Sqrt(2)/3:0.125f); 
      } else _gpoly(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5);
      return;
    } else if(View.grdm>1) {float[] fa=_radial(View.grdx,View.grdy,xy);b=_radialbrush(fa[0],fa[1],fa[2],c,Pal.White);}
    else { float[] fa=_linear(View.grdx,View.grdy,xy);b=_linearbrush(fa[0],fa[1],fa[2],fa[3],c,Pal.White);}
    //else g=ctx.createLinearGradient(xy[0],xy[1],xy[2],xy[3]);
  } else
    b=new SolidBrush(Pal.IntColor(c));
  GraphicsPath gp=new GraphicsPath();
  if(View.rou) {
    int i,i1;int[] b2=border(x,y);bool[] ba=new bool[4];
    for(i1=i=0;i<6;i1++,i+=2) ba[i1]=_brd(x+b2[i],y+b2[i+1]);
    ba[3]=ba[0];
    Push(ref xy,xy[0],xy[1]);
    float lx=(xy[0]+xy[4])/2,ly=(xy[1]+xy[5])/2,nx,ny;    
    if(H==H.tria) for(i1=i=0;i<n;i1++,i+=2) {_arc2(gp,ba[i1]||ba[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,cell3y/3);lx=nx;ly=ny;}
    else for(i1=i=0;i<n;i1++,i+=2) {_bez2(gp,ba[i1]||ba[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,0.125f);lx=nx;ly=ny;}
    gp.CloseFigure();
    gr.FillPath(b,gp);
    ShiftL(ba);
    for(i1=i=0;i<n;i1++,i+=2) if(ba[i1]) gr.DrawLine(Pen05,xy[i],xy[i+1],xy[i+2],xy[i+3]);    
  } else {
    gp.AddLine(xy[0],xy[1],xy[2],xy[3]);
    gp.AddLine(xy[2],xy[3],xy[4],xy[5]);
    gp.AddLine(xy[4],xy[5],xy[0],xy[1]);
    gr.FillPath(b,gp);
    gr.DrawPath(Pen05,gp);  
  }
}


void _drawcell3(Graphics gr,int x,int y) {
  var xy=_points3(x,y);
  var c=_color(x,y);
  _drawcell3x(gr,x,y,xy,c,_border3);
}

void _drawcell2(Graphics gr,int x,int y) {
  var xy=_points2(x,y);
  var c=_color(x,y);
  _drawcell3x(gr,x,y,xy,c,_border2);
}

void _drawcell5(Graphics gr,int x,int y) {
  var xy=_points5(x,y);
  var c=_color(x,y);
  _drawcell3x(gr,x,y,xy,c,_border5);
}

void _drawcell15(Graphics gr,int x,int y) {
  var xy=_points15(x,y);int cx=(x+1)/2|0,x1=x&1,v=(cx+y)&1;
  Brush b;int n=xy.Length,n2=n/2;
  var c=_color(x,y);
  if(View.grdm>0) {
    if(c==Pal.White) c=clr2[1];
    if(View.grdm>=5) {
      if(View.rou) {
        int[] b2=_border15(x,y);bool[] ba=_brdborder(x,y,null);
        _ground(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5,ba,H==H.tria?(float)Math.Sqrt(2)/3:0.125f); 
      } else _gpoly(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5);
      return;
    } else if(View.grdm>1) {float[] fa=_radial(View.grdx,View.grdy,xy);b=_radialbrush(fa[0],fa[1],fa[2],c,Pal.White);}
    else { float[] fa=_linear(View.grdx,View.grdy,xy);b=_linearbrush(fa[0],fa[1],fa[2],fa[3],c,Pal.White);}
    //else g=ctx.createLinearGradient(xy[0],xy[1],xy[2],xy[3]);
  } else
    b=new SolidBrush(Pal.IntColor(c));
  GraphicsPath gp=new GraphicsPath();
  if(View.rou) {
    int i,i1;var b2=_border15(x,y);var ba=new bool[n2+1];
    for(i1=i=0;i<10;i1++,i+=2) ba[i1]=_brd(x+b2[i],y+b2[i+1]);
    ba[n2]=ba[0];
    Push(ref xy,xy[0],xy[1]);
    float lx=(xy[0]+xy[n-2])/2,ly=(xy[1]+xy[n-1])/2,nx,ny;        
    for(i1=i=0;i<n;i1++,i+=2) {_bez2(gp,ba[i1]||ba[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,0.125f);lx=nx;ly=ny;}
    gp.CloseFigure();
    gr.FillPath(b,gp);
    ShiftL(ba);
    for(i1=i=0;i<n;i1++,i+=2) if(ba[i1]) gr.DrawLine(Pen05,xy[i],xy[i+1],xy[i+2],xy[i+3]);    
  } else {
    gp.AddLine(xy[0],xy[1],xy[2],xy[3]);
    gp.AddLine(xy[2],xy[3],xy[4],xy[5]);
    gp.AddLine(xy[4],xy[5],xy[6],xy[7]);
    gp.AddLine(xy[6],xy[7],xy[8],xy[9]);
    gp.AddLine(xy[8],xy[9],xy[0],xy[1]);
    gr.FillPath(b,gp);
    gr.DrawPath(Pen05,gp);  
  }

}

void _drawcell8(Graphics gr,int x,int y) {
  var xy=_points8(x,y);
  _drawcell8x(gr,x,y,xy,_border8);
}

void _drawcell8x(Graphics gr,int x,int y,float[] xy,fBorder border) {
  Brush b;int n=xy.Length,n2=n/2;
  var c=_color(x,y);
  if(View.grdm>0) {
    if(c==Pal.White) c=clr2[1];
    if(View.grdm>=5) {
      if(View.rou) {
        int[] b2=border(x,y);bool[] ba=_brdborder(x,y,null);
        _ground(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5,ba,H==H.tria?(float)Math.Sqrt(2)/3:0.125f); 
      } else _gpoly(gr,xy,Pal.White,c,Pen05,0,0,View.grdm>5);
      return;
    } else if(View.grdm>1) {float[] fa=_radial(View.grdx,View.grdy,xy);b=_radialbrush(fa[0],fa[1],fa[2],c,Pal.White);}
    else { float[] fa=_linear(View.grdx,View.grdy,xy);b=_linearbrush(fa[0],fa[1],fa[2],fa[3],c,Pal.White);}
    //else g=ctx.createLinearGradient(xy[0],xy[1],xy[2],xy[3]);
  } else
    b=new SolidBrush(Pal.IntColor(c));
  GraphicsPath gp=new GraphicsPath();
  if(View.rou) {
    int i,i1;var b2=border(x,y);var ba=new bool[n2+1];
    for(i1=i=0;i<n;i1++,i+=2) ba[i1]=_brd(x+b2[i],y+b2[i+1]);
    ba[n2]=ba[0];
    Push(ref xy,xy[0],xy[1]);
    float lx=(xy[0]+xy[n-2])/2,ly=(xy[1]+xy[n-1])/2,nx,ny;        
    for(i1=i=0;i<n;i1++,i+=2) {_bez2(gp,ba[i1]||ba[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,0.125f);lx=nx;ly=ny;}
    gp.CloseFigure();
    gr.FillPath(b,gp);
    ShiftL(ba);
    for(i1=i=0;i<n;i1++,i+=2) if(ba[i1]) gr.DrawLine(Pen05,xy[i],xy[i+1],xy[i+2],xy[i+3]);    
  } else {
    gp.AddLine(xy[0],xy[1],xy[2],xy[3]);
    gp.AddLine(xy[2],xy[3],xy[4],xy[5]);
    gp.AddLine(xy[4],xy[5],xy[6],xy[7]);
    if(xy.Length==12) {
      gp.AddLine(xy[6],xy[7],xy[8],xy[9]);
      gp.AddLine(xy[8],xy[9],xy[10],xy[11]);
      gp.AddLine(xy[10],xy[11],xy[0],xy[1]);
    } else
      gp.AddLine(xy[6],xy[7],xy[0],xy[1]);
    gr.FillPath(b,gp);
    gr.DrawPath(Pen05,gp);  
  }

}

void _drawcell9(Graphics gr,int x,int y) {
  var xy=_points9(x,y);
  _drawcell8x(gr,x,y,xy,_border9);
}

void _drawcell11(Graphics gr,int x,int y) {
  var xy=_points11(x,y);
  _drawcell8x(gr,x,y,xy,_border11);
}

void _drawcell7(Graphics gr,int x,int y) {
  var xy=_points7(x,y);
  _drawcell8x(gr,x,y,xy,_border7);
}



      static float[] _radial_sph(float x,float y,float r) {
         return new float[] {x-r*1/2,y-r*1/2,r};
      }
      static PathGradientBrush _radialbrush(float x,float y,float r,int c1,int c0) {
        GraphicsPath p=new GraphicsPath();
        p.AddEllipse(x-r,y-r,2*r,2*r);
        PathGradientBrush pg=new PathGradientBrush(p);
        pg.CenterPoint=new PointF(x,y);
        pg.CenterColor=Pal.IntColor(c0<0?0xffffff:c0);
        pg.SurroundColors=new Color[] {Pal.IntColor(c1<0?0:c1)};
        return pg;
      }
      static LinearGradientBrush _linearbrush(float x,float y,float x2,float y2,int c1,int c0) {
        LinearGradientBrush lg=new LinearGradientBrush(new PointF(x,y),new PointF(x2,y2),Pal.IntColor(c0<0?0xffffff:c0),Pal.IntColor(c1<0?0:c1));
        return lg;
      }
      static float _polyc(float[] xy,out float sy) {
        int i;float sx=0;sy=0;
        for(sx=sy=i=0;i<xy.Length;i+=2) {sx+=xy[i];sy+=xy[i+1];}
        i>>=1;sx/=i;sy/=i;
        return sx;
      }

      static float[] _radial_xy(float[] xy) {
        int i;float sx,sy,d,r,x,y,m=0,n=0,lx=xy[xy.Length-2],ly=xy[xy.Length-1];
        sx=_polyc(xy,out sy);
        for(i=0;i<xy.Length;i+=2) {
          x=xy[i];y=xy[i+1];d=_sqr(x-sx,y-sy);
          if(d>n) n=d;
          d=_sqr((x+lx)/2-sx,(y+ly)/2-sy);
          if(d>m) m=d;
          lx=x;ly=y;
         }
         m=(float)Math.Round(Math.Sqrt(m));
         return new float[] {sx,sy,m};
      }
float[] _radial(float dx,float dy,float[] xy) {
  int i,j=0,k=0;float m=float.PositiveInfinity,n=float.NegativeInfinity,x,sx,sy;
  if(View.grdm==3) {
    for(sx=sy=i=0;i<xy.Length;i+=2) {sx+=xy[i];sy+=xy[i+1];}
    i>>=1;sx/=i;sy/=i;
    i=H==H.deca||H==H.trap?4:H==H.tria2||H==H.tria4?2:0;
    x=_sqr(sx-xy[i],sy-xy[i+1]);
  } else {
    for(i=0;i<xy.Length;i+=2) {
      x=dx*xy[i]+dy*xy[i+1];
      if(x<m) {j=i;m=x;if(i<1) {k=i;n=x;}}
      else if(x>n) {k=i;n=x;}
    }
    sx=xy[j];sy=xy[j+1];x=_sqr(sx-xy[k],sy-xy[k+1]);
  }
  x=(float)Math.Sqrt(x);
  return new float[] {sx,sy,x};//:ctx.createRadialGradient(sx,sy,0,sx,sy,x);
}
float[] _linear(float dx,float dy,float[] xy) {
  int i,j=-1,k;float m=float.PositiveInfinity,n=float.NegativeInfinity,x,sx,sy,tx,ty;
  for(i=0;i<xy.Length;i+=2) {
    x=dx*xy[i]+dy*xy[i+1];
    if(x<m) {j=i;m=x;if(i<1) {k=i;n=x;}}
    else if(x>n) {k=i;n=x;}
  }
  sx=xy[j];sy=xy[j+1];
  x=(n-m)/_sqr(dx,dy);
  return new float[] {sx,sy,sx+x*dx,sy+x*dy};//:ctx.createLinearGradient(sx,sy,sx+x*dx,sy+x*dy);
}

      bool _brd(int x,int y) { return x>0&&y>0&&x<Width&&y<Height&&Data[Index(x,y)].ch>0;}
      static Pen Pen05=new Pen(Pal.IntColor(0),0.5f);
      static Pen Pen20=new Pen(Pal.IntColor(0),2f);
      static Pen Pen30=new Pen(Pal.IntColor(0),3f);
      static Pen Pen40=new Pen(Pal.IntColor(0),4f);
      static Pen Pen10W=new Pen(Pal.IntColor(~0),1f);
      public delegate void fDrawPeg(Graphics gr,int x,int y,bool white);
      public void _drawpeg(Graphics gr,int x,int y,bool white) {
        float sx=brd+x*Cell,sy=brd+y*Cell,c2=Cell/2,r=c2-3;
        int pc=Data[Index(x,y)].fore,fs=pc!=0?white?_whi(pc):pc:white?View.grdm2>0?0xcccccc:Pal.White:0;
        Brush bb=new SolidBrush(Pal.IntColor(fs)),bb2=null;
        Pen p=white?Pen20:Pen05;
        float[] rp;
        if(View.peg) {
          if(View.grdm2>0) {rp=_radial_xy(new float[] {sx+4,sy+4,sx+Cell-4,sy+4,sx+Cell-4,sy+Cell-4,sx+4,sy+Cell-4});bb2=_radialbrush(rp[0],rp[1],rp[2],fs,-1);};
          if(View.rou) {
            float cx=sx+c2,cy=sy+c2,d=4;r=c2-d;
            bool[] ba=new bool[] {_brd(x-1,y),_brd(x,y-1),_brd(x+1,y),_brd(x,y+1)};
            GraphicsPath gp=new GraphicsPath();
            float lx=sx+4,ly=sy+c2,nx,ny;
            _arc2(gp,ba[0]||ba[1],lx,ly,sx+d,sy+d,nx=sx+c2,ny=sy+d,r);lx=nx;ly=ny;
            _arc2(gp,ba[1]||ba[2],lx,ly,sx+Cell-d,sy+d,nx=sx+Cell-d,ny=sy+c2,r);lx=nx;ly=ny;
            _arc2(gp,ba[2]||ba[3],lx,ly,sx+Cell-d,sy+Cell-d,nx=sx+c2,ny=sy+Cell-d,r);lx=nx;ly=ny;
            _arc2(gp,ba[3]||ba[0],lx,ly,sx+d,sy+Cell-d,nx=sx+4,ny=sy+c2,r);
            if(View.grdm2>0) gr.FillPath(new SolidBrush(Pal.IntColor(pc)),gp);
            gr.FillPath(bb,gp);
            if(bb2!=null) gr.FillPath(bb2,gp);
            gr.DrawPath(p,gp);
          } else {
            gr.FillRectangle(bb,sx+4,sy+4,Cell-8,Cell-8);
            gr.DrawRectangle(p,sx+4,sy+4,Cell-8,Cell-8);
          }
        } else {
          gr.FillEllipse(bb,sx+c2-r,sy+c2-r,2*r,2*r);
          if(View.grdm2>0) {
            rp=_radial_sph(sx+c2,sy+c2,c2-3);bb2=_radialbrush(rp[0],rp[1],rp[2],fs,-1);
            gr.FillEllipse(bb2,sx+c2-r,sy+c2-r,2*r,2*r);
          }
          gr.DrawEllipse(p,sx+c2-r,sy+c2-r,2*r,2*r);
        }
      }
void _drawpeg3x(Graphics gr,int x,int y,bool white,bool pg,fPoints points,fBorder border) {
  int pc=_getpgc(x,y),fs=pc!=0?white?_whi(pc):pc:white?View.grdm2>0?0xcccccc:Pal.White:0;
  var gp=new GraphicsPath();
  Brush bb=new SolidBrush(Pal.IntColor(fs)),bb2=null;
  if(pg) {
    float[] pt=_polyex2(_points(x,y),0.75f,0,0),rp2;
    if(View.grdm2>0) {rp2=_radial_xy(pt);bb2=_radialbrush(rp2[0],rp2[1],rp2[2],fs,-1);}
    if(View.rou) {
      float[] xy=pt;int[] b=border(x,y);
      int i,i1;bool[] o=new bool[xy.Length/2+1];float lx,ly,nx,ny;int n=xy.Length;
      for(i1=i=0;i<n;i1++,i+=2) o[i1]=_brd(x+b[i],y+b[i+1]);
      o[i1]=o[0];Push(ref xy,xy[0],xy[1]);
      nx=(xy[0]+xy[n-2])/2;ny=(xy[1]+xy[n-1])/2;
      if(H==H.tria) 
      for(i1=i=0;i<n;i1++,i+=2)
        {lx=nx;ly=ny;_arc2(gp,o[i1]||o[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,cell3y*0.75f/3);}
      else for(i1=i=0;i<n;i1++,i+=2)
        {lx=nx;ly=ny;_bez2(gp,o[i1]||o[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,0.125f);}
    } else 
      for(int i=0,j=pt.Length-2;i<pt.Length;j=i,i+=2)
        gp.AddLine(pt[j],pt[j+1],pt[i],pt[i+1]);
  } else {
    float[] p=_peg(x,y);
    if(View.grdm2!=0) {float[] rp=_radial_sph(p[0],p[1],p[2]);bb2=_radialbrush(rp[0],rp[1],rp[2],fs,-1);}
    gp.AddArc(p[0]-p[2],p[1]-p[2],2*p[2],2*p[2],0,360);
  }
  gr.FillPath(bb,gp);
  if(bb2!=null) gr.FillPath(bb2,gp);
  Pen pp=white?Pen20:Pen05;
  gr.DrawPath(pp,gp);
}

    void _drawpeg3(Graphics gr,int x,int y,bool white) {
      _drawpeg3x(gr,x,y,white,View.peg,_points3,_border3);
     }

    void _drawpeg2(Graphics gr,int x,int y,bool white) {
      _drawpeg3x(gr,x,y,white,View.peg,_points2,_border2);
     }

    void _drawpeg5(Graphics gr,int x,int y,bool white) {
      _drawpeg3x(gr,x,y,white,View.peg,_points2,_border5);
     }
      void _drawpeg15(Graphics gr,int x,int y,bool white) {
         _drawpeg3x(gr,x,y,white,View.peg,_points15,_border15);
      }
      void _drawpeg8(Graphics gr,int x,int y,bool white) {
         _drawpeg3x(gr,x,y,white,View.peg,_points8,_border8);
      }
      void _drawpeg9(Graphics gr,int x,int y,bool white) {
         _drawpeg3x(gr,x,y,white,View.peg,_points9,_border9);
      }
      void _drawpeg11(Graphics gr,int x,int y,bool white) {
         _drawpeg3x(gr,x,y,white,View.peg,_points11,_border11);
      }
      void _drawpeg7(Graphics gr,int x,int y,bool white) {
         _drawpeg3x(gr,x,y,white,View.peg,_points7,_border7);
      }


      void _drawpeg16(Graphics gr,int x,int y,bool white) {
         _drawpeg3x(gr,x,y,white,View.peg,_points16,_border16);
      }

      public delegate float[] fPoints(int x,int y);
      public delegate int[] fBorder(int x,int y);
      void _circle(Graphics gr,Pen p,float r,float x,float y,Brush fill) {
        if(fill!=null) gr.FillEllipse(fill,x-r,y-r,2*r,2*r);
        else gr.DrawEllipse(p,x-r,y-r,2*r,2*r);
      }
      void _circle2(Graphics gr,Pen p,float r,float x,float y,float x2,float y2,Brush fill) {
        _circle(gr,p,r,x,y,fill);
        _circle(gr,p,r,x2,y2,fill);
      }
      float _arcc(ref float lx,ref float ly,float x,float y,ref float x2,ref float y2,float r,out float cx,out float cy) {
   float a=_sqr(lx-x,ly-y),b=_sqr(x2-x,y2-y),kx=lx,ky=ly,x3=x2,y3=y2;
   if(a>b) { a=(float)Math.Sqrt(b/a);lx=x+a*(lx-x);ly=y+a*(ly-y);}
   else { a=(float)Math.Sqrt(b/a);x2=x+a*(x2-x);y2=y+a*(y2-y);}
   float sx=(lx+x2)/2f,sy=(ly+y2)/2f;a=_sqr(sx-lx,sy-ly);b=_sqr(sx-x,sy-y);
   cx=sx+(sx-x)*a/b;cy=sy+(sy-y)*a/b;
   float r2=(float)Math.Sqrt(_sqr(cx-x2,cy-y2));
   if(r2>r) { a=r/r2;lx=x+a*(lx-x);ly=y+a*(ly-y);x2=x+a*(x2-x);y2=y+a*(y2-y);cx=x+a*(cx-x);cy=x+a*(cy-x);}
   else r=r2;
    return r;
      }
      float _arca(float cx,float cy,float x,float y,float x2,float y2,out float sweep) {
          float a=(float)(Math.Atan2(y-cy,x-cx)*180/Math.PI),b=(float)(Math.Atan2(y2-cy,x2-cx)*180/Math.PI);
          b-=a;if(b<-180) b+=360;else if(b>180) b-=360;   
          if(b<0) {a+=b;b=-b;}
          sweep=b;return a;
      }
      void _arc2(GraphicsPath gp,bool c,float lx,float ly,float x,float y,float x2,float y2,float r) {
        if(c) { gp.AddLine(lx,ly,x,y);gp.AddLine(x,y,x2,y2);}
        else {
          //ctx.arcTo(x,y,x2,y2,r);
          float kx=lx,ky=ly,x3=x2,y3=y2,cx,cy,a,b;
          r=_arcc(ref lx,ref ly,x,y,ref x2,ref y2,r,out cx,out cy);
          a=_arca(cx,cy,lx,ly,x2,y2,out b);
          if(lx!=kx||ly!=ky) gp.AddLine(kx,ky,lx,ly);
          gp.AddArc(cx-r,cy-r,2*r,2*r,a,b);
          if(x3!=x2||y3!=y2) gp.AddLine(x2,y2,x3,y3);

        }
      }
      void _bez(Graphics gr,Pen p,float x,float y,float x1,float y1,float x2,float y2,float r) {
       float r1=1-r;
        gr.DrawBezier(p,x,y,r1*x1+r*x,r1*y1+r*y,r1*x1+r*x2,r1*y1+r*y2,x2,y2);
      }

void _borderedge(Graphics gr,int e,float[] xy2) {
 int m,i;
 e=e|rol(xy2.Length>>1,e,-1);
 for(m=1,i=0;i<xy2.Length;i+=2,m<<=1) 
   if(i2b(e&m))
     _circle(gr,Pen40,4,xy2[i],xy2[i+1],Brushes.Black);
}

void _corn(Graphics gr,int m,float e,int n,float[] xy2,bool[] b) {
  int i,i2,n2=n>>1;float a,c,d,x3,y3,x2=xy2[n-4],y2=xy2[n-3],x=xy2[n-2],y=xy2[n-1],dx,dy,ex,ey;
  for(i=0,i2=n2-2;i<xy2.Length;i+=2,i2=(i2+1)%n2) {
    x3=x2;y3=y2;x2=x;y2=y;x=xy2[i];y=xy2[i+1];
    if(b!=null&&!b[i2]&&!b[(i2+1)%n2]) continue;
    dx=x-x2;dy=y-y2;ex=x3-x2;ey=y3-y2;
    if(Math.Abs(dx*ey-dy*ex)<1) continue;
    if(m==2) {
      a=(float)(Math.Atan2(ey,ex)*180/Math.PI);c=(float)(Math.Atan2(dy,dx)*180/Math.PI);
      GraphicsPath gp=new GraphicsPath();
      gp.AddLine(x2,y2,x3,y3);
      d=c-a;if(d>180) d-=360;else if(d<-180) d+=360;gp.AddArc(x2-e,y2-e,2*e,2*e,a,d);
      gp.AddLine(x,y,x2,y2);
      gr.FillPath(Brushes.Black,gp);
    } else {
      a=(float)Math.Sqrt(_sqr(ex,ey));
      c=(float)Math.Sqrt(_sqr(dx,dy));
      _poly(gr,Brushes.Black,null,x2,y2,x2+e*(x3-x2)/a,y2+e*(y3-y2)/a,x2+e*(x-x2)/c,y2+e*(y-y2)/c);
    }
  }
}



      public delegate void fDrawBorder(Graphics gr,int x,int y,fPoints points,fBorder border);
      public void DrawBorder(Graphics gr,int x,int y,fPoints points,fBorder border) {
        var xy=points(x,y);var b=border(x,y);
        int i,i2,n=xy.Length,e=Data[Index(x,y)].block;
        for(i=0,i2=n-2;i<n;i2=i,i+=2)
          if(!_xo(x+b[i],y+b[i+1])) gr.DrawLine(Pen20,xy[i],xy[i+1],xy[i2],xy[i2+1]);
        if(e>0) {
          int i3;
          _polyex2(xy,15f/16,0,0);
          for(i=0,i2=4;i<3;i++,i2=(i2+2)%6) if(0!=(e&(1<<i))) {
            i3=(i2+2)%n;
            gr.DrawLine(Pen20,xy[i2],xy[i2+1],xy[i3],xy[i3+1]);
            _circle2(gr,Pen20,1,xy[i2],xy[i2+1],xy[i3],xy[i3+1],null);
          }          
        }
      }

void _line(Graphics gr,Pen p,float x,float y,float x2,float y2) { gr.DrawLine(p,x,y,x2,y2);}
void _rline(Graphics gr,Pen p,float x,float y,float dx,float dy) { gr.DrawLine(p,x,y,x+dx,y+dy);}
void _arc(Graphics gr,Pen p,float cx,float cy,float r,float a,float s) {  
  gr.DrawArc(p,cx-r,cy-r,2*r,2*r,a,s);  
}


void _drawborder(Graphics gr,int x,int y,fPoints points,fBorder border) {
  float sx=brd+x*Cell,sy=brd+y*Cell,c2=Cell/2;int e=Data[Index(x,y)].block;
  Pen p=Pen20;bool[] b=null;
  if(View.rou) {
    float cx=sx+c2,cy=sy+c2;b=new bool[]{_brd(x-1,y),_brd(x,y-1),_brd(x+1,y),_brd(x,y+1)};
    if(!b[0]) {
      if(b[1]) _rline(gr,p,sx,sy,0,c2+1);else _arc(gr,p,cx,cy,c2,180,90);
      if(b[3]) _rline(gr,p,sx,sy+c2,0,c2);else _arc(gr,p,cx,cy,c2,90,90);
    } else {
      if(!b[1]) _rline(gr,p,sx,sy,c2,0);
      if(!b[3]) _rline(gr,p,sx,sy+Cell,c2+1,0);
    }
    if(!b[2]) {
      if(b[1]) _rline(gr,p,sx+Cell,sy,0,c2);else _arc(gr,p,cx,cy,c2,270,90);
      if(b[3]) _rline(gr,p,sx+Cell,sy+c2-1,0,c2+1);else _arc(gr,p,cx,cy,c2,0,90);
    } else {
      if(!b[1]) _rline(gr,p,sx+c2-1,sy,c2+1,0);
      if(!b[3]) _rline(gr,p,sx+c2,sy+Cell,c2,0);
    }
    if(View.corn!=0) b=new bool[]{_brd(x,y-1),_brd(x+1,y),_brd(x,y+1),_brd(x-1,y)};
  } else {
    if(!_xo2(x-1,y)) _line(gr,p,sx,sy,sx,sy+Cell);
    if(!_xo2(x,y-1)) _line(gr,p,sx,sy,sx+Cell,sy);
    if(!_xo2(x+1,y)) _line(gr,p,sx+Cell,sy,sx+Cell,sy+Cell);
    if(!_xo2(x,y+1)) _line(gr,p,sx,sy+Cell,sx+Cell,sy+Cell);
  }
  if(View.corn!=0) _corn(gr,View.corn,3,8,new float[] {sx,sy,sx+Cell,sy,sx+Cell,sy+Cell,sx,sy+Cell},b);
  if(e!=0) {
    if(i2b(e&1)) _line(gr,p,sx+1,sy,sx+1,sy+Cell);
    if(i2b(e&2)) _line(gr,p,sx,sy,sx+Cell,sy);
    if(i2b(e&4)) _line(gr,p,sx+Cell-1,sy,sx+Cell-1,sy+Cell);
    if(i2b(e&8)) _line(gr,p,sx,sy+Cell-1,sx+Cell,sy+Cell-1);
    _borderedge(gr,e,new float[]{sx,sy,sx+Cell,sy,sx+Cell,sy+Cell,sx,sy+Cell});
  }
}

void _drawborder16(Graphics gr,int x,int y,fPoints points,fBorder border) {
  float[] xy=_points(x,y);int[] b=_border(x,y);int i,i2,i3,e=Data[Index(x,y)].block;bool[] o=null;
  Pen p=Pen20;
  if(View.rou) {
    int i1,a;float cx=(xy[0]+xy[6])/2,cy=(xy[1]+xy[7])/2,r=(float)Math.Sqrt(_sqr((xy[0]+xy[2])/2-cx,(xy[1]+xy[3])/2-cy));o=new bool[6];
    for(i1=i=0;i<12;i1++,i+=2) o[i1]=_brd(x+b[i],y+b[i+1]);
    ShiftL(o);Push(ref o,o[0]);
    Push(ref xy,xy[0],xy[1],xy[2],xy[3]);
    for(i1=i=0;i<12;i1++,i+=2) {
      if(!o[i1]&&!o[i1+1]) {a=i*30-120;_arc(gr,p,cx,cy,r,a,60);}
      else if(!o[i1]) _line(gr,p,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3]);
      else if(!o[i1+1]) _line(gr,p,(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,xy[i+2],xy[i+3]);
    }
  } else for(i=0,i2=10;i<12;i2=i,i+=2)
    if(!_xo2(x+b[i],y+b[i+1])) _line(gr,p,xy[i],xy[i+1],xy[i2],xy[i2+1]);
  if(View.corn!=0) _corn(gr,View.corn,3,12,xy,o);
  if(e!=0) {
    Array.Resize(ref xy,12);    
    var xy2=FAC(xy);
    _polyex2(xy,15f/16,0,0);
    for(i=0,i2=10;i<6;i++,i2=(i2+2)%12) if(i2b(e&(1<<i))) {
      i3=(i2+2)%12;
      _line(gr,p,xy[i2],xy[i2+1],xy[i3],xy[i3+1]);
      //_circle2(1,xy[i2],xy[i2+1],xy[i3],xy[i3+1]);
    }
    _borderedge(gr,e,xy2);
  }
}

void _drawborder3x(Graphics gr,int x,int y,fPoints points,fBorder border) {
  float[] xy=points(x,y);int[] b=border(x,y);bool d=i2b(1^(y&1)^(x&1));int dy=d?-1:1,i,j,e=Data[Index(x,y)].block;bool[]o=null;
  Pen p=Pen20;
  int i1,i2,i3;

  if(View.rou) {
    float cx=(xy[0]+xy[4])/2,cy=(xy[1]+xy[5])/2,r=(float)Math.Sqrt(_sqr((xy[0]+xy[2])/2-cx,(xy[1]+xy[3])/2-cy));o=new bool[xy.Length/2];
    for(i1=i=0;i<6;i1++,i+=2) o[i1]=_brd(x+b[i],y+b[i+1]);
    ShiftL(o);Push(ref o,o[0]);
    Push(ref xy,xy[0],xy[1],xy[2],xy[3]);
    for(i1=i=0;i<6;i1++,i+=2) {
      if(!o[i1]&&!o[i1+1]) {
        if(H==H.tria) {i2=i+4;_arc(gr,p,(xy[i2]+xy[i]+xy[i+2])/3,(xy[i2+1]+xy[i+1]+xy[i+3])/3,cell3y/3,d?i*60+210:i*60+30,120);}
        else _bez(gr,p,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3],(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,0.125f);
      } else if(!o[i1]) _line(gr,p,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3]);
      else if(!o[i1+1]) _line(gr,p,(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,xy[i+2],xy[i+3]);
    }
  } else 
    for(i=0,i2=4;i<6;i2=i,i+=2)
      if(!_xo2(x+b[i],y+b[i+1])) _line(gr,p,xy[i],xy[i+1],xy[i2],xy[i2+1]);
  if(View.corn!=0) _corn(gr,View.corn,3,6,xy,o);
  if(i2b(e)) {
    _polyex2(xy,15f/16,0,0);
    for(i=0,i2=4;i<3;i++,i2=(i2+2)%6) if(i2b(e&(1<<i))) {
      i3=(i2+2)%6;
      _line(gr,p,xy[i2],xy[i2+1],xy[i3],xy[i3+1]);
      _circle2(gr,p,1,xy[i2],xy[i2+1],xy[i3],xy[i3+1],Brushes.Black);
    }
  }
}

void _drawborder3(Graphics gr,int x,int y,fPoints points,fBorder border) {
  _drawborder3x(gr,x,y,_points3,_border3);
}
void _drawborder2(Graphics gr,int x,int y,fPoints points,fBorder border) {
  _drawborder3x(gr,x,y,_points2,_border2);
}
void _drawborder5(Graphics gr,int x,int y,fPoints points,fBorder border) {
  _drawborder3x(gr,x,y,_points5,_border5);
}

void _drawborder15(Graphics gr,int x,int y,fPoints points,fBorder border) {
  float[] xy=points(x,y);int[] b=border(x,y);int i,i2,i3,e=Data[Index(x,y)].block,n=xy.Length,n2=n/2;bool[]o=null;
  Pen p=Pen20;
  if(View.rou) {
    int i1;float a,cx,cy,r;o=new bool[n2];
    cx=(xy[0]+xy[6])/2;cy=(xy[1]+xy[7])/2;r=(float)Math.Sqrt(_sqr((xy[0]+xy[2])/2-cx,(xy[1]+xy[3])/2-cy));
    for(i1=i=0;i<10;i1++,i+=2) o[i1]=_brd(x+b[i],y+b[i+1]);
    ShiftL(o);Push(ref o,o[0]);
    Push(ref xy,xy[0],xy[1],xy[2],xy[3]);    
    for(i1=i=0;i<10;i1++,i+=2) {
      if(!o[i1]&&!o[i1+1]) _bez(gr,p,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3],(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,0.125f);
      else if(!o[i1]) _line(gr,p,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3]);
      else if(!o[i1+1]) _line(gr,p,(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,xy[i+2],xy[i+3]);
    }
  } else 
    for(i=0,i2=8;i<10;i2=i,i+=2)
      if(!_xo2(x+b[i],y+b[i+1])) _line(gr,p,xy[i],xy[i+1],xy[i2],xy[i2+1]);
   if(View.corn!=0) _corn(gr,View.corn,3,10,xy,o);
   if(i2b(e)) {
    _polyex2(xy,15f/16,0,0);
    for(i=0,i2=8;i<5;i++,i2=(i2+2)%n) if(i2b(e&(1<<i))) {
      i3=(i2+2)%n;
      _line(gr,p,xy[i2],xy[i2+1],xy[i3],xy[i3+1]);
      _circle2(gr,p,1,xy[i2],xy[i2+1],xy[i3],xy[i3+1],Brushes.Black);
    }
  }
}

void _drawborder8(Graphics gr,int x,int y,fPoints points,fBorder border) {
  float[] xy=points(x,y);int[] b=border(x,y);int i,i2,i3,e=Data[Index(x,y)].block,n=xy.Length,n2=n/2;bool[]o=null;
  Pen p=Pen20;
  if(View.rou) {
    int i1;float a,cx,cy,r;o=new bool[n2];
    if(H==H.cubes||H==H.penta||H==H.trap||H==H.deca) {cx=(xy[0]+xy[4])/2;cy=(xy[1]+xy[5])/2;r=(float)Math.Sqrt(_sqr((xy[0]+xy[2])/2-cx,(xy[1]+xy[3])/2-cy));}
    else if(H==H.penta) {cx=(xy[0]+xy[6])/2;cy=(xy[1]+xy[7])/2;r=(float)Math.Sqrt(_sqr((xy[0]+xy[2])/2-cx,(xy[1]+xy[3])/2-cy));}
    for(i1=i=0;i<n;i1++,i+=2) o[i1]=_brd(x+b[i],y+b[i+1]);
    ShiftL(o);Push(ref o,o[0]);
    Push(ref xy,xy[0],xy[1],xy[2],xy[3]);    
    for(i1=i=0;i<n;i1++,i+=2) {
      if(!o[i1]&&!o[i1+1]) _bez(gr,p,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3],(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,0.125f);
      else if(!o[i1]) _line(gr,p,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3]);
      else if(!o[i1+1]) _line(gr,p,(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,xy[i+2],xy[i+3]);
    }
  } else 
    for(i=0,i2=n-2;i<n;i2=i,i+=2)
      if(!_xo2(x+b[i],y+b[i+1])) _line(gr,p,xy[i],xy[i+1],xy[i2],xy[i2+1]);
  if(View.corn!=0) _corn(gr,View.corn,3,n,xy,o);
  if(i2b(e)) {
    _polyex2(xy,15f/16,0,0);
    for(i=0,i2=n-2;i<n2;i++,i2=(i2+2)%n) if(i2b(e&(1<<i))) {
      i3=(i2+2)%n;
      _line(gr,p,xy[i2],xy[i2+1],xy[i3],xy[i3+1]);
      _circle2(gr,p,1,xy[i2],xy[i2+1],xy[i3],xy[i3+1],Brushes.Black);
    }
  }
}

void _drawsele(Graphics gr,int cx,int cy) {
   int i;float[] xy=_points(cx,cy);float x,y,x2,y2;
   var p=new GraphicsPath();
   x2=xy[xy.Length-2];y2=xy[xy.Length-1];
   for(i=0;i<xy.Length;i+=2) {
     p.AddLine(x2,y2,x=xy[i],y=xy[i+1]);x2=x;y2=y;
   }
   gr.DrawPath(Pen30,p);
   gr.DrawPath(Pen10W,p);
}


float[] _bezsplit(float p,bool s,float a,float b,float c,float d) {
  float q=1-p,p2=p*p,q2=q*q;
  if(s) {
    return new float[] {q2*q*a+3*q2*p*b+3*q*p2*c+p2*p*d,q2*b+2*p*q*c+p2*d,q*c+p*d,d};
  } else {
    return new float[] {a,q*a+p*b,q2*a+2*p*q*b+p2*c,q2*q*a+3*q2*p*b+3*q*p2*c+p2*p*d};
  }
}

void _bez3x(List<float> p,bool s,bool c,float x,float y,float x1,float y1,float x2,float y2,float r,bool m) {
  if(c) {
    if(s) {x=x1;y=y1;x1=x2;y1=y2;}
    if(m) Push(p,'m',x,y);
    Push(p,'l',x1,y1);
  } else {
    float r1=1-r;
    float[] xa=_bezsplit(0.5f,s,x,r1*x1+r*x,r1*x1+r*x2,x2),ya=_bezsplit(0.5f,s,y,r1*y1+r*y,r1*y1+r*y2,y2);
    if(m) Push(p,'m',xa[0],ya[0]);
    Push(p,'c',xa[1],ya[1],xa[2],ya[2],xa[3],ya[3]);
  }
}


      static float _sqr(float x,float y) { return x*x+y*y;}
      static int floor(float x) { return (int)Math.Floor(x);}
      static int ceil(float x) { return (int)Math.Ceiling(x);}
      static int sqrt(float x) { return (int)Math.Sqrt(x);}
      static float abs(float x) { return x<0?-x:x;}

float _min(float x,float y) { return y<x?y:x;}
bool _near(float x,float y,float[] a,float[] b) {
  return _sqr(x-a[0],y-a[1])<_sqr(x-b[0],y-b[1]);
}
bool _near2(float x,float y,float x2,float y2,float[] a,float[] b) {
  float da=_min(_sqr(x-a[0],y-a[1]),_sqr(x2-a[0],y-a[1]));
  float db=_min(_sqr(x-b[0],y-b[1]),_sqr(x2-b[0],y2-b[1]));
  return da<db;
}

public static int _idx(float ex,float ey,float[] xy,int v,float md) {
  int i=0,n=xy.Length,f=0;bool v2=v>1,v3=v>2;float x=xy[n-2],y=xy[n-1],x2,y2,d,m=float.PositiveInfinity;
  ex*=2;ey*=2;
  while(i<n) {
    x2=x;y2=y;x=xy[i++];y=xy[i++];
    if(md>0&&md>_sqr(2*x-ex,2*y-ey)) return -1;
    if(v3) {
       float x3=2*x,y3=2*y;
       if((d=_sqr(x3-ex,y3-ey))<m) {f=i|64;m=d;}
       x2+=x;y2+=y;
    } else if(v2) {
       float x3=(3*x2+x)/2,y3=(3*y2+y)/2;
       if((d=_sqr(x3-ex,y3-ey))<m) {f=i|64;m=d;}
       x2=(3*x+x2)/2;y2=(3*y+y2)/2;
    } else if(v!=0) {x2=2*x;y2=2*y;}
    else {x2+=x;y2+=y;}
    if((d=_sqr(x2-ex,y2-ey))<m) {f=i;m=d;}
  }
  //console.log("_idx",[(f>>1)-1,m]);
  i=(((f&63)>>1)-1)|(f&64);
  return i;
}


      static void _polyex3(float[] xy,float e) {
        if(e==0||e==1) return;
        int i,i2,n=xy.Length;float dx,dy,d,f;float[] da=new float[n],va=new float[n];
        for(i2=n-2,i=0;i<n;i2=i,i+=2) {
          d=(float)Math.Sqrt(_sqr(dx=xy[i]-xy[i2],dy=xy[i+1]-xy[i2+1]));
          da[i2]=dx/d;da[i2+1]=dy/d;
        }
        for(i2=n-2,i=0;i<n;i2=i,i+=2) {
          va[i]=da[i]-da[i2];va[i+1]=da[i+1]-da[i2+1];
        }
        for(i2=n-2,i=0;i<n;i2=i,i+=2) {
          dx=va[i];dy=va[i+1];
          d=_sqr(dx,dy);
          if(d>0.5) {
            d=(float)Math.Sqrt(_sqr(dx,dy));
            dx/=d;dy/=d;
            d=1/(-da[i+1]*dx+da[i]*dy);
          } else {
            dx=-da[i+1];dy=da[i];
            d=(float)Math.Sqrt(_sqr(dx,dy));
          }
          xy[i]+=e*dx*d;xy[i+1]+=e*dy*d;
        }
      }
      float[] _polyex2(float[] xy,float e,float dx,float dy) {
        int i,n;float x,y,a,b;
        if(e!=1) {
           if(H==H.delta||H==H.tria2||H==H.tria4||H==H.trap||H==H.deca) {
             _polyex3(xy,e==15f/16?1:4);
             dx=dy=0;
             return xy;
            } else {
              for(x=y=i=0;i<xy.Length;i+=2) {x+=xy[i];y+=xy[i+1];}
              i>>=1;x/=i;y/=i;
            } 
            for(i=0;i<xy.Length;i+=2) {xy[i]=dx+x+e*(xy[i]-x);xy[i+1]=dy+y+e*(xy[i+1]-y);}
         }
         return xy;
      }
PointF[] _polyex(float[] xy,float e,float dx,float dy) {
  _polyex2(xy,e,dx,dy);
  var pa=new PointF[xy.Length/2];
  for(int i=0,i1=0;i<xy.Length;i+=2,i1++)
    pa[i1]=new PointF(xy[i],xy[i+1]);   
  return pa;
}

      public int Draw(Graphics gr) {
         int x,y,n=0,m=0,i,c,t=_ticks();
         bool p=false;
         fDrawCell fdc=H==H.deca?_drawcell7:H==H.trap?_drawcell11:H==H.delta?_drawcell9:H==H.cubes?_drawcell8:H==H.penta?_drawcell15:H==H.tria4?_drawcell5:H==H.tria2?_drawcell2:H==H.tria?_drawcell3:H==H.hexa?_drawcell16:(fDrawCell)_drawcell;
         fDrawPeg fdp=H==H.deca?_drawpeg7:H==H.trap?_drawpeg11:H==H.delta?_drawpeg9:H==H.cubes?_drawpeg8:H==H.penta?_drawpeg15:H==H.tria4?_drawpeg5:H==H.tria2?_drawpeg2:H==H.tria?_drawpeg3:H==H.hexa?_drawpeg16:(fDrawPeg)_drawpeg;
         fDrawBorder fdb=H==H.deca||H==H.trap?_drawborder8:H==H.delta?_drawborder8:H==H.cubes?_drawborder8:H==H.penta?_drawborder15:H==H.tria4?_drawborder5:H==H.tria2?_drawborder2:H==H.tria?_drawborder3:H==H.hexa?_drawborder16:H==H.quad?_drawborder:(fDrawBorder)DrawBorder;
         for(y=i=0;y<Height;y++)
           for(x=0;x<Width;x++,i++) {
             if((c=Data[i].ch)>0) {
               fdc(gr,x,y);
               if(c>1) {
                 fdp(gr,x,y,c==3);
               }
             }
           }
         var pf=_pointsf(H);var bf=_borderf(H);
         for(y=i=0;y<Height;y++)
           for(x=0;x<Width;x++,i++) {
             if((c=Data[i].ch)>0)
               fdb(gr,x,y,pf,bf);
           }
         var se=View.sele;
         for(i=0;i<se.Count;i+=2)
           _drawsele(gr,se[i],se[i+1]);
         t=_ticks()-t;
         return n;
      }
static float[] _points2(int x,int y) {
  float sx=brd+((x+1)>>1)*cell2,sx2=sx+cell2,sy=brd+y*cell2,sy2=sy+cell2,r,xx=(x+1+2*(y&1))&3;
  if(xx==0) return FA(sx,sy,sx2,sy,sx,sy2);
  if(xx==1) return FA(sx2,sy2,sx,sy2,sx2,sy);
  if(xx==2) return FA(sx,sy2,sx,sy,sx2,sy2);
  return FA(sx2,sy,sx2,sy2,sx,sy);
  //if(!((x^y^((x+1)>>1))&1)) r=sy,sy=sy2,sy2=r;
  //if(!(x&1)) r=sx,sx=sx2,sx2=r;
  //return [sx,sy,sx2,sy,sx,sy2];
}

static float[] _points5(int x,int y) {
  float sx=brd+((x+1)>>1)*cell5,sx2=sx+cell5,sy=brd+((y+1)>>1)*cell5,sy2=sy+cell5,cx=sx+cell5/2,cy=sy+cell5/2,r=(((y+1)&1)<<1)|((x+1)&1);
  if(r==3) return FA(cx,cy,sx2,sy,sx2,sy2);
  if(r==2) return FA(cx,cy,sx2,sy2,sx,sy2);
  if(r==1) return FA(cx,cy,sx,sy,sx2,sy);
  return FA(cx,cy,sx,sy2,sx,sy);
}

static float[] _points14(int x,int y) {
        float sx=brd+x*Cell,sy=brd+y*Cell;
        return new float[]{sx,sy,sx+Cell,sy,sx+Cell,sy+Cell,sx,sy+Cell};
      }

static float[] _points11(int x,int y) {
  int d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d;
  float sx=brd+(cx+(y&1)/2f)*cell8x,sy=brd+((1+y*3)/4f)*cell8y,x2=cell8x/2,y2=cell8y/4;
  if(dd==1) return FA(sx,sy+3*y2 ,sx,sy+y2 ,sx+x2,sy,sx+x2,sy+4*y2);
  else if(dd==2) return FA(sx,sy+y2,sx+x2,sy,sx+2*x2,sy+y2,sx,sy+3*y2);
  else if(dd==3) return FA(sx+x2,sy ,sx+2*x2,sy+y2 ,sx+2*x2,sy+3*y2,sx,sy+y2 );
  else if(dd==4) return FA(sx+2*x2,sy+y2 ,sx+2*x2,sy+3*y2 ,sx+x2,sy+4*y2,sx+x2,sy );
  else if(dd==5) return FA(sx+2*x2,sy+3*y2 ,sx+x2,sy+4*y2 ,sx,sy+3*y2,sx+2*x2,sy+y2);
  return FA(sx+x2,sy+4*y2 ,sx,sy+3*y2,sx,sy+y2,sx+2*x2,sy+3*y2);
}
static float[] _points11h(int x,int y) {
  int d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d;
  float sx=brd+(cx+(y&1)/2f)*cell8x,sy=brd+((1+y*3)/4f)*cell8y,x2=cell8x/2f,y2=cell8y/4f;
  return FA(sx,sy+3*y2 ,sx,sy+y2 ,sx+x2,sy,sx+2*x2,sy+y2,sx+2*x2,sy+3*y2,sx+x2,sy+4*y2);
}


static float[] _points9(int x,int y) {
  int r=(x+2)%3,cx=(x+2-r)/3,d=1^(y&1)^(cx&1),dy=i2b(d)?-1:1;
  float sx=brd+cx*cell9x/2,sy=brd+(y+d)*cell9y,y2=sy+dy*cell9y,y3=(2*sy+y2)/3,y4=(sy+y2)/2,x4=sx+cell9x/2;
  if(i2b(d)) {
    if(r==1) return FA(x4,y2,sx+cell9x*3/4,y4,x4,y3,sx+cell9x/4,y4);
    if(r==2) return FA(sx+cell9x,sy,x4,sy,x4,y3,sx+cell9x*3/4,y4);
    return FA(sx,sy,sx+cell9x/4,y4,x4,y3,x4,sy);
  } else {
    if(r==1) return FA(x4,y2,sx+cell9x/4,y4,x4,y3,sx+cell9x*3/4,y4);
    if(r==2) return FA(sx+cell9x,sy,sx+cell9x*3/4,y4,x4,y3,x4,sy);
    return FA(sx,sy,x4,sy,x4,y3,sx+cell9x/4,y4);
  }
}

static float[] _points8(int x,int y) {
  int d=(x+2)%3,cx=(x+2-d)/3;
  float sx=brd+(cx+(y&1)/2f)*cell8x,sy=brd+((1+y*3)/4f)*cell8y,x2=cell8x/2,y2=cell8y/4;
  if(d==1) return FA(sx+x2,sy+4*y2,sx,sy+3*y2,sx,sy+y2,sx+x2,sy+2*y2);
  else if(d==2) return FA(sx+2*x2,sy+y2,sx+2*x2,sy+3*y2,sx+x2,sy+4*y2,sx+x2,sy+2*y2);
  return FA(sx,sy+y2,sx+x2,sy,sx+2*x2,sy+y2,sx+x2,sy+2*y2);
}

static float[] _points7(int x,int y) {
  int d=(x+2)%3,cx=(x+2-d)/3;
  float sx=brd+(cx+(y&1)/2f)*cell8x,sy=brd+((1+y*3)/4f)*cell8y,x2=cell8x/2,y2=cell8y/4,x3,y3;
  if(d==1) return FA(sx+2*x2,sy+2*y2,sx+2*x2,sy+3*y2,sx+3*x2/2,sy+7*y2/2,sx+x2,sy+2*y2,sx+3*x2/2,sy+y2/2,sx+2*x2,sy+y2);
  else if(d==2) return FA(sx+x2/2,sy+7*y2/2,sx,sy+3*y2,sx,sy+2*y2,sx+x2,sy+2*y2,sx+3*x2/2,sy+7*y2/2,sx+x2,sy+4*y2);
  return FA(sx+x2/2,sy+y2/2,sx+x2,sy,sx+3*x2/2,sy+y2/2,sx+x2,sy+2*y2,sx,sy+2*y2,sx,sy+y2);
}



static float[] _points15(int x,int y) {
  int cx=(x+1)/2|0,x1=x&1,v=(cx+y)&1;float sx=brd+cx*cell7,sy=brd+y*cell7,c2=cell7/2,c4=c2/2;
  if(i2b(v)) {
    if(i2b(x1)) 
      return FA(sx,sy+cell7,sx-c4,sy+c2,sx,sy,sx+c2,sy+c4,sx+c2,sy+cell7-c4);
    else {
      sx+=cell7;
      return FA(sx,sy,sx+c4,sy+c2,sx,sy+cell7,sx-c2,sy+cell7-c4,sx-c2,sy+c4);
    }
  } else {
    if(i2b(x1)) 
      return FA(sx,sy,sx+c2,sy-c4,sx+cell7,sy,sx+cell7-c4,sy+c2,sx+c4,sy+c2);
    else {
      sy+=cell7;
      return FA(sx+cell7,sy,sx+c2,sy+c4,sx,sy,sx+c4,sy-c2,sx+cell7-c4,sy-c2);
    }
  }
}

static float[] _points3(int x,int y) {
  int d=1^(y&1)^(x&1),dy=i2b(d)?-1:1;float sx=brd+x*cell3x/2,sy=brd+(y+d)*cell3y,y2=sy+dy*cell3y;
  return i2b(d)?FA(sx,sy,sx+cell3x/2,y2,sx+cell3x,sy):FA(sx+cell3x,sy,sx+cell3x/2,y2,sx,sy);
}

static float[] _points16(int x,int y) {
  float sx=brd+(x+(y&1)/2f)*cell6y,sy=brd+((1+y*3)/4f)*cell6x,x2=sx+cell6y/2,x3=sx+cell6y;
  return FA(sx,sy,x2,sy-cell6x/4,x3,sy,x3,sy+cell6x/2,x2,sy+cell6x*3/4,sx,sy+cell6x/2);
}

public float[] _points(int x,int y) {
  switch(H) {
   case H.deca:return _points7(x,y);
   case H.trap:return _points11(x,y);
   case H.delta:return _points9(x,y);
   case H.cubes:return _points8(x,y);
   case H.penta:return _points15(x,y);
   case H.tria4:return _points5(x,y);
   case H.tria2:return _points2(x,y);
   case H.tria:return _points3(x,y);
   case H.hexa:return _points16(x,y);
   default: return _points14(x,y);
      }
}
static public fPoints _pointsf(H h) {
  switch(h) {
   case H.deca:return _points7;
   case H.trap:return _points11;
   case H.delta:return _points9;
   case H.cubes:return _points8;
   case H.penta:return _points15;
   case H.tria4:return _points5;
   case H.tria2:return _points2;
   case H.tria:return _points3;
   case H.hexa:return _points16;
   default: return _points14;
      }
}

int[] _border(int x,int y) {
  switch(H) {
   case H.deca:return _border7(x,y);
   case H.trap:return _border11(x,y);
   case H.delta:return _border9(x,y);
   case H.cubes:return _border8(x,y);
   case H.penta:return _border15(x,y);
   case H.tria4:return _border5(x,y);
   case H.tria2:return _border2(x,y);
   case H.tria:return _border3(x,y);
   case H.hexa:return _border16(x,y);
   default: return _border4(x,y);
  }
}
static fBorder _borderf(H h) {
  switch(h) {
   case H.deca:return _border7;
   case H.trap:return _border11;
   case H.delta:return _border9;
   case H.cubes:return _border8;
   case H.penta:return _border15;
   case H.tria4:return _border5;
   case H.tria2:return _border2;
   case H.tria:return _border3;
   case H.hexa:return _border16;
   default: return _border4;
  }
}


static int[] _border3(int x,int y) {
  int d=1^(y&1)^(x&1),dy=d!=0?-1:1;
  return d!=0?IA(0,-dy,-1,0,1,0):IA(0,-dy,1,0,-1,0);
}

static int[] _border16(int x,int y) {
  int dx=(y&1)!=0?0:-1;
  return IA(-1,0,dx,-1,dx+1,-1,1,0,dx+1,1,dx,1);
}

static int[] _border2(int x,int y) {
  int xx=(x+1+2*(y&1))&3,r;
  if(xx==0) return IA(-1,0,0,-1,1,0);
  if(xx==1) return IA(1,0,0,1,-1,0);
  if(xx==2) return IA(0,1,-1,0,1,0);
  return IA(0,-1,1,0,-1,0);
}

static int[] _border4(int x,int y) {
  return IA(-1,0,0,-1,1,0,0,1);
}

static int[] _border5(int x,int y) {
  int r=(((1^y)&1)<<1)|(1^x&1);
  if(r==1) return IA(0,1,-1,0,-1,-1);
  else if(r==2) return IA(0,-1,1,0,1,1);
  else if(r==3) return IA(-1,0,0,-1,1,-1);
  else return IA(1,0,0,1,-1,1);
}

static int[] _border9(int x,int y) {
  int r=(x+2)%3,cx=(x+2-r)/3,d=1^(y&1)^(cx&1),dy=d!=0?-1:1;
  if(d!=0) {
    if(r==1) return IA(-2,0,2,0,1,0,-1,0);
    if(r==2) return IA(2,0,0,-dy,-2,0,-1,0);
    return IA(0,-dy,-2,0,1,0,2,0);
  } else {
    if(r==1) return IA(2,0,-2,0,-1,0,1,0);
    if(r==2) return IA(0,-dy,2,0,-1,0,-2,0);
    return IA(-2,0,0,-dy,2,0,1,0);
  }
}

static int[] _border15(int x,int y) {
  int cx=(x+1)/2|0,x1=x&1,v=(cx+y)&1;int[] b;
  if(v!=0) {
    if(x1!=0) b=IA(0,1,-1,0,-2,0,1,-1,1,0);
    else b=IA(0,-1,1,0,2,0,-1,1,-1,0);
  } else {
    if(x1!=0) b=IA(-1,0,0,-1,1,-1,2,0,1,0);
    else b=IA(1,0,0,1,-1,1,-2,0,-1,0);
  }
  return b;
}

static int[] _border8(int x,int y) {
  int d=(x+2)%3,dx=i2b(y&1)?0:-3;
  if(d==1) return IA(1,0,dx-1,1,-2,0,-1,0);
  if(d==2) return IA(-2,0,2,0,dx+1,1,-1,0);
  return IA(1,0,dx+2,-1,dx+4,-1,2,0);
}

static int[] _border7(int x,int y) {
  int d=(x+2)%3,dx=i2b(y&1)?0:-3;
  if(d==1) return IA(2,0,4,0,dx+2,1,1,0,-1,0,dx+4,-1);
  if(d==2) return IA(dx-1,1,dx-2,1,-4,0,-2,0,-1,0,dx+1,1);
  return IA(dx+2,-1,dx+1,-1,dx+5,-1,1,0,2,0,-2,0);
}


static int[] _border18(int x,int y) {
  int d=(x+2)%3,cx=(x+2-d)/3,dx=(y&1)!=0?0:-3;
  if(d==1) return IA(1,0,3,0,dx+2,1,dx+3,1  ,dx-1,1,dx,1,dx+1,1,dx+3,1  ,-2,0,-3,0,-4,0,dx,-1  ,-1,0,dx+3,-1,dx+1,-1,dx,-1);
  if(d==2) return IA(-2,0,dx,-1,dx+2,-1,dx+3,-1 ,2,0,3,0,1,0,dx+3,-1 ,dx+1,1,dx+3,1,dx+2,1,dx,1 ,-1,0,-3,0,dx-2,1,dx,1);
  return IA(1,0,dx,+1,-1,0,-3,0  ,dx+2,-1,dx,-1,dx+1,-1,-3,0  ,dx+4,-1,dx+3,-1,dx+5,-1,3,0  ,2,0,dx+3,1,4,0,3,0);
}

static int[] _border11(int x,int y) {
  int d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d,dx=(y&1)!=0?0:-2;
  if(dd==1) return IA(dx,1,-1,0,dx+1,-1,1,0);
  if(dd==2) return IA(-1,0,dx,-1,dx+2,-1,1,0);
  if(dd==3) return IA(dx,-1,dx+2,-1,1,0,-1,0);
  if(dd==4) return IA(dx+1,-1,1,0,dx+2,1,-1,0);
  if(dd==5) return IA(1,0,dx+1,1,dx,1,-1,0);
  return IA(dx+2,1,dx+1,1,-1,0,1,0);
}

  float[] _peg(int x,int y) {
  if(H==H.deca) {
    int d=(x+2)%3,cx=(x+2-d)/3;
    float sx=brd+(cx+(y&1)/2f)*cell8x,sy=brd+((1+y*3)/4f)*cell8y,x2=cell8x/2,y2=cell8y/4,r=y2*3/4-2,bx,by,px=sx+x2,py=sy+2*y2;
    if(d==1) {bx=x2;by=0;}
    else if(d==2) {bx=-x2/2;by=3*y2/2;}
    else {bx=-x2/2;by=-3*y2/2;}
    return FA(px+1.06f*bx/2,py+1.06f*by/2,r);
  } if(H==H.trap) {
    int d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d;
    float sx=brd+(cx+(y&1)/2f)*cell8x,sy=brd+((1+y*3)/4f)*cell8y,x2=cell8x/2,y2=cell8y/4,r2=cell8x/5;
    return dd==1?FA(sx+x2-x2/2,sy+2*y2,r2):dd==2?FA(sx+x2-x2*2/8,sy+2*y2-y2*6/8,r2):dd==3?FA(sx+x2+x2*2/8,sy+2*y2-y2*6/8,r2)
      :dd==4?FA(sx+x2+x2/2,sy+2*y2,r2):dd==5?FA(sx+x2+x2*2/8,sy+2*y2+y2*6/8,r2):FA(sx+x2-x2*2/8,sy+2*y2+y2*6/8,r2);
  } else if(H==H.delta) {
    int r=(x+2)%3,cx=((x+2-r)/3)|0,d=1^(y&1)^(cx&1),dy=d!=0?-1:1;float r2=cell9x/7;
    float sx=brd+cx*cell9x/2,sy=brd+(y+d)*cell9y,y2=sy+dy*cell9y,y3=(2*sy+y2)/3,y4=(sy+y2)/2,x4=sx+cell9x/2;
    return r==1?FA(x4,(5*y3+3*y2)/8,r2):r==2?FA((5*x4+3*(sx+cell9x))/8,(5*y3+3*sy)/8,r2):FA((5*x4+3*sx)/8,(5*y3+3*sy)/8,r2);
  } else if(H==H.cubes) {
    int d=(x+2)%3,cx=(x+2-d)/3;
    float sx=brd+(cx+(y&1)/2f)*cell8x,sy=brd+((1+y*3)/4f)*cell8y,x2=cell8x/2,y2=cell8y/4,r=y2*3/4-2;
    return d!=0?d==1?FA(sx+x2/2,sy+5*y2/2,r):FA(sx+3*x2/2,sy+5*y2/2,r):FA(sx+x2,sy+y2,r);
  } else if(H==H.penta) {
    float d=4.5f,cx=(x+1)/2|0,x1=x&1,sx=brd+cx*cell7,sy=brd+y*cell7,v=f2i(cx+y)&1,c2=cell7/2,c4=c2/2;
    return FA(sx+(v!=0?x1!=0?c4-d:c2+c4+d:c2),sy+(v!=0?c2:x1!=0?c4-d:c2+c4+d),c4+2);
  } else if(H==H.tria2) {
    float[] xy=_points2(x,y);float sx=(45*xy[0]+19*(xy[2]+xy[4]-xy[0]))/64,sy=(45*xy[1]+19*(xy[3]+xy[5]-xy[1]))/64;
    return FA(sx,sy,Cell*9/32);
  } else if(H==H.tria4) {
    float[] xy=_points5(x,y);float sx=(26*xy[0]+19*(xy[2]+xy[4]))/64,sy=(26*xy[1]+19*(xy[3]+xy[5]))/64;
    return FA(sx,sy,Cell*8/32);
  } else if(H==H.tria) {
    int d=1^(y&1)^(x&1),dy=d!=0?-1:1;float sx=brd+(x+1)*cell3x/2,sy=brd+(y+d)*cell3y;
    return FA(sx,sy+dy*cell3y/3,cell3y*36/128);
  } else if(H==H.hexa) {
    float sx=brd+(x+(y&1)/2f)*cell6y,sy=brd+((1+y*3)/4f)*cell6x,x2=cell6y/2,y2=cell6x/4;
    return FA(sx+x2,sy+y2,x2-3);
  } else return FA(brd+(x+0.5f)*Cell,brd+(y+0.5f)*Cell,Cell/2-3);

      }
      int[] Border4(int x,int y) {
        return new int[] {-1,0,0,-1,1,0,0,1};
      }
      
      public static void Back(Graphics gr,float w,float h,float x=0,float y=0) {
        LinearGradientBrush lg=new LinearGradientBrush(new PointF(x,y),new PointF(x+w,x+h),Pal.IntColor(0xccbbaa),Pal.IntColor(0xaabbcc));
        gr.FillRectangle(lg,x,y,w,h);
      }

      public void ExportSvg(string s) {
        int w=BWidth(),h=BHeight();
        string svg="<svg width=\""+w+"\" height=\""+h+"\" xmlns=\"http://www.w3.org/2000/svg\" >\n"+_svg()+"\n</svg>";
        using(var sw=new StreamWriter(s,false)) {
          sw.Write(svg);
        }
      }
      float _svgx(float x) { return (float)Math.Round(x,3,MidpointRounding.AwayFromZero);}
      string _svgline(float w,float x,float y,float x2,float y2) {
        return string.Format("<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" style=\"stroke:#000;stroke-width:{4};\" />\r\n",x,y,x2,y2,w);
      }

      string _svgcircle(float w,int c,float r,float x,float y) {
        return string.Format("<circle cx=\"{0}\" cy=\"{1}\" r=\"{2}\" style=\"fill:{3};stroke:#000;stroke-width:{4};\" />\r\n",x,y,r,Pal.IntHtml(c),w);
      }

      string _svgpoly(float w,int c,float[] xy,string gn) {
        int i;string s="",s1="",sc;
        for(i=0;i<xy.Length;i+=2)
          s+=(s==""?"":" ")+xy[i]+','+xy[i+1];
        if(gn!="") {
          float[] rp=_radial_xy(xy);
          s1=svgg[gn]="<defs>"+_svg_radialp(gn,c,rp[0],rp[1],rp[2],true,-1)+"</defs>\r\n";
          sc="url(#"+gn+")";
        } else sc=Pal.IntHtml(c);
        return s1+"<polygon points=\""+s+"\" style=\"fill:"+sc+";stroke:#000;stroke-width:"+w+";\" />\r\n";
      }
    
   public static void Push(ref int[] a,params int[] p) {
     int n=a.Length,i;Array.Resize(ref a,n+p.Length);
     for(i=0;i<p.Length;i++) a[n+i]=p[i];
   }
   public static void Push(ref float[] a,params float[] p) {
     int n=a.Length,i;Array.Resize(ref a,n+p.Length);
     for(i=0;i<p.Length;i++) a[n+i]=p[i];
   }
   public static void Push(ref bool[] a,params bool[] p) {
     int n=a.Length,i;Array.Resize(ref a,n+p.Length);
     for(i=0;i<p.Length;i++) a[n+i]=p[i];
   }
   public static void Push(List<float> a,params float[] p) {
     foreach(var x in p) a.Add(x);
   }
   public static List<int> Push(List<int> a,params int[] p) { 
     foreach(var x in p) a.Add(x);
     return a;
   }
   public List<int> IL(params int[] p) { return Push(null as List<int>,p);}
   public static float[] FA(params float[] fa) { return fa;}
   public static float[] FAC(params float[] fa) { return fa.Clone() as float[];}
   public static int[] IA(params int[] ia) { return ia;}
   public static float[] A(ref float[] a,int i,int v) {
     if(a.Length<=i) Array.Resize(ref a,i+1);
     a[i]=v;return a;
   }
   public static void ShiftL(bool[] ba) {
     bool b=ba[0];
     for(int i=1;i<ba.Length;i++) ba[i-1]=ba[i];
     ba[ba.Length-1]=b;
   }
   public static bool B0=false,B1=true;
   static Dictionary<int,int> DII() { return new Dictionary<int, int>();}   

string _svggpoly(float px,float py,float w,int c,float[] xy,bool[] b,bool m) {
  string d,s="",s2="",s3="",k="1",gn;float rr=(float)(H==H.quad?Math.Sqrt(2)/3:H==H.hexa?Math.Sqrt(3)/3:H==H.tria?Math.Sqrt(2)/3:0.125);int l=xy.Length;
  int i,i1,g;float x=xy[xy.Length-2],y=xy[xy.Length-1],x2,y2,x1,y1,sx,sy,lx,ly,nx=(xy[0]+xy[l-2])/2,ny=(xy[1]+xy[l-1])/2,mx,my,rx,ry;
  sx=_polyc(xy,out sy);
  if(b!=null) Push(ref b,b[0],b[1]);
  for(i=i1=0;i<xy.Length;i1++,i+=2) {
    x2=x;y2=y;x=xy[i];y=xy[i+1];mx=xy[(i+2)%xy.Length];my=xy[(i+3)%xy.Length];
    lx=nx;ly=ny;nx=(xy[i]+mx)/2;ny=(xy[i+1]+my)/2;
    gn=""+px+'_'+py+'_'+i1;
    rx=m?nx:x;ry=m?ny:y;s+=svgg[gn]="<defs>"+_svg_radialp(gn,0,rx,ry,(float)Math.Sqrt(_sqr(rx-sx,ry-sy)),B1,c)+"</defs>\r\n";
    if(m) {
      float qx=xy[(i+4)%l],qy=xy[(i+5)%l];var p=new List<float>();
      if(b==null||((b[i1]||b[i1+1])&&(b[i1+1]||b[i1+2]))) {
        Push(p,'m',x,y,'l',mx,my);
      } else {
        _bez3x(p,B1,b==null||b[i1]||b[i1+1],lx,ly,x,y,nx,ny,rr,B1);
        _bez3x(p,B0,b==null||b[i1+1]||b[i1+2],nx,ny,mx,my,(mx+qx)/2,(my+qy)/2,rr,B0);
      }
      Push(p,'l',sx,sy);
      d=_svg_pathx(p);
      s+="<path d=\""+d+" Z\" style=\"fill:url(#"+gn+");stroke:none;\" />\r\n";
    }
    float[] xy2=new float[] {lx,ly,x,y,nx,ny,sx,sy};
    if(s2=="") s2="M"+lx+","+ly;
    if(b==null||b[i1]||b[i1+1]) {
      if(!m) s+=_svgpoly(0,-1,xy2,gn);
      s2+=" L"+x+","+y+" L"+nx+","+ny;
    } else {
      d="M"+lx+","+ly+" "+(s3=_svg_bez2(B0,lx,ly,x,y,nx,ny,rr))+" L"+sx+","+sy;
      if(!m) s+="<path d=\""+d+" Z\" style=\"fill:url(#"+gn+");stroke:none;\" />\r\n";
      s2+=s3;
    }
  }
  if(w>0) {
    if(s3!="") s+="<path d=\""+s2+"\" style=\"fill:none;stroke:#000;stroke-width:"+w+"\" />\r\n";
    else s+=_svgpoly(w,-1,xy,"");
  }
  return s;
}

string _svg_arc(float w,float x,float y,float x2,float y2,float r) { 
  float r1=1-r;
  return "<path d=\"M"+x+","+y+" A"+r+","+r+" 0 0 1 "+x2+","+y2+"\" style=\"fill:none;stroke:#000;stroke-width:"+w+";\" />\r\n";
}

string _svg_arcf(float x,float y,float x2,float y2,float r,float sx,float sy) { 
  return "<path d=\"M"+sx+","+sy+" L"+x+","+y+" A"+r+","+r+" 0 0 0 "+x2+","+y2+" Z\" style=\"fill:#000;stroke:none;\" />\r\n";
}


delegate string f_svg_arc2(bool c,float x,float y,float x1,float y1,float x2,float y2,float r);
string _svg_arc2(bool c,float x,float y,float x1,float y1,float x2,float y2,float r) { 
  if(c) return " L"+x1+","+y1+" L"+x2+","+y2;
  else {
   return " A"+r+","+r+" "+" 0 0 1 "+x2+","+y2;
  }
}

string _svg_bez(float w,float x,float y,float x1,float y1,float x2,float y2,float r) { 
  var r1=1-r;
  return "<path d=\"M"+x+","+y+" C"+(r1*x1+r*x)+","+(r1*y1+r*y)+" "+(r1*x1+r*x2)+","+(r1*y1+r*y2)+" "+x2+","+y2+"\" style=\"fill:none;stroke:#000;stroke-width:"+w+";\" />\r\n";
}

string _svg_bez2(bool c,float x,float y,float x1,float y1,float x2,float y2,float r) { 
  if(c) return " L"+x1+","+y1+" L"+x2+","+y2;
  else {
   var r1=1-r;
   return " C"+(r1*x1+r*x)+","+(r1*y1+r*y)+" "+(r1*x1+r*x2)+","+(r1*y1+r*y2)+" "+x2+","+y2;
  }
}
string _svg_pathx(List<float> r) { 
  int i;string s="",ch;
  for(i=0;i<r.Count;)
    switch((char)r[i++]) {
     case 'm': s+=" M"+r[i]+","+r[i+1];i+=2;break;
     case 'l': s+=" L"+r[i]+","+r[i+1];i+=2;break;
     case 'c': s+=" C"+r[i]+","+r[i+1]+" "+r[i+2]+","+r[i+3]+" "+r[i+4]+","+r[i+5];i+=6;break;
    }
  return s;
}

string _svg_path(float w,string c,float[] xy,bool[] o,float rr) {
  string s="";int i,i1;float lx,ly,nx,ny;f_svg_arc2 f2=H==H.quad||H==H.hexa||H==H.tria?(f_svg_arc2)_svg_arc2:_svg_bez2;
  Push(ref o,o[0]);Push(ref xy,xy[0],xy[1]);
  nx=(xy[0]+xy[xy.Length-4])/2;ny=(xy[1]+xy[xy.Length-3])/2;
  s="M"+nx+","+ny;
  for(i1=i=0;i+2<xy.Length;i1++,i+=2) {
    lx=nx;ly=ny;s+=f2(o[i1]||o[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,rr);
  }
  s="<path d=\""+s+" Z\" style=\"fill:"+c+";stroke:#000;stroke-width:"+w+";\" />\r\n";
  return s;
}

delegate string f_svg_linear(string n,int c,float dx,float dy);
string _svg_linear(string n,int c,float dx,float dy) {
  string s="";int a=(int)(Math.Atan2(dy,dx)*180/Math.PI);
  s="<linearGradient id=\""+n+"\" gradientTransform=\"rotate("+a+")\">\r\n";
  s+="<stop offset=\"0%\" stop-color=\"#fff\" />\r\n";
  s+="<stop offset=\"100%\" stop-color=\""+c+"\" />\r\n";
  s+="</linearGradient>\r\n";
  return s;
}


      string _svg_radialp(string n,int c,float x,float y,float r,bool np,int c2) {
        string p=np?"":"%",s="<radialGradient id=\""+n+"\" cx=\""+_svgx(x)+p+"\" cy=\""+_svgx(y)+p+"\" r=\""+_svgx(r)+p+"\""+(np?" gradientUnits=\"userSpaceOnUse\"":"")+" >\r\n";
        s+="<stop offset=\"0%\" stop-color=\""+(c2<0?"#fff":Pal.IntHtml(c2))+"\" />\r\n";
        s+="<stop offset=\"100%\" stop-color=\""+(c<0?"#fff":Pal.IntHtml(c))+"\" />\r\n";
        s+="</radialGradient>\r\n";
        return s; 
      }

string _svg_radial(string n,int c,float dx,float dy) {
  string s="";float a=(float)Math.Atan2(dy,dx),x=(float)Math.Cos(a),y=(float)Math.Sin(a),e,f,r;
  e=Math.Abs(x);f=Math.Abs(y);
  if(e<f) e=f;
  //x/=e,y/=e;
  if(View.grdm==3) x=y=r=50;
  else {x=(float)Math.Round(50*(1-x));y=(float)Math.Round(50*(1-y));r=100;}
  s+=_svg_radialp(n,c,x,y,r,B0,-1);
  return s;
}

string _svg_radialxy(string n,int c,int x,int y) {
  int i;float[] xy=_points(x,y),p=_peg(x,y);float r=0,cx,cy,sx=p[0],sy=p[1],ix,iy,ax=ix=xy[0],ay=iy=xy[1],f=0,m=_sqr(ax-sx,ay-sy),dx,dy;
  for(i=2;i<xy.Length;i+=2) {
    if((r=xy[i])<ix) ix=r;else if(r>ax) ax=r; 
    if((r=xy[i+1])<iy) iy=r;else if(r>ay) ay=r; 
    if(f2b(r=_sqr(ax-sx,ay-sy))) {f=i;m=r;};
  }
  dx=ax-ix;dy=ay-iy;cx=(sx-ix)*100/dx;cy=(sy-iy)*100/dy;r=(float)Math.Sqrt(r);//*100/(ax-ix);
  return _svg_radialp(n,c,sx,sy,r,B1,-1);
}

int _t11(int x,int y) { int d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d; return dd; }
Dictionary<string,string> svgg=new Dictionary<string, string>();
string _svgf(int c,string gn) {return gn!=""?"url(#"+gn+")":Pal.IntHtml(c);}
string _svgcell(int x,int y,float rr) {
  string s="",gn="";int c=_color(x,y);
  if(View.grdm>0) { 
   if(c==0xffffff) c=clr2[1];
   if(View.grdm<5) {
     gn=Pal.IntHtml(c).Replace('#','x');string t,g;
     if(View.grdm==3) {
     //t=tria2||tria4?(x&1)+2*(y&1):_t11(x,y);
     t=""+x+'_'+y;gn+="_"+t;svgg.TryGetValue(gn,out g);
     if(g==null) s+=svgg[gn]="<defs>"+_svg_radialxy(gn,c,x,y)+"</defs>\r\n";
   } else {
     svgg.TryGetValue(gn,out g);
     if(g==null) s+=svgg[gn]="<defs>"+(View.grdm>1?(f_svg_linear)_svg_radial:_svg_linear)(gn,c,View.grdx,View.grdy)+"</defs>\r\n";
   }
  }}
  if(H==H.quad&&!View.rou&&View.grdm<5) {
    float sx=brd+x*Cell,sy=brd+y*Cell;    
    s+="<rect x=\""+sx+"\" y=\""+sy+"\" width=\""+Cell+"\" height=\""+Cell+"\" style=\"fill:"+Pal.IntHtml(c)+";stroke:#000;stroke-width:0.5;\" />\r\n";
  } else {
    var xy=_points(x,y);
    if(View.rou) {
      int i,i1;var o=new bool[xy.Length/2];float lx,ly,nx,ny;int[] r=_border(x,y);string s2="";
      for(i1=i=0;i<xy.Length;i1++,i+=2) o[i1]=_brd(x+r[i],y+r[i+1]);
      if(View.grdm>=5) s+=_svggpoly(x,y,0.5f,c,xy,o,View.grdm>5);
      else s+=_svg_path(0.5f,_svgf(c,gn),xy,o,rr);
    } else {
      if(View.grdm>=5) s+=_svggpoly(x,y,0.5f,c,xy,null,View.grdm>5);
      else s+=_svgpoly(0.5f,c,xy,gn);
    }
  }
  return s;
}

string _svgcorn(int m,int e,int n,float[] xy2,bool[] b) {
  string s="";int i,i2,n2=n>>1;float a,c,x3,y3,x2=xy2[n-4],y2=xy2[n-3],x=xy2[n-2],y=xy2[n-1],dx,dy,ex,ey,p,fx,fy,gx,gy;
  for(i=0,i2=n2-2;i<xy2.Length;i+=2,i2=(i2+1)%n2) {
    x3=x2;y3=y2;x2=x;y2=y;x=xy2[i];y=xy2[i+1];
    if(b!=null&&!b[i2]&&!b[(i2+1)%n2]) continue;
    dx=x-x2;dy=y-y2;ex=x3-x2;ey=y3-y2;
    if(Math.Abs(dx*ey-dy*ex)<1) continue;
    a=(float)Math.Sqrt(_sqr(ex,ey));
    c=(float)Math.Sqrt(_sqr(dx,dy));
    fx=x2+e*(x3-x2)/a;fy=y2+e*(y3-y2)/a;gx=x2+e*(x-x2)/c;gy=y2+e*(y-y2)/c;
    if(m==2) {
      s+=_svg_arcf(fx,fy,gx,gy,e,x2,y2);
    } else {
      s+=_svgpoly(0,0,new float[] {x2,y2,fx,fy,gx,gy},"");
    }
  }
  return s;
}


string _svgborder(int x,int y,float rr) {
  string s="";int i,i2;float[] xy=_points(x,y);int[] b=_border(x,y);int e=Data[Index(x,y)].block;int n=xy.Length;bool[] o=null;
  if(View.rou) {
    int i1;float a;o=new bool[xy.Length/2+1];
    for(i1=i=0;i<xy.Length;i1++,i+=2) o[i1>0?i1-1:o.Length-2]=_brd(x+b[i],y+b[i+1]);
    o[o.Length-1]=o[0];
    Push(ref xy,xy[0],xy[1],xy[2],xy[3]);
    for(i1=i=0;i+4<xy.Length;i1++,i+=2) {
      if(!o[i1]&&!o[i1+1]) {
         if(H==H.quad||H==H.hexa||H==H.tria) {
           var ar=H==H.tria?cell3y/3:H==H.hexa?cell6x/2:Cell/2;
           s+=_svg_arc(2,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,ar);
         } else s+=_svg_bez(2,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3],(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,rr);
      } else if(!o[i1]) s+=_svgline(2,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3]);
      else if(!o[i1+1]) s+=_svgline(2,(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,xy[i+2],xy[i+3]);
    }
  } else
    for(i=0,i2=b.Length-2;i<xy.Length;i2=i,i+=2) {
      if(Ch(x+b[i],y+b[i+1])<1) s+=_svgline(2,xy[i],xy[i+1],xy[i2],xy[i2+1]);
    }
  if(View.corn!=0) s+=_svgcorn(View.corn,3,n,xy,o);
  if(e!=0) {
    Array.Resize(ref xy,xy.Length-4);
    int m,i3;float[] xy2=xy.Clone() as float[];
    _polyex2(xy,15f/16,0,0);
    for(m=1,i=0,i2=b.Length-2;i<xy.Length;i2=i,i+=2,m<<=1) 
      if((e&m)!=0) 
        s+=_svgline(2,xy[i],xy[i+1],xy[i2],xy[i2+1]);
    e=e|rol(xy2.Length>>1,e,-1,B0);
    for(m=1,i=0;i<xy2.Length;i+=2,m<<=1) 
      if((e&m)!=0)
        s+=_svgcircle(3,0,1,xy2[i],xy2[i+1]);
  }
  return s;
}

      public string _svg() {
  int c,x,y,ii;
  var s="";bool p=View.peg;float rr=H==H.tria?cell3y/3f:H==H.hexa?cell6x/2f:H==H.quad?Cell/2f:0.125f,rr2=rr>0?rr*0.75f:rr;
  svgg.Clear();
  for(y=ii=0;y<Height;y++)
    for(x=0;x<Width;x++,ii++)
      if((c=Data[ii].ch)>0) {
        s+=_svgcell(x,y,rr)+"\r\n";
        if(c!=1) {
          bool wh=c==3;int pc=Data[ii].fore,bw=wh?2:1;string gn="";
          pc=pc!=0?wh?_whi(pc):pc:wh?View.grdm2>0?0xcccccc:0xffffff:0;
          if(p) {
            var xy=_polyex2(_points(x,y),0.75f,0,0);
            if(View.rou) {
              int i,i1;bool[] o=new bool[xy.Length/2];int[] r=_border(x,y);
              for(i1=i=0;i<xy.Length;i1++,i+=2) o[i1]=_brd(x+r[i],y+r[i+1]);
              if(View.grdm2>0) {
                gn="xp"+x+'_'+y;float[] rp=_radial_xy(xy);
                s+="<defs>"+_svg_radialp(gn,pc,rp[0],rp[1],rp[2],B1,-1)+"</defs>\r\n";                
              }
              s+=_svg_path(bw,Pal.IntHtml(pc),xy,o,rr2);
            } else 
              s+=_svgpoly(bw,pc,xy,View.grdm2>0?"xp"+x+'_'+y:"");
          } else {
            var g=_peg(x,y);
            if(View.grdm2>0) {
              gn=Pal.IntHtml(pc).Replace("#","xp");string gg;svgg.TryGetValue(gn,out gg);
              if(gg==null) s+=svgg[gn]="<defs>"+_svg_radialp(gn,pc,25,25,75,B0,-1)+"</defs>\r\n";
              
            }
            s+="<circle cx=\""+g[0]+"\" cy=\""+g[1]+"\" r=\""+g[2]+"\" style=\"fill:"+_svgf(pc,gn)+";stroke:#000;"+(bw!=1?"stroke-width:"+bw+";":"")+"\" />\r\n";
          }
      }
    }
  for(y=ii=0;y<Height;y++)
    for(x=0;x<Width;x++,ii++)
      if(Data[ii].ch>0) s+=_svgborder(x,y,rr)+"\r\n";
  return s;
      }
    
    float pdfh;
    List<string> pdfo=new List<string>();
    Dictionary<string,int> pdfd=new Dictionary<string,int>();
    string f2a(float x) { return x.ToString(System.Globalization.CultureInfo.InvariantCulture);}
    string _pdfx(float x) { return f2a((float)Math.Round(x,3,MidpointRounding.AwayFromZero));}
    string _pdfy(float y) { return f2a((float)Math.Round(pdfh-y,3,MidpointRounding.AwayFromZero));}

    string _pdfm(float x,float y) { return ""+_pdfx(x)+" "+_pdfy(y)+" m"; }
    string _pdfl(float x,float y) { return ""+_pdfx(x)+" "+_pdfy(y)+" l"; }
    string _pdfc(float x1,float y1,float x2,float y2,float x3,float y3) {
      return _pdfx(x1)+" "+_pdfy(y1)+" "+_pdfx(x2)+" "+_pdfy(y2)+" "+_pdfx(x3)+" "+_pdfy(y3)+" c";
    }
    string _pdf_color(int c) {
      int r=(c>>16)&255,g=(c>>8)&255,b=c&255;
      return ""+_pdfx(r/255f)+" "+_pdfx(g/255f)+" "+_pdfx(b/255f);
    }
    string _pdfline(float w,float x,float y,float x2,float y2) {
      return " "+w+" w "+_pdfm(x,y)+" "+_pdfl(x2,y2)+" S\r\n";
   }
   string _pdfcircle(float w,int c,float r,float x,float y,int sh) {
    float x0=x-r,y0=y-r,x1=x+r,y1=y+r,m;string s=_pdfm(x0,y);
    m=(float)(4*(Math.Sqrt(2)-1)/3*r);
    s+=" "+_pdfc(x0,y-m,x-m,y0,x,y0);
    s+=" "+_pdfc(x+m,y0,x1,y-m,x1,y);
    s+=" "+_pdfc(x1,y+m,x+m,y1,x,y1);
    s+=" "+_pdfc(x-m,y1,x0,y+m,x0,y);
    return " "+w+" w "+(c<0?"":_pdf_color(c)+" rg ")+s+(sh!=0?sh==-1?" s":" q W "+(w>0?"s":"n")+" /Sh"+sh+" sh Q \r\n":" b\r\n");
}

   string _pdf_arc(float w,float x,float y,float x1,float y1,float x2,float y2,float z) { 
  // var a=Math.PI/4,s=Math.sin(a),c=Math.cos(a),x=Math.sqrt(3),[s,c,4/3*(1-c),1-s*s/c,1-4/3*(1-c)/(s*s/c)],
  float r=(float)(H==H.tria?5f/9:H==H.hexa?1-4f/3*(2*Math.Sqrt(3)-3):1-4f/3*(Math.Sqrt(2)-1)),r1=1-r;
  return " "+w+" w "+_pdfm(x,y)+" "+_pdfc(r1*x1+r*x,r1*y1+r*y,r1*x1+r*x2,r1*y1+r*y2,x2,y2)+" S";
}

delegate string f_pdf_arc2(bool c,float x,float y,float x1,float y1,float x2,float y2,float z);
string _pdf_arc2(bool c,float x,float y,float x1,float y1,float x2,float y2,float z) { 
  if(c) return  " "+_pdfl(x1,y1)+" "+_pdfl(x2,y2);
  else {
   float r=(float)(H==H.tria?5f/9:H==H.hexa?1-4/3*(2*Math.Sqrt(3)-3):1-4/3*(Math.Sqrt(2)-1)),r1=1-r;
   return " "+_pdfc(r1*x1+r*x,r1*y1+r*y,r1*x1+r*x2,r1*y1+r*y2,x2,y2);
  }
}

string _pdf_arcf(float x1,float y1,float x2,float y2,float z,float sx,float sy) { 
  float mx=(x1+x2)/2,my=(y1+y2)/2,dx=mx-sx,dy=my-sy,d2=_sqr(dx,dy),k=_sqr(x1-mx,y1-my)/d2,ex=mx+k*dx,ey=my+k*dy;
  float r2=(float)Math.Sqrt(_sqr(x1-sx,y1-sy)),d3=(float)Math.Sqrt(d2),dd=r2-d3,r=1-4/3*dd/(k*d3),r1=1-r;
  return " 0 0 0 rg 0 w "+_pdfm(sx,sy)+" "+_pdfl(x1,y1)+" "+_pdfc(r1*ex+r*x1,r1*ey+r*y1,r1*ex+r*x2,r1*ey+r*y2,x2,y2)+" b";
}


string _pdf_bez(float w,float x,float y,float x1,float y1,float x2,float y2,float r) { 
  float r1=1-r;
  //return "<path d=\"M"+x+","+y+" C"+(r1*x1+r*x)+","+(r1*y1+r*y)+" "+(r1*x1+r*x2)+","+(r1*y1+r*y2)+" "+x2+","+y2+"\" style=\"fill:none;stroke:#000;stroke-width:"+w+";\" />\r\n";
  return " "+w+" w "+_pdfm(x,y)+" "+_pdfc(r1*x1+r*x,r1*y1+r*y,r1*x1+r*x2,r1*y1+r*y2,x2,y2)+" S";
}

string _pdf_bez2(bool c,float x,float y,float x1,float y1,float x2,float y2,float r) { 
  if(c) return " "+_pdfl(x1,y1)+" "+_pdfl(x2,y2);
  else {
   var r1=1-r;
   return " "+_pdfc(r1*x1+r*x,r1*y1+r*y,r1*x1+r*x2,r1*y1+r*y2,x2,y2);
  }
}

string _pdfpoly(float w,int c,float[] xy,int sh) {
  string s="";int i;
  for(i=0;i<xy.Length;i+=2)
    s+=" "+_pdfx(xy[i])+" "+_pdfy(xy[i+1])+(s==""?" m":" l");
  return s=" "+(w>0?""+w+" w ":"")+(c<0?"":_pdf_color(c)+" rg")+s+" "+(sh!=0?sh==-1?"s":"q W "+(w>0?"s":"n")+" /Sh"+sh+" sh Q ":"b");
}

string _pdf_pathx(List<float> r) { 
  int i;string s="";int ch;
  for(i=0;i<r.Count;)
    switch((char)r[i++]) {
     case 'm': s+=_pdfm(r[i],r[i+1]);i+=2;break;
     case 'l': s+=_pdfl(r[i],r[i+1]);i+=2;break;
     case 'c': s+=_pdfc(r[i],r[i+1],r[i+2],r[i+3],r[i+4],r[i+5]);i+=6;break;
    }
  return s;
}

string _pdfgpoly(int cx,int cy,float w,int c,float[] xy,bool[] b,bool m) {
  string d,g,s="",s2="",s3="",k="1",gn;float rr=(float)(H==H.quad?Math.Sqrt(2)/3:H==H.hexa?Math.Sqrt(3)/3:H==H.tria?Math.Sqrt(2)/3:0.125);int l=xy.Length;
  int sh=-1,i,i1;float x=xy[xy.Length-2],y=xy[xy.Length-1],x2,y2,x1,y1,sx,sy,lx,ly,nx=(xy[0]+xy[l-2])/2,ny=(xy[1]+xy[l-1])/2,mx,my,rx,ry;
  float[] xy2;
  if(c==0xffffff) c=clr2[0];
  sx=_polyc(xy,out sy);
  if(b!=null) Push(ref b,b[0],b[1]);
  for(i=i1=0;i<xy.Length;i1++,i+=2) {
    x2=x;y2=y;x=xy[i];y=xy[i+1];mx=xy[(i+2)%l];my=xy[(i+3)%l];
    lx=nx;ly=ny;nx=(xy[i]+mx)/2;ny=(xy[i+1]+my)/2;
    //xy2=[(x2+x)/2,(y2+y)/2,x,y,(x+x1)/2,(y+y1)/2,sx,sy];
    rx=m?nx:x;ry=m?ny:y;sh=_pdf_radial(-1,0,0,new float[]{rx,ry,(float)Math.Sqrt(_sqr(rx-sx,ry-sy))},c);
    if(m) {
      float px=xy[(i+4)%l],py=xy[(i+5)%l];var p=new List<float>();
      if(b==null||((b[i1]||b[i1+1])&&(b[i1+1]||b[i1+2]))) {
        Push(p,'m',x,y,'l',mx,my);
      } else {
        _bez3x(p,B1,b==null||b[i1]||b[i1+1],lx,ly,x,y,nx,ny,rr,B1);
        _bez3x(p,B0,b==null||b[i1+1]||b[i1+2],nx,ny,mx,my,(mx+px)/2,(my+py)/2,rr,B0);
      }
      Push(p,'l',sx,sy);
      d=_pdf_pathx(p);
      s+=" "+d+" q W n /Sh"+sh+" sh Q\r\n";
    } 
    xy2=new float[] {lx,ly,x,y,nx,ny,sx,sy};
    if(s2+""=="") s2=_pdfm(lx,ly);
    if(b==null||b[i1]||b[i1+1]) {
      if(!m) s+=" "+_pdfpoly(0,0xffffff,xy2,sh)+"\r\n";
      s2+=" "+_pdfl(x,y)+" "+_pdfl(xy2[4],xy2[5]);
    } else {
      d=_pdfm(xy2[0],xy2[1])+" "+(s3=_pdf_bez2(B0,xy2[0],xy2[1],xy2[2],xy2[3],xy2[4],xy2[5],rr))+" "+_pdfl(sx,sy);
      if(!m) s+=" "+d+" q W n /Sh"+sh+" sh Q\r\n";
      s2+=s3;
    }
  }
  if(w>0) {
    if(s3!="") s+=" "+w+" w "+s2+" s\r\n";
    else s+=" "+_pdfpoly(w,c,xy,-1)+"\r\n";
  }
  return s;
}

string _pdf_path(float w,int c,float[] xy,bool[] o,float rr,int sh) {
  string s="";int i,i1;float lx,ly,nx,ny;f_pdf_arc2 f2=H==H.quad||H==H.hexa||H==H.tria?(f_pdf_arc2)_pdf_arc2:_pdf_bez2;
  Push(ref o,o[0]);Push(ref xy,xy[0],xy[1]);
  nx=(xy[0]+xy[xy.Length-4])/2;ny=(xy[1]+xy[xy.Length-3])/2;
  s=""+_pdf_color(c)+" rg "+_pdfm(nx,ny);
  for(i1=i=0;i+2<xy.Length;i1++,i+=2) {
    lx=nx;ly=ny;s+=f2(o[i1]||o[i1+1],lx,ly,xy[i],xy[i+1],nx=(xy[i]+xy[i+2])/2,ny=(xy[i+1]+xy[i+3])/2,rr);
  }
  s=""+_pdf_color(c)+(w>0?" "+w+" w ":" ")+s+" "+(sh!=0?sh==-1?"s":"q W "+(w>0?"s":"n")+" /Sh"+sh+" sh Q ":"b");
  return s;
}

int _pdf_fx(int c0,int c1) {
  int s;
  if(pdfd.TryGetValue("#"+c0+"#"+c1,out s)) return s;
  pdfo.Add("<< /FunctionType 2 /Domain [0 1] /Range [0 1 0 1 0 1] /C0 ["+_pdf_color(c0)+"] /C1 ["+_pdf_color(c1)+"] /N 1 >>");
  return pdfd["#"+c0+"#"+c1]=pdfo.Count;
}

delegate int f_pdf_linear(int c,float dx,float dy,float[] xy,int c2);
int _pdf_linear(int c,float dx,float dy,float[] xy,int c2) {
  int n;string s;int fn=_pdf_fx(c2<0?0xffffff:c2,c);float[] g=_linear(View.grdx,View.grdy,xy);
  
  s="<< /ShadingType 2 /ColorSpace /DeviceRGB /Coords ["+_pdfx(g[0])+" "+_pdfy(g[1])+" "+_pdfx(g[2])+" "+_pdfy(g[3])+"] /Domain [0 1] /Extend [true true] /Function "+fn+" 0 R >>";
  pdfo.Add(s);
  pdfd["$"+(n=pdfo.Count)]=n;
  return n;
}

int _pdf_radial(int c,float dx,float dy,float[] xy,int c2) {
  string s;int n,fn=_pdf_fx(c2<0?0xffffff:c2,c<0?0xffffff:c);float[] g=xy.Length==3?xy:_radial(View.grdx,View.grdy,xy);
  s="<< /ShadingType 3 /ColorSpace /DeviceRGB /Coords ["+_pdfx(g[0])+" "+_pdfy(g[1])+" 0 "+_pdfx(g[0])+" "+_pdfy(g[1])+" "+_pdfx(g[2])+"] /Domain [0 1] /Extend [true true] /Function "+fn+" 0 R >>";
  pdfo.Add(s);
  pdfd["$"+(n=pdfo.Count)]=n;
  return n;
}

string _pdfcell(int x,int y,float rr,bool b) {
  string s="";int c=_color(x,y);float[] xy=_points(x,y);int sh=0;
  if(View.grdm>0) { 
   if(b) { if(c==0xffffff) c=clr2[1];
     if(View.grdm<5) sh=(View.grdm>1?(f_pdf_linear)_pdf_radial:_pdf_linear)(c,View.grdx,View.grdy,xy,-1);
   } else sh=-1;
  }
  if(H==H.quad&&!View.rou&&View.grdm<5) {
    float sx=brd+x*Cell,sy=brd+y*Cell;
    s+=" 0.5 w "+_pdf_color(c)+" rg";
    s+=" "+_pdfx(sx)+" "+_pdfy(sy+Cell)+" "+_pdfx(Cell)+" "+_pdfx(Cell)+" re "+(sh!=0?sh<0?"s":"q W n /Sh"+sh+" sh Q":"b")+"\n";
  } else {
    if(View.rou) {
      int i,i1;bool[] o=new bool[xy.Length];float lx,ly,nx,ny;int[] r=_border(x,y);string s2="";
      for(i1=i=0;i<xy.Length;i1++,i+=2) o[i1]=_brd(x+r[i],y+r[i+1]);
      if(b&&View.grdm>=5) s+=_pdfgpoly(x,y,0.5f,c,xy,o,View.grdm>5);
      else s+=_pdf_path(0.5f,c,xy,o,rr,sh);
    } else 
      if(b&&View.grdm>=5) s+=_pdfgpoly(x,y,0.5f,c,xy,null,View.grdm>5);
      else s+=_pdfpoly(0.5f,c,xy,sh);
  }
  return s;
}


string _pdfcorn(int m,int e,int n,float[] xy2,bool[] b) {
  string s="";int i,i2,n2=n>>1;float a,c,x3,y3,x2=xy2[n-4],y2=xy2[n-3],x=xy2[n-2],y=xy2[n-1],dx,dy,ex,ey,p,fx,fy,gx,gy;
  for(i=0,i2=n2-2;i<xy2.Length;i+=2,i2=(i2+1)%n2) {
    x3=x2;y3=y2;x2=x;y2=y;x=xy2[i];y=xy2[i+1];
    if(b!=null&&!b[i2]&&!b[(i2+1)%n2]) continue;
    dx=x-x2;dy=y-y2;ex=x3-x2;ey=y3-y2;
    if(Math.Abs(dx*ey-dy*ex)<1) continue;
    a=(float)Math.Sqrt(_sqr(ex,ey));
    c=(float)Math.Sqrt(_sqr(dx,dy));
    fx=x2+e*(x3-x2)/a;fy=y2+e*(y3-y2)/a;gx=x2+e*(x-x2)/c;gy=y2+e*(y-y2)/c;
    if(m==2) {
      s+=_pdf_arcf(fx,fy,gx,gy,e,x2,y2);
    } else {
      s+=_pdfpoly(0,0,new float[] {x2,y2,fx,fy,gx,gy},0);
    }
  }
  return s;
}

string _pdfborder(int x,int y,float rr) {
  var s="";int i,i2;float[] xy=_points(x,y);int[] b2,b=_border(x,y);int e=Data[Index(x,y)].block,n=xy.Length;bool[] o=null;
  if(View.rou) {
    int i1;float a;o=new bool[xy.Length/2+1];
    for(i1=i=0;i<xy.Length;i1++,i+=2) o[i1>0?i1-1:o.Length-2]=_brd(x+b[i],y+b[i+1]);
    o[o.Length-1]=o[0];
    Push(ref xy,xy[0],xy[1],xy[2],xy[3]);
    for(i1=i=0;i+4<xy.Length;i1++,i+=2) {
      if(!o[i1]&&!o[i1+1]) {
         if(H==H.quad||H==H.hexa||H==H.tria) {
           var ar=H==H.tria?cell3y/3:H==H.hexa?cell6x/2:Cell/2;
           s+=_pdf_arc(2,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3],(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,ar);
         } else s+=_pdf_bez(2,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3],(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,rr);
      } else if(!o[i1]) s+=_pdfline(2,(xy[i]+xy[i+2])/2,(xy[i+1]+xy[i+3])/2,xy[i+2],xy[i+3]);
      else if(!o[i1+1]) s+=_pdfline(2,(xy[i+2]+xy[i+4])/2,(xy[i+3]+xy[i+5])/2,xy[i+2],xy[i+3]);
    }
  } else
    for(i=0,i2=b.Length-2;i<xy.Length;i2=i,i+=2) {
      if(Ch(x+b[i],y+b[i+1])<1) s+=_pdfline(2,xy[i],xy[i+1],xy[i2],xy[i2+1]);
    }
  if(View.corn!=0) s+=_pdfcorn(View.corn,3,n,xy,o);
  if(e!=0) {
    int m;Array.Resize(ref xy,xy.Length-4);
    float[] xy2=xy.Clone() as float[];
    _polyex2(xy,15f/16,0,0);
    for(m=1,i=0,i2=b.Length-2;i<xy.Length;i2=i,i+=2,m<<=1) 
      if((e&m)!=0) 
        s+=_pdfline(2,xy[i],xy[i+1],xy[i2],xy[i2+1]);
    e=e|rol(xy2.Length>>1,e,-1,B0);
    for(m=1,i=0;i<xy2.Length;i+=2,m<<=1) 
      if((e&m)!=0) 
        s+=_pdfcircle(3,0,1,xy2[i],xy2[i+1],0);
  }
  return s;
}

string _pdf() {
  int w=BWidth(),h=BHeight();
  var s="1 j 1 J 1 w\n";var xr=new List<int>();
  int c,x,y,ii;
  bool p=View.peg;float rr=(float)(H==H.tria?cell3y/3:H==H.hexa?cell6x/2:H==H.quad?Cell/2:0.125),rr2=rr>0?rr*0.75f:rr;
  pdfh=h;pdfo.Clear();pdfd.Clear();
  pdfo.Add("<<\r\n/Type /Catalog\r\n/Pages 2 0 R\r\n>>");
  pdfo.Add("<<\r\n/Type /Pages\r\n/Count 1\r\n/Kids[3 0 R]\r\n>>");
  pdfo.Add("<<\r\n/Type /Page\r\n/Parent 2 0 R\r\n/Resources << /ProcSet 5 0 R /Shading <<>> >>\r\n/MediaBox[0 0 "+w+" "+h+"]\r\n/Contents 4 0 R\r\n>>");
  s+="0 0 0 RG\n";
  if(View.grdm>0) {
  for(y=ii=0;y<Height;y++)
    for(x=0;x<Width;x++,ii++)
      if((c=Data[ii].ch)>0) s+=_pdfcell(x,y,rr,B1)+"\r\n";
  }
for(y=ii=0;y<Height;y++)
    for(x=0;x<Width;x++,ii++)
      if((c=Data[ii].ch)>0) {
        s+=_pdfcell(x,y,rr,B0)+"\r\n";
        if(c!=1) {
          bool wh=c==3;int pc=Data[Index(x,y)].fore;
          pc=pc!=0?wh?_whi(pc):pc:wh?View.grdm2>0?0xcccccc:0xffffff:0;
          if(p) {
            var xy=_polyex2(_points(x,y),0.75f,0,0);int sh=0;float[] rp;
            if(View.grdm2>0) {rp=_radial_xy(xy);sh=_pdf_radial(pc,0,0,rp,-1);}
            if(View.rou) {
              int i,i1;bool[] o=new bool[xy.Length];int[] r=_border(x,y);
              for(i1=i=0;i<xy.Length;i1++,i+=2) o[i1]=_brd(x+r[i],y+r[i+1]);
              s+=_pdf_path(wh?2:1,pc,xy,o,rr2,sh);
            } else 
              s+=_pdfpoly(wh?2:1,pc,xy,sh);
          } else {
            float[] g=_peg(x,y);int sh=0;
            if(View.grdm2>0) sh=_pdf_radial(pc,0,0,new float[] {g[0]-g[2]/2,g[1]-g[2]/2,g[2]},-1);
            s+=" "+_pdfcircle(wh?2:1,pc,g[2],g[0],g[1],sh);
          }
        }
    }
for(y=ii=0;y<Height;y++)
    for(x=0;x<Width;x++,ii++)
      if((c=Data[ii].ch)>0) s+=_pdfborder(x,y,rr)+"\r\n";
  s="<< /Length "+s.Length+" >>\r\nstream\r\n"+s+"\r\nendstream";
  pdfo.Add(s);
  s=pdfo[2].Replace("5 0 R",""+(pdfo.Count+1)+" 0 R").Replace("4 0 R",""+(pdfo.Count)+" 0 R");
  string sx="",sy;
  foreach(var kv in pdfd)
    if(kv.Key[0]=='$') sx+="/Sh"+kv.Value+" "+kv.Value+" 0 R ";
  if(sx!="") s=s.Replace("<<>>","<<"+sx+">>");
  pdfo[2]=s;
  pdfo.Add("[/PDF]");
  s="%PDF-1.0\r\n";
  for(x=0;x<pdfo.Count;x++) {
    xr.Add(s.Length);
    sx=pdfo[x];
    s+=(s[s.Length-1]=='\n'?"":"\r\n")+(1+(+x))+" 0 obj "+sx+(sx[sx.Length-1]=='\n'?"":"\r\n")+"endobj\r\n";
  }
  c=s.Length;
  s+="xref\r\n0 "+(1+pdfo.Count)+"\r\n0000000000 65535 f\r\n";
  foreach(int xrx in xr) {
    sy="00000000"+xrx;
    s+=sy.Substring(sy.Length-10)+" 00000 n\r\n";
  }
  s+="trailer\r\n<<\r\n/Size "+pdfo.Count+"\r\n/Root 1 0 R\r\n>>\r\nstartxref\r\n"+c+"\r\n%%EOF";
  pdfo.Clear();pdfd.Clear();
  return s;
}


    public void ExportPdf(string s) {
      int w=BWidth(),h=BHeight();
      string pdf=_pdf();
      using(var sw=new StreamWriter(s,false)) {
        sw.Write(pdf);
      }
    }

  public static float[] _mxy(H H,float ex,float ey) {
  if(H==H.deca) {
    int d,x=floor((ex-brd)*2/cell8x),y=2+floor((ey-brd)*4/cell8y),y3=y%3;
    int cy=floor(y/3)-1,cx=floor((x-(cy&1))/2);
    int dx=cy&1;float sx=brd+(cx+dx/2f)*cell8x,sy=brd+((1+cy*3)/4f)*cell8y,vx=cell8x,vy=cell8y/2,x2=sx+vx,x3=sx+cell8x,y2=sy+vy;
    float ax=2*(ex-sx)-cell8x,ay=2*(ey-sy)-cell8y;
    bool up;float fup=abs(ax)*vy-(2*vy+ay)*vx;
    if(fup>0) {
      ay+=3*vy;sy-=3*vy;cy--;
      up=ax>0;ax+=(up?-1:1)*cell8x;sx+=cell8x*(up?1:-1)/2;cx+=b2i(up)+dx-1;
    }
    vx=cell8x/2;vy=3*cell8y/4;
    fup=ax*vy-abs(ay)*vx;
    d=fup>0?1:ay>0?2:0;
    return FA(3*cx+d-2,cy,ax-cell8x,ay-cell8y*3/2,2,ex,ey,0);
  } if(H==H.trap) {
    int d,dd,x=floor((ex-brd)*2/cell8x),y=2+floor((ey-brd)*4/cell8y),y3=y%3;
    int cy=floor(y/3)-1,cx=floor((x-(cy&1))/2);
    int dx=cy&1;float sx=brd+(cx+dx/2f)*cell8x,sy=brd+((1+cy*3)/4f)*cell8y,vx=cell8x,vy=cell8y/2,x2=sx+vx,x3=sx+cell8x,y2=sy+vy;
    float ax=2*(ex-sx)-cell8x,ay=2*(ey-sy)-cell8y*4/4;
    bool up=ax*vy+ay*vx<0&&-ax*vy+ay*vx<0;
    if(up) {
      ay+=2*vy;
      up=ax*vy-ay*vx<0&&-ax*vy-ay*vx<0;
      if(!up) {
        d=ax<0?2:1;cy--;dx^=1;sy-=cell8y*3/4;ay+=2*cell8y*3/4;
        sx+=(1-dx/2)*cell8x;cx-=dx;
        if(d==1) {cx++;sx+=cell8x;ax-=2*cell8x;}
      }
    }
    d=(cx+2*(cy&1))%3;
    sx=brd+(cx+dx/2)*cell8x;sy=brd+((1+cy*3)/4)*cell8y;
    ax=2*(ex-sx)-cell8x;ay=2*(ey-sy)-cell8y*4/4;
    if(d==1) d=ax>0?1:0;
    else if(d==2) d=ax+ay*cell8x/cell8y*2>0?1:0;
    else d=ax-ay*cell8x/cell8y*2>0?1:0;
    return FA(2*cx+d-1,cy,ax-cell8x,ay-cell8y*3/2,2,ex,ey,0);
  } else if(H==H.cubes) {
    int d,x=floor((ex-brd)*2/cell8x),y=2+floor((ey-brd)*4/cell8y),y3=y%3;
    int cy=floor(y/3)-1,cx=floor((x-(cy&1))/2);
    int dx=cy&1;float sx=brd+(cx+dx/2)*cell8x,sy=brd+((1+cy*3)/4)*cell8y,vx=cell8x,vy=cell8y/2,x2=sx+vx,x3=sx+cell8x,y2=sy+vy;
    float ax=2*(ex-sx)-cell8x,ay=2*(ey-sy)-cell8y*4/4;
    bool up=ax*vy+ay*vx<0&&-ax*vy+ay*vx<0;
    if(!up) d=ax>0?2:1;
    else {
      ay+=2*vy;
      up=ax*vy-ay*vx<0&&-ax*vy-ay*vx<0;
      if(up) d=0;
      else {
        d=ax<0?2:1;cy--;dx^=1;sy-=cell8y*3/4;
        sx+=(1-dx/2)*cell8x;cx-=dx;
        if(d==1) {cx++;sx+=cell8x;}
      }
    }
    return FA(3*cx+d-2,cy,ax-cell8x,ay-cell8y*3/2,2,ex,ey,0);
    //float d=(x+2)%3,cx=(x+2-d)/3;
    //float sx=brd+(cx+(y&1)/2)*cell8x,sy=brd+((1+y*3)/4)*cell8y,x2=cell8x/2,y2=cell8y/4;
  } else if(H==H.delta) {
    float c62=cell9x/2;int cy=floor((ey-brd)/cell9y),cx=floor((ex-brd)/c62),d=1^(cy&1)^(cx&1),dy=d!=0?1:-1;
    float sx=brd+cx*c62,sy=brd+(cy+d)*cell9y,ax,ay=2*(ey-sy)-dy*cell9y,gx,gy,r,yy;
    gx=ex-sx;gy=ey-sy;
    //alert(\"\"+gx+\" \"+gy);
    if(dy*(gx*dy*cell9y+gy*c62)<0) {cx--;sx-=c62;dy*=-1;d=b2i(!i2b(d));}
    sy=brd+(cy+d)*cell9y;
    ax=2*(ex-sx)-cell9x;yy=dy*(ey-(sy-dy*cell9y/3));
    r=ax>0?2:0;
    if(yy<-abs(ax)/3) r=1;
    //if(dy*yy<0) r=1;else r=0;
    return FA(3*cx-2+r,cy,ax,ay,0,ex,ey,0);
  } else if(H==H.penta) {
    int cx=floor((ex-brd)/cell7),cy=floor((ey-brd)/cell7),v=(cx+cy)&1,x1;float c2=cell7/2,c4=c2/2;
    float sx=brd+cx*cell7,sy=brd+cy*cell7,ax=2*(ex-sx),ay=2*(ey-sy);int r=0;
    if(v!=0) {
      if(ax>2*ay&&(2*cell7-ax)>2*ay) r=-1;
      else if((4*cell7-ax)<2*ay&&(2*cell7+ax)<2*ay) r=1;
      if(r!=0) {sy+=r*cell7;ay-=r*2*cell7;cy+=r;v=0;}
    } else {
      if(ay>2*ax&&(2*cell7-ay)>2*ax) r=-1;
      else if((4*cell7-ay)<2*ax&&(2*cell7+ay)<2*ax) r=1;
      if(r!=0) sx+=r*cell7;ax-=r*2*cell7;cx+=r;v=1;
    }
    if(v!=0) {
      x1=b2i(ax>cell7);ax-=i2b(x1)?cell7:c2;ay-=cell7;
    } else {
      x1=b2i(ay>cell7);ax-=cell7;ay-=i2b(x1)?cell7:c2;
    }
    return FA(2*cx-1+x1,cy,ax,ay,0,ex,ey,0);
  } else if(H==H.tria4) {
    int cx=floor((ex-brd)/cell5),cy=floor((ey-brd)/cell5);
    float sx=brd+cx*cell5,sy=brd+cy*cell5,ax=0,ay=0;int r;
    r=((ex-sx)+(ey-sy-cell5)>0?2:0)|((ex-sx)-(ey-sy)>0?1:0);
    return FA(2*cx+(r&1)-1,2*cy+((r&2)>>1)-1,ax,ay,0,ex,ey,0);
  } else if(H==H.tria2) {
    int cx=floor((ex-brd)/cell2),cy=floor((ey-brd)/cell2),d=(0^cy^cx)&1;
    float sx=brd+cx*cell2,sy=brd+cy*cell2,ax,ay;int n;
    n=b2i(i2b(d)?ex-sx>ey-sy:sx+cell2-ex<ey-sy);
    ax=2*(ex-sx)-cell2;ay=2*(ey-sy)-cell2;
    return FA(2*cx+n-1,cy,ax,ay,0,ex,ey,0);
  } else if(H==H.tria) {
    float c62=cell3x/2;int cy=floor((ey-brd)/cell3y),cx=floor((ex-brd)/c62),d=1^(cy&1)^(cx&1),dy=i2b(d)?1:-1;
    float sx=brd+cx*c62,sy=brd+(cy+d)*cell3y,ax,ay=2*(ey-sy)-dy*cell3y,gx,gy;
    gx=ex-sx;gy=ey-sy;
    //alert(\"\"+gx+\" \"+gy);
    if(dy*(gx*dy*cell3y+gy*c62)<0) {cx--;sx-=c62;sy+=dy*cell3y;dy*=-1;d=b2i(!i2b(d));}
    ax=2*(ex-sx)-cell3x;
    return FA(cx,cy,ax,ay,0,ex,ey,0);
  } else if(H==H.quad) {
    int cx=floor((ex-brd)/Cell),cy=floor((ey-brd)/Cell);
    float sx=brd+cx*Cell,sy=brd+cy*Cell,ax=2*(ex-sx)-Cell,ay=2*(ey-sy)-Cell,gx,gy;int c;
    gx=ax+ay;gy=-ax+ay;
    c=gx<0?2:0;c|=(c>>1)^(b2i(gy<0));
    return FA(cx,cy,ax,ay,c,ex,ey,0);
  } else {
  int x=floor((ex-brd)*2/cell6y),y=floor((ey-brd)*4/cell6x),y3=y%3;
  int cy=floor(y/3),cx=floor((x-(cy&1))/2);
  int dx=i2b(cy&1)?0:-1;float sx=brd+(cx+(cy&1)/2)*cell6y,sy=brd+((1+cy*3)/4)*cell6x,x2=sx+cell6y/2,x3=sx+cell6y,ax=2*(ex-sx)-cell6y,ay=2*(ey-sy)-cell6x/2,fx=ex-sx,fy=ey-sy;
  if(y3!=0) {
    fx=ax;fy=ay;
    float c=0;
    if(fx<0) {c=3;fx=-fx;fy=-fy;}
    if(fx*-cell6x/2-fy*cell6y<0)
      c+=fx*cell6x/2-fy*cell6y>0?1:2;
    return FA(cx,cy,ax,ay,c,ex,ey,0);
  }
  if(fx*-cell6x/2-fy*cell6y>0) {
    return FA(cx+dx,cy-1,ax-cell6y,ay-cell6x*3/2,2,ex,ey,0);
  }
  fx=ex-sx-cell6y;
  if(fx*cell6x/2-fy*cell6y>0) return FA(cx+dx+1,cy-1,ax+cell6y,ay-cell6x*3/2,3,ex,ey);
  return FA(cx,cy,ax,ay,ax>0?0:5,ex,ey,0);
  }
 }

bool _extx(int x) {
  for(int y=0;y<Height;y++) if(Data[y*Width+x].ch>0) return true;
  return false;
}
bool _exty(int y) {
  for(int x=0;x<Width;x++) if(Data[y*Width+x].ch>0) return true;
  return false;
}

void _extentx() {
  _insertx(0,1,B0);
  C c=new C();
  Rect(0,0,0,Height-1,ref c);
}

void _extenty() {
  _inserty(0,1,B0);
/*  int h=Height;
  Array.Resize(ref Data,Width*(Height=h+1));  
  for(int n=Width*h,s=Width*h-1,d=Width*Height-1;n>0;n--,s--,d--)
      Data[d]=Data[s];*/
  C c=new C();
  Rect(0,0,Width-1,0,ref c);
}

public void Rect(int x,int y,int x2,int y2,ref C val) {
  for(int j=y;j<=y2;j++)
    for(int i=x;i<=x2;i++)
      Data[j*Width+i]=val;
}
public void _insertx(int x,int n,bool clear) {
  int w=Width,nw=w+n;  
  if(x<0||x>=w) return;
  Array.Resize(ref Data,(Width=w+n)*Height);
  for(int y=Height-1,s=w*y+w,d=Width*y+nw;y>=0;y--) {    
    Array.Copy(Data,s-=w-x,Data,d-=w-x,w-x);
    d-=n;
    if(clear) Array.Clear(Data,d,n);
    else Rect(x,y,x+n-1,y,ref Data[Width*y+(x+n<Width?x+n:x-1)]);
    Array.Copy(Data,s-=x,Data,d-=x,x);
  }
}

public void _deletex(int x,int n) {
  int w=Width,nw=w-n;  
  if(x<0||n<1||x+n>=w) return;
  for(int y=0,s=0,d=0;y<Height;y++) {
    Array.Copy(Data,s,Data,d,x);
    Array.Copy(Data,s+x+n,Data,d+x,nw-x);
    d+=nw;s+=w;
  }
  Array.Resize(ref Data,(Width=nw)*Height);
}

public void _inserty(int y,int n,bool clear) {
  int h=Height;
  if(y<0||y>=h) return;
  Array.Resize(ref Data,Width*(Height=h+n));
  Array.Copy(Data,y*Width,Data,(y+n)*Width,(h-y)*Width);
  if(clear) Array.Clear(Data,y*Width,n*Width);
  else for(int x=0;x<Width;x++) Rect(x,y,x,y+n-1,ref Data[(y+n<Height?y+n:y-1)*Width+x]);
    
}

public void _deletey(int y,int n) {
  int h=Height;
  if(y<0||n<1||y+n>=h) return;
  Array.Copy(Data,(y+n)*Width,Data,y*Width,(h-y-n)*Width);
  Array.Resize(ref Data,Width*(Height=h-n));
}


bool _extent(int ex,int ey) {
  int w=Width,h=Height,nw=Math.Max(Width,ex+1),nh=Math.Max(Height,ey+1);
  if(w==nw&&h==nh) return false;
  Array.Resize(ref Data,(Width=nw)*(Height=nh));
  for(int y=h-1;y>=0;y--) {
    for(int n=w,s=w*y+w-1,d=nw*y+w-1;n>0;n--,s--,d--)
      Data[d]=Data[s];
    for(int i=w,d=nw*y+w;i<nw;i++,d++) Data[d]=new C();
  }
  return true;
}

int[] _max() {
  int mx=0,my=0,r,x,y,c,i;
  for(y=0;y<Height;y++)
    for(x=Width-1,i=y*Width+x;x>=0;x--,i--) 
      if(Data[i].ch>0) {
        if((x=2*x+(y&1))>mx) mx=x;
        my=y;
        break;        
      }
  return IA(mx,my);
}

void _resizea(int mx,int my) {
  int[] s=_max();
  if(mx>0&&mx>s[0]) s[0]=mx;
  if(my>0&&my>s[1]) s[1]=my;
  int[] wh=_resize2(s[0]+4,s[1]+2);
  _resize(wh[0],wh[1]);
}

public int[] _resize2(int w2,int h) {
  float wn,hn;
  if(H==H.deca) {
    wn=2*brd+((w2+14)/4|0)*cell8x;hn=2*brd+h*cell8y*3/4;
  } else if(H==H.trap) {
    wn=2*brd+((w2+14)/4|0)*cell8x;hn=2*brd+h*cell8y*3/4;
  } else if(H==H.delta) {
    wn=2*brd+((w2+26)/12|0)*cell9x;hn=2*brd+h*cell9y;
  } else if(H==H.cubes) {
    wn=2*brd+((w2+14)/6|0)*cell8x;hn=2*brd+h*cell8y*3/4;
  } else if(H==H.penta) {
    wn=2*brd+(((w2+2)>>2)+1)*cell7;hn=2*brd+h*cell7;
  } else if(H==H.tria4) {
    wn=2*brd+(((w2+2)>>2)+1)*cell5;hn=2*brd+((h+3)>>1)*cell5;
  } else if(H==H.tria2) {
    wn=2*brd+(((w2+2)>>2)+1)*cell2;hn=2*brd+h*cell2;
  } else if(H==H.tria) {
    wn=2*brd+((w2>>1)+1)*cell3x/2;hn=2*brd+h*cell3y;
  } else if(H==H.hexa) {
    wn=2*brd+w2*cell6y/2;hn=2*brd+(h*cell6x*3+1)/4;
  } else {wn=2*brd+(w2>>1)*Cell;hn=2*brd+h*Cell;}
  return new int[] {ceil(wn),ceil(hn)};
}

  public int BWidth() { return _resize2(2*Width+1,Height)[0];}
  public int BHeight() { return _resize2(2*Width+1,Height)[1];}

void _resize(int w,int h) {
  View.f.UpdateBitmap(w,h);
}


int[] _dub1(int x,int y,int i) {
  if(H==H.quad) {
    if(i==0) return IA(x-1,y,2);
    if(i==1) return IA(x,y-1,3);
    if(i==2) return IA(x+1,y,0);
    if(i==3) return IA(x,y+1,1);
  } else if(H==H.hexa) {
    int[] b=_border16(x,y);
    if(i==0) return IA(x+b[0],y+b[1],3);
    if(i==1) return IA(x+b[2],y+b[3],4);
    if(i==2) return IA(x+b[4],y+b[5],5);
    if(i==3) return IA(x+b[6],y+b[7],0);
    if(i==4) return IA(x+b[8],y+b[9],1);
    if(i==5) return IA(x+b[10],y+b[11],2);
  } else if(H==H.tria) {
    int d=(1^y^x)&1,dy=i2b(d)?-1:1;
    if(i==0) return i2b(d)?IA(x,y-dy,0):IA(x,y-dy,0);
    if(i==1) return i2b(d)?IA(x-1,y,1):IA(x+1,y,1);
    if(i==2) return i2b(d)?IA(x+1,y,2):IA(x-1,y,2);
  } else if(H==H.tria2) {
     int xx=(x+1+2*(y&1))&3;int[] r=null;
     if(xx==0) r=i==0?IA(-1,0,1):i==1?IA(0,-1,0):IA(1,0,2);
     if(xx==1) r=i==0?IA(1,0,1):i==1?IA(0,1,0):IA(-1,0,2);
     if(xx==2) r=i==0?IA(0,1,1):i==1?IA(-1,0,0):IA(1,0,2);
     if(xx==3) r=i==0?IA(0,-1,1):i==1?IA(1,0,0):IA(-1,0,2);
     r[0]+=x;r[1]+=y;
     return r;
  } else if(H==H.tria4) {
     int xx=(((y+1)&1)<<1)|((x+1)&1);int[] r=null;
     if(xx==0) r=i==0?IA(1,0,1):i==1?IA(0,1,0):IA(-1,1,2);
     if(xx==1) r=i==0?IA(0,1,1):i==1?IA(-1,0,0):IA(-1,-1,2);
     if(xx==2) r=i==0?IA(0,-1,1):i==1?IA(1,0,0):IA(1,1,2);
     if(xx==3) r=i==0?IA(-1,0,1):i==1?IA(0,-1,0):IA(1,-1,2);
     //console.log("i",i,"xx",xx);
     r[0]+=x;r[1]+=y;
     return r;
  } else if(H==H.trap) {
     int i2=i,d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d,dx=i2b(y&1)?0:-2;int[] r,b=_border11(x,y);
     if(dd==0) i2=i==0?1:i==1?0:i==2?0:3;
     if(dd==1) i2=i==0?2:i==1?2:i==2?1:3;
     if(dd==2) i2=i==0?1:i==1?0:i==2?0:3;
     if(dd==3) i2=i==0?2:i==1?2:i==2?1:3;
     if(dd==4) i2=i==0?1:i==1?0:i==2?0:3;
     if(dd==5) i2=i==0?2:i==1?2:i==2?1:3;
     r=IA(b[2*i],b[2*i+1],i2);
     r[0]+=x;r[1]+=y;
     return r;
  } else if(H==H.deca) {
    int i2=i==1?2:i==2?1:i==3?4:i==4?3:i==5?0:5;int[] r,b=_border7(x,y);
    r=IA(b[2*i],b[2*i+1],i2);
    r[0]+=x;r[1]+=y;
    return r;
  } else if(H==H.cubes) {
    int i2=i==1?2:i==2?1:i==3?0:3;int[] r,b=_border8(x,y);
    r=IA(b[2*i],b[2*i+1],i2);
    r[0]+=x;r[1]+=y;
    return r;
  } else if(H==H.delta) {
    int i2=i==0?1:i==1?0:i==2?3:2,r2=(x+2)%3,cx=(x+2-r2)/3,d=1^(y&1)^(cx&1),dy=i2b(d)?-1:1;int[] r,b=_border9(x,y);
    r=IA(b[2*i],b[2*i+1],i2);
    r[0]+=x;r[1]+=y;
    return r;
  } else if(H==H.penta) {
    int i2=i2=i==0?1:i==1?0:i==2?3:i==3?2:4;int[] r,b=_border15(x,y);
    r=IA(b[2*i],b[2*i+1],i2);
    r[0]+=x;r[1]+=y;
    return r;
  }
  return null;
}

void _dublock(int x,int y,int e) {
  var r=new List<int>();
  if(H==H.quad) {
    if(i2b(e&1)&&_xo(x-1,y)) Push(r,_dub1(x,y,0));
    if(i2b(e&2)&&_xo(x,y-1)) Push(r,_dub1(x,y,1));
    if(i2b(e&4)&&_xo(x+1,y)) Push(r,_dub1(x,y,2));
    if(i2b(e&8)&&_xo(x,y+1)) Push(r,_dub1(x,y,3));
    return;
  }
  var b=_border(x,y);
  int x2,y2,i,i2;
  if(H==H.hexa||H==H.deca) {
    for(i=0,i2=10;i<6;i++,i2=(i2+2)%12)
      if(i2b(e&(1<<i))&&_xo(x2=x+b[i2],y2=y+b[i2+1])) Push(r,_dub1(x,y,i));
  } else if(H==H.tria||H==H.tria2||H==H.tria4) {
    for(i=0,i2=0;i<3;i++,i2=(i2+2)%6)
      if(i2b(e&(1<<i))&&_xo(x2=x+b[i2],y2=y+b[i2+1])) Push(r,_dub1(x,y,i));
  } else if(H==H.tria4) {
    for(i=0,i2=2;i<3;i++,i2=(i2+2)%6)
      if(i2b(e&(1<<i))&&_xo(x2=x+b[i2],y2=y+b[i2+1])) Push(r,_dub1(x,y,i));
  } else if(H==H.trap||H==H.cubes||H==H.delta) {
    for(i=0,i2=0;i<4;i++,i2=(i2+2)%8)
      if(i2b(e&(1<<i))&&_xo(x2=x+b[i2],y2=y+b[i2+1])) Push(r,_dub1(x,y,i));
  } else if(H==H.penta) {
    for(i=0,i2=0;i<5;i++,i2=(i2+2)%10)
      if(i2b(e&(1<<i))&&_xo(x2=x+b[i2],y2=y+b[i2+1])) Push(r,_dub1(x,y,i));
  }
  for(i=0;i<r.Count;i+=3) _setblock(r[i],r[i+1],1<<r[i+2]);
}

void _dublocka() {
  for(int y=0,i=0,e;y<Height;y++)
    for(int x=0;x<Width;x++,i++)
      if((e=Data[i].block)!=0) _dublock(x,y,e);
}

char _getblock1(char c,out int e) {
  c=Char.ToLowerInvariant(c);
  if(c=='.'||c=='+'||c=='*') c='x';
  else if(c=='q') c='w';
  e=0;
  if(c>=48&&c<=57) {e=c-48;c='o';}
  else if(c>=97&&c<=106) {e=c-97;c='w';}
  else switch(c) {
   case '.':c='x';e=0;break;
   case ',':c='x';e=1;break;
   case '(':c='x';e=2;break;
   case ')':c='x';e=3;break;
   case '[':c='x';e=4;break;
   case ']':c='x';e=5;break;
   case '{':c='x';e=6;break;
   case '}':c='x';e=7;break;
   case '<':c='x';e=8;break;
   case '>':c='x';e=9;break;
  }
  return c;
}

char _setblock1(int c,int e) {
  if(i2b(e)) return c==1?'.':c==2?'O':c=='3'?'W':' ';
  if(c==3) return "ABCDEFGHIJ"[e];
  if(c==2) return ".,()[]{}<>"[e];
  return "0123456789"[e];
}

int _txt2block(int x,int y,int e,bool r) {
  if(e==0) return e;
  int i;
  /* if(trap) {
    var d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d;
    i=dd==0?2:dd==1?1:dd==2?0:dd==3?0:dd==4?3:3;
    e=rol(4,e,i,r);
  } */
  if(H==H.trap||H==H.cubes||H==H.delta) {e=rol(4,e,1,r);}
  if(H==H.tria) {
    //var d=(x^y)&1; e=rol(3,e,d?2:1,r);
  }
  if(H==H.tria2) {
    //var xx=(x+1+2*(y&1))&3;e=rol(3,e,xx==2?1:xx==1?2:0,r);
  }
  if(H==H.tria4) {
    //var d=(((y+1)&1)<<1)|((x+1)&1);e=rol(3,e,d==3?1:d==2?0:d==1?1:2,r);
  }
  if(H==H.penta) { e=rol(5,e,3,r); }
  if(r) {
    if(H==H.quad||H==H.delta) e&=3;
    //else if(tria||tria2||tria4) e&=3;
    else e&=7;
  }
  return e;
}



bool _blocked(int x,int y,int x2,int y2) {
  if(!In(x,y)||!In(x2,y2)) return true;
  int e=Data[Index(x,y)].block;
  if(e==0) return false;
  if(H==H.quad) {
    int dx=x2-x,dy=y2-y;
    return 0!=(e&(dx>0?4:dx<0?1:dy>0?8:2));
  } else if(H==H.hexa) {
    int ax=2*x+(y&1),ax2=2*x2+(y2&1),dx=ax2-ax,dy=y2-y,i=i2b(dy)?dy>0?dx<0?5:4:dx<0?1:2:dx<0?0:3;
    return 0!=(e&(1<<i));
  } else if(H==H.tria) {
    int d=1^(y&1)^(x&1),dx=x2-x,dy=y2-y,dy2=i2b(d)?-1:1,i;
    if(i2b(dy)) i=0;
    else i=i2b(d^(b2i(dx<0)))?2:1;
    return 0!=(e&(1<<i));
  } else if(H==H.tria2) {
     int xx=(x+1+2*(y&1))&3,i=0;
     if(xx==0) i=y2<y?1:x2<x?0:2;
     if(xx==1) i=y2>y?1:x2>x?0:2;
     if(xx==2) i=y2>y?0:x2<x?1:2;
     if(xx==3) i=y2<y?0:x2>x?1:2;
     return 0!=(e&(1<<i));
  } else if(H==H.tria4) {
    int xx=(((1^y)&1)<<1)|(1^x&1),i=0;
    if(xx==0) i=x2<x?2:y2>y?1:0;
    if(xx==1) i=y2<y?2:x2<x?1:0;
    if(xx==2) i=y2>y?2:x2>x?1:0;
    if(xx==3) i=x2>x?2:y2<y?1:0;
    return 0!=(e&(1<<i));
  } else if(H==H.trap) {
    var xy=_peg(x2,y2);int i=_idx(xy[0],xy[1],_points11(x,y),0,0);
    return 0!=(e&(1<<i));
  } else if(H==H.deca) {
    var xy=_peg(x2,y2);int i=_idx(xy[0],xy[1],_points7(x,y),0,0);
    return 0!=(e&(1<<i));
  } else if(H==H.cubes) {
    var xy=_peg(x2,y2);int i=_idx(xy[0],xy[1],_points8(x,y),0,0);
    return 0!=(e&(1<<i));
  } else if(H==H.delta) {
    var xy=_peg(x2,y2);int i=_idx(xy[0],xy[1],_points9(x,y),0,0);
    return 0!=(e&(1<<i));
  } else if(H==H.penta) {
    var xy=_peg(x2,y2);int i=_idx(xy[0],xy[1],_points15(x,y),0,0);
    return 0!=(e&(1<<i));
  } 
  return false;
}


public void _mdesign(D dm,int up,float[] mp,float[] mm2,float[] mu,ME ev) {
 if(dm==0) return;
 int lx=(int)mu[0],ly=(int)mu[1];
 if(i2b(up)) {
   if(lx<1||_extx(0)) {lx=1;_extentx();mp[0]++;}
   if(ly<1||_exty(0)) {ly=1;_extenty();mp[1]++;}
 }
 if(lx>=Width||ly>=Height) _extent(lx,ly);
 int px=(int)mp[0],py=(int)mp[1],ex=lx,ey=ly;
 if(dm==D.edge) {
   if(mu[7]>=0) {
     int a=(int)(mp[8])&(1<<(int)mp[7]);int i=(int)mu[7],m=1<<i;int[] r=_dub1(lx,ly,i);int m2=1<<r[2];
     if(i2b(a)) {_rstblock(lx,ly,m);_rstblock(r[0],r[1],m2);} else {_setblock(lx,ly,m);_setblock(r[0],r[1],m2);}
   }
 } else if(dm==D.color) {
   if(up<0) {
     int fc=Pal.IntColor(View.f.bBg.BackColor); 
     if(ev.shiftKey) {View.dm9c=_getuco(lx,ly);if(View.dm9c==fc||!i2b(View.dm9c)) View.dm9c=0;else View.f.bBg.BackColor=Pal.IntColor(View.dm9c);}
     else View.dm9c=fc;
   }
   _setuco(lx,ly,View.dm9c);
 } else if(dm==D.color2) {
   if(up<0) {
     int fc=Pal.IntColor(View.f.bFg.BackColor); 
     if(ev.shiftKey) {View.dm10c=_getpgc(lx,ly);if(View.dm10c==fc||!i2b(View.dm10c)) View.dm10c=0;else View.f.bFg.BackColor=Pal.IntColor(View.dm10c);}
     else View.dm10c=fc;
   }
   _setpgc(lx,ly,View.dm10c);
 } else if(dm==D.free||px==lx&&py==ly) {
    int c=Ch(px,py),y;
    _extent(lx,ly);
    if(up!=0) {
      if(lx==px&&ly==py) {
        bool wh=View.white;
        Data[Index(lx,ly)].ch=ev!=null&&ev.ctrlKey?wh?c==2?3:2:c==2?0:2:c==2?wh?3:1:c==3?1:c==1?0:2;
      } else Data[Index(lx,ly)].ch=c;
    } else {
      if(abs((int)mm2[0]-lx)>1||abs((int)mm2[1]-ly)>1) _dline((int)mm2[0],(int)mm2[1],lx,ly,c);
      else if(ly>=0&&In(lx,ly)) Data[Index(lx,ly)].ch=c;
    }
 } else {
   View.sele.Clear();
   var c=i2b(up)?Ch(px,py):-1;
   if(dm==D.circ||dm==D.circ2) { int[] e=_dcircl2(mp[5],mp[6],mu[5],mu[6],c,dm==D.circ2,ev.shiftKey);ex=e[0];ey=e[1];}
   else if(dm==D.fill) _dfill(lx,ly,c,B0);
   else if(dm==D.rect) _drect(px,py,lx,ly,c);
   else _dline3((int)mp[5],mp[6],mu[5],mu[6],c);
   //else _dline(px,py,lx,ly,c);
 }
 _resizea(2*ex+(ey&1),ey); 
}

void _dline(int px,int py,int mx,int my,int c) {
  int i,x,y,dx=mx-px,dy=my-py,ax=(int)abs(dx),ay=(int)abs(dy);bool d=ax>ay;int a=d?ax:ay;
  for(i=0;i<=a;i++) {
    x=px+(((i*dx+(d?0:a/2))/a)|0);y=py+(((i*dy+(d?a/2:0))/a)|0);
    if(In(x,y)) if(c<0) Push(View.sele,x,y);else Data[Index(x,y)].ch=c;
  }
}

void _dline3(float px,float py,float mx,float my,int c) {
  int k,i,xx,yy;float dx=mx-px,dy=my-py,ax=abs(dx),ay=abs(dy);bool d=ax>ay;float a=d?ax:ay,x,y;
  var m=new Dictionary<int,bool>();float[] xy;
  for(i=0;i<=a;i++) {
    x=px+dx*i/a;y=py+dy*i/a;
    xy=_mxy(H,x,y);xx=(int)xy[0];yy=(int)xy[1];
    k=xx|(yy<<16);
    if(!m.ContainsKey(k)&&In(xx,yy)) {
      m[k]=B1;
      if(c<0) Push(View.sele,xx,yy);else Data[Index(xx,yy)].ch=c;
    }
  }
}

float _d2(float dx,float dy) { return dx*dx+dy*dy;}
void _dcircl(int px,int py,int mx,int my,int c,bool p2) {
  float[] s=_peg(px,py),t=_peg(mx,my);
  _dcircl2(s[0],s[1],t[1],t[1],c,p2,false);
}

int[] _dcircl2(float px,float py,float mx,float my,int c,bool p2,bool circ) {
  float sx,sy,r,r1;float[] ml;byte[] l,l2;float[] s;int x,y;
  if(p2) {
    sx=(px+mx)/2;sy=(py+my)/2;r=_d2(sx-mx,sy-my)+1;
  } else { sx=px;sy=py;r=_d2(sx-mx,sy-my)+1;}
  r1=(float)Math.Sqrt(r);
  ml=_mxy(H,sx+r1,sy+r1);
  _extent((int)ml[0]+1,(int)ml[1]+1);
  for(y=1;y+1<Height;y++) {
    for(x=1,l2=new byte[Width],l=new byte[Width];x+1<Width;x++) {
      s=_peg(x,y);
      if(_d2(sx-s[0],sy-s[1])<r) {
        if(c<0||circ) l[x]=1;
        else Data[Index(x,y)].ch=c;
      }
    }
    if(c<0||circ) for(x=1;x+1<Width;x++) {
      if(l2[x]!=0&&(l[x-1]==0||l[x]==0||l[x+1]==0)) {if(c<0) Push(View.sele,x,y-1);else Data[Index(x,y-1)].ch=c;}
      if(l[x]!=0&&(l[x-1]==0||l[x+1]==0||l2[x-1]==0||l2[x]==0||l2[x+1]==0)) {if(c<0) {l[x]=2;Push(View.sele,x,y);} else Data[Index(x,y)].ch=c;};
    }
  }
  return IA((int)ml[0]+1,(int)ml[1]+1);
}

void _drect(int px,int py,int mx,int my,int c) {
  int x,y,r;
  if(px>mx) {r=px;px=mx;mx=r;}
  if(py>my) {r=py;py=my;my=r;}
  for(y=py;y<=my;y++) {
    if(In(0,y)) 
      if(c<0) {
        Push(View.sele,px,y,mx,y);
        if(y==py||y==my) for(x=px+1;x<mx;x++) Push(View.sele,x,y);
      } else for(x=px;x<=mx;x++)
        Data[Index(x,y)].ch=c;
  }
}

void _dfill(int px,int py,int c,bool k) {
  if(!In(px,py)||py<1||py+1>Height) return;
  int s=Data[Index(px,py)].ch;var fifo=IL(px,py);int n=0,x,x1,y,r,m,dx,dy,a,t,f,nc=c<0?c-1:c;bool d,u;

  var bf=_borderf(H);
  if(s==c) return;
  Data[Index(px,py)].ch=nc;
  while(n<fifo.Count) {
    x=x1=fifo[n];y=fifo[n+1];n+=2;m=fifo.Count;u=y>1;d=y+1<Height;
    t=0;
    var b=bf(x,y);
    int i,l=b.Length,x2,y2;
    for(i=0;i<l;i+=2) {
      f=0;x2=x+b[i];y2=y+b[i+1];
      if(y2>0&&y2<Height&&(f=Data[Index(x2,y2)].ch)==s&&(k||!_blocked(x,y,x2,y2))) {t++;Push(fifo,x2,y2);}
      else if(f==nc||f=='f') t++;
    }
    if(c<0&&t==(H==H.penta?5:H==H.hexa?6:H==H.tria||H==H.tria2||H==H.tria4?3:4)) 
       Data[Index(fifo[n-2],fifo[n-1])].ch='f';
    for(;m<fifo.Count;m+=2)
      Data[Index(fifo[m],fifo[m+1])].ch=nc;
  }
  if(c<0) for(m=0;m<fifo.Count;m+=2) {
    f=Data[Index(x=fifo[m],y=fifo[m+1])].ch;
    Data[Index(x,y)].ch=s;
    if(f!='f') Push(View.sele,x,y);
  }
}



public bool _mdown(ME e) {   
   float[] mm=View.mm=pmap._mxy(H,e.x,e.y),mp=View.mp=mm;
   int cx,cy; 
    if(mp!=null) {
            cx=(int)mp[0];cy=(int)mp[1];  //_mdown
            if(View.design==D.edge) { Array.Resize(ref mp,9);View.mm=View.mp=mp;mp[7]=pmap._idx(mp[5],mp[6],_points(cx,cy),0,0);mp[8]=_getblock(cx,cy);}
            else if(View.design==D.color||View.design==D.color2)  {
              _mdesign(View.design,-1,mp,mp,mm,e);
              return true;
            }
            if(mp!=null&&Ch(cx,cy)>0) {
              View.sele.Clear();pmap.Push(View.sele,cx,cy);
              return true;
            }
       }
  return false;
}

public int _ticks() { return System.Environment.TickCount;}
public bool _mmove(ME e,bool d) {
  if(!i2b(e.buttons)||!o2b(View.mp)) return false;
  float[] xy=_mxy(H,e.x,e.y),mm2=View.mm;float r;int x7=0;var dm=View.design;
  if(dm==D.edge) {x7=1;A(ref xy,7,_idx(xy[5],xy[6],_points((int)xy[0],(int)xy[1]),0,128));}
  //if(design==) x7=1,xy[7]=_idx(xy[5],xy[6],_points(xy[0],xy[1]),tria?2:0);
  if(Game==Game.Shift&&dm==0) {x7=1;A(ref xy,7,_idx(xy[5],xy[6],H==H.trap?_points11h((int)xy[0],(int)xy[1]):_points((int)xy[0],(int)xy[1]),H==H.trap?b2i(Diag):2,0));}
  dm=_design(e);
  if(dm==D.circ2||dm==D.fill) {
    if(!d) {
      r=_d2(xy[5]-View.mm[5],xy[6]-View.mm[6]);
      if(r==0) return false ;
      if(!o2b(View.mm1)) View.mm1=IA(_ticks());
      View.mm1e=e;
      return false;
    }
  } else if((dm==D.edge&&xy[7]==-1)||(xy[0]==View.mm[0]&&xy[1]==View.mm[1]&&(!i2b(x7)||xy[7]==View.mm[7]))) return false;
  View.mm=xy;View.mm1=null;
  if(View.design!=0) {_mdesign(View.design,0,View.mp,mm2,xy,e);return true;}
  else if(Game==Game.Shift) {_shifter(0,xy,e);return true;}   
  return false;
}

bool _full() {
  for(int i=0;i<Data.Length;i++) if(Data[i].ch==1) return B0;
  return B1;
}

bool _fullxy(int x,int y) {
  if(!_xo(x,y)) return false;
  var d=new Dictionary<int,bool>();var f=new List<int>{};int i=0,x2,y2,k,c,b,j;bool dd;
  d[(y<<16)|x]=B1;
  Push(f,x,y);
  while(i<f.Count) {
    if(i>64) {f.RemoveRange(0,64);i=0;}
    x2=f[i++];y2=f[i++];
    var ja=_jumpsx()(x2,y2);
    for(j=0;j<ja.Length;j+=4) {
      x=x2+ja[j+0];y=y2+ja[j+1];
      if(_blocked(x2,y2,x,y)) continue;
      c=Ch(x,y);
      if(c==1) return B0;
      if((c==2||c==3)&&!d.TryGetValue(k=(y<<16)|x,out dd)) {d[k]=B1;Push(f,x,y);}
    }
  }
  return B1;
}


public void AddUndo(U x) {
  var u=View.undo;  
  u.RemoveRange(View.redo,u.Count-View.redo);
  View.undo.Add(x);
  View.redo=u.Count;
}
public bool _mup(ME e) { 
  if(View.mp==null) return false;
  View.sele.Clear();
  float[] mxy=_mxy(H,e.x,e.y);int lx=(int)mxy[0],ly=(int)mxy[1],px=(int)View.mp[0],py=(int)View.mp[1],qx,qy,rx,ry,qr;
  D dm=_design(e);
  if(dm>0) {
    if(dm==D.edge) {Array.Resize(ref mxy,8);mxy[7]=_idx(mxy[5],mxy[6],_points((int)mxy[0],(int)mxy[1]),0,0);}
    _mdesign(dm,1,View.mp,View.mm,mxy,e);
    return true;
  }
  if(!In(lx,ly)||!In(px,py)) return true;
  if(Game==Game.Shift) {
    var u=_shifter(1,mxy,e);
    AddUndo(u);
    _moves(1);_beep();
    return true;
  }
  if(Game==Game.OnOff) {
    int c=Data[Index(lx,ly)].ch;
    if(c==3||c==2||c==1) {
      int mc=(int)mxy[4],p,p2=0,p3=0,x6=View.onoff6;int[] xyc;
      {
        if(H==H.trap) {p=11;p3=_idx(mxy[5],mxy[6],_points11(lx,ly),b2i(x6>=6),0);xyc=_switch11(lx,ly,p2=_mask(x6,lx,ly,p3),p3);}
        else if(H==H.delta) {p=19;p3=4*_idx(mxy[5],mxy[6],_points9(lx,ly),b2i(x6>=6),0);xyc=_switch19(lx,ly,p2=_mask(x6,lx,ly,p3),p3);}
        else if(H==H.deca) {p=7;p3=_idx(mxy[5],mxy[6],_points7(lx,ly),b2i(x6>=6),0);xyc=_switch7(lx,ly,p2=_mask(x6,lx,ly,p3),p3);}
        else if(H==H.cubes) {p=18;p3=4*_idx(mxy[5],mxy[6],_points8(lx,ly),b2i(x6>=6),0);xyc=_switch18(lx,ly,p2=_mask(x6,lx,ly,p3),p3);}
        else if(H==H.penta) {p=15;xyc=_switch15(lx,ly,p2=_mask(x6,0,0,0),p3=_idx(mxy[5],mxy[6],_points15(lx,ly),b2i(x6>=6),0));}
        else if(H==H.hexa) {p=16;xyc=_switch16(lx,ly,p2=_mask(x6,0,0,0),p3=_idx(mxy[5],mxy[6],_points16(lx,ly),b2i(x6==6),0));}
        else if(H==H.tria) {p=4;xyc=_switch3(lx,ly,p2=_mask(x6,0,0,0),p3=8*_idx(mxy[5],mxy[6],_points3(lx,ly),b2i(x6==6),0));} //tria?(p=4,_switch4(lx,ly))
        else if(H==H.tria4) {p=5;xyc=_switch5(lx,ly,0,0);}
        else if(H==H.tria2) {p=2;xyc=_switch2(lx,ly,0,0);} //:hexa?_switch16(lx,ly,p=mc&1?5:3)
        else {p=13;xyc=_switch14(lx,ly,p2=Diag&&abs(mxy[2])+abs(mxy[3])>Cell?170:_mask(x6,0,0,0),p3=2*_idx(mxy[5],mxy[6],_points14(lx,ly),b2i(x6==6),0));}
      }
      var u=U.OnOff(xyc,lx,ly,p,p2,p3);
      AddUndo(u);
      _moves(1);_beep();
    }
    return true;
  }
  { int ip,iq,ir,b,j,dx=-1,dy=0,l,r,u2,d,d2,ld,lu,rd,ru,s,ii=Index(px,py),c=Data[ii].ch,c1,ex;U u;
    if(c==2||c==3) {
    var ja=_jumpsx()(px,py);
    int i,n=_jumpsl(ja);
      for(i=0,j=0;i<n;i++,j+=4) {
        qx=px+ja[j+0];qy=py+ja[j+1];rx=px+ja[j+2];ry=py+ja[j+3];
        if(((lx==qx&&ly==qy)||(lx==rx&&ly==ry))&&_can5(px,py,qx,qy,rx,ry)) {
          if(i2b(dy)) {
            int kx=px+ja[4*dx+0],ky=py+ja[4*dx+1];
            if(_near(mxy[5],mxy[6],_peg(qx,qy),_peg(kx,ky))) dx=i;
            break;
          } else {dx=i;dy=1;}
        }
      }
      if(dy==0) {
    	if(_full()||_fullxy(px,py)) {
        int ipx=Index(px,py);
  	     Data[ipx].ch=1;
         u=U.Sol(px,py,0,0,c,Data[ipx].fore,0);
         AddUndo(u);
        }
	      return true;      
      }
      j=4*(i=dx);qx=px+ja[j+0];qy=py+ja[j+1];rx=px+ja[j+2];ry=py+ja[j+3];
    } else return false;
  ip=Index(px,py);iq=Index(qx,qy);ir=Index(rx,ry);
  c1=Data[iq].ch;
  u=U.Sol(px,py,dx,dy,c,c1,_getpgc(qx,qy),0);
  AddUndo(u);
  _moves(1);_beep();
  Data[ip].ch=1;
  b=u.ia[5];
  if(View.white) {
    r=2*(b2i(c==3))+b2i(c1==3);
    r=View.whiter[r];
    b=r=='b'?2:r=='e'?c1:r=='f'?c1==2?3:2:r=='w'?3:r=='o'?2:1;
  } else b=c==2?1:c1==2?3:2;
  Data[iq].ch=b;
  Data[ir].ch=c;
  Data[ir].fore=Data[ip].fore;
  
  return true; 
  }
}

        internal void Undo(int n) { 
         if(n<1) n=1;
         var v=View;
         while(v.redo>0&&pmap.i2b(n--)) {
           U u=v.undo[--v.redo];int[] ia=u.ia;int ii,px=ia[0],py=ia[1],dx=ia[2],dy=ia[3],m=-1;
           if(u.t==2) {
             int pi;
             int[] xy=_shiftxy(px,py,dx,i2b(dy),out pi);
             _shift(xy,-ia[4]);
            } else if(u.t==1) {
             _switchx(ia[2])(px,py,ia[3],ia[4],u.xy);
            } else if(dx!=0||dy!=0) {
	           var qr=_jumps(px,py,dx);int qx=qr[0],qy=qr[1],rx=qr[2],ry=qr[3],ir,iq;
	           ii=Index(px,py);Data[ii].ch=ia[4];
             iq=Index(qx,qy);Data[iq].ch=ia[5];
             ir=Index(rx,ry);Data[ir].ch=1;
             Data[ii].fore=Data[ir].fore;
             Data[iq].fore=ia[6];             
            } else {
	            m=0;ii=Index(px,py);Data[ii].ch=ia[4];Data[ii].fore=ia[5];              
            }
            _moves(m);
           }
        }
        internal void Redo(int n) { 
         if(n<1) n=1;
         var v=View;
         while(v.redo<v.undo.Count&&pmap.i2b(n--)) {
           U u=v.undo[v.redo++];int[] ia=u.ia;int ii,px=ia[0],py=ia[1],dx=ia[2],dy=ia[3],m=1;
           if(u.t==2) {
             int pi;
             int[] xy=_shiftxy(px,py,dx,i2b(dy),out pi);
             _shift(xy,ia[4]);
            } else if(u.t==1) {
             _switchx(ia[2])(px,py,ia[3],ia[4]);
            } else if(dx!=0||dy!=0) {
	           var qr=_jumps(px,py,dx);int qx=qr[0],qy=qr[1],rx=qr[2],ry=qr[3],ir,iq;
	           ii=Index(px,py);Data[ii].ch=1;
             iq=Index(qx,qy);Data[iq].ch=1;
             ir=Index(rx,ry);Data[ir].ch=ia[4];
             Data[ir].fore=Data[ii].fore;
             ia[6]=Data[iq].fore;
            } else {
	            m=0;ii=Index(px,py);Data[ii].ch=1;ia[5]=Data[ii].fore;
            }
            _moves(m);
           }
        }

int _div2p(int x) { return (x+1)>>1;}
int _div2n(int x) { return (x)>>1;}

public void transf2(int mode) {
   int b,ii,jj,x2,y2,sx=0,sy=0,x,x1,y1,x4,y,j,n,xi=1<<20,xa=0,yi=1<<20,ya=0,bfx=0,nw=0,nh=0;
   C[] b2=null;
   for(j=n=0;j<2;j++) {
    for(y=ii=0;y<Height;y++)
      for(x=0;x<Width;x++,ii++) {
        if(Data[ii].ch>0) {
          if(H==H.trap) {
            int d=(x+1)%2,cx=(x+1-d)/2,dx=i2b(y&1)?0:-2,dd=(cx+2*(y&1))%3,m;
            y2=y;
            if(mode==1)  {cx=-cx;d=1-d;cx+=1^y&1;}
            else if(mode==2) {cx+=0;d=dd==1?d:1-d;y2=-y;}
            else {m=cx;y2=m-_div2n(y);cx=-y-_div2p(y2);d=(d+2)%3;}
            x2=2*cx+d-1;
          } else if(H==H.deca) {
            int d=(x+2)%3,cx=(x+2-d)/3,dx=i2b(y&1)?0:-3,m;
            y2=y;
            if(mode==1)  {cx=-cx;cx+=1^y&1;}
            else if(mode==2) {cx+=0;y2=-y;d=d==1?d:2-d;}
            else {m=cx;y2=m-_div2n(y);cx=-y-_div2p(y2);d=(d+1)%3;}
            x2=3*cx+d-2;
          } else if(H==H.cubes) {
            int d=(x+2)%3,cx=(x+2-d)/3,dx=i2b(y&1)?0:-3,m;
            y2=y;
            if(mode==1)  {cx=-cx;d=i2b(d)?3-d:d;cx+=1^y&1;}
            else if(mode==2) {cx+=(d==2&&i2b(y&1)?1:0)+(d==1&&!i2b(y&1)?-1:0);y2=-y+(i2b(d)?-1:0);d=i2b(d)?3-d:d;}
            //else m=cx,y2=m-(y>>1),cx=-y+((y+1)/3|0)-_div2p(m)+((((m^1)&(y2^0))&1)^1),d=(d+2)%3;
            //else m=cx,y2=m-_div2n(y),cx=-y-_div2n(m)-_div2p(y2),d=(d+2)%3;
            else {m=cx;y2=m-_div2n(y);cx=-y-_div2p(y2);d=(d+2)%3;}
            x2=3*cx+d-2;
          } else if(H==H.penta) {
            int cx=(x+1)/2,v=(cx+y)&1,m;x1=x&1;
            y2=y;
            if(mode==1) {cx=-cx;x1^=v;}
            else if(mode==2) {y2=-y;x1^=v^1;}
            else {m=cx;cx=-y2+1;y2=m;x1^=v^1;}
            x2=2*cx-x1;
          } else if(H==H.tria4) {
            int dx=1^x&1,dy=1^y&1,m,r;
            r=dx+2*dy;x2=x-dx;y2=y-dy;
            if(mode==1) {r=r==0?3:r==3?0:r;x2=-x2;}
            else if(mode==2) {r=r==1?2:r==2?1:r;y2=-y2;}
            else {r=r==1?3:r==3?2:r==2?0:1;m=x2;x2=-y2;y2=m;}
            x2+=r&1;y2+=i2b(r&2)?1:0;
          } else if(mode==1) {x2=(H==H.hexa?(1^y&1):H==H.tria2?1:0)-x;y2=y;}
          else if(mode==2) {x2=x;y2=(H==H.tria?1:H==H.tria2?1:0)-y;}
          else {
            if(H==H.tria2) {
              x1=x-1;y1=y-1;int d=(0^x1^(x1>>1)^y1)&1;
              y2=-(x1>>1)+1;x2=2*y1+d+1;
            } else if(H==H.tria) {
              //y2=_div2p(-x-y+((y^x)&1)),x2=-x+y+-y2;
              y2=-_div2p(x-y);x2=x+y+y2;
            } else if(H==H.hexa) {
             x1=2*x+(y&1);y2=_div2n(y+x1);x4=_div2p(x1-3*y);x2=_div2p(x4+(y2&1));
             bfx=6;//function(e) { e=(((e&63)>>5)|(e<<1))&63;return e;}
            } else {
              x2=-y;y2=x;
              bfx=4;//function(e) { e=(((e&15)>>3)|(e<<1))&15;return e;}
            }
          }
          if(i2b(j)) {
            b2[jj=nw*(sy+y2)+sx+x2]=Data[ii];
            if(bfx>0) {
              int e=b2[jj].block;
              b2[jj].block=rol(bfx,e,1);
            }
          } else {
            if(!i2b(n)) {xi=xa=x2;yi=ya=y2;n=1;}
            else {
             if(x2<xi) xi=x2;else if(x2>xa) xa=x2;
             if(y2<yi) yi=y2;else if(y2>ya) ya=y2;
            }
          }
        }
      }
    if(!i2b(j)) {
      sx=1-xi;sy=1-yi;
      if(!i2b(mode)) {
        if(H==H.tria2) {sx-=(sx-6)%4;sy-=(sy-6)%4;sx+=sx<1-xi?4:0;sy+=sy<1-yi?4:0;}
        //if(tria&&((0^yi^xi)&1)) sy+=1;
        if(H==H.tria&&i2b((1^yi^xi)&1)) sy+=1;
        if(H==H.hexa&&i2b(yi&1)) sy+=1;
        if(H==H.penta) {
          int dx=sx&3,dy=sy&1;
          if(i2b(dx)) dx=4-dx;
          if(dx>2) {dy^=1;dx-=2;};
          sx+=dx;sy+=dy;
        }
        if(H==H.cubes||H==H.deca) {
          var d=sx%3;if(i2b(d)) sx+=d>0?3-d:-d;
          if(i2b(sy&1)) sy++;
        }
      } else if(mode==1||mode==2) { 
        if((H==H.penta||H==H.hexa||H==H.tria||H==H.tria2||H==H.tria4)&&i2b(sy&1)) sy++;
        if((H==H.tria||H==H.tria4)&&i2b(sx&1)) sx++;
        if((H==H.penta||H==H.tria2)&&i2b(sx&3)) sx+=4-(sx&3);
        if(H==H.cubes||H==H.deca) {
          int d=sx%3;
          if(H==H.cubes) {
            if(i2b(d)) sx+=d>0?3-d:-d;
          } else 
            if(mode==1) { if(i2b(d)) sx+=d>0?3-d:-d;}
          if(i2b(sy&1)) sy++;
        }
        if(H==H.trap) {
          var d=sx%2;if(i2b(d)) sx+=d>0?2-d:-d;
          if(i2b(sy&1)) sy++;
        }
      }
      nw=xa+sx+1;nh=ya+sy+1;
      b2=new C[nw*nh];
    }
  }
  Data=b2;Width=nw;Height=nh;
  _resizea(0,0);  
}


void _beep() {
  GDI.Beep(220,50); 
}
D _design(ME e) { return View.design==D.edge?D.edge:e.ctrlKey?e.shiftKey?D.rect:D.free:View.design;}
void _moves(int n) {
  View.moves+=n;
  View.f.Moves();
}

 
  /*var n,m,b,x,xe,y,c,nx=board[0].length-1,ny=board.length-1,w,ms=+new Date(),p=f.peg.checked;
  n=m=0,_clear();
  ctx.strokeStyle="#000";
  if(rou) ctx.lineJoin="round';
  for(y=0;y<ny;y++)
    for(x=0,b=board[y],xe=b.length;x<xe;x++)
      if((c=b[x])!=' ') {
        trap?_drawcell11(x,y):delta?_drawcell9(x,y):cubes?_drawcell8(x,y):penta?_drawcell15(x,y):tria4?_drawcell5(x,y):tria2?_drawcell2(x,y):tria?_drawcell3(x,y):hexa?_drawcell16(x,y):_drawcell(x,y);
        if((w=c=='w')||c=='o') {
          n+=onoff?c=='o':c=='o'||w,trap?_drawpeg11(x,y,w,p):delta?_drawpeg9(x,y,w,p):cubes?_drawpeg8(x,y,w,p):penta?_drawpeg15(x,y,w,p):tria4?_drawpeg5(x,y,w,p):tria2?_drawpeg2(x,y,w,p):tria?_drawpeg3(x,y,w,p):hexa?_drawpeg16(x,y,w,p):_drawpeg(x,y,w,p);
          m+=onoff?c=='o':+_can(x,y,0,1)+_can(x,y,0,-1)+_can(x,y,1,0)+_can(x,y,-1,0)+(diag?_can(x,y,1,-1)+_can(x,y,1,1)+_can(x,y,-1,-1)+_can(x,y,-1,1):0);
        }
      }
  ctx.lineWidth=2;
  ctx.fillStyle='#000';
  for(y=1;y<ny;y++)
    for(x=1;x<nx;x++)
      if((b=board[y])&&(c=b[x])&&c!=' ') 
        trap?_drawborder11(x,y):delta?_drawborder9(x,y):cubes?_drawborder8(x,y):penta?_drawborder15(x,y):tria4?_drawborder5(x,y):tria2?_drawborder2(x,y):tria?_drawborder3(x,y):hexa?_drawborder16(x,y):_drawborder(x,y);
  _pegs(n);
  for(n=0;n<sele.length;n+=2)
    _drawsele(sele[n],sele[n+1]);
  drawms=(drawt=+new Date())-ms;
   */
    }

}
