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
delegate void f_switch(int x,int y);
void _switch(int x,int y) {
  int i=Index(x,y),c=Data[i].ch,c2=c==2?3:c==3?2:c;
  if(c2!=c) Data[i].ch=c2;
}
void _switchP(int x,int y) {
  int i=Index(x,y),c=Data[i].ch,c2=c==2?3:c==3?1:c==1?2:c;
  if(c2!=c) Data[i].ch=c2;
}
void _switchN(int x,int y) {
  int i=Index(x,y),c=Data[i].ch,c2=c==2?1:c==3?2:c==1?3:c;
  if(c2!=c) Data[i].ch=c2;
}

int[] _switchi(int x,int y,int[] xy,int[] u) {
  f_switch swP=u!=null?(f_switch)_switchN:_switchP,swN=u!=null?(f_switch)_switchP:_switchN,_f1=ooc?swN:oo3?swP:_switch,_f2=oo3?swP:_switch;
  int i,x2,y2;var xyc=new int[0];int c;
  if(!ooo) _f1(x,y);
  c=_getpgc(x,y);
  for(i=0;i<xy.Length;i+=2) {
    _f2(x2=xy[i],y2=xy[i+1]);
    if(u==null) {Push(ref xyc,x2,y2,_getpgc(x2,y2));_setpgc(x2,y2,c);}
  }
  if(u!=null) for(i=0;i<u.Length;i+=3) _setpgc(u[i],u[i+1],u[i+2]);
  return u!=null?u:xyc;
}

int[] _border14(int x,int y,int f) {
  return new int[] {-1,0,-1,-1,0,-1,1,-1,1,0,1,1,0,1,-1,1};//:[-1,0,0,-1,1,0,0,1];
}

delegate int[] f_switch4(int x,int y,int mask,int d,int[] u=null);

int[] _switch14(int x,int y,int mask,int d,int[] u=null) {
  int i,x2,y2,m;int[] xy=new int[0],b=_border14(x,y,0);
  for(i=2*d,m=1;m<256;i+=i==14?-14:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _switch2(int x,int y,int mask,int d,int[] u=null) {
  int dy=i2b((1^x^y^((x+1)>>1))&1)?1:-1;
  var xy=new int[0];
  if(!_blocked(x,y,x,y+dy)) Push(ref xy,x,y+dy);
  if(!_blocked(x,y,x-1,y)) Push(ref xy,x-1,y);
  if(!_blocked(x,y,x+1,y)) Push(ref xy,x+1,y);
  return _switchi(x,y,xy,u);
}

int[] _switch4(int x,int y,int mask,int d,int[] u=null) {
  int dy=i2b(1^(x&1)^(y&1))?1:-1;
  var xy=new int[0];
  if(!_blocked(x,y,x,y+dy)) Push(ref xy,x,y+dy);
  if(!_blocked(x,y,x-1,y)) Push(ref xy,x-1,y);
  if(!_blocked(x,y,x+1,y)) Push(ref xy,x+1,y);
  return _switchi(x,y,xy,u);
}

int[] _border13(int x,int y,int f) {
  int d=1^(y&1)^(x&1);
  return i2b(d)?new int[] {0,1,1,1,-1,1,-2,1,-3,1,1,2,0,2,-1,2 ,-1,0,-2,0,-1,-1,0,-1,0,-2,-3,0,-3,-1,-2,-1 ,1,0,1,-1,2,0,2,1,3,1,2,-1,3,-1,3,0 }
   :new int[] {0,-1,-1,-1,1,-1,2,-1,3,-1,-1,-2,0,-2,1,-2 ,1,0,2,0,1,1,0,1,0,2,3,0,3,1,2,1 ,-1,0,-1,1,-2,0,-2,-1,-3,-1,-2,1,-3,-1,-3,0 };
}

int[] _switch3(int x,int y,int mask,int d,int[] u=null) {
  int i,x2,y2,m;int[] xy=new int[0],b=_border13(x,y,0);
  for(i=2*d,m=1;m<0x1000000;i+=i==46?-46:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _switch15(int x,int y,int mask,int d,int[] u=null) {
  int i,m,x2,y2;int[] xy=new int[0],b=_border15(x,y);
  for(i=2*d,m=1;m<32;i+=i==8?-8:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _switch16(int x,int y,int mask,int d,int[] u=null) {
  int i,m,x2,y2;int[] xy=new int[0],b=_border16(x,y);
  for(i=2*d,m=1;m<64;i+=i==10?-10:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _switch8(int x,int y,int mask,int d,int[] u) {
  int i,m;int[] xy=new int[0],b=_border8(x,y);
  for(i=2*d,m=1;m<16;i+=i==6?-6:2,m<<=1) 
   if(i2b(mask&m)) Push(ref xy,x+b[i],y+b[i+1]);
  return _switchi(x,y,xy,u);
}

int[] _switch7(int x,int y,int mask,int d,int[] u=null) {
  int x2,y2,i,m;int[] xy=new int[0],b=_border7(x,y);
  for(i=2*d,m=1;m<64;i+=i==10?-10:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _switch11(int x,int y,int mask,int d,int[] u=null) {
  int x2,y2,i,m;int[] xy=new int[0],b=_border11(x,y);
  for(i=2*d,m=1;m<16;i+=i==6?-6:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _switch18(int x,int y,int mask,int d,int[] u=null) {
  int i,m,x2,y2;int[] xy=new int[0],b=_border18(x,y);
  if(i2b(mask&0x8)&&i2b(mask&0x80)) mask^=0x8;
  if(i2b(mask&0x800)&&i2b(mask&0x8000)) mask^=0x800;
  for(i=2*d,m=1;m<65536;i+=i==30?-30:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _border19(int x,int y) {
  int r=(x+2)%3,cx=(x+2-r)/3;bool d=i2b(1^(y&1)^(cx&1));int dy=d?-1:1;
  if(d) {
    if(r==1) return new int[]{2,0,4,0,2,-1,3,0 ,-2,0,-4,0,-2,-1,-3,0 ,-1,0,-1,1,-3,0,1,0 ,1,0,1,1,3,0,-1,0};
    if(r==2) return new int[]{0,1,-1,1,2,1,-2,1 ,2,0,3,0,4,0,1,0 ,-1,0,-3,0,1,0,-2,0 ,-2,0,-4,0,-2,1,-1,0};
    return new int[]{-2,0,-3,0,-4,0,-1,0 ,0,1,0,3,-1,0,2,0 ,2,0,4,0,2,1,1,0 ,1,0,3,0,-1,0,2,0 };
  } else {
    if(r==1) return new int[]{-2,0,-4,0,-2,1,-3,0 ,2,0,4,0,2,1,3,0 ,1,0,1,-1,3,0,-1,0 ,-1,0,-1,-1,-3,0,1,0};
    if(r==2) return new int[]{2,0,3,0,4,0,1,0 ,0,-1,-1,-1,2,-1,-2,-1 ,-2,0,-4,0,-2,-1,-1,0 ,-1,0,-3,0,1,0,-2,0};
    return new int[]{0,-1,1,-1,-2,-1,2,-1 ,-2,0,-3,0,-4,0,-1,0 ,1,0,3,0,-1,0,2,0  ,2,0,4,0,2,-1,1,0};
  }
}


int[] _switch19(int x,int y,int mask,int d,int[] u=null) {
  int i,m,x2,y2;int[] xy=new int[0],b=_border19(x,y);
  if(i2b(mask&0x8)&&i2b(mask&0x80)) mask^=0x8;
  if(i2b(mask&0x800)&&i2b(mask&0x8000)) mask^=0x800;
  for(i=2*d,m=1;m<65536;i+=i==30?-30:2,m<<=1) 
   if(i2b(mask&m)) if(!_blocked(x,y,x2=x+b[i],y2=y+b[i+1])) Push(ref xy,x2,y2);
  return _switchi(x,y,xy,u);
}

int[] _switch5(int x,int y,int mask,int d,int[] u=null) {
  int i,r=(((1^y)&1)<<1)|(1^x&1);int[] a=null,xy=new int[0];
  if(r==0) a=new int[]{x-1,y+1,x+1,y,x,y+1};
  else if(r==1) a=new int[]{x-1,y,x-1,y-1,x,y+1};
  else if(r==2) a=new int[]{x,y-1,x+1,y+1,x+1,y};
  else if(r==3) a=new int[]{x-1,y,x,y-1,x+1,y-1};
  //if(r==0) _f2(x-1,y+1),_f2(x+1,y),_f2(x,y+1);
  //else if(r==1) _f2(x-1,y),_f2(x-1,y-1),_f2(x,y+1);
  //else if(r==2) _f2(x,y-1),_f2(x+1,y+1),_f2(x+1,y);
  //else if(r==3) _f2(x-1,y),_f2(x,y-1),_f2(x+1,y-1);
  if(a!=null) for(i=0;i<6;i+=2)
    if(!_blocked(x,y,a[i],a[i+1])) Push(ref xy,a[i],a[i+1]);
  return _switchi(x,y,xy,u);
}

int _f25(int x) { return x+1;}
int _t25(int x) { return x-1;}
int _k25(int x,int y) { return (x<<16)|y;}


void _line23(int x,int y,int m,int dx,int dy,Dictionary<int,int> dic,List<int> xy) {
  int px=x,py=y,xy1,v,b,k,n=88;
  m&=3;
  if(!i2b(m)) m=3;
  if(i2b(dx)&&i2b(dy)) {
    for(;i2b(n--);) {
      int x2=x,y2=y;
      if(i2b(1&(1^x^y^(b2i(dy>0))^(1)))) y+=dy;else x+=dx;
      if(Ch(x,y)<1) break;
      if(_blocked(x2,y2,x,y)) break;
      if(!i2b(m&(1<<(1&(px^py^x^y))))) continue;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
    }
  } else {
    for(;i2b(n--);) {
      int x2=x,y2=y;
      x+=dx;y+=dy;
      if(Ch(x,y)<1) break;
      if(_blocked(x2,y2,x,y)) break;
      if(!i2b(m&(1<<(1&(px^x))))) continue;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
    }
  }
}

int[] _switch23(int x,int y,int mask,int d,int[] u=null) {
  int i,j,k,k2,j2,c,b,m;var xy=IL();var dic=DII();

  int e=1^(y&1)^(x&1),dx=i2b(e)?-1:1,dy=i2b(e)?1:-1;
  mask&=0xffffff;mask=(mask<<d)|(mask>>(24-d));

  if(i2b(mask&0x03)) _line23(x,y,mask&3,-dx,dy,dic,xy);
  if(i2b(mask&0x0c)) _line23(x,y,(mask>>2)&3,dx,dy,dic,xy);
  if(i2b(mask&0x0300)) _line23(x,y,(mask>>8)&3,+dx,0,dic,xy);
  if(i2b(mask&0x0c00)) _line23(x,y,(mask>>10)&3,+dx,-dy,dic,xy);
  if(i2b(mask&0x030000)) _line23(x,y,(mask>>16)&3,-dx,-dy,dic,xy);
  if(i2b(mask&0x0c0000)) _line23(x,y,(mask>>18)&3,-dx,0,dic,xy);

  if(i2b(mask&0x10))  for(i=1;Ch(k=x,j=y-dy*i)>0;i++) Push(xy,k,j);
  if(i2b(mask&0x100000))  for(i=1;Ch(k=x+dx*(((i-1)>>1)*3+3-(i&1)),j=y+dy*((i+1)>>1))>0;i++) Push(xy,k,j);
  if(i2b(mask&0x1000))  for(i=1;Ch(k=x-dx*(((i-1)>>1)*3+3-(i&1)),j=y+dy*((i+1)>>1))>0;i++) Push(xy,k,j);
  return _switchi(x,y,xy.ToArray(),u);
}


void _line25(int x,int y,int m,int dx,int dy,Dictionary<int,int> dic,List<int> xy,int xx,int yy) {
  int x4,x2,px=x,py=y,cx=_f25(x),c,xy1,v,k,n=88;var r=IA();
  if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
  int x3=x,y3=y;
  if(i2b(dx)&&i2b(dy)) {
    for(;i2b(n--);x3=x,y3=y) {
      v=(((x+1)>>1)+y)&1;
      x=_f25(x);
      if(i2b(v)) {
        if(i2b(x&1)==(dx>0)) x=((x+2*dx)&~1)|b2i(dy>0);
        else {y+=dy;x=(x&~1)|b2i(dy<0);}
      } else {
        if(i2b(x&1)==(dy<0)) x=((x+2*dx)&~1)|b2i(dx<0);
        else {y+=dy;x=(x&~1)|b2i(dx>0);}
      }
      x=_t25(x);
      if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
    }
  } else if(i2b(dx)||i2b(dy)) {
    if(i2b(dx)) {      
      for(;i2b(n--);x3=x,y3=y) {
        v=(((x+1)>>1)+y)&1;
        if(i2b(v)&&(i2b(x&1)==(dx<0))) {
          x4=x+3*dx;c=0;x2=_t25(x);
          if(i2b(m&1)&&Ch(x2=x+(dx<0?2:1)*dx,y)>0&&!_blocked(x2,y,x3,y3)) {c=b2i(!_blocked(x2,y,x4,y));if(!i2b(dic[k=_k25(x2,y)])) {dic[k]=1;Push(xy,x2,y);}}
          if(i2b(m&2)&&Ch(x2=x+(dx<0?1:2)*dx,y)>0&&!_blocked(x2,y,x3,y3)) {c|=b2i(!_blocked(x2,y,x4,y));if(!i2b(dic[k=_k25(x2,y)])) {dic[k]=1;Push(xy,x2,y);}}
          if(!i2b(c)||Ch(x=x4,y)<1) break;
          if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
        } else {
          x+=dx;
          if(!i2b(v)) {
            if((i2b(x&1)!=(dx>0))) x+=dx;
          }
          if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
          if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
        }
      }
    } else {
      for(;i2b(n--);x3=x,y3=y) {
        v=(((x+1)>>1)+y)&1;
        if(!i2b(v)&&(i2b(x&1)==(dy<0))) {
          y+=dy;
          x2=((x-1)&~1)+1;x4=x2+b2i(dy<0);c=0;
          if(i2b(m&1)&&Ch(x2,y)>0&&!_blocked(x2,y,x3,y3)) {c=b2i(!_blocked(x2,y,x4,y+dy));if(!i2b(dic[k=_k25(x2,y)])) {dic[k]=1;Push(xy,x2,y);}}
          x2++;if(i2b(m&2)&&Ch(x2,y)>0&&!_blocked(x2,y,x3,y3)) {c|=b2i(!_blocked(x2,y,x4,y+dy));if(!i2b(dic[k=_k25(x2,y)])) {dic[k]=1;Push(xy,x2,y);}}
          x=x4;y+=dy;
          if(!i2b(c)||Ch(x,y)<1) break;
          if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
        } else {
          if(!i2b(v)) x+=dy;
          else {
            if(i2b(x&1)==(dy<0)) x-=dy;
            y+=dy;
          }
          if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
          if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy,x,y);}
        }
      }
    }
  }
}

int[] _switch25(int x,int y,int mask,int d,int[] u=null) {
  int r,cx=(x+1)/2|0,x1=x&1,v=(cx+y)&1;float sx=brd+cx*cell7,sy=brd+y*cell7,c2=cell7/2,c4=c2/2;var dic=DII();
  int i,m;var xy=IL();var b=_border15(x,y);

  mask&=0xfffff;mask=(mask<<d)|(mask>>(20-d));
  if(i2b(mask&0xf)) {
    int x2=x+b[0],y2=y+b[1];x1=x2&1;v=(((x2+1)>>1)+y2)&1;
    if(!_blocked(x,y,x2,y2)) {
      if(i2b(mask&1)) {_line25(x2,y2,i2b(v)?i2b(x1)?2:1:i2b(x1)?1:2,i2b(v)?i2b(x1)?1:-1:0,i2b(v)?0:i2b(x1)?1:-1,dic,xy,x,y);}
      if(i2b(mask&2)) {r=i2b(v)?i2b(x1)?1:2:i2b(x1)?3:0;_line25(x2,y2,0,i2b(r&1)?1:-1,i2b(r&2)?1:-1,dic,xy,x,y);}
    }
  }
  if(i2b(mask&0xf0)) {
    int x2=x+b[2],y2=y+b[3];x1=x2&1;v=(((x2+1)>>1)+y2)&1;
    if(!_blocked(x,y,x2,y2)) {
      if(i2b(mask&0x10)) _line25(x2,y2,i2b(v)&&!i2b(x1)||!i2b(v)&&!i2b(x1)?2:1,i2b(v)?0:i2b(x1)?1:-1,i2b(v)?i2b(x1)?-1:1:0,dic,xy,x,y);
      if(i2b(mask&0x20)) {r=i2b(v)?i2b(x1)?0:3:i2b(x1)?1:2;_line25(x2,y2,0,i2b(r&1)?1:-1,i2b(r&2)?1:-1,dic,xy,x,y);}
    }
  }
  if(i2b(mask&0xf00)) {
    int x2=x+b[4],y2=y+b[5];x1=x2&1;v=(((x2+1)>>1)+y2)&1;
    if(!_blocked(x,y,x2,y2)) {
      if(i2b(mask&0x100)) _line25(x2,y2,!i2b(v)&&!i2b(x1)||i2b(v)&&!i2b(x1)?2:1,i2b(v)?0:i2b(x1)?-1:1,i2b(v)?i2b(x1)?1:-1:0,dic,xy,x,y);
      if(i2b(mask&0x200)) {r=i2b(v)?i2b(x1)?2:1:i2b(x1)?0:3;_line25(x2,y2,0,i2b(r&1)?1:-1,i2b(r&2)?1:-1,dic,xy,x,y);}
    }
  }
  if(i2b(mask&0xf000)) {
    int x2=x+b[6],y2=y+b[7];x1=x2&1;v=(((x2+1)>>1)+y2)&1;
    if(!_blocked(x,y,x2,y2)) {
      if(i2b(mask&0x1000)) _line25(x2,y2,i2b(v)?i2b(x1)?1:2:i2b(x1)?2:1,i2b(v)?i2b(x1)?1:-1:0,i2b(v)?0:i2b(x1)?1:-1,dic,xy,x,y);
      if(i2b(mask&0x2000)) {r=i2b(v)?i2b(x1)?3:0:i2b(x1)?2:1;_line25(x2,y2,0,i2b(r&1)?1:-1,i2b(r&2)?1:-1,dic,xy,x,y);}
    }
  }
  if(i2b(mask&0xf0000)) {
    int x2=x+b[8],y2=y+b[9];x1=x2&1;v=(((x2+1)>>1)+y2)&1;
    if(!_blocked(x,y,x2,y2)) {
      if(i2b(mask&0x10000)) _line25(x2,y2,3,i2b(v)?i2b(x1)?-1:1:0,i2b(v)?0:i2b(x1)?-1:1,dic,xy,x,y);
      if(i2b(mask&0x20000)) {
	 r=i2b(v)?i2b(x1)?2:1:i2b(x1)?0:3;_line25(x2,y2,0,i2b(r&1)?1:-1,i2b(r&2)?1:-1,dic,xy,x,y);
	 r=i2b(v)?i2b(x1)?0:3:i2b(x1)?1:2;_line25(x2,y2,0,i2b(r&1)?1:-1,i2b(r&2)?1:-1,dic,xy,x,y);
      }
    }
  }
  return _switchi(x,y,xy.ToArray(),u);
}

void _line21(int x,int y,int i,Dictionary<int,int> dic,List<int> xy2) {
  int x3,y3,px=x,py=y,r,v,b,k,n=88;
  int d=(x+1)%2,cx=(x+1)>>1,dd=(cx+2*(y&1))%3+3*d,pdd=dd,dx;int[] xy=null;
  if(i2b(i&1)) {
    pdd=(dd+(i==3?3:0))%6;
    for(;i2b(n--);) {
      x3=x;y3=y;
      dx=i2b(y&1)?0:-2;
      if(pdd==0) {xy=IA(1+dx,1,4 ,dx,1,2 ,1,0,5 ,-1,0,0 ,-1,0,1 ,dx,1,3);dd*=3;x+=xy[dd];y+=xy[dd+1];dd=xy[dd+2];}
      else if(pdd==1) x--; 
      else if(pdd==2) {xy=IA(1,0,3 ,dx+1,-1,5 ,dx,-1,0 ,dx,-1,4 ,-1,0,1 ,-1,0,2);dd*=3;x+=xy[dd];y+=xy[dd+1];dd=xy[dd+2];}
      else if(pdd==3) {xy=IA(1,0,3 ,1,0,4 ,dx+2,-1,1 ,dx+2,-1,5 ,dx+1,-1,0 ,-1,0,2);dd*=3;x+=xy[dd];y+=xy[dd+1];dd=xy[dd+2];}
      else if(pdd==4) x++;
      else if(pdd==5) {xy=IA(dx+2,1,2 ,1,0,4 ,1,0,5 ,-1,0,0 ,dx+2,1,3 ,dx+1,1,1);dd*=3;x+=xy[dd];y+=xy[dd+1];dd=xy[dd+2];}
      if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
    }
  } else {
    pdd=(dd+(i==2?3:0))%6;
    for(;i2b(n--);) {
      x3=x;y3=y;
      dx=i2b(y&1)?0:-2;
      if(pdd==0) {xy=IA(dx+2,1,2 ,1,0,4 ,1,0,5 ,1,0,1 ,dx+2,1,3 ,1,0,0);}
      else if(pdd==1) {xy=IA(dx+1,1,4 ,dx,1,2 ,1,0,5 ,-1,0,0 ,dx+2,1,3 ,dx+1,1,1);}
      else if(pdd==2) {xy=IA(-1,0,5 ,dx,1,2 ,-1,0,4 ,-1,0,0 ,-1,0,1 ,dx,1,3);}
      else if(pdd==3) {xy=IA(-1,0,5 ,-1,0,3 ,dx,-1,0 ,dx,-1,4 ,-1,0,1 ,-1,0,2);}
      else if(pdd==4) {xy=IA(1,0,3 ,dx+1,-1,5 ,dx+2,-1,1 ,dx,-1,4 ,dx+1,-1,0 ,-1,0,2);}
      else if(pdd==5) {xy=IA(1,0,3 ,1,0,4 ,dx+2,-1,1 ,dx+2,-1,5 ,1,0,2 ,1,0,0);};
      dd*=3;x+=xy[dd];y+=xy[dd+1];dd=xy[dd+2];
      if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
    }
  }
}

int[] _switch21(int x,int y,int mask,int d,int[] u=null) {
  int r=(((y+1)&1)<<1)|((x+1)&1);var dic=DII();
  int i,m;var xy=IL();

  mask&=0xf;mask=rol(4,mask,d);
  if(i2b(mask&1)) _line21(x,y,0,dic,xy);
  if(i2b(mask&2)) _line21(x,y,1,dic,xy);
  if(i2b(mask&4)) _line21(x,y,2,dic,xy);
  if(i2b(mask&8)) _line21(x,y,3,dic,xy);
  return _switchi(x,y,xy.ToArray(),u);
}

int fc2(int a,int b) { return b;}
void _line29(int x,int y,int m,int dx,int dy,Dictionary<int,int> dic,List<int> xy2) {
  int x3,y3,k,n=88;
  for(;i2b(n--);) {
    x3=x;y3=y;
    if(i2b(dy)) {
      int r=(x+2)%3,cx=(x+2-r)/3,dd=1^(y&1)^(cx&1);
      if(dy<0) {
	if(dx>0) {
	  if(i2b(dd)) r=r==1?3:r==2?4:1;
	  else r=r==1?2:r==2?fc2(y+=dy,2):fc2(y+=dy,0);
	} else {
	  if(i2b(dd)) r=r==1?-1:r==2?1:-2;
	  else r=r==1?0:r==2?fc2(y+=dy,2):fc2(y+=dy,0);
	}
      } else {
	if(dx>0) {
	  if(i2b(dd)) r=r==1?2:r==2?fc2(y+=dy,2):fc2(y+=dy,0);
	  else r=r==1?3:r==2?4:1;
	} else {
	  if(i2b(dd)) r=r==1?0:r==2?fc2(y+=dy,2):fc2(y+=dy,0);
	  else r=r==1?-1:r==2?1:-2;
	}
      }
      x=cx*3-2+r;
    } else {
      x+=2*dx;
    }
    if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
    if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
  }
}

int[] _switch29(int x,int y,int mask,int d,int[] u=null) {
  int r=(x+2)%3,cx=(x+2-r)/3,dd=1^(y&1)^(cx&1);var dic=DII();
  int i,m;var xy=IL();

  //console.log(x,y,d,x1,x2);
  mask&=0xffff;
  mask=0x202;
  int mx=0,a=i2b(dd)?2*r:(7-2*r)%6,xl=b2i(d==0||d==1),xr=b2i(d==1||d==2);
  if(i2b(xl)) mask=mask>>8;
  
  if(i2b(mask&1)) mx|=1<<((a+(i2b(xr)?5:0))%6);
  if(i2b(mask&2)) mx|=1<<((a+(i2b(xr)?4:1))%6);
  if(i2b(mask&4)) mx|=1<<((a+(i2b(xr)?3:2))%6);
  if(i2b(mask&8)) mx|=1<<((a+(i2b(xr)?2:3))%6);
  if(i2b(mask&16)) mx|=1<<((a+(i2b(xr)?1:4))%6);
  if(i2b(mask&32)) mx|=1<<((a+(i2b(xr)?0:5))%6);
  if(i2b(mx)) {
    if(i2b(mx&8)) _line29(x,y,0,1,0,dic,xy);
    if(i2b(mx&1)) _line29(x,y,0,-1,0,dic,xy);
    if(i2b(mx&16)) _line29(x,y,0,1,1,dic,xy);
    if(i2b(mx&32)) _line29(x,y,0,-1,1,dic,xy);
    if(i2b(mx&4)) _line29(x,y,0,1,-1,dic,xy);
    if(i2b(mx&2)) _line29(x,y,0,-1,-1,dic,xy);
  }
  return _switchi(x,y,xy.ToArray(),u);
}


void _line28(int x,int y,int m,int dx,int dy,Dictionary<int,int> dic,List<int> xy2) {
  int n=88,x3,y3,d,dd,cx,bx,k;int[] aa=null;
  for(;i2b(n--);) {
    x3=x;y3=y;
    d=(x+2)%3;bx=i2b(y&1)?0:-3;
    if(i2b(dy)) {
      if(dy<0) {
        if(dx<0) aa=IA(bx+2,-1 ,bx,-1 ,-2,0);
        else aa=IA(bx+4,-1 ,-1,0 ,bx+3,-1);
      } else {
        if(dx<0) aa=IA(1,0 ,bx-1,1 ,bx,1);
        else aa=IA(2,0 ,bx+3,1 ,bx+1,1);
      }
    } else {
      if(dx<0) aa=IA(-3,0 ,-2,0 ,-1,0);
      else aa=IA(3,0 ,1,0 ,2,0);
    }
    x+=aa[d*=2];y+=aa[d+1];
    if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
    if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
  }
}

int[] _switch28(int x,int y,int mask,int d,int[] u=null) {
  int r=(x+2)%3;var dic=DII();
  int i,m2,m;var xy=IL();

  mask&=0xffff;mask=(mask<<d)|(mask>>(4-d));
  if(i2b(mask&(r==1?1<<2:r==2?1<<3:0))) _line28(x,y,0,-1,0,dic,xy);
  if(i2b(mask&(r==1?1<<0:r==2?1<<1:0))) _line28(x,y,0,+1,0,dic,xy);
  if(i2b(mask&(r==1?0:r==2?1<<0:1<<1))) _line28(x,y,0,-1,-1,dic,xy);
  if(i2b(mask&(r==1?1<<3:r==2?0:1<<2))) _line28(x,y,0,+1,-1,dic,xy);
  if(i2b(mask&(r==1?1<<1:r==2?0:1<<0))) _line28(x,y,0,-1,+1,dic,xy);
  if(i2b(mask&(r==1?0:r==2?1<<2:1<<3))) _line28(x,y,0,+1,+1,dic,xy);
  return _switchi(x,y,xy.ToArray(),u);
}

void _line22(int x,int y,int m,int dx,int dy,Dictionary<int,int> dic,List<int> xy2) {
  int x3,y3,px=x,py=y,b,k,n=88;
  if(i2b(dx)&&i2b(dy)) {
    int r=(((y+1)&1)<<1)|((x+1)&1),u=(x+1)&~1,v=(y+1)&~1;bool p=dx==dy;
    if(p) r=3&(r>>1)|(r<<1);
    for(;i2b(n--);) {
      x3=x;y3=y;
      if(p) {
        r=(r+dx)&3;
        if(i2b(r&1)==(dx<0)) {if(i2b((i2b(r&2)?0:1)^b2i(dx<0))) u+=2*dx;else v+=2*dy;}
        x=u+(i2b(r&2)?1:0)-1;y=v+(r&1)-1;
      } else {
        r=(r+dx)&3;
        if(i2b(r&1)==(dx<0)) {if(i2b((i2b(r&2)?0:1)^b2i(dx<0))) u+=2*dx;else v+=2*dy;}
        x=u+(r&1)-1;y=v+(i2b(r&2)?1:0)-1;
      }
      if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
    }
  } else {
    bool p=B0,l;int r2,r=(((y+1)&1)<<1)|((x+1)&1),u=(x+1)&~1,v=(y+1)&~1,o=0,t=dx+dy>0?3:0;
    if(i2b(dy)) r^=1;
    for(;i2b(n--);) {
      if(i2b(dx)) {
        r=(r+dx)&3;
        if(r==3-t) u+=2*dx;
        if(m==1&&r!=(dx<0?1:2)) continue;
        if(m==2&&r!=(dx<0?2:1)) continue;
      } else {
        r=(r+dy)&3;
        if(r==3-t) v+=2*dy;
        if(m==1&&r!=(dy<0?2:1)) continue;
        if(m==2&&r!=(dy<0?1:2)) continue;
      }
      if(r==3-t) o=0;
      else if(!i2b(m&4)&&(r==t-3)) { if(!i2b(o)) break;}
      //else o++;
      x3=x;y3=y;l=B0;
      x=u+((i2b(dy)?1:0)^(r&1))-1;y=v+(i2b(r&2)?1:0)-1;
      if(r==t) {l=!p;p=B0;}
      else {
        if(r==(i2b(dx)?dx>0?2:1:dy>0?3:0)) {
          r2=(r-2*(dx+dy))&3;
          x3=u+((i2b(dy)?1:0)^(r2&1))-1;y3=v+(i2b(r2&2)?1:0)-1;
        }
        l=_blocked(x,y,x3,y3);
      }      
      if(Ch(x,y)<1||l) {
        if(m==1||m==2||r==0||r==3) break;
        o++;if(o==2) break;
        continue;
      }
      if((r==1||r==2)&&!p) {
        r2=t;
        x3=u+((i2b(dy)?1:0)^(r2&1))-1;y3=v+(i2b(r2&2)?1:0)-1;
        if(!_blocked(x,y,x3,y3)) p=B1;
      }
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
      o=0;
    }
  }
}

int[] _switch22(int x,int y,int mask,int d,int[] u=null) {
  int r=(((y+1)&1)<<1)|((x+1)&1);var dic=DII();
  int i,m;var xy=IL();

  //console.log(x,y,d,x1,x2);
  mask&=0x7;mask=(mask<<d)|(mask>>(3-d));
  if(i2b(mask&1)) _line22(x,y,0,r==0||r==1?1:-1,r==0||r==2?-1:1,dic,xy);
  if(i2b(mask&2)) _line22(x,y,0,r==0||r==2?1:-1,r==2||r==3?-1:1,dic,xy);
  if(i2b(mask&4)) _line22(x,y,0,r==0?-1:r==3?1:0,r==1?-1:r==2?1:0,dic,xy);
  return _switchi(x,y,xy.ToArray(),u);
}

void _line27(int x,int y,int m,int dx,int dy,Dictionary<int,int> dic,List<int> xy2) {
  int px=x,py=y,x3,y3,xy1,v,b,k,n=88;bool bx;
  if(i2b(dx)&&i2b(dy)) {
    for(;i2b(n--);) {
      x3=x;y3=y;
      x++;
      x^=1;
      x--;
      if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
      x3=x;y3=y;
      int o=0,x2,y2,xx,xy,i,p=0;
      for(i=1;i<=2;i++) {
	if(i2b(m&i)) { o++;
	  x++;bx=dx==dy;
	  if((i==1)) {x2=x-1+(bx?0:dx);y2=y+(bx?dy:0);}
	  else {x2=x-1+(bx?dx:0);y2=y+(bx?0:dy);}
	  x--;
	  if(Ch(x2,y2)>0&&!_blocked(x2,y2,x3,y3)) {
      if(!i2b(dic[k=_k25(x2,y2)])) {dic[k]=1;Push(xy2,x2,y2);}
         if(!_blocked(x2,y2,x+dx,y+dy)) p++;
	     }
        }
      }
      if(!i2b(p)&&!i2b(m&4)) break;
      x3=x;y3=y;
      x+=dx;y+=dy;
      if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
    }
  } else {
    int x2=(x+1)&3,x4;x3=(x2^(x2>>1))&1;
    for(;i2b(n--);) {
      x3=x;y3=y;
      if(i2b(dx)) {
        x+=dx;x4=((x+1)^((x+1)>>1))&1;
        if(m==1&&x3!=x4) continue;
        if(m==2&&x3==x4) continue;
      } else {
        x++;
        if(i2b(b2i(dy>0)^(y&1)^(x&1)^b2i(i2b(x&2)))) x^=1;
        else y+=dy;
        x4=x--;
        if(m==1&&(x4&1)!=(x2&1)) continue;
        if(m==2&&(x4&1)==(x2&1)) continue;
      }
      if(Ch(x,y)<1||_blocked(x,y,x3,y3)) break;
      if(!i2b(dic[k=_k25(x,y)])) {dic[k]=1;Push(xy2,x,y);}
    }
  }
}

int[] _switch27(int x,int y,int mask,int d,int[] u=null) {
  int dd=1^(y&1)^(x&1),xx=x+1+2*(y&1);bool x1=i2b(xx&1),x2=i2b(xx&2);var dic=DII();
  int i,m;var xy=IL();

  //console.log(x,y,d,x1,x2);
  mask&=0xfffff;mask=(mask<<d)|(mask>>(3-d));
  if(i2b(mask&1)) _line27(x,y,0,x2?0:x1?1:-1,x2?x1?-1:1:0,dic,xy);
  if(i2b(mask&2)) _line27(x,y,0,x2?x1?1:-1:0,x2?0:x1?1:-1,dic,xy);
  if(i2b(mask&4)) _line27(x,y,3,x1?-1:1,x1^x2?-1:1,dic,xy);
  return _switchi(x,y,xy.ToArray(),u);
}

int[] _switch26(int x,int y,int mask,int d,int[] u=null) {
  int x2,i,j,k,j2,k2,c,b,m;var xy=IL();

  mask&=63;mask=(mask<<d)|(mask>>(6-d));
  if(i2b(mask&1)) for(i=1,k2=k=x,j2=j=y;Ch(k-=1,j)>0&&!_blocked(k2,j2,k,j);i++,k2=k,j2=j) Push(xy,k,j);
  if(i2b(mask&2))  for(i=1,k2=k=x,j2=j=y;Ch(k+=i2b(j&1)?0:-1,j-=1)>0&&!_blocked(k2,j2,k,j);i++,k2=k,j2=j) Push(xy,k,j);
  if(i2b(mask&4))  for(i=1,k2=k=x,j2=j=y;Ch(k+=i2b(j&1)?1:0,j-=1)>0&&!_blocked(k2,j2,k,j);i++,k2=k,j2=j) Push(xy,k,j);
  if(i2b(mask&8)) for(i=1,k2=k=x,j2=j=y;Ch(k+=1,j)>0&&!_blocked(k2,j2,k,j);i++,k2=k,j2=j) Push(xy,k,j);
  if(i2b(mask&16))  for(i=1,k2=k=x,j2=j=y;Ch(k+=i2b(j&1)?1:0,j-=1)>0&&!_blocked(k2,j2,k,j);i++,k2=k,j2=j) Push(xy,k,j);
  if(i2b(mask&32))  for(i=1,k2=k=x,j2=j=y;Ch(k+=i2b(j&1)?0:-1,j-=1)>0&&!_blocked(k2,j2,k,j);i++,k2=k,j2=j) Push(xy,k,j);
  return _switchi(x,y,xy.ToArray(),u);
}

int[] _switch24(int x,int y,int mask,int d,int[] u=null) {
  int i,j,k,c,b,m;var xy=IL();

  mask&=255;mask=(mask<<d)|(mask>>(8-d));
  if(i2b(mask&1)) for(i=1;Ch(j=x-i,y)>0&&!_blocked(j+1,y,j,y);i++) Push(xy,j,y);
  if(i2b(mask&2))  for(i=1;Ch(k=x-i,j=y-i)>0;i++) Push(xy,k,j);
  if(i2b(mask&4))  for(i=1;Ch(k=x,j=y-i)>0&&!_blocked(x,j+1,x,j);i++) Push(xy,x,j);
  if(i2b(mask&8)) for(i=1;Ch(k=x+i,j=y-i)>0;i++) Push(xy,k,j);
  if(i2b(mask&16))  for(i=1;Ch(j=x+i,y)>0&&!_blocked(j-1,y,j,y);i++) Push(xy,j,y);
  if(i2b(mask&32))  for(i=1;Ch(j=y+i,k=x+i)>0;i++) Push(xy,k,j);
  if(i2b(mask&64))  for(i=1;Ch(x,j=y+i)>0&&!_blocked(x,j-1,x,j);i++) Push(xy,x,j);
  if(i2b(mask&128))  for(i=1;Ch(k=x-i,j=y+i)>0;i++) Push(xy,k,j);
  return _switchi(x,y,xy.ToArray(),u);
}


f_switch4 _switchx(int x) {
  return x==2?_switch2:x==4?_switch4:x==5?_switch5
   :x==11?_switch11:x==15?_switch15:x==16?_switch16:x==18?_switch18:x==19?_switch19
   :x==21?_switch21:x==22?_switch22:x==23?_switch23:x==24?_switch24:x==25?_switch25:x==26?_switch26:x==27?_switch27:x==28?_switch28:x==29?_switch29
   :(f_switch4)_switch14;
}

int _mask(int x6,int x,int y,int p3) {
  if(H==H.hexa||H==H.deca) return x6==9?62:x6==7?43:x6==6?39:x6==5?34:x6==4?9:x6==3?54:x6==2?35:x6==1?21:63;
  if(H==H.trap) {
    if(oox) return 1;
    return x6==2?15:x6==1?2:1;
  }
  if(H==H.delta) { 
    int m;
    if(oox) return x6==2?3:x6==1?2:1;
    m=x6==3?0x1:x6==2?3:x6==1?0x1111:0xf;
    return m;
  }
  if(H==H.cubes) {
    int d=(x+2)%3,m;bool b=(p3==4||p3==12);
    if(oox) return 1;
    m=x6==6?23:x6==4?8:x6==3?0x3333:x6==2?0x55:x6==1?0x1111:65535;
    if(!b) m=(m&0xf)|((m&0xff00)>>4)|((m&0xf0)<<8);
    return m;
  }
  if(H==H.penta) {
    if(oox) return x6==2?0x2:x6==1?0x1:0x11111;
    return x6==6?23:x6==4?18:x6==3?30:x6==2?19:x6==1?13:31;
  }
  if(H==H.tria2||H==H.tria4) return 1;
  if(H==H.tria) {
    if(oox) return x6==4?0xc0300:x6==3?8:x6==2?4:x6==1?0x30303:0xf0f0f;
    return x6==9?0xd0308:x6==7?0x8000f:x6==6?0x10305:x6==4?0x10100:x6==3?0x70707:x6==2?0xd030f:x6==1?0x10101:0xf0f0f;
  }
  return x6==9?192|6:x6==7?131+16:x6==6?119:x6==4?17:x6==3?187:x6==2?69:x6==1?85:255;
}

    }

}
