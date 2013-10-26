using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;

namespace AlumnoEjemplos.Jet_Pilot
{
    public class Skybox
    {
        private int anchoPantalla = GuiController.Instance.Panel3d.Width;
        private int altoPantalla = GuiController.Instance.Panel3d.Height;

        TgcPlaneWall up;
        //TgcPlaneWall dn;
        TgcPlaneWall lt;
        TgcPlaneWall rt;
        TgcPlaneWall ft;
        TgcPlaneWall bk;

        private float size = 49000.0f;

        public Skybox()
        {
            string skyTexPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";

            TgcTexture texture;

            up = new TgcPlaneWall();
            texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, skyTexPath + "lostatseaday_up.jpg");
            up.setTexture(texture);
            up.Origin = new Vector3(-size * 0.5f, size * 0.5f, size * 0.5f);
            up.Size = new Vector3(size, size, -size);
            up.Orientation = TgcPlaneWall.Orientations.XZplane;

            //dn = new TgcPlaneWall();
            //texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, skyTexPath + "lostatseaday_dn.jpg");
            //dn.setTexture(texture);
            //dn.Origin = new Vector3(-size * 0.5f, -size * 0.5f, -size * 0.5f);
            //dn.Size = new Vector3(size, size, size);
            //dn.Orientation = TgcPlaneWall.Orientations.XZplane;

            lt = new TgcPlaneWall();
            texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, skyTexPath + "lostatseaday_bk.jpg");
            lt.setTexture(texture);
            lt.Origin = new Vector3(size * 0.5f, -size * 0.5f, size * 0.5f);
            lt.Size = new Vector3(size, size, -size);
            lt.Orientation = TgcPlaneWall.Orientations.YZplane;

            rt = new TgcPlaneWall();
            texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, skyTexPath + "lostatseaday_ft.jpg");
            rt.setTexture(texture);
            rt.Origin = new Vector3(-size * 0.5f, -size * 0.5f, -size * 0.5f);
            rt.Size = new Vector3(size, size, size);
            rt.Orientation = TgcPlaneWall.Orientations.YZplane;

            ft = new TgcPlaneWall();
            texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, skyTexPath + "lostatseaday_lf.jpg");
            ft.setTexture(texture);
            ft.Origin = new Vector3(size * 0.5f, -size * 0.5f, -size * 0.5f);
            ft.Size = new Vector3(-size, size, size);
            ft.Orientation = TgcPlaneWall.Orientations.XYplane;

            bk = new TgcPlaneWall();
            texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, skyTexPath + "lostatseaday_rt.jpg");
            bk.setTexture(texture);
            bk.Origin = new Vector3(-size * 0.5f, -size * 0.5f, size * 0.5f);
            bk.Size = new Vector3(size, size, size);
            bk.Orientation = TgcPlaneWall.Orientations.XYplane;

            this.Reset();
        }

        public void Reset()
        {
            //Setea el centro del SkyBox en el medio de la pantalla, tomando los valores del GuiController
            SetCenter(new Vector3(anchoPantalla / 2, altoPantalla / 2, anchoPantalla / 2));
        }

        public void SetCenter(Vector3 center)
        {
            up.Origin = center + new Vector3(-size * 0.5f, size * 0.5f, size * 0.5f);
            //dn.Origin = center + new Vector3(-size * 0.5f, -size * 0.5f, -size * 0.5f);
            lt.Origin = center + new Vector3(size * 0.5f, -size * 0.5f, size * 0.5f);
            rt.Origin = center + new Vector3(-size * 0.5f, -size * 0.5f, -size * 0.5f);
            ft.Origin = center + new Vector3(size * 0.5f, -size * 0.5f, -size * 0.5f);
            bk.Origin = center + new Vector3(-size * 0.5f, -size * 0.5f, size * 0.5f);
            
            up.updateValues();
            //dn.updateValues();
            lt.updateValues();
            rt.updateValues();
            ft.updateValues();
            bk.updateValues();
        }

        public void renderSkybox(Vector3 center)
        {
            this.SetCenter(center);
            this.Render();
        }

        public void Render()
        {
            up.render();
            //dn.render();
            lt.render();
            rt.render();
            ft.render();
            bk.render();
        }

        public void dispose()
        {
            up.dispose();
            lt.dispose();
            rt.dispose();
            ft.dispose();
            bk.dispose();
        }

        
    }

}
