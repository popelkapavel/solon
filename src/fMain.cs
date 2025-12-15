using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using System.Text.RegularExpressions;

namespace solon {
    public partial class fMain : Form {
        const int ZoomBase=120,MinZoom=8,MaxZoom=1960;
        int NewWidth,NewHeight;
        Brush BackBrush=Brushes.Wheat;// Brushes.LightGray;
        bool Dirty=false;
        OpenFileDialog ofd=new OpenFileDialog();
        SaveFileDialog sfd,efd;
        PrintDialog pd;PageSetupDialog paged;
        int mapWidth,mapHeight;
        List<UndoItem> undos=new List<UndoItem>();
        int undoc;string undop;
        const int UndoMax=5;
        int FColor,BColor,NUI;
        Bitmap bm;
        pmap map;
        List<Set> set=new List<Set>();
        bool White=true;
        ColorDialog CDialog=new ColorDialog();
        bool timeDraw=false;
        int NoDraw=0;
        int lmx,lmy; // last mouse position
        int pmk,pix,piy; // press mouse position and keys
        MouseButtons pmb;
        int sx=0,sy=0,zoom=ZoomBase,angle=0;
        bool mirx;
        double cos=1,sin=0;
				fHelp Help;
        
        int IX(int x,int y) { int r=(int)(((x-sx)*cos+(y-sy)*sin)*ZoomBase/zoom);return mirx?-r:r;}
        int IY(int x,int y) { int r=(int)((-(x-sx)*sin+(y-sy)*cos)*ZoomBase/zoom);return r;}
        void S2I(int x,int y,out int ix,out int iy) {
          ix=IX(x,y);iy=IY(x,y);
        }
        int SX(int x,int y) { if(mirx) x=-x;double rx=x*zoom/ZoomBase,ry=y*zoom/ZoomBase;return sx+(int)(rx*cos-ry*sin);}
        int SY(int x,int y) { if(mirx) x=-x;double rx=x*zoom/ZoomBase,ry=y*zoom/ZoomBase;return sy+(int)(+rx*sin+ry*cos);}
        
        void NewSize(string par) {
          string[] sa=par.Split('x','X',',');
          int.TryParse(sa[0],out NewWidth);
          if(sa.Length>1) int.TryParse(sa[1],out NewHeight);else NewHeight=NewWidth;
        }

        public fMain(string[] arg) {
            //for(int y=0;y<map.Data.Length;y++) map.Data[y]=(float)(y/512/512.0);
            //map.FuncRadial(Bez,Combine.Max,.5,.5,.3,1);           
            //map.FloodFill(0.9,0.9,0.33);
            InitializeComponent();            
            int a=0;
            while(a<arg.Length&&arg[a].Length>0&&arg[a][0]=='-') {
              string opt=arg[a++],opt2="";
              if(opt.Length<2) break;
              if(opt.Length>2) opt2=opt.Substring(2);
              switch(opt[1]) {
               case 'n':if(opt2=="") opt2=arg[a++];NewSize(opt2);break;               
              }
            }
            string fname=a<arg.Length?arg[a]:null;
            map=new pmap(this);
            map.View.onoff6=1;
            NUI++;
            if(!LoadFile(fname="x.sol",false)) {
              map.Alloc(32,32);
            }
            cbOnOff6.SelectedIndex=map.View.onoff6;
            UpdateControls();
            NUI--;
            if(fname==null) {
              NewFile();
              UpdateBitmap(0,0);
              Center();
            } else
              LoadFile1(fname,true,map.View);
        }
        char _wx(char e,char x) {
          if(x=='e') x=e;else if(x=='f') x=e=='w'?'b':'w';
          return x;
        }
        void _ws(ComboBox cb,char e,char x) {
          x=_wx(e,x);
          cb.SelectedIndex=x=='b'?1:x=='w'?2:0;
        }
        void _w4() {          
           var v=map.View;
          _ws(chBB,'b',v.whiter[0]);
          _ws(chBW,'w',v.whiter[1]);
          _ws(chWB,'b',v.whiter[2]);
          _ws(chWW,'w',v.whiter[3]);
        }
        public void Moves() {
          var v=map.View;
          lMoves.Text=""+v.moves;
        }
        void UpdateH() {
          UIIgnore++;
          var h=map.H;
          quad.Checked=h==H.quad;
          hexa.Checked=h==H.hexa;
          tria.Checked=h==H.tria;
          tria2.Checked=h==H.tria2;
          tria4.Checked=h==H.tria4;
          penta.Checked=h==H.penta;
          cubes.Checked=h==H.cubes;
          delta.Checked=h==H.delta;
          trap.Checked=h==H.trap;
          deca.Checked=h==H.deca;
          UIIgnore--;
        }
        void UpdateG() {
          UIIgnore++;
          miGameSol.Checked=sol.Checked=map.Game==Game.Sol;
          miGameShift.Checked=shifter.Checked=map.Game==Game.Shift;
          miGameOnOff.Checked=onoff.Checked=map.Game==Game.OnOff;
          UIIgnore--;
        }
        void UpdateCh() {
          UIIgnore++;
          chDiag.Checked=map.Diag;
          chOnOffx.Checked=map.oox;
          chOnOffO.Checked=map.ooo;
          chOnOff3.Checked=map.oo3;
          chOnOffC.Checked=map.ooc;
          UIIgnore--;
        }
        public void UpdateControls() {
         try {
          UIIgnore++;
           var v=map.View;
          UpdateG();
          UpdateCh();
          //cbOnOff6.SelectedIndex=map.View.onoff6;
          _w4();
          UpdateWhite2(v.whiter);
          UpdateH();

          chPeg.Checked=v.peg;
          chRound.Checked=v.rou;
          chCorn.Checked=v.corn!=0;
          chGrd1.Checked=v.grdm!=0;
          chGrd2.Checked=v.grdm2!=0;
          miViewPeg.Checked=chPeg.Checked;
          miViewRound.Checked=chRound.Checked;
          miViewCorn.Checked=chCorn.Checked;
          miViewGrd1.Checked=chGrd1.Checked;
          miViewGrd2.Checked=chGrd2.Checked;


          var d=v.design;
          dplay.Checked=d==D.play;
          miEditFree.Checked=dfree.Checked=d==D.free;
          miEditLine.Checked=dline.Checked=d==D.line;
          miEditRect.Checked=drect.Checked=d==D.rect;
          miEditCirc.Checked=dcirc.Checked=d==D.circ;
          miEditCirc2.Checked=dcirc2.Checked=d==D.circ2;
          miEditFill.Checked=dfill.Checked=d==D.fill;
          miEditEdge.Checked=dedge.Checked=d==D.edge;
          dcolo.Checked=d==D.color;
          dcolo2.Checked=d==D.color2;
          chWhite.Checked=v.white;
          miGamePlay.Checked=d==D.play;

          col1.Checked=v.mono<2;
          col2.Checked=v.mono==2;
          col21.Checked=v.mono==21;
          col30.Checked=v.mono==30;
          col31.Checked=v.mono==31;
          col4.Checked=v.mono==4;
          miViewColor1.Checked=col1.Checked;
          miViewColor2.Checked=col2.Checked;
         } finally {
          UIIgnore--;
          }
        }
        public void CheckBitmap() {
          var wh=map._resize2(2*map.Width+1,map.Height);
          if(bm==null||bm.Width!=wh[0]||bm.Height!=wh[1]) UpdateBitmap(wh[0],wh[1]);
        }
        public void UpdateBitmap(int w,int h) {
          if(bm!=null) bm.Dispose();
          if(w<1) { int[] wh=map._resize2(2*map.Width+1,map.Height);w=wh[0];h=wh[1];}          
          bm=new Bitmap(w,h,PixelFormat.Format32bppRgb);//PixelFormat.Format24bppRgb);          
        }
        public void Repaint(bool dirty) {
          if(NoDraw<1&&bm!=null)
            Repaint(0,0,bm.Width,bm.Height,dirty);
        }
        int Clip(int x,int max) {
          return x<0?0:x>max?max:x;
        }
				void HelpCmd() {
				  if(Help==null) {
            string file=GetType().Assembly.Location;
            file=file.Substring(0,file.Length-3)+"rtf";
            if(!File.Exists(file)) return;
            Help=new fHelp(file);
          }  
					Help.ShowDialog(this);
				}
        public void BMResize(int w,int h) {
        }
        public void Repaint(int x0,int y0,int x1,int y1,int e,bool dirty) {
          int x;
          if(x0>x1) {x=x0;x0=x1;x1=x;}
          if(y0>y1) {x=y0;y0=y1;y1=x;}
          Repaint(x0-e,y0-e,x1+e,y1+e,dirty);
        }
        public void Repaint(Graphics xgr,bool print) {
          if(map.View.back) pmap.Back(xgr,bm.Width,bm.Height);
          else xgr.FillRectangle(print?Brushes.White:BackBrush,0,0,bm.Width,bm.Height);
          xgr.SmoothingMode=SmoothingMode.HighQuality;
          map.Draw(xgr);
        }
        public void Repaint(int x0,int y0,int x1,int y1,bool dirty) {
          int w=bm.Width,h=bm.Height;
          x0=Clip(x0,w);x1=Clip(x1,w);
          y0=Clip(y0,h);y1=Clip(y1,h);
          //int bpl=(w*3+3)&~3;         
          int i=0;
          //w*=3;
          //int color=0xff00ff;
         if(dirty) {
           using(Graphics xgr=Graphics.FromImage(bm)) Repaint(xgr,false);
           
         }
          Graphics gr=this.CreateGraphics();
          if(angle>0||mirx) {
            float z=zoom*1f/ZoomBase;
            if(mirx) {
              gr.ScaleTransform(-1,1);
              gr.TranslateTransform(-sx,sy);
              gr.RotateTransform(-angle);
          } else {
              gr.TranslateTransform(sx,sy);
              gr.RotateTransform(angle);
            }
            gr.ScaleTransform(z,z);
            gr.DrawImage(bm,new Rectangle(x0,y0,x1-x0,y1-y0),x0,y0,x1-x0,y1-y0,GraphicsUnit.Pixel);
            if(x0==0&&y0==0&&x1==w&&y1==h) {
              gr.FillRectangle(BackBrush,-2*Width,-2*Height,2*Width,4*Height+h);
              gr.FillRectangle(BackBrush,w,-2*Height,2*Width,4*Height+h);
              gr.FillRectangle(BackBrush,0,-2*Height,w,2*Height);
              gr.FillRectangle(BackBrush,0,h,w,2*Height);
            }
          } else {
            //gr.DrawImageUnscaled(bm,0,0);
            //int sy=this.MainMenuStrip.Height;
            gr.DrawImage(bm,new Rectangle(SX(x0,y0),SY(x0,y0),zoom*(x1-x0)/ZoomBase,zoom*(y1-y0)/ZoomBase),x0,y0,x1-x0,y1-y0,GraphicsUnit.Pixel);          
            if(x0==0&&y0==0&&x1==w&&y1==h) {
              if(sy>0) gr.FillRectangle(BackBrush,0,0,Width,sy);
              int ey=sy+zoom*h/ZoomBase;
              if(ey<Height) gr.FillRectangle(BackBrush,0,ey,Width,Height-ey);
              if(sx>0) gr.FillRectangle(BackBrush,0,sy,sx,ey-sy);
              int ex=sx+zoom*w/ZoomBase;
              if(ex<Width) gr.FillRectangle(BackBrush,ex,sy,Width-ex,ey-sy);
            
            }
          }
          gr.Dispose();
        }
        void UpdateSin() {
          cos=Math.Cos(angle*Math.PI/180);sin=Math.Sin(angle*Math.PI/180);
        }
        void SetAngle(int x,int y,int a) {
          a%=360;
          if(a<0) a+=360;
          int ox=IX(x,y),oy=IY(x,y),nx;
          angle=a;
          UpdateSin();
          sx=0;sy=0;
          nx=x-SX(ox,oy);sy=y-SY(ox,oy);sx=nx;
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
          int x=IX(e.X,e.Y),y=IY(e.X,e.Y),nx;
          int d=e.Delta;
          bool shift=GDI.ShiftKey,ctrl=GDI.CtrlKey;
          if(shift&&ctrl) {
            SetAngle(e.X,e.Y,angle+(-d/120*15));
            timeDraw=true;
            return;
          }
          if(shift|ctrl) {
            if(shift) sx+=d;
            else sy+=d;
            timeDraw=true;
            return;
          }           
          if(d<0) 
            while(d<0&&zoom>=MinZoom) {
              zoom=zoom*3/4;
              d+=120;
            }
          else 
            while(d>0&&zoom<MaxZoom) {
              zoom=zoom*4/3;
              d-=120;
            }  
          sx=0;sy=0;          
          nx=e.X-SX(x,y);sy=e.Y-SY(x,y);sx=nx;
          timeDraw=true;
        }
        void Fullscreen() {
          bool f=FormBorderStyle!=FormBorderStyle.None;
          if(!f&&(zoom!=ZoomBase||angle!=0)) {
            Center();
            Repaint(false);
            return;
          }
          NoDraw++;
          //MainMenuStrip.Visible=!f;
          FormBorderStyle=f?FormBorderStyle.None:FormBorderStyle.Sizable;
          WindowState=f?FormWindowState.Maximized:FormWindowState.Normal;          
          Center();
          NoDraw--;
        }
        void Center() {
          sin=angle=0;cos=1;
          sx=sy=0;zoom=ZoomBase;
          if(map!=null&&!map.IsEmpty()) {            
            sx=SX(map.Width,0);sy=SY(0,map.Height);
            sx=(Width-sx)/2;sy=(Height-sy)/2;
          }
        }
        void Clear(bool conly) {
          PushUndo(true);
          map.Clear(conly);
          Center();
          Repaint(true);
        }
        void Rotate90(bool counter) {
          if(pmb==MouseButtons.None) PushUndo(false);
          map.Rotate90(counter);
          bm.RotateFlip(counter?RotateFlipType.Rotate90FlipNone:RotateFlipType.Rotate270FlipNone);
          
          
          //UpdateBitmap();
          

          int ix=IX(lmx,lmy),iy=IY(lmx,lmy),yx=ix,yy=iy;
          if(ix>=0&&iy>=0&&ix<map.Height-2&&iy<map.Width-2) {            
            if(counter) {yx=iy;yy=map.Height-2-ix;}
            else {yx=map.Width-2-iy;yy=ix;}
          } else {
            if(ix>map.Height/2-1) yx-=map.Height-map.Width;
            if(iy>map.Width/2-1) yy-=map.Width-map.Height;
          } 
          int nx=SX(yx,yy),ny=SY(yx,yy);
          sx-=nx-lmx;sy-=ny-lmy;
                    
          int px=pix,py=piy;
          if(counter) {pix=py;piy=map.Height-2-px;}
          else {pix=map.Width-2-py;piy=px;}
          
          Repaint(true);
        }
        void MirrorView() {
          int ix=IX(lmx,lmy),iy=IY(lmx,lmy),x,y;
          mirx^=true;
          x=SX(ix,iy);y=SY(ix,iy);
          sx-=(x-lmx);sy-=(y-lmy);
          Repaint(true);
        }
        void MirrorBitmap(bool vertical) {
          PushUndo(false);
          //map.Mirror(vertical);
          //map.Mirror(vertical);
          if(pmb!=MouseButtons.None) {            
            if(vertical) {
              int iy=IY(lmx,lmy),y=map.Height-1-iy,d=y-iy;
              sy-=d*zoom/ZoomBase;
              piy=map.Height-1-piy;
            } else {
              int ix=IX(lmx,lmy),x=map.Width-1-ix,d=x-ix;
              sx-=d*zoom/ZoomBase;
              pix=map.Width-1-pix;
            }
          }
          Repaint(true);
        }
        void AutoShrink() {
          int x0=0,y0=0,x1=map.Width,y1=map.Height;
          if(map.Bounding(ref x0,ref y0,ref x1,ref y1))
            Shrink(x0-1,y0-1,x1+1,y1+1);
        }
        void Shrink(int x0,int y0,int x1,int y1) {
          if(!map.Intersected(ref x0,ref y0,ref x1,ref y1)||(x0==0&&y0==0&&x1==map.Width-1&&y1==map.Height-1)) return;          
          PushUndo(false);
          pmap m2=new pmap(x1-x0+1,y1-y0+1);
          m2.Copy(0,0,map,x0,y0,x1,y1);
          map=m2;
          CheckBitmap();
          Repaint(true);
        }
        void Extent(int x,int y) {
          map.Extent(x,y);
          UpdateBitmap(0,0);
          int dx=0,dy=0;
          if(x<0) dx=-x;
          if(y<0) dy=-y;
          sx-=(int)((dx*cos-dy*sin)*zoom/ZoomBase);
          sy-=(int)((+dx*sin+dy*cos)*zoom/ZoomBase);
          Repaint(true);
          
        }
        void Duplicate(int x0,int y0,int x1,int y1,bool vertical) {
           bool nx=x1<x0,ny=y1<y0;
           int dx=vertical?nx?x1:x0:nx?2*x1-x0+1:x1+1,dy=vertical?ny?2*y1-y0+1:y1+1:ny?y1:y0;
           PushUndo(true);
           map.Copy(dx,dy,map,x0,y0,x1,y1);
           Repaint(true);
           if(vertical) piy=y1+(ny?-1:1);
           else pix=x1+(nx?-1:1);
        }
        Bitmap GetClipboard(string etext) {
         try {
          return Clipboard.GetImage() as Bitmap;
         } catch(Exception ex) {
           MessageBox.Show(this,ex.Message,etext+""==""?"Get clipboard":etext);
           return null;
         }
        } 
        void NoScale() {
          sx=sy=0;zoom=ZoomBase;mirx=false;
          Center();
          Repaint(false);
        }
        Bitmap BMResized() {
          var bm2=new Bitmap(map.Width*zoom/ZoomBase,map.Height*zoom/ZoomBase);
          using(Graphics g=Graphics.FromImage(bm2)) {
            g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            g.DrawImage(bm,0,0,bm2.Width,bm2.Height);
          }
          return bm2;
        }
        static double Sqrt(double x,double y) {
          return Math.Sqrt(x*x+y*y);
        }
        static int Sgn(double x) { return x<0?-1:x>0?1:0;}
        static int Sgn(double x,int y) { return Sgn(x)*(y<0?-y:y);}
    protected override void OnKeyDown(KeyEventArgs e) {
      if(e.KeyCode==Keys.Space) {
        e.SuppressKeyPress=true;
        e.Handled=true;        
      } else
        base.OnKeyDown(e);
    }
    protected override void OnKeyUp(KeyEventArgs e) {
      if(e.KeyCode==Keys.Space) {
        e.SuppressKeyPress=true;
        e.Handled=true;
      } else
        base.OnKeyDown(e);
    }
    static int CS(bool ctrl,bool shift,params int[] val) {
      int i=(ctrl?2:0)+(shift?1:0);
      return i<val.Length?val[i]:val[val.Length-1];
    }
    protected override bool ProcessCmdKey(ref Message msg,Keys keyData) {
          switch(keyData) {
            case Keys.ControlKey|Keys.Control:return false;
            case Keys.F11:Fullscreen();return true;
            case Keys.Escape:
              if(!panel.Visible) panel.Visible=true;
              else if(FormBorderStyle==FormBorderStyle.None) {
                Fullscreen();
              } else 
                NoScale();
              break;
          }          
          bool ctrl=0!=(keyData&Keys.Control);
          bool shift=0!=(keyData&Keys.Shift);
          Keys k=keyData&~(Keys.Shift|Keys.Control);
          if(ctrl&&(k<Keys.F1||k>Keys.F12)&&k!=Keys.OemPeriod&&k!=Keys.Oemcomma&&k!=Keys.Oem5&&k!=Keys.Oem2&&k!=Keys.Oem1) {
            switch(k) {
             case Keys.O:OpenFile();Repaint(true);break;
             case Keys.S:SaveFile1(GDI.ShiftKey,false);break;
             case Keys.N:NewFile();break;
             case Keys.P:if(shift) if(!PrintPage(true)) break;Print();break;
             case Keys.R:if(shift) AutoShrink();else Shrink(pix,piy,IX(lmx,lmy),IY(lmx,lmy));break;
             case Keys.E:if(shift) NoScale();else ExportFile(true);break;
             case Keys.Z:Undo(shift?10:1);break;             
             case Keys.Y:Redo(shift?10:1);break;
             case Keys.Insert:Insert(shift);break;
             case Keys.C:Clipboard.SetImage(bm);break;
             default:goto ret;
            }
            return true;
          } else {
            switch(k) {
              case Keys.Z:Redo(shift?10:1);break;
              case Keys.F1:HelpCmd();break;
              case Keys.F11:panel.Visible^=true;break;
              case Keys.M:MirrorView();break;              
              default: goto ret;
            }
            return true;
          } 
         ret:
          return base.ProcessCmdKey(ref msg,keyData);
        }

        private void fMain_MouseDown(object sender,MouseEventArgs e) {
          bool lb=0!=(e.Button&MouseButtons.Left),rb=0!=(e.Button&MouseButtons.Right),sh=GDI.ShiftKey,ct=GDI.CtrlKey;
          if(pmb!=MouseButtons.None) return;
          if(!lb&&!rb&&sh&&ct) {
            SetAngle(e.X,e.Y,0);
            Repaint(false);
            return;
          }
          int cx,cy;
          pix=cx=IX(e.X,e.Y);piy=cy=IY(e.X,e.Y);
          if(map._mdown(new ME(cx,cy,(int)e.Button>>20,GDI.ShiftKey,GDI.CtrlKey)))
            Repaint(true);          

          //if((lb||rb)&&(AutoSnap||GDI.ShiftKey)) map.SnapPoint(32,pix,piy,out pix,out piy);
          if(lb||rb) {
          } else if(0!=(e.Button&MouseButtons.Middle)) {
            
          }
          lmx=e.X;lmy=e.Y;
          pmb=e.Button;
          pmk=(sh?1:0)|(ct?2:0);
        }
        
        private void fMain_MouseMove(object sender, MouseEventArgs e) {
          if(e.Button==pmb) {
            MouseButtons mb=MouseButtons.Left;
            if(0!=(pmb&MouseButtons.Middle)) {
              if(0==(3&pmk)) {
                sx+=e.X-lmx;sy+=e.Y-lmy;
                timeDraw=true;
              }
            } else {
              int ix=IX(e.X,e.Y),iy=IY(e.X,e.Y);              
              if(map._mmove(new ME(ix,iy,(int)e.Button>>20,GDI.ShiftKey,GDI.CtrlKey),false))
                Repaint(true);
            }
          }
          lmx=e.X;lmy=e.Y;
        }
        private void fMain_MouseUp(object sender,MouseEventArgs e) { // _mup
          if(e.Button!=pmb) return;
          MouseButtons mb=MouseButtons.Left;
          if(0!=(pmb&MouseButtons.Middle)) {
            if(0!=(3&pmk)) {
              ZoomTo(pix,piy,IX(e.X,e.Y),IY(e.X,e.Y),0!=(pmk&1),3==(pmk&3));
              timeDraw=true;
            } else {
              sx+=e.X-lmx;sy+=e.Y-lmy;
              timeDraw=true;
            } 
          } else if(0!=(e.Button&mb)) {
            int ix=IX(e.X,e.Y),iy=IY(e.X,e.Y);
            if(map._mup(new ME(ix,iy,(int)e.Button>>20,GDI.ShiftKey,GDI.CtrlKey)))
              Repaint(true);
          }
          lmx=e.X;lmy=e.Y;pmb=MouseButtons.None;
        }
        void ZoomTo(int x0,int y0,int x1,int y1,bool bigger,bool z100) {
          Rectangle cr=ClientRectangle;
          int r;
          if(x1<x0) {r=x1;x1=x0;x0=r;}
          if(y1<y0) {r=y1;y1=y0;y0=r;}
          x1-=x0;y1-=y0;
          if(x1==0&&y1==0) {
            x0=y0=0;x1=map.Width-1;y1=map.Height-1;
          }
          x1++;y1++;
          int zx=cr.Width*ZoomBase/x1,zy=cr.Height*ZoomBase/y1;
          if(zy<zx^bigger) zx=zy;
          if(zx>ZoomBase*16) zx=ZoomBase*16;
          else if(zx<12) zx=12;
          zoom=z100?ZoomBase:zx;
          sx=cr.Width/2-(x0+x1/2)*zoom/ZoomBase;
          sy=cr.Height/2-(y0+y1/2)*zoom/ZoomBase;
        }       
        
        bool CheckDirty(string caption) {
          if (!Dirty) return true;
          DialogResult dr=MessageBox.Show(this,"Save changes?",caption,MessageBoxButtons.YesNoCancel,MessageBoxIcon.Exclamation,MessageBoxDefaultButton.Button3);
          if(dr!=DialogResult.Yes) return dr==DialogResult.No;
          return SaveFile1(false,false);
        }
        void SetDirty() {
          if(Dirty) return;
          Dirty=true;
          if(!Text.EndsWith("*")) Text+="*";
        }
        void UnsetDirty() {
          if(!Dirty) return;
          Dirty=false;
          if(Text.EndsWith("*")) Text=Text.Substring(0,Text.Length-1);        
        }

        private void miFileClear_Click(object sender, EventArgs e) { 
          Clear(false);
        }
        
        void ChangeFileName(string filename) {
          ofd.FileName=filename;
          string fn=Path.GetFileName(filename);
          Dirty=false;
          Text="Solon"+(string.IsNullOrEmpty(fn)?"":" - "+fn);//+(Dirty?"*":"");
        }
        void LoadFile1(string filename,bool update,V view) {
          if(!File.Exists(filename)) return;
          if(map==null) {map=new pmap(this);if(view!=null) map.View=view;}
          map.ParseFile(filename);
          ChangeFileName(filename);
          ClearUndo();                      
          if(update) {
            UpdateBitmap(0,0);
            Repaint(true);
          }          
        }
        bool SaveFile1(bool saveas,bool one) {
          if(ofd.FileName+""==""||saveas) {
            if(sfd==null) sfd=new SaveFileDialog();
            sfd.FileName=ofd.FileName;
            sfd.Filter="*.sol|*.sol|*.*|*.*";
            sfd.DefaultExt="sol";
            sfd.Title=saveas?"Save as":"Save";
            if(DialogResult.OK!=sfd.ShowDialog()) return false;
            string fname=sfd.FileName;
            if(Path.GetExtension(fname)=="") fname+=".sol";
            ChangeFileName(sfd.FileName);
          }          
          using(TextWriter w=new StreamWriter(ofd.FileName+"_"+DateTime.Now.ToString("yyyyMMddHHmmss"))) {
            if(one) {
              string n=SetIndex<set.Count?set[SetIndex].id:"set_"+Environment.MachineName+"_"+Environment.UserName+"_"+DateTime.Now.ToString("yyyyMMddHHmmss");
              map._game2txt(n,"");
            } else {
              for(int i=0;i<set.Count;i++) {
                w.WriteLine(set[i].txt);
              }   
              UnsetDirty();
            }
          }
          return true;
        }
        bool ExportFile(bool dialog) {   
          if(efd==null) {
            efd=new SaveFileDialog();
            if(ofd.FileName+""!="") efd.FileName=Path.GetFileNameWithoutExtension(ofd.FileName)+".png";
          }
          if(dialog||efd.FileName+""=="") {
            efd.Filter="png image|*.png|svg vector file|*.svg|pdf document|*.pdf|*.*|*.*";
            efd.DefaultExt="png";
            efd.Title="Export";
            if(DialogResult.OK!=efd.ShowDialog()) return false;
          }          
          ExportFile(efd.FileName,GDI.ShiftKey,GDI.CapsLock,efd.FilterIndex);
          return true;
        }

        void ExportFile(string file,bool shift,bool caps,int fi) {
          var m=map;
          if(fi==3) m.ExportPdf(file);
          else if(fi==2) m.ExportSvg(file);
          else m.ExportPng(file);
        }        
        bool PrintPage(bool show) {
          if(paged==null) {
            paged=new PageSetupDialog();
            paged.EnableMetric=true;
            paged.AllowPaper=paged.AllowMargins=paged.AllowOrientation=true;
            paged.PageSettings=new System.Drawing.Printing.PageSettings() {Landscape=true,Margins=new System.Drawing.Printing.Margins(0,0,0,0)};
          }          
          return show&&paged.ShowDialog(this)==DialogResult.OK;
        }
        bool PrintBg,PrintRs,PrintBm;
        void Print() {
          PrintPage(false);
          if(pd==null) pd=new PrintDialog();
          pd.PrinterSettings.DefaultPageSettings.Landscape=paged.PageSettings.Landscape;
          if(pd.ShowDialog(this)==DialogResult.OK) {
            using(System.Drawing.Printing.PrintDocument doc=new System.Drawing.Printing.PrintDocument()) {
              paged.PageSettings.Landscape=pd.PrinterSettings.DefaultPageSettings.Landscape;
              doc.DocumentName=ofd==null?"solon":Path.GetFileName(ofd.FileName);
              doc.PrinterSettings=pd.PrinterSettings;
              doc.DefaultPageSettings=paged.PageSettings.Clone() as System.Drawing.Printing.PageSettings;
              doc.PrintPage+=new System.Drawing.Printing.PrintPageEventHandler(doc_PrintPage);
             try {
              PrintBg=GDI.ShiftKey;PrintRs=!GDI.CtrlKey;PrintBm=GDI.AltKey;
              doc.Print();
             } catch(Exception ex) {
               MessageBox.Show(this,ex.Message,"Exception",MessageBoxButtons.OK);
             }
            }
          }
        }
        void doc_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e) {
          Graphics gr=e.Graphics;
          Rectangle rect=e.MarginBounds;RectangleF rf;
          gr.SmoothingMode=SmoothingMode.HighQuality;
          if(PrintRs) { 
            float rx=1f*rect.Width*1f/bm.Width,ry=1f*rect.Height/bm.Height;
            bool r=rx<ry;rf=new RectangleF(rect.X,rect.Y,rect.Width*(r?1:ry/rx),rect.Height*(r?rx/ry:1));
            pmap.Sizes(r?rx:ry);            
          } else if(PrintBm) rf=new RectangleF(0,0,bm.Width,bm.Height);
          else rf=new RectangleF(rect.X,rect.Y,rect.Width,rect.Height);          
          if(PrintBm) {
            if(!PrintBg) using(Graphics xgr=Graphics.FromImage(bm)) Repaint(xgr,true);
            gr.DrawImage(bm,rf);
            if(!PrintBg) using(Graphics xgr=Graphics.FromImage(bm)) Repaint(xgr,false);
          } else {
            if(PrintBg) pmap.Back(gr,rf.Width,rf.Height,rf.X,rf.Y);
            map.Draw(gr);
            if(PrintRs) pmap.Sizes(1);
          }
          e.HasMorePages=false;
        }

        void NewFile() {
          if(!CheckDirty("New file")) return;          
          ChangeFileName(null);
          NewMap(true);
        }
        void NewMap(bool update) {
          ClearUndo();
          if(NewWidth==0||NewHeight==0) {
            NewWidth=Screen.PrimaryScreen.Bounds.Width;
            NewHeight=Screen.PrimaryScreen.Bounds.Height;
          }
          map=new pmap(NewWidth,NewHeight);
          if(update) {UpdateBitmap(0,0);Repaint(true);}        
        }
        void OpenFile() {
          if(!CheckDirty("Open file")) return;
          string dir=Directory.GetCurrentDirectory();
          ofd.Title="Open";
          ofd.Filter="*.sol|*.sol|*.txt|*.txt|*.*|*.*";
          ofd.DefaultExt="sol";
          if(DialogResult.OK==ofd.ShowDialog(this)) {
            ClearUndo();
            LoadFile(ofd.FileName,GDI.CtrlKey||GDI.ShiftKey);
          }
          Directory.SetCurrentDirectory(dir);
        }
        protected override void OnClosing(CancelEventArgs e) {
          if(!CheckDirty("Close window")) e.Cancel=true;
        }

        void Set2CB(int pos) {
          string[] sa=new string[set.Count];
          for(int i=0;i<sa.Length;i++) sa[i]=set[i].id;
          UIIgnore++;
         try {
          cbSet.DataSource=sa;
          SetIndex=cbSet.SelectedIndex=pos;
         } finally { UIIgnore--;}
        }

        int UIIgnore,SetIndex;
        bool LoadFile(string fn,bool append) {
          if(!File.Exists(fn)) return false;
         try {
          UIIgnore++;
          if(!append) {
            set.Clear();
            cbSet.SelectedIndex=-1;
          }
          Set g=new Set();
          using(var r=new StreamReader(fn)) {
            string l;Match m;
            while(null!=(l=r.ReadLine())) {
              if((m=Set.Rex.Match(l)).Success) {
                if(g.txt!=null||g.id!=null) set.Add(g);
                g=new Set() {id=m.Groups[1]+""};
              }
              g.txt+=(g.txt==null?"":"\r\n")+l;
            }
            if(g.id!=null) set.Add(g);
          }
          if(!append) {
            if(set.Count<1) set.Add(new Set() {id="*",txt="//*,quad,sol"});          
            map.Parse(set[0].txt+"");
          }
          Set2CB(0);
         } finally {
          UIIgnore--;
         }
         return true;
        }

        void GetUndo(UndoItem u) {
          u.sx=sx;u.sy=sy;u.zoom=zoom;u.angle=angle;
          u.pix=pix;u.piy=piy;
        }

        void SwapUndo(UndoItem u) {
          int r;
          r=sx;sx=u.sx;u.sx=r;r=sy;sy=u.sy;u.sy=r;
          r=zoom;zoom=u.zoom;u.zoom=r;
          r=angle;angle=u.angle;u.angle=r;
          r=pix;pix=u.pix;u.pix=r;r=piy;piy=u.piy;u.piy=r;
          UpdateSin();
          pmap x=map;
          map=u.map;
          u.map=x;            
        }

        internal void PushUndo(bool clone) { PushUndo(clone,null);}
        internal void PushUndo(bool clone,string op) {
          if(op!=null&&op==undop) return;
          undop=op;
          UndoItem ui=undoc<undos.Count?undos[undoc]:new UndoItem();
          ui.map=clone?map.Clone():map;
          GetUndo(ui);
          undos.RemoveRange(undoc,undos.Count-undoc);
          if(undoc>UndoMax) {
            undos.RemoveAt(0);
            GC.Collect();
          }
          undos.Add(ui);
          undoc=undos.Count;          
          SetDirty();
        }

        public void xxUndo(bool redo) {
          if(redo) {
            if(undoc>=undos.Count) return;
            UndoItem ui=undos[undoc];
            SwapUndo(ui);
            undoc++;
          } else {
            if(undoc<1) return;
            undoc--;
            UndoItem ui=undos[undoc];
            SwapUndo(ui);
          }
          CheckBitmap();
          Repaint(true);
        }

        internal void Undo(int n) { 
           map.Undo(n);
           Repaint(true);
        }
        internal void Redo(int n) { 
           map.Redo(n);
           Repaint(true);
        }
        internal void ClearUndo() { map.View.undo.Clear();map.View.redo=0;}
        
        string GetTag(object sender) {
          ToolStripMenuItem i = sender as ToolStripMenuItem;
          Button b;
          if(i!=null) return ""+i.Tag;
          else if((b=sender as Button)!=null) return ""+b.Tag;
          else return "";
        }
        private void fMain_Resize(object sender,EventArgs e) {
          Repaint(false);
        }

        private void miMenu_Click(object sender, EventArgs e) { 
          string tag=""+(sender as ToolStripItem).Tag;
          switch(tag) {
             case "new":NewFile();break;
           case "open":OpenFile();break;
           case "save":SaveFile1(false,false);break;
           case "saveas":SaveFile1(true,false);break;
           case "export":ExportFile(true);break;
           case "page":PrintPage(true);break;
           case "print":Print();break;
           case "exit":Close();break;
           case "redo":Redo(GDI.CtrlKey?10:1);return;
           case "undo":Undo(GDI.CtrlKey?10:1);return;
           default:
            UITag(tag);
            break;
          }
        }
        private void miEdit_Click(object sender, EventArgs e) {
          var mi = sender as ToolStripMenuItem;
          switch(mi.Tag+"") {
          }
          Repaint(true);
        }

    private void Tag_Click(object sender, EventArgs e) { 
       Control c=sender as Control;
       UITag(""+c.Tag);
    }
    private void UITag(string tag) {
       if(GDI.ShiftKey) tag='+'+tag;
       if(GDI.CtrlKey) tag='^'+tag;
       if(ProcTag(tag))
        Repaint(true);
    }

    bool ProcTag(string tag) {
      var v=map.View;
      int ct=0,sh=0;
      if(tag[0]=='^') {ct=1;tag=tag.Substring(1);}
      if(tag[0]=='+') {sh=1;tag=tag.Substring(1);}
      if(tag=="addb"&&ct!=0) tag="clonb";
      if(tag=="clear"&&sh!=0) tag="clearc";

      if(IsEdit) {
       if(tag=="rota") {map.transf2(0);Repaint(true);}
       else if(tag=="hori") {map.transf2(1);Repaint(true);}
       else if(tag=="vert") {map.transf2(2);Repaint(true);}
       else if(tag=="shl") {map._deletex(0,1);if(map.Width<4) map._insertx(map.Width,1,true);UpdateBitmap(0,0);Repaint(true);}
       else if(tag=="shr") {map._insertx(0,1,true);UpdateBitmap(0,0);Repaint(true);}
       else if(tag=="shu") {map._deletey(0,1);if(map.Height<4) map._inserty(map.Height,1,true);UpdateBitmap(0,0);Repaint(true);}
       else if(tag=="shd") {map._inserty(0,1,true);UpdateBitmap(0,0);Repaint(true);}
       else if(tag=="clear"||tag=="clearc") { Clear(tag=="clearc");}
       else if(tag=="invert") { map.Invert();Repaint(true);}
       else goto n;
       return true;
      }
     n:
      if(tag=="sol") {map.Game=Game.Sol;UpdateG();}
      else if(tag=="shift") {map.Game=Game.Shift;UpdateG();}
      else if(tag=="onoff") { map.Game=Game.OnOff;UpdateG();}
      else if(tag=="quad"||tag=="hexa"||tag=="tria"||tag=="tria2"||tag=="tria4"||tag=="penta"||tag=="cubes"||tag=="delta"||tag=="trap"||tag=="deca") {
        map.H=(H)Enum.Parse(typeof(H),tag);
        UpdateH();
        CheckBitmap();
      } else if(tag=="diag"||tag=="onoffo"||tag=="onoffx"||tag=="onoff3"||tag=="onoffc") {
         if(tag=="diag") map.Diag^=true;
         if(tag=="onoffo") map.ooo^=true;
         if(tag=="onoffc") map.ooc^=true;
         if(tag=="onoff3") map.oo3^=true;
         if(tag=="onoffx") map.oox^=true;
         UpdateCh();
      } else if(tag=="play"||tag=="free"||tag=="line"||tag=="rect"||tag=="circ"||tag=="circ2"||tag=="fill"||tag=="edge"||tag=="color"||tag=="color2") {
        D d;
        map.View.design=d=(D)Enum.Parse(typeof(D),tag);
        UpdateControls();
      } else if(tag=="bg"||tag=="fg") ColorDiag(tag=="fg");
       else if(tag.StartsWith("col")) {
         map.View.mono=int.Parse(tag.Substring(3));
         UpdateControls();
       } else if(tag=="loadb") {
          string txt=SetIndex<set.Count?set[SetIndex].txt:"";
          map.Parse(txt);
          Repaint(true);
       } else if(tag=="saveb") {
          if(SetIndex<set.Count) {
            Set s=set[SetIndex];
            s.txt=map._game2txt(s.id,s.txt);
          }
          Repaint(true);
       } else if(tag=="renb") {
         set[SetIndex].id=tName.Text;
         Set2CB(SetIndex);
       } else if(tag=="addb"||tag=="clonb") {
         Set s=new Set(),t=set[SetIndex];
         s.id=tName.Text;
         if(s.id=="") s.id=SPlus(t.id);
         s.txt=tag=="clonb"?t.txt:map._game2txt(s.id,"");
         UIIgnore++;
         int pos=SetIndex+1;
         if(SetIndex<0) pos=0;else if(SetIndex>set.Count) pos=set.Count;
         set.Insert(pos,s);
         Set2CB(pos);
         UIIgnore--;
       } else if(tag=="delb") {
         if(set.Count>0) {
           int n=set.Count-1;
           if(SetIndex<0) SetIndex=0;if(SetIndex>n) SetIndex=n;
           set.RemoveAt(SetIndex);
           if(SetIndex==n) SetIndex--;
           Set2CB(SetIndex);
         }
       } else if(tag=="bup"||tag=="bdown") {
         int i=SetIndex,j=i+(tag.Length==3?-1:1);
         if(j>=0&&j<set.Count) {
           Set s=set[i];set[i]=set[j];set[j]=s;
           Set2CB(j);
         }
       } else if(tag=="peg"||tag=="rou"||tag=="corn"||tag=="grd1"||tag=="grd2") { 
         if(tag=="peg") map.View.peg^=true;
         else if(tag=="rou") map.View.rou^=true;
         else if(tag=="corn") map.View.corn=(map.View.corn+1)%3;
         else if(tag=="grd1") Grd1(ct,sh);
         else if(tag=="grd2") map.View.grdm2=map.View.grdm2!=0?0:1;
         UpdateControls();
         return true;
       }
       else
        return false; 
      return true;
    }
    static string SPlus(string s) {
      int n=s.Length,p=n-1;
      while(p>=0&&s[p]=='9') p--;
      char ch=p<0?' ':s[p];
      bool d=char.IsDigit(ch);
      s=s.Substring(0,p+(d?0:1));
      p=n-1-p;      
      return s+(d?(char)(ch+1):p>0?'1':'2')+new string('0',p);
    }
    void Grd1(int sh,int ct) {       
       map.View.grdm=map.View.grdm==0?1+ct+2*sh:0;
       if((sh|ct)!=0&&map.View.grdm==0) {
         map.View.grdm=5+sh*(1+ct);
       }
       if(map.H==H.tria||map.H==H.tria2||map.H==H.tria4) {map.View.grdx=1;map.View.grdy=2;}
       else {map.View.grdx=1;map.View.grdy=1;}
    }
    private void CheckedChanged(object sender, EventArgs e) {
      if(UIIgnore>0) return;
      var c=sender as Control;var ch=c as CheckBox;RadioButton r=c as RadioButton;
      string tag=""+c.Tag;
      if(r!=null) {
        if(r.Checked) {
          ProcTag(tag);
        }
      } else  if(tag=="peg") { map.View.peg=ch.Checked;UpdateControls();}
      else if(tag=="rou") {map.View.rou=ch.Checked;UpdateControls();}
      else if(tag=="corn") {map.View.corn=(map.View.corn+1)%3;ch.Checked=map.View.corn!=0;UpdateControls();} 
      else if(tag=="diag") map.Diag=ch.Checked; 
      else if(tag=="onoffx") map.oox=ch.Checked; 
      else if(tag=="grd1") {
        Grd1(GDI.CtrlKey?1:0,GDI.ShiftKey?1:0);
        UpdateControls();        
      } else if(tag=="grd2") {map.View.grdm2=ch.Checked?1:0;UpdateControls();}
      else if(tag=="white") map.View.white=ch.Checked;        

      Repaint(true);

    }

    float[] _mxy(int x,int y) {
       int ix=IX(lmx,lmy),iy=IY(lmx,lmy);
       return pmap._mxy(map.H,ix,iy);
    }

    private void chBB_SelectedIndexChanged(object sender, EventArgs e) {
       //ComboBox cb=sender as ComboBox;
       string r;
       if(NUI==0) {
         NUI++;
         if(sender==cbwhite2) {
           r=""+cbwhite2.SelectedItem;
           map.View.whiter=(r+"xxxx").Substring(0,4);
           _w4();
           if(NUI==1) chWhite.Checked=true;
         } else if(sender==cbOnOff6) {
           map.View.onoff6=cbOnOff6.SelectedIndex;
         } else {
           r="xbw";
           map.View.whiter=r=""+r[chBB.SelectedIndex]+r[chBW.SelectedIndex]+r[chWB.SelectedIndex]+r[chWW.SelectedIndex];
           UpdateWhite2(r);
         }
         NUI--;
       }
    }
    bool strcmp(string a,string b,int n) {
      int al=Math.Min(4,a.Length),bl=Math.Min(4,b.Length);
      if(al!=bl) return true;
      for(int i=0;i<al;i++) if(a[i]!=b[i]) return true;
      return false;
    }

    bool IsPlay { get { return map!=null&&map.View.design==D.play;}}
    bool IsEdit { get { return !IsPlay;}}

    private void cbSet_SelectedIndexChanged(object sender, EventArgs e) {
      cbSet_event('i',sender as ComboBox,e);
    }

    private void fMain_DoubleClick(object sender, EventArgs e) {
       if(IsPlay) Fullscreen();
    }

    void cbSet_event(char a,ComboBox cb,EventArgs e) {
      if(UIIgnore>0) return;
      int i=cb.SelectedIndex;
      if(i<0) i=SetIndex;
      if(i>=0&&i<set.Count) {
        if(a=='i') {
          SetIndex=i;
          map.Parse(set[i].txt);          
          Repaint(true);
        } else if(a=='t') {
          set[i].id=cbSet.Text;
        }
      }
    }

    void UpdateWhite2(string r) {
      int i=0,n=cbwhite2.Items.Count;
      var l=cbwhite2.Items;
      NUI++;
      for(;i<n;i++)
        if(!strcmp(""+l[i],r,4)) {
          cbwhite2.SelectedIndex=i;
          goto f;
        }
      cbwhite2.SelectedIndex=n-1;
      cbwhite2.Items[n-1]=r;          
     f:NUI--;
    }

    void Insert(bool vert) {
      var xy=_mxy(lmx,lmy);
      int x=(int)xy[0],y=(int)xy[1];
      if(vert) {
        map._inserty(y,1,false);
      } else {
        map._insertx(x,1,false);
      }
      UpdateBitmap(0,0);Repaint(true);
    }


    protected override void OnPaintBackground(PaintEventArgs e) {
          //base.OnPaintBackground(e);
        }
        protected override void OnPaint(PaintEventArgs e) {
          //base.OnPaint(e);
          //Repaint(e.Graphics,e.ClipRectangle.Left,e.ClipRectangle.Top,e.ClipRectangle.Width,e.ClipRectangle.Height);
          timeDraw=true;
        }        

        private void timer_Tick(object sender,EventArgs e) {
          if(timeDraw) {
            Repaint(false);
            timeDraw=false;
          }
        }
        int ColorIdx(int idx,bool shift) {
          idx=idx*2+(shift?1:0)-1;
          switch(idx) {
           case 1:return 0xff0000;
           case 2:return 0xff0088;
           case 3:return 0xffff00;
           case 4:return 0xff8800;
           case 5:return 0x00ff00;
           case 6:return 0x88ff00;
           case 7:return 0x00ffff;
           case 8:return 0x00ff88;
           case 9:return 0x0000ff;
           case 10:return 0x0088ff;
           case 11:return 0xff00ff;
           case 12:return 0x8800ff;
           case 13:return 0xffffff;
           case 14:return 0xcccccc;
           case 15:return 0x888888;
           case 16:return 0x444444;
           default:return 0;
          }
        }
        void SetColor(int x) {
          bBg.BackColor=ColorInt(BColor=x);
        }
        static Color ColorInt(int color) {return Color.FromArgb(color|(255<<24));}


    private void bBW_MouseUp(object sender, MouseEventArgs e)
    {
      Clear(true);
    }


    private void chColor_CheckedChanged(object sender,EventArgs e) {

    }

    private void Button_MouseUp(object sender, MouseEventArgs e) {
      string tag=GetTag(sender);
      int d=e.Button==MouseButtons.Right?-2:-1;
    }

    private void ColorDiag(bool fore) {
          Button b=fore?bBg:bBg;
          CDialog.Color=b.BackColor;
          CDialog.FullOpen=true;
          if(DialogResult.OK==CDialog.ShowDialog(this)) {
            b.BackColor=CDialog.Color;
            int[] cc=CDialog.CustomColors;
            int h,c=(CDialog.Color.B<<16)|(CDialog.Color.G<<8)|CDialog.Color.R;
            if(cc[0]==c) return;
            for(h=0;h<cc.Length-1;h++)
              if(cc[h]==c) break;
            while(h>0) {
              cc[h]=cc[h-1];
              h--;
            }
            cc[0]=c;  
            CDialog.CustomColors=cc;
          }

        }

     void RotateColor(byte[] data,int i) {
       byte r=data[i],g=data[i+1],b=data[i+2];
       data[i]=b;data[i+1]=r;data[i+2]=g;
     }
        

        private void panel_MouseUp(object sender,MouseEventArgs e) {
          AnchorStyles anch=panel.Anchor;
          Point l=panel.Location;
          if(e.X<0) {
            l.X=0;
            anch=anch&~AnchorStyles.Right|AnchorStyles.Left;
          } else if(e.X>panel.Width) {
            l.X=ClientSize.Width-panel.Width;
            anch=anch&~AnchorStyles.Left|AnchorStyles.Right;
          }
          if(e.Y<0) {
            l.Y=0;
            anch=anch&~AnchorStyles.Bottom|AnchorStyles.Top;
          } else if(e.Y>panel.Height) {
            l.Y=ClientSize.Height-panel.Height;
            anch=anch&~AnchorStyles.Top|AnchorStyles.Bottom;
          }
          if(l.X!=panel.Left||l.Y!=panel.Top) panel.Location=l;
          if(anch!=panel.Anchor) panel.Anchor=anch;
        }       

    }

    public class Set {
      public string id,txt;
      public static Regex Rex=new Regex(@"^//.*@([^,]+)");
    }

    public class UndoItem {
      public pmap map;
      public int sx,sy,zoom,angle,pix,piy;
    }
    
    public static class GDI {
      public static bool CtrlRKey {get{ return 0!=(0x8000&GDI.GetKeyState(Keys.RControlKey));}}
      public static bool CtrlLKey {get{ return 0!=(0x8000&GDI.GetKeyState(Keys.LControlKey));}}
      public static bool CtrlKey {get{ return 0!=(0x8000&GDI.GetKeyState(Keys.ControlKey));}}
      public static bool ShiftKey {get{ return 0!=(0x8000&GDI.GetKeyState(Keys.ShiftKey));}}
      public static bool ShiftLKey {get{ return 0!=(0x8000&GDI.GetKeyState(Keys.LShiftKey));}}
      public static bool ShiftRKey {get{ return 0!=(0x8000&GDI.GetKeyState(Keys.RShiftKey));}}
      public static bool AltKey { get { return 0 != (0x8000 & GDI.GetKeyState(Keys.Menu)); } }
      public static bool AltLKey { get { return 0 != (0x8000 & GDI.GetKeyState(Keys.LMenu)); } }
      public static bool AltRKey { get { return 0 != (0x8000 & GDI.GetKeyState(Keys.RMenu)); } }
      public static bool CapsLock { get { return 0 != (0x0001 & GDI.GetKeyState(Keys.CapsLock)); } }
      public static bool NumLock { get { return 0 != (0x0001 & GDI.GetKeyState(Keys.NumLock)); } }
      public static bool LButton { get { return 0 != (0x8000 & GDI.GetKeyState(Keys.LButton)); } }
      public static bool RButton { get { return 0 != (0x8000 & GDI.GetKeyState(Keys.RButton)); } }
      public static bool Space { get { return 0 != (0x8000 & GDI.GetKeyState(Keys.Space)); } }
    
      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern bool Beep(int dwFreq, int dwDuration);

     [DllImport("user32"), SuppressUnmanagedCodeSecurity, PreserveSig]
     public static extern short GetKeyState(Keys key);

     [DllImport("gdi32"), SuppressUnmanagedCodeSecurity, PreserveSig]
     public static extern int SetROP2(IntPtr hdc, int fnDrawMode);    
     [DllImport("gdi32") ,SuppressUnmanagedCodeSecurity, PreserveSig]  
     public static extern bool DeleteObject(IntPtr hObject);
     [DllImport("gdi32") ,SuppressUnmanagedCodeSecurity, PreserveSig]  
     public static extern IntPtr CreateSolidBrush( int crColor );
     [DllImport("user32") ,SuppressUnmanagedCodeSecurity, PreserveSig]  
     public static extern int FillRect(IntPtr hdc, ref RECT lprc,IntPtr hbr);
     [StructLayout(LayoutKind.Sequential)]
     public struct RECT { 
       public int Left, Top, Right, Bottom;
       public RECT(int left,int top,int right,int bottom) {
         Left=left;Top=top;Right=right;Bottom=bottom;
       }
     }
    }

    public class Pal {        
        public static double max(double a,double b,double c) {
          return a>b?a>c?a:c:b>c?b:c;  
        }
        public static double size(double a,double b,double c) {
          return Math.Sqrt(a*a+b*b+c*c);
        }
        public static void Color(byte[] data,int offset,double value,double[] palette,bool hsv) {         
          int p;
          if(value<=palette[0]) {
            data[offset+2]=(byte)(palette[1]*255.5);
            data[offset+1]=(byte)(palette[2]*255.5);
            data[offset]=(byte)(palette[3]*255.5);
          } else if(value>=palette[palette.Length-4]) {
            p=palette.Length-3;
            data[offset+2]=(byte)(palette[p++]*255.5);
            data[offset+1]=(byte)(palette[p++]*255.5);
            data[offset]=(byte)(palette[p++]*255.5);
          } else {
            for(p=0;p<palette.Length&&value>palette[p+4];p+=4);
            double r1=(value-palette[p])/(palette[p+4]-palette[p]),r0=1-r1;
            if(hsv) {
              double r=palette[p+1]*r0+palette[p+5]*r1;
              double g=palette[p+2]*r0+palette[p+6]*r1;
              double b=palette[p+3]*r0+palette[p+7]*r1;
              double s=size(palette[p+1],palette[p+2],palette[p+3])*r0+size(palette[p+5],palette[p+6],palette[p+7])*r1;
              double s2=size(r,g,b);
              if(s2>0) {
                double m=max(r,g,b);
                s/=s2;
                if(m*s>1) s=1/m;
                r*=s;g*=s;b*=s;
              }
              data[offset+2]=(byte)(255.5*r);
              data[offset+1]=(byte)(255.5*g);
              data[offset]=(byte)(255.5*b);
            } else {
              data[offset+2]=(byte)(255.5*(palette[p+1]*r0+palette[p+5]*r1));
              data[offset+1]=(byte)(255.5*(palette[p+2]*r0+palette[p+6]*r1));
              data[offset]=(byte)(255.5*(palette[p+3]*r0+palette[p+7]*r1));
            }
          }
        }
        public static void RGB2HSV(double r,double g,double b,out double h,out double s,out double v) {
           double min=r<g?r<b?r:b:g<b?g:b;
           double max=r>g?r>b?r:b:g>b?g:b;
           v=max;
           if(max==0) {
             s=0;
             h=-1;
             return;
           }
           double delta=max-min;
           s=delta/max;
           if(r==max) h=(g-b)/delta;
           else if(g==max) h=2+(b-r)/delta;
           else h=4+(r-g)/delta;
           h*=60;
           if(h<0) h+=360;
        }
        public static void HSV2RGB(double h,double s,double v,out double r,out double g,out double b) {
          if(s==0) {
            r=g=b=s;
            return;
          }
          h/=60;
          int i=(int)Math.Floor(h);
          double f=h-i;
          double p=v*(1-s),q=v*(1-s*f),t=v*(1-s*(1-f));
          switch(i) {
           case 0:r=v;g=t;b=p;break;
           case 1:r=q;g=v;b=p;break;
           case 2:r=p;g=v;b=t;break;
           case 3:r=p;g=q;b=v;break;
           case 4:r=t;g=p;b=v;break;
           default:r=v;g=p;b=q;break;               
          }          
        }
        public static int ColorIntensity(int color,int i) {
          if(i==100) return color;
          if(i<=0) return 0;
          int r=color&255,g=(color>>8)&255,b=(color>>16)&255;
          r=r*i/256;if(r>255) r=255;
          g=g*i/256;if(g>255) g=255;
          b=b*i/256;if(b>255) b=255;
          return r|(g<<8)|(b<<16);
        }
        public const int White=0xffffff,Black=0;
        public static int RGBSum(int color) {
          return (color&255)+((color>>8)&255)+((color>>16)&255);
        }        
        public static int ColorIntensity765(int color,int i) {
          if(i<0) return Black;else if(i>765) return White;
          int r=color&255,g=(color>>8)&255,b=(color>>16)&255;
          int mi,ma,s=r+g+b;
          if(s==i) goto end;
          if(r==b&&b==g) { r=g=i/3;b=i-r-g;goto end;}          
          if(r<g) { mi=r;ma=g;} else {mi=g;ma=r;}
          if(b<mi) mi=b;else if(b>ma) ma=b;
          if(mi>0||ma<255) {
            int sr=(r-mi)*255/(ma-mi),sg=(g-mi)*255/(ma-mi),sb=(b-mi)*255/(ma-mi);
            int ss=r+g+b;
            if(i<s&&ss<s||i>s&&ss>s) {
              r=sr;g=sg;b=sb;s=ss;
            }
          }
          if(i<s) {
            r=r*i/s;g=g*i/s;b=i-r-g;
          } else {
            i=765-i;s=765-s;
            r=255-((255-r)*i/s);g=255-((255-g)*i/s);b=(765-i)-r-g;
          }
         end: 
          return r|(g<<8)|(b<<16);
        }
        public static Color IntColor(int rgb) {
          return System.Drawing.Color.FromArgb((255<<24)|rgb);
        }
        public static string IntHtml(int rgb) {
          if(rgb<0) return "none";
          bool x3=(rgb&0xf0f0f)==((rgb>>4)&0xf0f0f);
          return "#"+(x3?(((rgb>>8)&0xf00)|((rgb>>4)&0xf0)|(rgb&0xf)).ToString("x3"):(rgb&0xffffff).ToString("x6"));
        }
        public static int IntColor(Color c) { return c.ToArgb()&0xffffff;}
        public static int MixColor(int c,int c2,int a,int n) {
          if(a<1) return c;else if(a>=n) return c2;
          int an=n-a;
          int b=(c&255)*an+(c2&255)*a,g=((c>>8)&255)*an+((c2>>8)&255)*a,r=((c>>16)&255)*an+((c2>>16)&255)*a;
          return (b/n)|((g/n)<<8)|((r/n)<<16);
        }
        public static int NegColor(int rgb) {
          int r=rgb&255,g=(rgb>>8)&255,b=(rgb>>16)&255,mi,ma;
          if(r<g) {mi=r;ma=g;} else {mi=g;ma=r;}
          if(b>mi) mi=b;else if(b>ma) ma=b;
          ma=255-ma;
          r=ma+r-mi;
          g=ma+g-mi;
          b=ma+b-mi;
          return r|(g<<8)|(b<<16);
        }
        public static void NegColor(byte[] data,int i) {
          byte r=data[i],g=data[i+1],b=data[i+2],mi,ma;
          if(r<g) {mi=r;ma=g;} else {mi=g;ma=r;}
          if(b<mi) mi=b;else if(b>ma) ma=b;
          ma=(byte)(255-ma);
          data[i]=(byte)(ma+r-mi);
          data[i+1]=(byte)(ma+g-mi);
          data[i+2]=(byte)(ma+b-mi);
        }        
        public static unsafe void NegColor(byte *data,int i) {
          byte r=data[i],g=data[i+1],b=data[i+2],mi,ma;
          if(r<g) {mi=r;ma=g;} else {mi=g;ma=r;}
          if(b<mi) mi=b;else if(b>ma) ma=b;
          ma=(byte)(255-ma);
          data[i]=(byte)(ma+r-mi);
          data[i+1]=(byte)(ma+g-mi);
          data[i+2]=(byte)(ma+b-mi);
        }
       public static unsafe void RotateColor(byte* p) {
         byte r=*p,g=p[1],b=p[2];
         *p=b;p[1]=r;p[2]=g;
       }
       public static unsafe void RGB2CMY(byte* p) {
          byte r=*p,g=p[1],b=p[2],mi,ma;
          if(r<g) {mi=r;ma=g;} else {mi=g;ma=r;}
          if(b<mi) mi=b;else if(b>ma) ma=b;          
          *p=(byte)(mi+ma-r);
          p[1]=(byte)(mi+ma-g);
          p[2]=(byte)(mi+ma-b);
        }
       public static unsafe void RXB(byte* p) {
         byte r=*p;
         *p=p[2];
         p[2]=r;
       }
    }


}
