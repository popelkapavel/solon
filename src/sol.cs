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
    public partial class pmap {

delegate int[] f_jumps(int x,int y);
int[] _jumps5(int px,int py) {
  int r=(((1^py)&1)<<1)|(1^px&1);  
  if(r==1) return new int[] {0,1,-1,1 ,-1,0,-1,1 ,-1,-1,-1,-2 ,-1,-1,0,-1 ,0,1,+1,0 ,-1,0,-2,1 ,-1,1,0,2 ,-1,-1,0,-2};
  else if(r==2) return new int[] {0,-1,+1,-1 ,1,0,1,-1 ,+1,+1,0,1 ,1,1,+1,+2 ,0,-1,-1,0 ,1,0,2,-1 ,1,-1,0,-2 ,1,1,0,2};
  else if(r==3)return new int[] {0,-1,-1,-1 ,-1,0,-1,-1 ,+1,-1,+2,-1 ,1,-1,+1,0 ,0,-1,-1,-2 ,-1,0,0,+1 ,-1,-1,-2,0 ,1,-1,2,0};
  return new int[] {+1,0,+1,+1 ,0,+1,+1,+1 ,-1,+1,-1,0 ,-1,+1,-2,+1 ,+1,0,0,-1 ,0,+1,+1,+2 ,-1,1,-2,0 ,1,1,2,0};
} 

int[] _jumps11(int x,int y) {
  int d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d,dx=i2b(y&1)?0:-2;
  if(dd==1) return new int[] {dx,1,dx-1,1 ,-1,0,dx-1,-1 ,dx+1,-1,dx,-1 ,1,0,2,0  ,dx,1,-2,0 ,dx,1,dx+1,1 ,-1,0,-2,0 ,dx+1,-1,dx+2,-1 ,1,0,dx+2,-1 ,1,0,dx+3,1};
  if(dd==2) return new int[] {-1,0,-2,0 ,dx,-1,dx-1,-1 ,dx+2,-1,1,-2 ,1,0,dx+2,1  ,-1,0,dx+1,1 ,dx,-1,dx+1,-1 ,dx+2,-1,dx+1,-1 ,dx+2,-1,dx+3,-1 ,1,0,2,0 ,1,0,dx+1,1};
  if(dd==3) return new int[] {dx,-1,-1,-2 ,dx+2,-1,dx+3,-1 ,1,0,2,0 ,-1,0,dx,1  ,dx,-1,dx-1,-1 ,dx,-1,dx+1,-1 ,dx+2,-1,dx+1,-1 ,1,0,dx+1,1 ,-1,0,dx+1,1 ,-1,0,-2,0};
  if(dd==4) return new int[] {dx+1,-1,dx+2,-1 ,1,0,dx+3,-1 ,dx+2,1,dx+3,1 ,-1,0,-2,0  ,dx+1,-1,dx,-1 ,1,0,2,0 ,dx+2,1,2,0 ,dx+2,1,dx+1,1 ,-1,0,dx-1,1 ,-1,0,dx,-1};
  if(dd==5) return new int[] {1,0,dx+3,1 ,dx+1,1,-1,2 ,dx,1,dx-1,1 ,-1,0,dx-1,-1 ,1,0,2,0 ,1,0,dx+2,1 ,dx+1,1,dx+2,1 ,dx,1,-2,0 ,-1,0,-2,0 ,-1,0,dx+1,-1};
  return new int[] {dx+2,1,dx+3,1 ,dx+1,1,1,2 ,-1,0,dx-1,1 ,1,0,dx+3,-1  ,dx+2,1,2,0 ,dx+1,1,dx,1 ,-1,0,-2,0 ,-1,0,dx,1 ,1,0,dx+1,-1 ,1,0,2,0};
/*  if(dd==1) return new int[] {dx,1,-1,0,dx+1,-1,1,0};
  if(dd==2) return new int[] {-1,0,dx,-1,dx+2,-1,1,0};
  if(dd==3) return new int[] {dx,-1,dx+2,-1,1,0,-1,0};
  if(dd==4) return new int[] {dx+1,-1,1,0,dx+2,1,-1,0};
  if(dd==5) return new int[] {1,0,dx+1,1,dx,1,-1,0};
  return new int[] {dx+2,1,dx+1,1,-1,0,1,0};*/
}

int[] _jumps3(int x,int y) {
  int d=(y^x)&1,dy=i2b(d)?-1:1;
  return new int[] {-1,0,-2,0 ,-1,0,-1,-dy ,1,0,2,0 ,1,0,1,-dy ,0,dy,-1,dy ,0,dy,1,dy
    ,-2,dy,-3,dy ,2,dy,3,dy ,0,-dy,0,-2*dy  ,-1,0,-3,-dy ,1,0,3,-dy ,0,dy,0,2*dy};
}

int[] _jumps2(int x,int y) {
  int xx=(x+1+2*(y&1))&3;
  if(xx==1) return new int[] {-1,0,-2,0 ,-1,0,-1,-1 ,0,1,-1,1 ,0,1,1,1 ,1,0,1,1 ,1,0,2,0  ,-1,0,-2,-1 ,1,1,2,1};
  if(xx==2) return new int[] {-1,0,-2,0 ,-1,0,-1,1 ,1,0,1,-1 ,1,0,2,0 ,0,1,-1,1 ,0,1,1,1  ,-1,1,-2,1 ,1,0,2,-1};
  if(xx==3) return new int[] {-1,0,-2,0 ,-1,0,-1,1 ,0,-1,1,-1 ,0,-1,-1,-1 ,1,0,2,0 ,1,0,1,-1  ,1,-1,2,-1 ,-1,0,-2,1};
  return new int[] {-1,0,-2,0 ,-1,0,-1,-1 ,0,-1,-1,-1 ,0,-1,1,-1 ,1,0,1,1 ,1,0,2,0  ,-1,-1,-2,-1 ,1,0,2,1};
}

int[] _jumps15(int x,int y) {
  int cx=(x+1)/2|0;bool x1=i2b(x&1),v=i2b((cx+y)&1);
  if(v) {
    if(x1) return new int[] {-2,0,-3,0 ,-2,0,-2,-1 ,1,-1,0,-1 ,1,-1,2,-1 ,1,0,2,0 ,1,0,3,0 ,0,1,1,1 ,0,1,2,1 ,-1,0,-3,0 ,-1,0,-2,1 // {-2,0,-3,-1 ,-1,0,-4,1 ,-1,-1,-1,-2 ,-1,1,-2,2}};
                  ,-2,0,-1,-1 ,1,-1,-1,-1 ,1,0,1,-1 ,0,1,-1,1 ,-1,0,-1,1};// ,{-2,0,-1,0 ,1,-1,1,0 ,1,0,0,1 ,0,1,1,0 ,-1,0,-2,0}};
    return new int[] {0,-1,-1,-1 ,0,-1,-2,-1 ,1,0,3,0 ,1,0,2,-1 ,2,0,3,0 ,2,0,2,1 ,-1,1,0,1 ,-1,1,-2,1 ,-1,0,-2,0 ,-1,0,-3,0 // {1,0,4,-1 ,2,0,3,1 ,1,-1,2,-2 ,1,1,1,2}};
                  ,0,-1,1,-1 ,1,0,1,-1 ,2,0,1,1 ,-1,1,1,1}; // ,{0,-1,-1,0 ,1,0,2,0 ,2,0,1,0 ,-1,1,-1,0 ,-1,0,-1,1 ,-1,0,0,-1}};
  } else {
    if(x1) return new int[] {-1,0,-2,0 ,-1,0,-2,1 ,0,-1,1,-2 ,0,-1,-2,-1 ,1,-1,1,-2 ,1,-1,2,-1 ,2,0,3,0 ,2,0,2,1 ,1,0,1,1 ,1,0,0,1
                  ,-1,0,-1,-1 ,0,-1,-1,-1 ,1,-1,3,-1 ,2,0,3,-1};
    return new int[] {-1,1,-1,2 ,-1,1,-2,1 ,-2,0,-3,0 ,-2,0,-2,-1 ,-1,0,-1,-1 ,-1,0,0,-1 ,1,0,2,0 ,1,0,2,-1 ,0,1,-1,2 ,0,1,2,1
                  ,-1,1,-3,1 ,-2,0,-3,1 ,1,0,1,1 ,0,1,1,1};
  }
}

int[] _jumps6(int x,int y) {
  int ex=i2b(y&1)?0:-1;
  return new int[] {-1,0,-2,0 ,1,0,2,0 ,ex,-1,-1,-2 ,ex+1,-1,1,-2 ,ex,1,-1,2 ,ex+1,1,1,2
         ,-1,0,ex-1,-1 ,-1,0,ex-1,1 ,1,0,ex+2,-1 ,1,0,ex+2,1
         ,ex,-1,ex-1,-1 ,ex,-1,0,-2 ,ex+1,-1,ex+2,-1 ,ex+1,-1,0,-2
         ,ex,1,ex-1,1 ,ex,1,0,2 ,ex+1,1,ex+2,1 ,ex+1,1,0,2};
}
int[] _jumps4(int x,int y) {
  return new int[] {-1,0,-2,0 ,1,0,2,0 ,0,-1,0,-2 ,0,1,0,2
         ,-1,0,-1,-1 ,-1,0,-1,1 ,1,0,1,-1 ,1,0,1,1
         ,0,-1,-1,-1 ,0,-1,1,-1 ,0,1,-1,1 ,0,1,1,1};
}

int[] _jumps8(int x,int y) {
  int d=(x+2)%3,cx=(x+2-d)/3,dx=i2b(y&1)?0:-3;
  if(d==1) return new int[] {-2,0,-3,0 ,1,0,3,0 ,-1,0,dx+3,-1 ,dx-1,1,dx,1 ,1,0,dx+2,1 ,dx-1,1,dx+1,1 ,-2,0,-4,0 ,-1,0,dx+1,-1};
  if(d==2) return new int[] {-2,0,dx,-1 ,2,0,3,0 ,dx+1,1,dx+3,1 ,-1,0,-3,0 ,-2,0,dx+2,-1 ,2,0,1,0 ,dx+1,1,dx+2,1 ,-1,0,dx-2,1};
  return new int[] {1,0,dx,+1 ,dx+2,-1,dx,-1 ,dx+4,-1,dx+3,-1 ,2,0,dx+3,1 ,1,0,-1,0 ,dx+2,-1,dx+1,-1 ,dx+4,-1,dx+5,-1 ,2,0,4,0};
}

int[] _jumps7(int x,int y) {
  int d=(x+2)%3,cx=(x+2-d)/3,dx=i2b(y&1)?0:-3;
  if(d==1) return new int[] {4,0,3,0 ,4,0,dx+5,1 ,dx+2,1,dx+3,1 ,dx+2,1,dx+4,1 ,1,0,dx,1 ,1,0,dx-1,1 ,1,0,-3,0 ,-1,0,-3,0 ,-1,0,dx+1,-1 ,-1,0,dx,-1 ,dx+4,-1,dx+2,-1 ,dx+4,-1,dx+3,-1 ,2,0,dx+7,-1 ,2,0,3,0
    ,4,0,dx+3,1 ,dx+2,1,dx,1 ,dx+4,-1,dx,-1 ,2,0,dx+3,-1};
  if(d==2) return new int[] {dx-2,1,dx,1 ,dx-2,1,dx-4,1 ,-4,0,-3,0 ,-4,0,-5,0 ,-2,0,dx,-1 ,-2,0,dx-1,-1 ,-2,0,dx+3,-1 ,-1,0,dx+3,-1 ,-1,0,1,0 ,-1,0,3,0 ,dx+1,1,dx+2,1 ,dx+1,1,dx+3,1 ,dx-1,1,-2,2 ,dx-1,1,dx,1
    ,dx-2,1,-3,0 ,-4,0,dx,-1 ,dx+1,1,3,0 ,dx-1,1,dx+3,1};
  return new int[] {dx+1,-1,dx,-1 ,dx+1,-1,2,-2 ,dx+5,-1,dx+3,-1 ,dx+5,-1,dx+4,-1 ,1,0,3,0 ,1,0,5,0 ,1,0,dx+3,1 ,2,0,dx+3,1 ,2,0,dx,1 ,2,0,dx+1,1 ,-2,0,-1,0 ,-2,0,-3,0 ,dx+2,-1,dx-2,-1 ,dx+2,-1,dx,-1
   ,dx+1,-1,dx+3,-1 ,dx+4,-1,3,0 ,-2,0,dx,1 ,dx+2,-1,-3,0};
}


int[] _jumps9(int x,int y) {
  int r=(x+2)%3,cx=(x+2-r)/3;bool d=i2b(1^(y&1)^(cx&1));int dy=d?-1:1;
  if(d) {
    if(r==1) return new int[] {-1,0,-1,-dy ,1,0,1,-dy ,2,0,4,0 ,-2,0,-4,0 ,-2,0,-2,dy ,2,0,2,dy  ,-2,0,-3,0 ,-1,0,-3,0 ,1,0,3,0 ,2,0,3,0};
    if(r==2) return new int[] {2,0,3,0 ,-1,0,-3,0 ,-2,0,-4,0 ,0,-dy,-1,-dy ,0,-dy,2,-dy ,2,0,4,0  ,-2,0,-2,-dy ,0,-dy,-2,-dy ,2,0,1,0 ,-1,0,1,0};
    return new int[] {-2,0,-3,0 ,0,-dy,1,-dy ,2,0,4,0 ,1,0,3,0 ,-2,0,-4,0 ,0,-dy,-2,-dy  ,-2,0,-1,0 ,0,-dy,2,-dy ,2,0,2,-dy ,1,0,-1,0};
  } else {
    if(r==1) return new int[] {-1,0,-1,-dy ,1,0,1,-dy ,2,0,4,0 ,-2,0,-4,0 ,-2,0,-2,dy ,2,0,2,dy  ,-2,0,-3,0 ,-1,0,-3,0 ,1,0,3,0 ,2,0,3,0};
    if(r==2) return new int[] {2,0,3,0 ,-1,0,-3,0 ,-2,0,-4,0 ,0,-dy,-1,-dy ,0,-dy,2,-dy ,2,0,4,0  ,-2,0,-2,-dy ,0,-dy,-2,-dy ,2,0,1,0 ,-1,0,1,0};
    return new int[] {-2,0,-3,0 ,0,-dy,1,-dy ,2,0,4,0 ,1,0,3,0 ,-2,0,-4,0 ,0,-dy,-2,-dy  ,-2,0,-1,0 ,0,-dy,2,-dy ,2,0,2,-dy ,1,0,-1,0};
  }
}

int[] _add2(int x,int y,int i,int[] jmp) {
  int[] r=new int[4];
  Array.Copy(jmp,4*i,r,0,4);
  r[0]+=x;r[1]+=y;r[2]+=x;r[3]+=y;
  return r;
}

int[] _jumps(int px,int py,int i) {
  var f=_jumpsx();
  return f==null?null:_add2(px,py,i,f(px,py));
}

f_jumps _jumpsx() {
  if(H==H.deca) return _jumps7;
  if(H==H.trap) return _jumps11;
  if(H==H.delta) return _jumps9;
  if(H==H.cubes) return _jumps8;
  if(H==H.penta) return _jumps15;
  if(H==H.hexa) return _jumps6;
  if(H==H.tria4) return _jumps5;
  if(H==H.quad) return _jumps4;
  if(H==H.tria) return _jumps3;
  if(H==H.tria2) return _jumps2;
  return null;
}

int _jumpsl(int[] ja) {
  return Diag?ja.Length/4:H==H.trap?4:H==H.quad?4:H==H.hexa?6:H==H.cubes?4:H==H.delta?6:H==H.tria?6:H==H.tria2||H==H.tria4?6:10;
}

bool _can5(int x,int y,int qx,int qy,int rx,int ry) {
  int i=Index(x,y),qi=Index(qx,qy),ri=Index(rx,ry);
  int c=Data[i].ch,c1=Data[qi].ch,c2=Data[ri].ch;
  if(!((c==2||c==3)&&(c1==2||c1==3)&&c2==1)) return B0;
  return !(_blocked(x,y,qx,qy)||_blocked(qx,qy,rx,ry));
}

    }

}
