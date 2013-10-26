﻿using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils._2D;
using TgcViewer.Utils;
using TgcViewer.Utils.Shaders;



namespace AlumnoEjemplos.Jet_Pilot
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class Jet_Pilot : TgcExample
    {


        //Variables Previas Terreno

        //Width es el ancho de referencia de cada seccion de terreno. Se inicializa en init
        float width, valor_grande;

        //Estas variables se utilizan para manejar el plano en el que se encuentra la cámara y tambien para medir distancias
        Vector3 pos_original, pos_actual, proy_pos_actual, look_at_actual, normal_actual;
        //Plane plano_vision;
        Vector3 punto_para_proy, punto_proy_en_pl, vector_final;

        //Muestras de terreno de alta, media y baja calidad        
        Terrain terrain_hq, terrain_mq, terrain_lq;

        //Esta es la lista de las posiciones que vana a ocupar los terrenos que posiblemente se renderizaran
        List<Vector3> posiciones_centros;

        string currentHeightmap_hq, currentHeightmap_mq, currentHeightmap_lq;
        string currentTexture;
        float ScaleXZ_hq, ScaleXZ_mq, ScaleXZ_lq;
        float currentScaleY;


        //Variables Previas Avion
        Vector3 CAM_DELTA = new Vector3(0, 50, 250);
        Plane player;
        FreeCam cam;
        bool gameOver;
        bool reset;



        //Variables Previas Skybox
        TgcMesh nube;
        List<TgcMesh> meshes;
        Texture zBufferTexture;
        Microsoft.DirectX.Direct3D.Effect effect;
        Surface pOldRT;
        Skybox skyBox2;
        List<TgcMesh> nubes = new List<TgcMesh>();
        int anchoPantalla = GuiController.Instance.Panel3d.Width;
        int altoPantalla = GuiController.Instance.Panel3d.Height;


        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Grupo Jet Pilot";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "Simulador de vuelo";
        }




        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {

            initPlane();
            initTerrain();
            initSkybox();


        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {

            renderPlane(elapsedTime);

            renderSkybox(elapsedTime);

            renderTerrain(elapsedTime);
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            closePlane();
            closeTerrain();
            closeSkybox();
        }












        //Metodos para el terreno      

        private bool dist_menor_a_n_width(Vector3 pos_camara, Vector3 pos_espacio, int n)
        {

            Vector3 resultante = pos_espacio - pos_camara;
            float distancia = resultante.Length();

            if (distancia <= (width * n))
            {
                return true;
            }

            return false;
        }

        private bool dist_mayor_a_n_width(Vector3 pos_camara, Vector3 pos_espacio, int n)
        {
            Vector3 resultante = pos_espacio - pos_camara;
            float distancia = resultante.Length();

            if (distancia >= (width * n))
            {
                return true;
            }

            return false;
        }

        private void generar_puntos_alrededor(Vector3 posicion)
        {
            //Se utilizaran las variables
            //posiciones_centros
            //width
            //posicion
            //Las posicioes se numeran teniendo en cuenta que la posicion enviada por parametro seria "la posicion 5", es decir la central, en una grilla de 3x3

            List<Vector3> posiciones_a_analizar = new List<Vector3>();

            Vector3 _1_, _2_, _3_, _4_, _6_, _7_, _8_, _9_;
            _1_ = new Vector3();
            _2_ = new Vector3();
            _3_ = new Vector3();
            _4_ = new Vector3();
            _6_ = new Vector3();
            _7_ = new Vector3();
            _8_ = new Vector3();
            _9_ = new Vector3();

            _1_.Z = posicion.Z - width;
            _1_.X = posicion.X + width;
            _1_.Y = posicion.Y;

            _2_.Z = posicion.Z;
            _2_.X = posicion.X + width;
            _2_.Y = posicion.Y;

            _3_.Z = posicion.Z + width;
            _3_.X = posicion.X + width;
            _3_.Y = posicion.Y;

            _4_.Z = posicion.Z - width;
            _4_.X = posicion.X;
            _4_.Y = posicion.Y;

            _6_.Z = posicion.Z + width;
            _6_.X = posicion.X;
            _6_.Y = posicion.Y;

            _7_.Z = posicion.Z - width;
            _7_.X = posicion.X - width;
            _7_.Y = posicion.Y;

            _8_.Z = posicion.Z;
            _8_.X = posicion.X - width;
            _8_.Y = posicion.Y;

            _9_.Z = posicion.Z + width;
            _9_.X = posicion.X - width;
            _9_.Y = posicion.Y;

            posiciones_a_analizar.Add(_1_);
            posiciones_a_analizar.Add(_2_);
            posiciones_a_analizar.Add(_3_);
            posiciones_a_analizar.Add(_4_);
            posiciones_a_analizar.Add(_6_);
            posiciones_a_analizar.Add(_7_);
            posiciones_a_analizar.Add(_8_);
            posiciones_a_analizar.Add(_9_);

            foreach (Vector3 nueva_pos in posiciones_a_analizar)
            {

                if (!posiciones_centros.Contains(nueva_pos))
                {
                    posiciones_centros.Add(nueva_pos);
                }
            }
        }


        public void initTerrain()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Path de Heightmap high quality del terreno
            currentHeightmap_hq = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "Heightmap_hq.jpg";


            //Path de Heightmap medium quality del terreno
            currentHeightmap_mq = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "Heightmap_mq.jpg";


            //Path de Heightmap low quality del terreno
            currentHeightmap_lq = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "Heightmap_lq.jpg";



            //Escala del mapa
            ScaleXZ_hq = 40f;
            ScaleXZ_mq = (ScaleXZ_hq * 2) + 0.7f;
            ScaleXZ_lq = (ScaleXZ_mq * 2) + 5f;


            currentScaleY = 1.3f;


            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "TerrainTexture.jpg";
            GuiController.Instance.Modifiers.addTexture("texture", currentTexture);


            //Carga terrenos de alta,media y baja definicion: cargar heightmap y textura de color

            Bitmap bitmap_hq = (Bitmap)Bitmap.FromFile(currentHeightmap_hq);

            Bitmap bitmap_mq = (Bitmap)Bitmap.FromFile(currentHeightmap_mq);

            Bitmap bitmap_lq = (Bitmap)Bitmap.FromFile(currentHeightmap_lq);

            //Este es el ancho de referencia gral
            width = ((bitmap_hq.Size.Width * ScaleXZ_hq) - 85);

            //No se van a renderizar mas de 5 terrenos"hacia adelante". Se utiliza para hallar intersecciones con el plano
            valor_grande = 5 * width;

            terrain_hq = new Terrain();
            terrain_hq.loadHeightmap(currentHeightmap_hq, ScaleXZ_hq, currentScaleY, new Vector3(0, 0, 0));
            terrain_hq.loadTexture(currentTexture);


            terrain_mq = new Terrain();
            terrain_mq.loadHeightmap(currentHeightmap_mq, ScaleXZ_mq, currentScaleY, new Vector3(0, 0, 0));
            terrain_mq.loadTexture(currentTexture);


            terrain_lq = new Terrain();
            terrain_lq.loadHeightmap(currentHeightmap_lq, ScaleXZ_lq, currentScaleY, new Vector3(0, 0, 0));
            terrain_lq.loadTexture(currentTexture);



            //Hay que llamar primero a initPlane para que esto funcione correctamente

            pos_original = cam.getPosition();
            pos_original.Y = 0;

            //Generar lista de posiciones inicial
            Vector3 nuevo_punto;
            float inner_width = width;
            posiciones_centros = new List<Vector3>();

            pos_original.X = pos_original.X - (width * 4);
            for (int i = 0; i < 9; i++)
            {

                nuevo_punto = new Vector3();
                nuevo_punto = pos_original;
                posiciones_centros.Add(nuevo_punto);


                for (int j = 0; j < 4; j++)
                {

                    nuevo_punto = new Vector3();
                    nuevo_punto = pos_original;
                    nuevo_punto.Z = pos_original.Z + inner_width;
                    posiciones_centros.Add(nuevo_punto);

                    nuevo_punto = new Vector3();
                    nuevo_punto = pos_original;
                    nuevo_punto.Z = pos_original.Z - inner_width;
                    posiciones_centros.Add(nuevo_punto);

                    inner_width = inner_width + width;
                }
                inner_width = width;
                pos_original.X = pos_original.X + width;
            }

        }


        public void renderTerrain(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar terreno

            pos_actual = cam.getPosition();
            look_at_actual = cam.getLookAt();

            normal_actual = look_at_actual - pos_actual;

            //plano_vision = Plane.FromPointNormal(pos_actual, normal_actual);

            proy_pos_actual = pos_actual;
            proy_pos_actual.Y = 0;

            List<Vector3> a_borrar = new List<Vector3>();
            List<Vector3> a_revisar_para_generar = new List<Vector3>();


            //genero las posiciones de los centros que se requieran que aparezcan "a lo lejos"
            foreach (Vector3 posicion in posiciones_centros)
            {
                //Esta forma de averiguar que puntos estan delante de la camara funciona, pero no resultó performante, por lo que se reemplazo la condicion del if
                //if (esta_delante_del_plano(plano_vision, posicion))
                if (dist_menor_a_n_width(proy_pos_actual, posicion, 5))
                {
                    if (dist_mayor_a_n_width(proy_pos_actual, posicion, 3))
                    {
                        a_revisar_para_generar.Add(posicion);
                    }
                }
                else
                {
                    a_borrar.Add(posicion);
                }
            }

            foreach (Vector3 posicion_a_revisar in a_revisar_para_generar)
            {
                generar_puntos_alrededor(posicion_a_revisar);
            }

            foreach (Vector3 posicion_a_borrar in a_borrar)
            {
                posiciones_centros.Remove(posicion_a_borrar);
            }


            //renderizo terrenos de alta, media y baja calidad de acuerdo a la distancia a la que se encuentren de la proyeccion de la camara en el plano xz

            foreach (Vector3 posicion in posiciones_centros)
            {
                if (dist_menor_a_n_width(proy_pos_actual, posicion, 2))
                {
                    terrain_hq.render(posicion);
                    //terrain_hq.render();
                }
                else
                {
                    if (dist_menor_a_n_width(proy_pos_actual, posicion, 4))
                    {
                        terrain_mq.render(posicion);
                        //terrain_mq.render();
                    }
                    else
                    {
                        terrain_lq.render(posicion);
                        //terrain_lq.render();
                    }

                }
            }


        }

        public void closeTerrain()
        {
            terrain_hq.dispose();
            terrain_mq.dispose();
            terrain_lq.dispose();
        }





        //Metodos para el avion

        public void initPlane()
        {

            // Crear avion del jugador
            player = new Plane();

            // Crear la cámara
            cam = new FreeCam();
            cam.SetCenterTargetUp(CAM_DELTA, new Vector3(0, 0, 0), new Vector3(0, 1, 0), true);
            cam.Enable = true;
            GuiController.Instance.CurrentCamera = cam;

            GuiController.Instance.Modifiers.addFloat("Velocidad de rotación", 0.5f, 1.0f, 0.6f);
            GuiController.Instance.Modifiers.addFloat("Velocidad de pitch", 1.0f, 3.0f, 2.0f);
            GuiController.Instance.Modifiers.addFloat("Velocidad de roll", 1.0f, 3.0f, 2.5f);

            GuiController.Instance.UserVars.addVar("Posición en X");
            GuiController.Instance.UserVars.addVar("Posición en Y");
            GuiController.Instance.UserVars.addVar("Posición en Z");

            GuiController.Instance.UserVars.addVar("Avión respecto a X");
            GuiController.Instance.UserVars.addVar("Avión respecto a Y");
            GuiController.Instance.UserVars.addVar("Avión respecto a Z");

            ResetPlane();
        }


        void ResetPlane()
        {
            gameOver = false;

            reset = false;

            player.Reset();

            // Extraigo los ejes del avion de su matriz transformación
            Vector3 plane = player.GetPosition();
            Vector3 z = player.ZAxis();
            Vector3 y = player.YAxis();

            // Seteo la cámara en función de la posición del avion
            Vector3 camera = plane + CAM_DELTA.Y * y + CAM_DELTA.Z * z;
            Vector3 target = plane + CAM_DELTA.Y * y;
            cam.SetCenterTargetUp(camera, target, y, true);

        }


        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public void renderPlane(float elapsedTime)
        {
            // Este método registra el input del jugador
            CheckInput(elapsedTime);

            // Acá esta la lógica de juego
            UpdatePlane(elapsedTime);

            // Acá se hace el verdadero render, se dibuja la pantalla
            DrawPlane(elapsedTime);
        }

        public void closePlane()
        {
        }

        /// <param name="dt">Tiempo desde la última ejecución</param>
        public void CheckInput(float dt)
        {
            if (!gameOver)
            {
                // El menu se activa solo al chocar, y no se puede salir, solo resetear
                bool enter = GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Return);
            }

            bool up = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.UpArrow);
            bool down = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow);
            bool left = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow);
            bool right = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.RightArrow);

            bool plus = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Q);
            bool minus = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A);

            player.SetYoke(up, down, left, right);
            player.SetThrottle(plus, minus);

            bool shoot = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Space);

            if (shoot)
            {
                Vector3 posIni = player.GetPosition();
                Vector3 z = -player.ZAxis();
            }



            // Check modifiers
            float turnSpeed = (float)GuiController.Instance.Modifiers["Velocidad de rotación"];
            float pitchSpeed = (float)GuiController.Instance.Modifiers["Velocidad de pitch"];
            float rollSpeed = (float)GuiController.Instance.Modifiers["Velocidad de roll"];
            player.SetTurnSpeed(turnSpeed);
            player.SetPitchSpeed(pitchSpeed);
            player.SetRollSpeed(rollSpeed);

        }


        /// <param name="dt">Tiempo desde la última ejecución</param>
        public void UpdatePlane(float dt)
        {
            // Comando de resetear el juego
            if (reset)
            {
                ResetPlane();
                return;
            }

            // Control de saltos en casos de bajo rendimiento y pérdida del foco
            if (dt > 0.1f) dt = 0.1f;

            player.Update(dt);

            // Extraigo los ejes del avion de su matriz transformación
            Vector3 plane = player.GetPosition();
            Vector3 z = player.ZAxis();
            Vector3 y = player.YAxis();
            Vector3 x = player.XAxis();

            GuiController.Instance.UserVars.setValue("Posición en X", plane.X);
            GuiController.Instance.UserVars.setValue("Posición en Y", plane.Y);
            GuiController.Instance.UserVars.setValue("Posición en Z", plane.Z);

            GuiController.Instance.UserVars.setValue("Avión respecto a X", x);
            GuiController.Instance.UserVars.setValue("Avión respecto a Y", y);
            GuiController.Instance.UserVars.setValue("Avión respecto a Z", z);

            Vector3 camera;
            Vector3 target;

            camera = plane + CAM_DELTA.Y * y + CAM_DELTA.Z * z;
            target = plane + CAM_DELTA.Y * y;
            cam.SetCenterTargetUp(camera, target, y);

        }

        /// <param name="dt">Tiempo desde la última ejecución</param>
        public void DrawPlane(float dt)
        {
            player.Render();
        }





        //Metodos para el Skybox

        public void initSkybox()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            //   string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            /////////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            ////Camara en primera persona, tipo videojuego FPS
            ////Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //////Por default la camara FPS viene desactivada
            //GuiController.Instance.FpsCamera.Enable = true;
            ////Configurar posicion y hacia donde se mira
            //GuiController.Instance.FpsCamera.setCamera(new Vector3(anchoPantalla / 2, altoPantalla / 2, anchoPantalla / 2), new Vector3(0, 0, 0));

            //  string avionPath = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\" + "AvionCaza-TgcScene.xml";

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;

            //Cargar shader de zbuffer
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\EjemploGetZBuffer.fx");

            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            // TgcScene scene = loader.loadSceneFromFile(avionPath);

            //cargo la mesh de la nube
            nube = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "nube-TgcScene.xml").Meshes[0];

            meshes = new List<TgcMesh>();
            meshes.Add(player.getMesh());
            meshes.Add(nube);

            for (int i = 0; i < 8; i++)
            {
                nubes.Add(nube);
            }
            meshes.AddRange(nubes);

            for (int i = 1; i < meshes.Count; i++)
            {
                meshes[i].Effect = effect;
            }

            //Le setea a todos los meshes de la scene el efecto de zbuffer
            //foreach (TgcMesh mesh in meshes)
            //{
            //    mesh.Effect = effect;
            //}

            //Crear textura para almacenar el zBuffer. Es una textura que se usa como RenderTarget y que tiene un formato de 1 solo float de 32 bits.
            //En cada pixel no vamos a guardar un color sino el valor de Z de la escena
            //La creamos con un solo nivel de mipmap (el original)
            zBufferTexture = new Texture(d3dDevice, d3dDevice.Viewport.Width, d3dDevice.Viewport.Height, 1, Usage.RenderTarget, Format.R32F, Pool.Default);

            //Posicionar el avion
            meshes[0].Position = new Vector3(anchoPantalla / 2, altoPantalla / 2, anchoPantalla / 2);
            nube.Position = meshes[0].Position + new Vector3(10, 0, 0);

            ////Camara en tercera persona que apunta al avion
            //GuiController.Instance.ThirdPersonCamera.Enable = true;
            //GuiController.Instance.ThirdPersonCamera.setCamera(meshes[0].Position, 20.0f, 150.0f);

            skyBox2 = new Skybox();

            //Agrandamos la distancia del Far Plane para tener un skybox mas grande y que gracias a esto se dibuja y se ve 
            d3dDevice.Transform.Projection = Matrix.PerspectiveFovLH(FastMath.ToRad(45.0f), 16 / 9, 10.0f, 50000.0f);

            ///////////////MODIFIERS//////////////////

            ////Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            ////Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[] { "opcion1", "opcion2", "opcion3" };
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            ////Crear un modifier para modificar un v�rtice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-1000, -1000, -1000), new Vector3(5000, 5000, 5000), new Vector3((anchoPantalla / 2) - 20, (altoPantalla / 2) - 100, anchoPantalla / 2));

        }



        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el �ltimo frame</param>
        public void renderSkybox(float elapsedTime)
        {

            //Device de DirectX para renderizar
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            ////con esto la camara en 3ra persona sigue al avion y por ende el skybox lo acompa�a
            //GuiController.Instance.ThirdPersonCamera.setCamera(meshes[0].Position, 20.0f, 150.0f);

            //Guardar render target original
            pOldRT = d3dDevice.GetRenderTarget(0);

            // 1) Mandar a dibujar todos los mesh para que se genere la textura de ZBuffer
            d3dDevice.BeginScene();

            //Seteamos la textura de zBuffer como render  target (en lugar de dibujar a la pantalla)
            Surface zBufferSurface = zBufferTexture.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, zBufferSurface);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Green, 1.0f, 0);

            //Render de cada mesh
            for (int i = 1; i < meshes.Count; i++)
            {
                meshes[i].Technique = "GenerateZBuffer";
                meshes[i].render();
            }

            //foreach (TgcMesh mesh in meshes)
            //{
            //    mesh.Technique = "GenerateZBuffer";
            //    mesh.render();
            //}

            zBufferSurface.Dispose();
            d3dDevice.EndScene();



            // 2) Volvemos a dibujar la escena y pasamos el ZBuffer al shader como una textura.
            // Para este ejemplo particular utilizamos el valor de Z para alterar el color del pixel
            d3dDevice.BeginScene();

            //Restaurar render target original
            d3dDevice.SetRenderTarget(0, pOldRT);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.GhostWhite, 1.0f, 0);

            //Cargar textura de zBuffer al shader
            effect.SetValue("texZBuffer", zBufferTexture);
            effect.SetValue("screenDimensions", new float[] { d3dDevice.Viewport.Width, d3dDevice.Viewport.Height });


            //Render de cada mesh
            for (int i = 1; i < meshes.Count; i++)
            {
                meshes[i].Technique = "AlterColorByDepth";
                meshes[i].render();
            }

            ////Render de cada mesh
            //foreach (TgcMesh mesh in meshes)
            //{
            //    mesh.Technique = "AlterColorByDepth";
            //    mesh.render();
            //}

            d3dDevice.EndScene();


            foreach (TgcMesh mesh in meshes)
            {
                mesh.render();
            }


            //Obtener valores de Modifiers
            float valorFloat = (float)GuiController.Instance.Modifiers["valorFloat"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];

            //foreach (TgcMesh nub in nubes)
            //{
            //    nub.render();
            //}

            //    nubes[0].Position = valorVertice;

            //nubes[0].render();
            //auto.render();

            nube.Position = valorVertice;

            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

            ////Capturar Input teclado 
            //if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow))
            //{
            //    //se mueve para atras
            //    meshes[0].move(new Vector3(0, 0, 500 * elapsedTime));
            //}

            //if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.UpArrow))
            //{
            //    //se mueve para adelante
            //    meshes[0].move(new Vector3(0, 0, -500 * elapsedTime));

            //}

            //if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow))
            //{
            //    //rota para el costado izquierdo
            //    meshes[0].rotateY(0.3f * elapsedTime);
            //    //meshes[0].moveOrientedY(50 * elapsedTime);
            //    //    GuiController.Instance.ThirdPersonCamera.rotateY(0.3f * elapsedTime);
            //}

            ////Capturar Input Mouse
            //if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            //{
            //    //Boton izq apretado               
            //}

            skyBox2.renderSkybox(meshes[0].Position);
            //skyBox2.Render();

            //Mostrar FPS
            d3dDevice.BeginScene();
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
            d3dDevice.EndScene();

        }


        public void closeSkybox()
        {
            try
            {
                pOldRT.Dispose();
                zBufferTexture.Dispose();
                skyBox2.dispose();
                nube.dispose();
                foreach (TgcMesh mesh in meshes)
                {
                    mesh.dispose();
                }
            }
            catch (Exception e)
            {

            }

        }

    }

}
