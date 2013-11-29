using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Jet_Pilot
{
	class Plane
	{
		TgcMesh plane;
        TgcMesh exhaust;
		float pitchSpeed;
		float rollSpeed;
		float velocidad_aceleracion;
		int pitch;
		int roll;
		int acelerador;

		float velocidad_tangente;
		float velocidad_normal;


        float specularEx = 20f;
        public float SpecularEx
        {
            get { return specularEx; }
            set { specularEx = value; }
        }
        public Vector3 lightPos { get; set; }

        //variables para shaders
        private Effect efFuego;
        private Texture g_pRenderTargetFuego;
        private TgcTexture fuegoBase;
        private TgcTexture fuegoDistorsion;
        private TgcTexture fuegoOpacidad;
        private VertexBuffer g_pVBV3D;
        private Surface g_pDepthStencil;
        private float totalTime;

		public Plane()
		{
            String path = GuiController.Instance.AlumnoEjemplosMediaDir + @"Jet_Pilot\caza\caza-TgcScene.xml";
			TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(path);
			plane = scene.Meshes[0];
			plane.AutoTransformEnable = false;
            plane.setColor(Color.DarkGray);
            
            exhaust = scene.Meshes[1];
            exhaust.AutoTransformEnable = false;
            exhaust.setColor(Color.Orange);
            exhaust.AlphaBlendEnable = true;

			rollSpeed = 2.5f;
			pitchSpeed = 2.0f;
			velocidad_normal = 0.6f;
			velocidad_aceleracion = 500.0f;
            
            initShaders();
			
            Reset();
		}

        //hago el getter del avion
        public TgcMesh getMesh()
        {
            return this.plane;
        }

		public void Reset()
		{
			pitch = 0;
			roll = 0;
			acelerador = 0;

			velocidad_tangente = 500.0f;

			Matrix m = plane.Transform;
			m = Matrix.Identity;
			plane.Transform = m;
          		exhaust.Transform = m;
			SetPosition(new Vector3 (0, 1500f, 0));

		}

		public void Update(float dt)
		{
            totalTime += dt;

			// Mover el avion hacia donde esta apuntando
			Vector3 dir = -ZAxis();
			Translate(dir * velocidad_tangente * dt);

			// Rotar el avion de acuerdo a los comandos recibidos
			if (pitch != 0)
			{
				RotateAroundX(pitch * pitchSpeed * dt);
			}
			if (roll != 0)
			{
				RotateAroundZ(roll * rollSpeed * dt);
			}

			if (acelerador != 0)
			{
				velocidad_tangente += velocidad_aceleracion * acelerador * dt;
				if (velocidad_tangente < 500.0f) velocidad_tangente = 500.0f;
				if (velocidad_tangente > 20000.0f) velocidad_tangente = 20000.0f;
			}

			// La rotacion depende de cuanto este inclinado hacia los lados,
			// esto se puede calcular con la componente en Y del eje X del avión
			float turn = plane.Transform.M12 * velocidad_normal * dt;

			// Y rotamos el avion respecto a la vertical que lo atraviesa
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), new Vector3(0, 1, 0), turn);
			m.Multiply(r);
			plane.Transform = m;
            exhaust.Transform = m;
		}

        public Vector3 Get_Center() {
            return plane.BoundingBox.calculateBoxCenter();
        }

        public float Get_Radius() {
            return plane.BoundingBox.calculateBoxRadius();
        }
 

		public void SetYoke(bool up, bool down, bool left, bool right)
		{
			pitch = 0;
			roll = 0;
			if (up && !down)
			{
				pitch = -1;
			}
			else if (down && !up)
			{
				pitch = 1;
			}
			if (left && !right)
			{
				roll = -1;
			}
			else if (right && !left)
			{
				roll = 1;
			}
		}

		public void SetThrottle(bool plus, bool minus)
		{
			acelerador = 0;
			if (plus && !minus)
			{
				acelerador = 1;
			}
			else if (!plus && minus)
			{
				acelerador = -1;
			}
		}

		public void SetPitchSpeed(float s)
		{
			pitchSpeed = s;
		}

		public void SetRollSpeed(float s)
		{
			rollSpeed = s;
		}

		public void SetTurnSpeed(float s)
		{
			velocidad_normal = s;
		}

        public void SetVelocidad_aceleracion(float new_acel) {
            velocidad_aceleracion = new_acel; 
        }


        private void initShaders() {
            plane.Effect = GuiController.Instance.Shaders.TgcMeshPhongShader;

            //inicializacion fuego
            string pathFuego = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\fuego\\";
            Device d3dDevice = GuiController.Instance.D3dDevice;

            efFuego = TgcShaders.loadEffect(pathFuego + "Fire.fx");
            //render target
            g_pRenderTargetFuego = new Texture(d3dDevice,
                                            d3dDevice.PresentationParameters.BackBufferWidth,
                                            d3dDevice.PresentationParameters.BackBufferHeight, 
                                            1, Usage.RenderTarget, Format.A16B16G16R16, Pool.Default);
            //cargo texturas
            fuegoBase = TgcTexture.createTexture( pathFuego + "FireBase.bmp");
            fuegoDistorsion = TgcTexture.createTexture(pathFuego + "FireDistortion.bmp");
            fuegoOpacidad = TgcTexture.createTexture(pathFuego + "FireOpacity.bmp");

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                         d3dDevice.PresentationParameters.BackBufferHeight,
                                                                         DepthFormat.D24S8,
                                                                         MultiSampleType.None,
                                                                         0,
                                                                         true);

            //creo un Screen Aligned Quad para renderizar la textura del fuego
            vertexFuego[] vertices = new vertexFuego[]
		    {
    			new vertexFuego( new Vector3(-1, 1, 1),new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(0,0) ), 
			    new vertexFuego(new Vector3(1, 1, 1), new Vector2(1,0), new Vector2(1,0), new Vector2(1,0), new Vector2(1,0) ), 
			    new vertexFuego(new Vector3(-1, -1, 1), new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(0,1) ), 
			    new vertexFuego(new Vector3(1,-1, 1), new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(1,1) )
    		};
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(vertexFuego),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        vertexFuego.Format , Pool.Default);

            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

        }

        private void updateShaders() 
        {
            Device device = GuiController.Instance.D3dDevice;

            //Cargar variables Phong shader
            plane.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array((Vector3)GuiController.Instance.Modifiers["lightPos"]));
            plane.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.CurrentCamera.getPosition()));
            plane.Effect.SetValue("ambientColor", ColorValue.FromColor(Color.White));
            plane.Effect.SetValue("diffuseColor", ColorValue.FromColor(Color.Wheat));
            plane.Effect.SetValue("specularColor", ColorValue.FromColor(Color.White));
            plane.Effect.SetValue("specularExp", specularEx);

            //variables Fuego
            

            // dibujo la escena una textura 

            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pRenderTargetFuego.GetSurfaceLevel(0);
            
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = device.DepthStencilSurface;
            // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
            // por no soportar usualmente el multisampling en el render to texture.
            device.DepthStencilSurface = g_pDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            efFuego.Technique = "Fire";

            efFuego.SetValue("fire_base_Tex", fuegoBase.D3dTexture);
            efFuego.SetValue("fire_distortion_Tex", fuegoDistorsion.D3dTexture);
            efFuego.SetValue("fire_opacity_Tex", fuegoOpacidad.D3dTexture);
            efFuego.SetValue("time",totalTime);

            device.VertexFormat = vertexFuego.Format;
            device.SetStreamSource(0, g_pVBV3D,0);
            efFuego.Begin(FX.None);
            efFuego.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            efFuego.EndPass();
            efFuego.End();

            device.EndScene();
            pSurf.Dispose();

            // restuaro el render target y el stencil
            device.DepthStencilSurface = pOldDS;
            device.SetRenderTarget(0, pOldRT);

            exhaust.DiffuseMaps[0] = new TgcTexture("","",g_pRenderTargetFuego,false);

        }


		public void Render(bool BB_activado){
			
	    plane.BoundingBox.scaleTranslate(GetPosition(), plane.Scale);
			
            if (BB_activado)
            {
                plane.BoundingBox.render();
            }
            updateShaders();
		    plane.render();
            exhaust.render();
		}

		public float GetAirSpeed()
		{
			return velocidad_tangente;
		}

		public Vector3 XAxis()
		{
			Matrix m = plane.Transform;
			return new Vector3(m.M11, m.M12, m.M13);
		}

		public Vector3 YAxis()
		{
			Matrix m = plane.Transform;
			return new Vector3(m.M21, m.M22, m.M23);
		}

		public Vector3 ZAxis()
		{
			Matrix m = plane.Transform;
			return new Vector3(m.M31, m.M32, m.M33);
		}

		public void Translate(Vector3 delta)
		{
			Matrix m = plane.Transform;
			Matrix t = Matrix.Identity;
			t.M41 = delta.X;
			t.M42 = delta.Y;
			t.M43 = delta.Z;
			m.Multiply(t);
			plane.Transform = m;
            exhaust.Transform = m;
		}

		public Vector3 GetPosition()
		{
		    Matrix m = plane.Transform;
			return new Vector3(m.M41, m.M42, m.M43);
		}

		public void SetPosition(Vector3 p)
		{
			Matrix m = plane.Transform;
			m.M41 = p.X;
			m.M42 = p.Y;
			m.M43 = p.Z;
			plane.Transform = m;
            exhaust.Transform = m;
		}

		public void RotateAroundX(float ang)
		{
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), XAxis(), ang);
			m.Multiply(r);
			plane.Transform = m;
            exhaust.Transform = m;
		}
		public void RotateAroundY(float ang)
		{
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), YAxis(), ang);
			m.Multiply(r);
			plane.Transform = m;
            exhaust.Transform = m;
		}
		public void RotateAroundZ(float ang)
		{
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), ZAxis(), ang);
			m.Multiply(r);
			plane.Transform = m;
            exhaust.Transform = m;
		}

		Matrix CreateRotationMatrix(Vector3 p, Vector3 dir, float ang)
		{
			dir.Normalize();

			float cos = (float)Math.Cos(ang);
			float sin = (float)Math.Sin(ang);

			Matrix r = Matrix.Identity;
			r.M11 = dir.X * dir.X + (dir.Y * dir.Y + dir.Z * dir.Z) * cos;
			r.M21 = dir.X * dir.Y * (1 - cos) - dir.Z * sin;
			r.M31 = dir.X * dir.Z * (1 - cos) + dir.Y * sin;

			r.M12 = dir.X * dir.Y * (1 - cos) + dir.Z * sin;
			r.M22 = dir.Y * dir.Y + (dir.X * dir.X + dir.Z * dir.Z) * cos;
			r.M32 = dir.Y * dir.Z * (1 - cos) - dir.X * sin;

			r.M13 = dir.X * dir.Z * (1 - cos) - dir.Y * sin;
			r.M23 = dir.Y * dir.Z * (1 - cos) + dir.X * sin;
			r.M33 = dir.Z * dir.Z + (dir.X * dir.X + dir.Y * dir.Y) * cos;

			r.M41 = (p.X * (dir.Y * dir.Y + dir.Z * dir.Z) - dir.X * (p.Y * dir.Y + p.Z * dir.Z)) * (1 - cos) + (p.Y * dir.Z - p.Z * dir.Y) * sin;
			r.M42 = (p.Y * (dir.X * dir.X + dir.Z * dir.Z) - dir.Y * (p.X * dir.X + p.Z * dir.Z)) * (1 - cos) + (p.Z * dir.X - p.X * dir.Z) * sin;
			r.M43 = (p.Z * (dir.X * dir.X + dir.Y * dir.Y) - dir.Z * (p.X * dir.X + p.Y * dir.Y)) * (1 - cos) + (p.X * dir.Y - p.Y * dir.X) * sin;

			return r;
		}

        public void close() {
            plane.dispose();
            exhaust.dispose();
        }
    }
}
