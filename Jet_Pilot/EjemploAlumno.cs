using System;
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
using TgcViewer.Utils.Sound;
using Microsoft.DirectX.DirectSound;



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
        bool terreno_inicializado = false;

        //Estas variables son para manejar la lista de posiciones de nubes, que se genera a partir de la lista de centros de terreno
        List<Vector3> posiciones_centros_nubes;
        int ultimo_centro_de_posiciones_centros, avance_random;



        //Variables Previas Avion
        Vector3 CAM_DELTA = new Vector3(0, 50, 350);
        Plane player;
        FreeCam cam;
        bool avion_inicializado = false;

        Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

        //Variables del menu
        bool juego_iniciado = false;
        bool juego_pausado = true;
        private TgcSprite Imagen_Menu;
        private TgcText2d Texto_Start;
        private TgcText2d Texto_Titulo;
        private TgcText2d Texto_Score;
        private TgcMp3Player sound;
        private TgcMp3Player motor;

        //Varibles de mensajes por pantalla
        private TgcText2d Msj_Choque;
        private TgcText2d Msj_Triunfo;
        DateTime hora_choque;
        DateTime hora_trunfo;
        bool mostrar_msj = false;

        //Variables del modo cazar globos
        bool modo_capturar_globos = false;
        private float Score = 0;
        private TgcMesh Globo;
        private Vector3[] objetivos;
        private int cantidad_globos = 10;
        private bool BBAvion = false;
        private bool BBGlobos = false;

        //Variables Previas Skybox
        List<TgcMesh> nube;
        List<TgcMesh> meshes;
        Texture zBufferTexture;
        Microsoft.DirectX.Direct3D.Effect effect;
        Surface pOldRT;
        Skybox skyBox2;
        List<TgcMesh> nubes = new List<TgcMesh>();
        int anchoPantalla = GuiController.Instance.Panel3d.Width;
        int altoPantalla = GuiController.Instance.Panel3d.Height;
        bool skybox_inicializado = false;
        Random generador = new Random();
        List<Vector3> distancias;

        //Colisiones
        Colisionador colisionador;
        float[] altura_terrenos;
        private List<Vector3> centros_terrains_colisionables;





        //Metodos principales

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
        /// Método que se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            initGlobos();
            initScore();
            initMenu();
            initPlane();
            initTerrainAndClouds();
            initSkybox();
            initColisionador();
            initMsjs();
        }

        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        /// 
        public override void render(float elapsedTime)
        {
            if (juego_pausado)
            {
                renderMenu();
            }
            else
            {
                if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.P))
                {
                    juego_pausado = true;
                    motor.stop();
                    motor.closeFile();
                }
                else
                {

                    renderTerrainAndClouds(elapsedTime);

                    renderPlane(elapsedTime);
                    updateColision();
                    renderSkybox(elapsedTime);

                    if (sound.getStatus() != TgcMp3Player.States.Playing && motor.getStatus() != TgcMp3Player.States.Playing)
                    {
                        sound.closeFile();
                        motor = GuiController.Instance.Mp3Player;
                        motor.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                        "Jet_Pilot\\Sonido\\avion_3.mp3";
                        motor.play(true);
                    }

                    if (modo_capturar_globos)
                    {
                        render_score();
                        renderGlobos(elapsedTime);
                    }

                    if (mostrar_msj)
                    {
                        if (DateTime.Now.Subtract(hora_choque).Seconds <= 2)
                        {
                            Msj_Choque.render();
                        }
                        else
                        {

                            if (DateTime.Now.Subtract(hora_trunfo).Seconds <= 2)
                            {
                                Msj_Triunfo.render();
                            }
                            else
                            {
                                mostrar_msj = false;
                            }
                        }
                    }
                }
            }
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


        private void reset()
        {
            initPlane();
            posiciones_centros.Clear();
            initTerrainAndClouds();
            //meshes.Clear();
            initSkybox();
        }




        //Metodos para mostrar msjs por pantalla
        private void initMsjs()
        {

            Msj_Choque = new TgcText2d();
            Msj_Choque.Text = "Guarda con el terreno capo!!";
            Msj_Choque.Position = new Point((int)player.GetPosition().X, altoPantalla / 3);
            Msj_Choque.Color = Color.DarkRed;
            Msj_Choque.changeFont(new System.Drawing.Font("Cataclysmic", 30.0f));

            Msj_Triunfo = new TgcText2d();
            Msj_Triunfo.Text = "Felicitaciones has capturado todas las calaveras!!";
            Msj_Triunfo.Position = new Point((int)player.GetPosition().X, altoPantalla / 3);
            Msj_Triunfo.Color = Color.DarkViolet;
            Msj_Triunfo.changeFont(new System.Drawing.Font("Cataclysmic", 30.0f));
        }




        //Metodos para el menú y el marcador

        private void initMenu()
        {

            Imagen_Menu = new TgcSprite();
            Imagen_Menu.Texture =
            TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir +
                                     "Jet_Pilot\\Menu\\JP4.jpg");

            float factor_ancho = (float)anchoPantalla / (float)Imagen_Menu.Texture.Width;
            float factor_alto = (float)altoPantalla / (float)Imagen_Menu.Texture.Height;
            Imagen_Menu.Position = new Vector2(0, 0);
            Imagen_Menu.Scaling = new Vector2(factor_ancho, factor_alto);

            Texto_Start = new TgcText2d();
            Texto_Titulo = new TgcText2d();

            Texto_Start.Color = Color.Green;
            Texto_Titulo.Color = Color.LightGray;

            Texto_Start.Text = "Presione enter para iniciar";
            Texto_Titulo.Text = "JET PILOT";

            Texto_Start.changeFont(new System.Drawing.Font("Cataclysmic", 25.0f));
            Texto_Titulo.changeFont(new System.Drawing.Font("Chiller", 50.0f));

            Texto_Start.Size = new Size(0, 0);
            Texto_Titulo.Size = new Size(0, 0);

            Texto_Start.Position = new Point(anchoPantalla / 2 + Texto_Start.Text.Length / 2, altoPantalla * 9 / 10);
            Texto_Titulo.Position = new Point(anchoPantalla / 6, 0);

            sound = GuiController.Instance.Mp3Player;
            sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
            "Jet_Pilot\\Sonido\\102_prologue.mp3";
        }

        private void renderMenu()
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            Imagen_Menu.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
            Texto_Start.render();
            Texto_Titulo.render();

            if (sound.getStatus() != TgcMp3Player.States.Playing)
            {
                sound = GuiController.Instance.Mp3Player;
                sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                "Jet_Pilot\\Sonido\\102_prologue.mp3";
                sound.play(true);
            }

            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Return))
            {
                juego_pausado = false;
                sound.stop();
                sound.closeFile();
                motor = GuiController.Instance.Mp3Player;
                motor.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                "Jet_Pilot\\Sonido\\avion_3.mp3";
                motor.play(true);
                juego_iniciado = true;
            }

            if (juego_iniciado)
            {
                Texto_Start.Text = "Presione enter para reanudar";
                if (modo_capturar_globos)
                {
                    render_score();
                }
            }
        }

        private void initScore()
        {
            Texto_Score = new TgcText2d();
            Texto_Score.Color = Color.LightGreen;
            Texto_Score.changeFont(new System.Drawing.Font("Cataclysmic", 25.0f));
            Texto_Score.Size = new Size(0, 0);
            Texto_Score.Position = new Point(anchoPantalla * 9 / 10, 0);
        }

        private void render_score()
        {
            Texto_Score.Text = "Score:" + Score;
            Texto_Score.render();
        }




        //Metodos para el terreno

        private bool dist_menor_a_n_width(Vector3 pos_camara, Vector3 pos_espacio, int n)
        {

            Vector3 resultante = pos_espacio - pos_camara;
            float distancia = resultante.Length();

            return (distancia <= (width * n));

        }

        private bool dist_mayor_a_n_width(Vector3 pos_camara, Vector3 pos_espacio, int n)
        {
            Vector3 resultante = pos_espacio - pos_camara;
            float distancia = resultante.Length();

            return (distancia >= (width * n));

        }

        private void generar_puntos_alrededor(Vector3 posicion)
        {
            //Se utilizaran las variables
            //posiciones_centros
            //width
            //posicion
            //Las posicioes se numeran teniendo en cuenta que la posicion enviada por parametro seria "la posicion 5", es decir la central, en una grilla de 3x3

            //Tras generar nuevos puntos, se analiza si se deben agregar a la lista de centros de nubes a partir de un "randomizado"..

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
                    analisis_agregado_a_centros_nube(nueva_pos);
                }
            }
        }

        public void analisis_agregado_a_centros_nube(Vector3 nuevo_centro)
        {
            Boolean vertice_existente = new Boolean();
            vertice_existente = false;

            if (avance_random == 0)
            {
                foreach (Vector3 elem in posiciones_centros_nubes)
                {
                    if (elem.X == nuevo_centro.X && elem.Z == nuevo_centro.Z)
                    {
                        vertice_existente = true;
                        break;
                    }
                }
                if (!vertice_existente)
                {
                    Vector3 _1_ = new Vector3();
                    _1_.Z = nuevo_centro.Z;
                    _1_.X = nuevo_centro.X;
                    _1_.Y = nuevo_centro.Y + generador.Next(60000) + 5000;
                    posiciones_centros_nubes.Add(_1_);
                }
                avance_random = Convert.ToInt32(generador.Next(10)) + 70;
            }
            else
            {
                avance_random--;
            }

        }

        public void initTerrainAndClouds()
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


            currentScaleY = 5.3f;


            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "TerrainTexture.jpg";

            if (!terreno_inicializado)
            {
                GuiController.Instance.Modifiers.addTexture("texture", currentTexture);
            }



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


            //Seteo el primer valor random que se va a considerar como contador general..
            avance_random = Convert.ToInt32(generador.Next(5)) + 10;


            //Generar lista de posiciones de centros de terreno inicial
            Vector3 nuevo_punto;
            float inner_width = width;
            posiciones_centros = new List<Vector3>();
            posiciones_centros_nubes = new List<Vector3>();

            pos_original.X = pos_original.X - (width * 9);
            for (int i = 0; i < 18; i++)
            {

                nuevo_punto = new Vector3();
                nuevo_punto = pos_original;
                posiciones_centros.Add(nuevo_punto);


                for (int j = 0; j < 9; j++)
                {

                    nuevo_punto = new Vector3();
                    nuevo_punto = pos_original;
                    nuevo_punto.Z = pos_original.Z + inner_width;
                    posiciones_centros.Add(nuevo_punto);
                    analisis_agregado_a_centros_nube(nuevo_punto);

                    nuevo_punto = new Vector3();
                    nuevo_punto = pos_original;
                    nuevo_punto.Z = pos_original.Z - inner_width;
                    posiciones_centros.Add(nuevo_punto);
                    analisis_agregado_a_centros_nube(nuevo_punto);

                    inner_width = inner_width + width;
                }
                inner_width = width;
                pos_original.X = pos_original.X + width;
            }

            terreno_inicializado = true;
            
            //Generar lista de posiciones de centros de nubes inicial
            posiciones_centros_nubes = new List<Vector3>();
            for (int x = 0; x < posiciones_centros.Count; x = x + avance_random)
            {
                nuevo_punto = new Vector3();
                nuevo_punto.X = posiciones_centros[x].X;
                nuevo_punto.Y = posiciones_centros[x].Y + generador.Next(15000) + 6000;
                nuevo_punto.Z = posiciones_centros[x].Z;
                posiciones_centros_nubes.Add(nuevo_punto);
                avance_random = Convert.ToInt32(generador.Next(5)) + 40;//ERA 10---------------
            }
            

            //Carga de la nube
            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            //GuiController.Instance.CustomRenderEnabled = true;

            //Cargar shader de zbuffer
            //effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\EjemploGetZBuffer.fx");

            //Genero una instancia de loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //cargo la mesh de la nube
            nube = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "nube-TgcScene.xml").Meshes;

            //Posicionamiento de la nube
            Vector3 pos_mesh_sub_cero,distancia_al_mesh_sub_cero;
            pos_mesh_sub_cero = nube[0].Position;

            distancias=new List<Vector3>();

            distancias.Add(new Vector3(0, 0, 0));
            for (int i =1; i<nube.Count;i++) 
            {
                distancia_al_mesh_sub_cero = nube[i].Position - pos_mesh_sub_cero;
                distancias.Add(distancia_al_mesh_sub_cero);
            }
            
            int index=0;

            foreach (TgcMesh malla_individual in nube)
            {
                malla_individual.Position = new Vector3(0, 0, 0) + distancias[index];
                malla_individual.Scale = new Vector3(15, 15, 15);
                index++;
            }

            ////Seteo efecto zbuffer a la nube
            //nube.Effect = effect;
            
            /*
            //Posicionamiento de la nube
            nube.Position = new Vector3(0, 0, 0);
            nube.Scale = new Vector3(2, 2, 2);
*/
            //Seteo configuracion de la niebla
            GuiController.Instance.Fog.StartDistance = 1;
            GuiController.Instance.Fog.EndDistance = 1000;
            GuiController.Instance.Fog.Density = 1;
            GuiController.Instance.Fog.Color = Color.Gray;
            GuiController.Instance.Fog.updateValues();
        }


        public void renderTerrainAndClouds(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Calculos iniciales varios (Actualizacion de listas de centros)

            pos_actual = cam.getPosition();
            look_at_actual = cam.getLookAt();

            normal_actual = look_at_actual - pos_actual;

            //plano_vision = Plane.FromPointNormal(pos_actual, normal_actual);

            proy_pos_actual = pos_actual;
            proy_pos_actual.Y = 0;

            List<Vector3> a_borrar = new List<Vector3>();
            List<Vector3> a_revisar_para_generar = new List<Vector3>();


            //genero las posiciones de los centros que se requieran que se agreguen "a lo lejos"

            foreach (Vector3 posicion in posiciones_centros)
            {
                //Esta forma de averiguar que puntos estan delante de la camara funciona, pero no resultó performante, por lo que se reemplazo la condicion del if
                //if (esta_delante_del_plano(plano_vision, posicion))
                if (dist_menor_a_n_width(proy_pos_actual, posicion, 9))
                {
                    if (dist_mayor_a_n_width(proy_pos_actual, posicion, 7))
                    {
                        a_revisar_para_generar.Add(posicion);
                    }
                }
                else
                {
                    a_borrar.Add(posicion);
                }

            }


            //Borrado de centros de terreno alejados
            foreach (Vector3 posicion_a_borrar in a_borrar)
            {
                posiciones_centros.Remove(posicion_a_borrar);
            }


            a_borrar.Clear();

            //Generacion de nuevos centros de terreno. Esta accion tambien generara un nuevo centro de nube de forma random..
            foreach (Vector3 posicion_a_revisar in a_revisar_para_generar)
            {
                generar_puntos_alrededor(posicion_a_revisar);
            }

            //Borrado de centros de nubes alejados
            Vector3 proyeccion_posicion_nube = new Vector3();
            foreach (Vector3 posicion in posiciones_centros_nubes)
            {
                proyeccion_posicion_nube.X = posicion.X;
                proyeccion_posicion_nube.Y = 0;
                proyeccion_posicion_nube.Z = posicion.Z;
                if (dist_mayor_a_n_width(proy_pos_actual, proyeccion_posicion_nube, 10))
                {
                    a_borrar.Add(posicion);
                }

            }

            foreach (Vector3 posicion_a_borrar in a_borrar)
            {
                posiciones_centros_nubes.Remove(posicion_a_borrar);
            }

            ultimo_centro_de_posiciones_centros = 0;
            avance_random = 0;
            
            pos_actual = player.GetPosition();
            
            GuiController.Instance.Fog.Enabled = true;
            GuiController.Instance.Fog.updateValues();
            foreach (Vector3 centro_nube in posiciones_centros_nubes)
            {
                if (dist_menor_a_n_width(pos_actual, centro_nube, 8))
                {
                    GuiController.Instance.Fog.Enabled = false;
                    GuiController.Instance.Fog.updateValues();
                    render_nube(centro_nube);
                    GuiController.Instance.Fog.Enabled = true;
                    GuiController.Instance.Fog.updateValues();
                }
                else
                {
                    render_nube(centro_nube);
                }
            }
            GuiController.Instance.Fog.Enabled = false;
            GuiController.Instance.Fog.updateValues();


            ////Renderizado de nubes
            ////Guardar render target original
            //pOldRT = d3dDevice.GetRenderTarget(0);

            //// 1) Mandar a dibujar todos los mesh para que se genere la textura de ZBuffer
            //d3dDevice.BeginScene();

            ////Seteamos la textura de zBuffer como render target (en lugar de dibujar a la pantalla)
            //Surface zBufferSurface = zBufferTexture.GetSurfaceLevel(0);
            //d3dDevice.SetRenderTarget(0, zBufferSurface);
            //d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Green, 1.0f, 0);

            //ultimo_centro_de_posiciones_centros = 0;
            //avance_random = 0;

            ////Render de cada nube
            //nube.Technique = "GenerateZBuffer";
            //foreach (Vector3 centro_nube in posiciones_centros_nubes)
            //{

            //    nube.Position = centro_nube;
            //    nube.render();
            //}

            //zBufferSurface.Dispose();
            //d3dDevice.EndScene();



            //// 2) Volvemos a dibujar la escena y pasamos el ZBuffer al shader como una textura.
            //// Para este ejemplo particular utilizamos el valor de Z para alterar el color del pixel
            //d3dDevice.BeginScene();

            ////Restaurar render target original
            //d3dDevice.SetRenderTarget(0, pOldRT);
            //d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.GhostWhite, 1.0f, 0);

            ////Cargar textura de zBuffer al shader
            //effect.SetValue("texZBuffer", zBufferTexture);
            //effect.SetValue("screenDimensions", new float[] { d3dDevice.Viewport.Width, d3dDevice.Viewport.Height });


            ////Render de cada mesh
            //nube.Technique = "AlterColorByDepth";
            //foreach (Vector3 centro_nube in posiciones_centros_nubes)
            //{
            //    nube.Position = centro_nube;
            //    nube.render();
            //}

            //d3dDevice.EndScene();

            //renderizo terrenos de alta, media y baja calidad de acuerdo a la distancia a la que se encuentren de la proyeccion de la camara en el plano xz
           centros_terrains_colisionables.Clear();
           //int i = 0;
           altura_terrenos = new float[9];
            //Renderizado de terreno
            foreach (Vector3 posicion in posiciones_centros)
            {
                if (dist_menor_a_n_width(proy_pos_actual, posicion, 2))
                {
                    terrain_hq.render(posicion);
                    /*
                    if (i <= 8)
                    {
                        altura_terrenos.SetValue(posicion.Y, i);
                    }
                    i += 1;*/
                   centros_terrains_colisionables.Add(posicion);
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

        public void render_nube(Vector3 position)
        {
            Vector3 distancia_relativa=position-new Vector3(0,0,0);
            int index = 0;

            foreach (TgcMesh malla_individual in nube)
            {
                malla_individual.Position = distancia_relativa + distancias[index];
                index++;
                malla_individual.render();
            }
        }
        
        public void closeTerrain()
        {
            terrain_hq.dispose();
            terrain_mq.dispose();
            terrain_lq.dispose();
            terreno_inicializado = false;
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


            if (!avion_inicializado)
            {
                // Crear modifiers


                GuiController.Instance.Modifiers.addBoolean("Modo capturar calaveras", "Activado", false);
                GuiController.Instance.Modifiers.addBoolean("BoundingBox Avión", "Activado", false);
                GuiController.Instance.Modifiers.addBoolean("BoundingBox Calaveras", "Activado", false);
                GuiController.Instance.Modifiers.addInt("Cantidad de objetivos", 0, 100, 10);
                GuiController.Instance.Modifiers.addFloat("Velocidad de aceleración", 0, 1500.0f, 500);
                GuiController.Instance.Modifiers.addFloat("Velocidad de rotación", 0.5f, 1.0f, 0.6f);
                GuiController.Instance.Modifiers.addFloat("Velocidad de pitch", 1.0f, 3.0f, 2.0f);
                GuiController.Instance.Modifiers.addFloat("Velocidad de roll", 1.0f, 3.0f, 2.5f);

                // Crear UserVars
                GuiController.Instance.UserVars.addVar("Posición en X");
                GuiController.Instance.UserVars.addVar("Posición en Y");
                GuiController.Instance.UserVars.addVar("Posición en Z");

                //GuiController.Instance.UserVars.addVar("Avión respecto a X");
                //GuiController.Instance.UserVars.addVar("Avión respecto a Y");
                //GuiController.Instance.UserVars.addVar("Avión respecto a Z");
                GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-5000, -5000, -5000), new Vector3(5000, 8000, 5000), new Vector3(0, 4750, -2500));
            }
            
            avion_inicializado = true;
            ResetPlane();
        }

        void ResetPlane()
        {
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
            player.close();
            avion_inicializado = false;
        }

        /// <param name="dt">Tiempo desde la última ejecución</param>
        public void CheckInput(float dt)
        {


            // El menu se activa solo al chocar, y no se puede salir, solo resetear
            bool enter = GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Return);


            bool up = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.W);
            bool down = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.S);
            bool left = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A);
            bool right = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.D);

            bool plus = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Space);
            bool minus = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.RightControl);

            player.SetYoke(up, down, left, right);
            player.SetThrottle(plus, minus);

            // Check modifiers
            float turnSpeed = (float)GuiController.Instance.Modifiers["Velocidad de rotación"];
            float pitchSpeed = (float)GuiController.Instance.Modifiers["Velocidad de pitch"];
            float rollSpeed = (float)GuiController.Instance.Modifiers["Velocidad de roll"];
            float vel_acel = (float)GuiController.Instance.Modifiers["Velocidad de aceleración"];
            modo_capturar_globos = (bool)GuiController.Instance.Modifiers["Modo capturar calaveras"];
            int cantidad_actual = (int)GuiController.Instance.Modifiers["Cantidad de objetivos"];
            BBAvion = (bool)GuiController.Instance.Modifiers["BoundingBox Avión"];
            BBGlobos = (bool)GuiController.Instance.Modifiers["BoundingBox Calaveras"];

            if (cantidad_globos != cantidad_actual)
            {

                cantidad_globos = cantidad_actual;
                generarGlobos();

            }


            player.SetTurnSpeed(turnSpeed);
            player.SetPitchSpeed(pitchSpeed);
            player.SetRollSpeed(rollSpeed);
            player.SetVelocidad_aceleracion(vel_acel);

        }

        /// <param name="dt">Tiempo desde la última ejecución</param>
        public void UpdatePlane(float dt)
        {
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

            //GuiController.Instance.UserVars.setValue("Avión respecto a X", x);
            //GuiController.Instance.UserVars.setValue("Avión respecto a Y", y);
            //GuiController.Instance.UserVars.setValue("Avión respecto a Z", z);

            Vector3 camera;
            Vector3 target;

            camera = plane + CAM_DELTA.Y * y + CAM_DELTA.Z * z;
            target = plane + CAM_DELTA.Y * y;
            cam.SetCenterTargetUp(camera, target, y, true);
            cam.updateViewMatrix(d3dDevice);

        }

        /// <param name="dt">Tiempo desde la última ejecución</param>
        public void DrawPlane(float dt)
        {
            player.Render(BBAvion);
        }




        //colisionador

        private void initColisionador()
        {
            centros_terrains_colisionables = new List<Vector3>();
            colisionador = new Colisionador(terrain_hq, width, currentScaleY);
        }

        private void updateColision()
        {
            //hago colisionar el avion
            if (colisionador.colisionar(player.getMesh().BoundingBox, centros_terrains_colisionables))
            {
                mostrar_msj = true;
                hora_choque = DateTime.Now;
                motor.stop();
                motor.closeFile();
                sound = GuiController.Instance.Mp3Player;
                sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                "Jet_Pilot\\Sonido\\Motor_Apagandose.mp3";
                sound.play(false);
                mostrar_msj = true;
                reset();
            }
        }

        /*
        {
            altura_terrenos = new float[9];
        }
        *//*
        private void updateColision()
        {

            bool choca = false;

            foreach (float altura in altura_terrenos)
            {
                float umbral = 300;
                choca = false;

                Vector3 pos_avion = player.GetPosition();

                if ((pos_avion.Y - altura) <= umbral)
                {
                    choca = true;
                    break;
                }
            }

            if (choca)
            {
                motor.stop();
                motor.closeFile();
                sound = GuiController.Instance.Mp3Player;
                sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                "Jet_Pilot\\Sonido\\Motor_Apagandose.mp3";
                sound.play(false);
                mostrar_msj = true;
                hora_choque = DateTime.Now;
                reset();
                //System.Threading.Thread.Sleep(200);
            }

        }*/


        //Metodos para el Skybox

        public void initSkybox()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            //Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            // string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            /////////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            ////Camara en primera persona, tipo videojuego FPS
            ////Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //////Por default la camara FPS viene desactivada
            //GuiController.Instance.FpsCamera.Enable = true;
            ////Configurar posicion y hacia donde se mira
            //GuiController.Instance.FpsCamera.setCamera(new Vector3(anchoPantalla / 2, altoPantalla / 2, anchoPantalla / 2), new Vector3(0, 0, 0));

            // string avionPath = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\" + "AvionCaza-TgcScene.xml";

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;

            //Crear textura para almacenar el zBuffer. Es una textura que se usa como RenderTarget y que tiene un formato de 1 solo float de 32 bits.
            //En cada pixel no vamos a guardar un color sino el valor de Z de la escena
            //La creamos con un solo nivel de mipmap (el original)
            zBufferTexture = new Texture(d3dDevice, d3dDevice.Viewport.Width, d3dDevice.Viewport.Height, 1, Usage.RenderTarget, Format.R32F, Pool.Default);

            //Posicionar el avion
            //meshes[0].Position = new Vector3(anchoPantalla / 2, altoPantalla / 2, anchoPantalla / 2);


            ////Camara en tercera persona que apunta al avion
            //GuiController.Instance.ThirdPersonCamera.Enable = true;
            //GuiController.Instance.ThirdPersonCamera.setCamera(meshes[0].Position, 20.0f, 150.0f);

            skyBox2 = new Skybox();

            //Agrandamos la distancia del Far Plane para tener un skybox mas grande y que gracias a esto se dibuja y se ve
            d3dDevice.Transform.Projection = Matrix.PerspectiveFovLH(FastMath.ToRad(45.0f), 16 / 9, 10.0f, 50000.0f);

            ///////////////MODIFIERS//////////////////

            if (!skybox_inicializado)
            {
                ////Crear un modifier para un valor FLOAT
                GuiController.Instance.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

                ////Crear un modifier para un ComboBox con opciones
                string[] opciones = new string[] { "opcion1", "opcion2", "opcion3" };
                GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

                ////Crear un modifier para modificar un v�rtice
                GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-1000, -1000, -1000), new Vector3(5000, 5000, 5000), new Vector3((anchoPantalla / 2) - 20, (altoPantalla / 2) - 100, anchoPantalla / 2));
            }

            skybox_inicializado = true;
        }


        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el �ltimo frame</param>
        public void renderSkybox(float elapsedTime)
        {

            //Device de DirectX para renderizar
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            ////con esto la camara en 3ra persona sigue al avion y por ende el skybox lo acompa�a
            //GuiController.Instance.ThirdPersonCamera.setCamera(meshes[0].Position, 20.0f, 150.0f);


            //Obtener valores de Modifiers
            float valorFloat = (float)GuiController.Instance.Modifiers["valorFloat"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];

            //foreach (TgcMesh nub in nubes)
            //{
            // nub.render();
            //}

            // nubes[0].Position = valorVertice;

            //nubes[0].render();
            //auto.render();

            //nube.Position = valorVertice;

            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

            ////Capturar Input teclado
            //if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow))
            //{
            // //se mueve para atras
            // meshes[0].move(new Vector3(0, 0, 500 * elapsedTime));
            //}

            //if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.UpArrow))
            //{
            // //se mueve para adelante
            // meshes[0].move(new Vector3(0, 0, -500 * elapsedTime));

            //}

            //if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow))
            //{
            // //rota para el costado izquierdo
            // meshes[0].rotateY(0.3f * elapsedTime);
            // //meshes[0].moveOrientedY(50 * elapsedTime);
            // // GuiController.Instance.ThirdPersonCamera.rotateY(0.3f * elapsedTime);
            //}

            ////Capturar Input Mouse
            //if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            //{
            // //Boton izq apretado
            //}

            skyBox2.renderSkybox(player.GetPosition());
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
                foreach (TgcMesh mesh in nube)
                {
                    mesh.dispose();
                }
            }
            catch (Exception e)
            {

            }
            skybox_inicializado = false;

        }




        //Métodos para generación, renderizado, colisión y eliminación de globos

        public void initGlobos()
        { /*Cargar Mesh de objetivo*/
            String path = GuiController.Instance.ExamplesMediaDir + @"MeshCreator\Meshes\Esqueletos\Calabera\Calabera-TgcScene.xml";
            TgcSceneLoader loader = new TgcSceneLoader();
            Globo = loader.loadSceneFromFile(path).Meshes[0];
            Globo.Scale = new Vector3(3, 3, 3);
            generarGlobos();
        }

        public void renderGlobos(float elapsedTime)
        {/*Envía los Globos a renderizar*/

            Vector3 centro_globo;
            Vector3 centro_avion = player.Get_Center();
            Vector3 vector_centros;

            int i = 0;

            foreach (Vector3 pos in objetivos)
            {
                Globo.Position = pos;

                if (pos.Length() != 0)
                {

                    centro_globo = Globo.BoundingBox.calculateBoxCenter();

                    vector_centros = centro_globo - centro_avion;

                    if (vector_centros.Length() <= (player.Get_Radius() + Globo.BoundingBox.calculateBoxRadius())) //Verifico si colisiona el globo con el avión
                    {
                        quitarGlobo(i);
                        Score = Score + 1;
                        motor.stop();
                        motor.closeFile();
                        sound = GuiController.Instance.Mp3Player;
                        sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                        "Jet_Pilot\\Sonido\\Sonido_Captura1.mp3";
                        sound.play(false);

                        if (Score == cantidad_globos)
                        { //Si completé el nivel debo mostrar msj de felicitaciones
                            hora_trunfo = DateTime.Now;
                            mostrar_msj = true;
                            //sound = GuiController.Instance.Mp3Player;
                            sound.closeFile();
                            sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                            "Jet_Pilot\\Sonido\\Musica_Victoria.mp3";
                            sound.play(false);
                            Score = 0;
                        }

                    }
                    else //No colisiona, entonces lo dibujo
                    {
                        if (BBGlobos)
                        {
                            Globo.BoundingBox.render();
                        }
                        Globo.render();
                    }
                }

                i += 1;
            }
        }

        private void generarGlobos()
        {/*Crear vector de globos*/

            objetivos = new Vector3[cantidad_globos];
            objetivos.Initialize();

            float numberx;
            float numbery;
            float numberz;
            float signox;
            float signoz;

            for (int i = 0; i <= (cantidad_globos - 1); ++i)
            {
                numberx = generador.Next(10000);
                numbery = generador.Next(10000) + 1500; //Para que no toque el terreno
                numberz = generador.Next(10000);
                signox = generador.Next(100); //Para que me el signo de la componente en x (la función random sólo devuelve valores positivos)
                signoz = generador.Next(100); //Para que me el signo de la componente en z (la función random sólo devuelve valores positivos)

                if (signox >= 50)
                {
                    numberx = -numberx;
                }

                if (signoz >= 50)
                {
                    numberz = -numberz;
                }

                objetivos.SetValue(new Vector3(numberx, numbery, numberz), i);
            }
            Score = 0;
        }

        private void quitarGlobo(int index)
        {/*Elimino un globo ya capturado*/
            Array.Clear(objetivos, index, 1);
        }


    }


}
