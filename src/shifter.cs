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



int[] _shiftxy(int px,int py,int b,bool opt,out int pi) {
  var xy=new List<int>();int x,y,p,nx,ny,f;int[] n;
  f=H==H.penta&&opt?b^4:H==H.tria4||H==H.penta?(((b+4)^1)&7):H==H.quad||H==H.tria2?i2(b=b&3,b^2):i2(b%=6,(b%6+3)%6);
  for(x=px,y=py;_next(x,y,b,opt,out nx,out ny)&&_xo(nx,ny)&&!_blocked(x,y,nx,ny);x=nx,y=ny)
    Push(xy,ny,nx);
  xy.Reverse();pi=xy.Count;Push(xy,px,py);
  for(x=px,y=py;_next(x,y,f,opt,out nx,out ny)&&_xo(nx,ny)&&!_blocked(x,y,nx,ny);x=nx,y=ny)
    Push(xy,nx,ny);  
  return xy.ToArray();
}

bool _next(int x,int y,int d,bool opt,out int nx,out int ny) {
  if(H==H.deca) {
    int e=(x+2)%3,bx=i2b(y&1)?0:-3,x2,d2=d>>1;bool r=i2b(d&1);
    //console.log(d,d2,r,opt);
    if(opt) {
      if(d2==0) x+=(r?IA(-2,1,-2):IA(2,-1,-4))[e];
      else if(d2==1) {x+=(r?IA(bx+1,1,-2):IA(bx+2,-1,-1))[e];y+=e==0?-1:e==1?0:0;}
      else if(d2==2) {x+=(r?IA(1,bx+4,-2):IA(bx+5,-1,-1))[e];y+=e==(r?1:0)?-1:0;}
      else if(d2==3) x+=(r?IA(1,4,-2):IA(2,2,-1))[e];
      else if(d2==4) {x+=(r?IA(1,1,bx+1):IA(2,bx+2,-1))[e];y+=e==(r?2:1)?1:0;}
      else if(d2==5) {x+=(r?IA(1,1,bx-2):IA(2,-1,bx-1))[e];y+=e==2?1:0;}
    } else {
      if(d2==0) x+=e==0?-2:e==1?r?-1:1:-4;
      else if(d2==1) {x+=e==0?bx+(r?1:2):e==1?-1:-2;y+=e==0?-1:e==1?0:0;}
      else if(d2==2) {x+=e==0?bx+5:e==1?bx+4:r?-1:-2;y+=e==0?-1:e==1?-1:0;}
      else if(d2==3) x+=e==0?1:e==1?r?4:2:-1;
      else if(d2==4) {x+=e==0?r?2:1:e==1?bx+2:bx+1;y+=e==0?0:e==1?1:1;}
      else if(d2==5) {x+=e==0?2:e==1?1:bx+(r?-2:-1);y+=e==0?0:e==1?0:1;}
    }
  } else if(H==H.cubes) {
    int e=(x+2)%3,bx=i2b(y&1)?0:-3;
    if(opt) {
      if(d==0) x+=e==0?1:e==1?-2:-2;
      else if(d==1) {x+=e==0?bx+2:e==1?-1:-1;y+=e==0?-1:0;}
      else if(d==2) {x+=e==0?bx+4:e==1?1:-2;y+=e==0?-1:0;}
      else if(d==3) x+=e==0?2:e==1?-1:2;
      else if(d==4) {x+=e==0?1:e==1?1:bx+1;y+=e==2?1:0;}
      else if(d==5) {x+=e==0?2:e==1?bx-1:-1;y+=e==1?1:0;}
    } else {
      if(d==0) x+=e==0?-3:e==1?-2:-1;
      else if(d==1) {x+=e==0?bx+2:e==1?bx:-2;y+=e==0?-1:e==1?-1:0;}
      else if(d==2) {x+=e==0?bx+4:e==1?-1:bx+3;y+=e==0?-1:e==1?0:-1;}
      else if(d==3) x+=e==0?3:e==1?1:2;
      else if(d==4) {x+=e==0?2:e==1?bx+3:bx+1;y+=e==0?0:e==1?1:1;}
      else if(d==5) {x+=e==0?1:e==1?bx-1:bx;y+=e==0?0:e==1?1:1;}
    }
  } else if(H==H.trap) {
    int e=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*e,dx=i2b(y&1)?0:-2;
    if(opt) {
      if(d==0) {x+=dd==5?dx:dd==1?dx:-1;y+=dd==5||dd==1?1:0;}
      else if(d==1) {x+=dd==2?dx:dd==3?dx:-1;y+=dd==2||dd==3?-1:0;}
      else if(d==2) {x+=dd==0?1:dd==1?dx+1:dd==2?dx+2:dd==3?dx:dd==4?dx+1:-1;y+=dd==0||dd==5?0:-1;}
      else if(d==3) {x+=dd==2?dx+2:dd==3?dx+2:1;y+=dd==2||dd==3?-1:0;}
      else if(d==4) {x+=dd==0?dx+2:dd==4?dx+2:1;y+=dd==0||dd==4?1:0;}
      else if(d==5) {x+=dd==0?dx+1:dd==1?dx:dd==2?1:dd==3?-1:dd==4?dx+2:dx+1;y+=dd==2||dd==3?0:1;}
    } else {
      if(d==0) x+=-1;
      else if(d==1) {x+=dd==0?1:dd==1?dx+1:dd==2?dx:dd==3?dx:-1;y+=dd==1||dd==2||dd==3?-1:0;}
      else if(d==2) {x+=dd==2?dx+2:dd==3?dx+2:dd==4?dx+1:dd==5?-1:1;y+=dd==2||dd==3||dd==4?-1:0;}
      else if(d==3) x+=1;
      else if(d==4) {x+=dd==0?dx+2:dd==3?-1:dd==4?dx+2:dd==5?dx+1:1;y+=dd==4||dd==5||dd==0?1:0;}
      else if(d==5) {x+=dd==0?dx+1:dd==1?dx:dd==2?1:dd==5?dx:-1;y+=dd==5||dd==0||dd==1?1:0;}
    }
  } else if(H==H.delta) {
    int r=(x+2)%3,cx=(x+2-r)/3,e=1^(y&1)^(cx&1),dy=i2b(e)?-1:1,xx=3*e+r;
    if(d==0) x+=2;
    else if(d==1) {x+=xx==0||xx==2?0:xx==1?-1:xx==3?-2:xx==4?-2:-1;y+=xx==0||xx==2?-1:0;}
    else if(d==2) {x+=xx==0||xx==2?0:xx==1?1:xx==3?1:xx==4?2:2;y+=xx==0||xx==2?-1:0;}
    else if(d==3) x+=-2;
    else if(d==4) {x+=xx==3||xx==5?0:xx==0?1:xx==1?2:xx==2?2:1;y+=xx==3||xx==5?1:0;}
    else if(d==5) {x+=xx==3||xx==5?0:xx==0?-2:xx==1?-2:xx==2?-1:-1;y+=xx==3||xx==5?1:0;}
  } else if(H==H.penta) {
    int d2=d>>1,cx=(x+1)>>1,x1=x&1,vx=(((cx+y)&1)<<1)|((x^1)&1);
    if(opt) {
      if(d2==0) x+=vx==2&&d==1?-2:vx==1&&d==1?-2:vx==0&&d==1?1:-1;
      else if(d2==1) {x+=vx==0&&d==3?1:vx==3&&d==3?-1:vx==2?1:vx==1?-1:0;y+=vx==1||vx==2&&d==2||vx==3&&d==3?0:-1;}
      else if(d2==2) x+=vx==3&&d==5?2:vx==0&&d==5?2:vx==1&&d==5?-1:1;
      else if(d2==3) {x+=vx==1&&d==7?-1:vx==2&&d==7?1:vx==3?-1:vx==0?1:0;y+=vx==0||vx==2&&d==7||vx==3&&d==6?0:1;}
    } else {
      if(d2==0) x+=vx==2&&d==1?-2:vx==1?-2:-1;
      else if(d2==1) {x+=vx==0&&d==3?1:vx==2?1:vx==1?-1:0;y+=vx==1?0:-1;}
      else if(d2==2) x+=vx==3&&d==5?2:vx==0?2:1;
      else if(d2==3) {x+=vx==1&&d==7?-1:vx==3?-1:vx==0?1:0;y+=vx==0?0:1;}
    }
  } else if(H==H.tria4) {
    int d2=d>>1,r=(((y+1)&1)<<1)|((x+1)&1);
    if(d2==0) {x+=r==3&&d==1?0:r==2?0:-1;y+=r==3&&d==1?-1:r==0?1:r==2?-1:0;}
    else if(d2==1) {x+=r==2&&d==3?1:r==1?-1:r==0?1:0;y+=r==2&&d==3?0:r==0?0:-1;}
    else if(d2==2) {x+=r==0&&d==5?0:r==1?0:1;y+=r==0&&d==5?1:r==1?1:r==3?-1:0;}
    else if(d2==3) {x+=r==1&&d==7?-1:r==2?1:r==3?-1:0;y+=r==1&&d==7?0:r==3?0:1;}
  } else if(H==H.tria2) {
    int xx=(x+1+2*(y&1))&3;
    if(d==0||d==2) x+=i2b(d)?1:-1;
    else { x++;if((xx==1||xx==2)==(d>1)) y+=d>1?1:-1;else x^=1;x--;};
  } else if(H==H.tria) {
    int dy=i2b((y&1)^(x&1))?-1:1;
    if(dy>0?d==4||d==5:d==1||d==2) y+=dy; 
    else if(d==0||d==1||d==5) x--;
    else x++;
  } else if(H==H.hexa) {
    int dx=i2b(y&1)?0:-1;
    x+=d==0?-1:d==1?dx:d==2?dx+1:d==3?1:d==4?dx+1:d==5?dx:0;
    y+=d==1||d==2?-1:d==4||d==5?1:0;
  } else {
    x+=d==0?-1:d==2?1:0;
    y+=d==1?-1:d==3?1:0;
  }
  nx=x;ny=y;
  return true;
}

int _shiftb(int px,int py,int ex,int ey) {
  int b;
  if(H==H.cubes) {
    var e=(px+2)%3;//,bx=y&1?0:-3;
    b=_idx(ex,ey,_points(px,py),0,0);
    if(e==0) b=b==0?5:b==3?4:b;
    else if(e==1) b=b==0?3:b==1?5:b==2?0:2;
    else if(e==2) b=b==0?1:b==1?3:b==2?4:0;
  } else if(H==H.trap) {
    int e=(px+1)%2,cx=(px+1)>>1,dd=(cx+2*(py&1))%3+3*e;bool opt=Diag;
    b=_idx(ex,ey,_points11h(px,py),b2i(opt),0);
    b+=opt?0:5;
    b=b%6;
  } else if(H==H.delta) {
    int f,r=(px+2)%3,cx=(px+2-r)/3,e=1^(py&1)^(cx&1),dy=i2b(e)?-1:1,xx=3*e+r;
    b=_idx(ex,ey,_points(px,py),2,0);
    b=((f=b&3)<<1)|(i2b(b&64)?0:1);
    if(xx==0) b=f>1?f+1:b+5;
    else if(xx==1) b=f>1?f-1:b+3;
    else if(xx==2) b=f>1?f+3:b+4;
    else if(xx==3) b=f>1?f:b+4;
    else if(xx==4) b=f>1?f+2:b;
    else if(xx==5) b=f>1?f+4:b+5;
    b%=6;
  } else if(H==H.penta) {
    int b2,cx=(px+1)>>1,vx=(((cx+py)&1)<<1)|((px^1)&1),f;
    b=_idx(ex,ey,_points(px,py),2,0);
    b=((f=b&7)<<1)|(i2b(b&64)?0:1);
    if(vx==0) b=b<2?b:b<6?f+1:b+6;
    else if(vx==1) b=b<2?b+4:b<6?f+5:b+2;
    else if(vx==2) b=b<2?b+6:b<6?f+7:b+4;
    else if(vx==3) b=b<2?b+2:b<6?f+3:b+0;
    b&=7;
  } else if(H==H.tria4) {
    int r=(((py+1)&1)<<1)|((px+1)&1),f;
    b=_idx(ex,ey,_points(px,py),2,0);
    b=((f=b&3)<<1)|(i2b(b&64)?0:1);
    if(r==0) b+=b>3?4:b==3?4:b>0?3:2;
    else if(r==1) b+=b>3?6:b==3?6:b>0?5:4;
    else if(r==2) b+=b>3?2:b==3?2:b>0?1:0;
    else if(r==3) b+=b>3?0:b==3?0:b>0?7:6;
    b&=7;
  } else if(H==H.tria2) {
    int f,xx=(px+1+2*(py&1))&3,r;
    b=_idx(ex,ey,_points(px,py),2,0);
    b=((f=b&3)<<1)|(i2b(b&64)?0:1);
    if(xx==0) b=b>=4?b-2:f;
    else if(xx==1) b=b>=4?b-4:f+2;
    else if(xx==2) b=b>=4?b-3:f+3;
    else if(xx==3) b=b>=4?b-1:f+1;
    b&=3;
  } else if(H==H.tria) {
    var e=1^(py&1)^(px&1);
    b=_idx(ex,ey,_points(px,py),2,0);
    b=((b&3)<<1)|(i2b(b&64)?0:1);
    b=(b+3*e+1)%6;
  } else if(H==H.hexa) {
    b=_idx(ex,ey,_points(px,py),0,0);
  } else {
    //b=Math.abs(px-mx)>Math.abs(py-my)?0:1,
    b=_idx(ex,ey,_points(px,py),0,0);
    b&=3;
  }
  return b;
}

void _shift(int[] xy,int d) {
  int n=xy.Length>>1,i,ix,j,x,y,k;int[] m=new int [n],pc=new int[n];
  d=(d%n+n)%n;
  for(i=j=0;i+1<xy.Length;j++,i+=2) { ix=Index(x=xy[i],y=xy[i+1]);m[j]=Data[ix].ch;pc[j]=Data[ix].fore;}
  for(i=j=0;i+1<xy.Length;j++,i+=2) {
    ix=Index(x=xy[i],y=xy[i+1]);
    Data[ix].ch=m[k=(j+d)%n];
    Data[ix].fore=pc[k];    
  }
}

U _shifter(int up,float[] mu,ME ev) {
   int px=(int)View.mp[0],py=(int)View.mp[1],ex=(int)mu[5],ey=(int)mu[6];int b,i=0,j,pi,mi=0,x,y;var opt=Diag;
   float[] p;float pv,mv;
  b=_shiftb(px,py,ex,ey);
  int[] xy=_shiftxy(px,py,b,opt,out pi);
  if(i2b(up)) {
    for(i=0,mv=1<<22;i<xy.Length;i+=2) {
      p=_peg(x=xy[i],y=xy[i+1]);pv=_sqr(ex-p[0],ey-p[1]);
      if(pv<mv) {mi=i;mv=pv;}
    }
     View.sele.Clear();
    _shift(xy,i=(pi-mi)/2);
    _moves(1);
    //_drawboard();    
  } else {
    View.sele.Clear();
    for(j=0;j<xy.Length;j+=2) if(xy.Length==2||xy[j]!=px||xy[j+1]!=py) Push(View.sele,xy[j],xy[j+1]);
    //_drawboard();
  }
  return U.Shifter(px,py,b,opt,i);
}

    }

}
