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
        List<Terrain> hq_terrains = new List<Terrain>();
        List<Terrain> mq_terrains = new List<Terrain>();
        List<Terrain> lq_terrains = new List<Terrain>();

        List<string> texturas = new List<string>();
        //Terrain terrain_hq, terrain_mq, terrain_lq;

        //Esta es la lista de las posiciones que van a ocupar los terrenos que posiblemente se renderizaran
        enum textures {sea, sea_to_sand, sand, sand_to_terrain, terrain}
        
        public struct tipo_posicion_con_index_heightmap {
            public Vector3 posicion_centro;
            public int index;

            public void cargar_index(float ancho)
            {
                if (this.posicion_centro.Z < 0)
                {
                    this.index = (int)textures.sea;
                }
                else if (this.posicion_centro.Z == 0)
                {
                    this.index = (int)textures.sea_to_sand;
                }
                else if ((this.posicion_centro.Z > 0) && (this.posicion_centro.Z < 4 * ancho))
                {
                    this.index = (int)textures.sand;
                }
                else if (this.posicion_centro.Z == 4*ancho)
                {
                    this.index = (int)textures.sand_to_terrain;
                }
                else if (this.posicion_centro.Z > 4 * ancho)
                {
                    this.index = (int)textures.terrain;
                }
            }
        }
        
        List<tipo_posicion_con_index_heightmap> posiciones_centros;
        List<tipo_posicion_con_index_heightmap> posiciones_centros_iniciales;

        string currentHeightmap_hq, currentHeightmap_mq, currentHeightmap_lq;
        string current_sea_texture, current_sea_to_sand_texture, current_sand_texture, current_sand_to_terrain_texture, current_terrain_texture;
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
        private TgcText2d Msj_Advertencia;
        DateTime hora_choque;
        DateTime hora_trunfo;
        DateTime hora_advertencia;
        bool mostrar_msj_choque = false;
        bool mostrar_msj_advertencia = false;
        bool mostrar_msj_triunfo = false;

        //Variables del modo cazar globos
        bool modo_capturar_globos = false;
        private float Score = 0;
        private TgcMesh Globo;
        private Vector3[] objetivos;
        private int cantidad_globos = 10;
        private bool BBAvion = false;
        private bool BBGlobos = false;

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
        bool skybox_inicializado = false;
        Random generador = new Random();

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


                    if (mostrar_msj_choque)
                    {
                        if (DateTime.Now.Subtract(hora_choque).Seconds <= 2)
                        {
                            Msj_Choque.render();
                        }
                        else {
                            mostrar_msj_choque = false;
                        }
                    }

                    if (mostrar_msj_triunfo){
                        if (DateTime.Now.Subtract(hora_trunfo).Seconds <= 2)
                        {
                            Msj_Triunfo.render();
                        }
                        else {
                            mostrar_msj_triunfo = false;
                        }
                    }

                    if (mostrar_msj_advertencia){

                        if (DateTime.Now.Subtract(hora_advertencia).Seconds <= 2)
                        {
                            Msj_Advertencia.render();
                        }
                        else {
                            mostrar_msj_advertencia = false;
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
            //posiciones_centros.Clear();
            //initTerrainAndClouds();
            //meshes.Clear();
            posiciones_centros = posiciones_centros_iniciales;
            initSkybox();
        }




        //Metodos para mostrar msjs por pantalla
        private void initMsjs()
        {

            Msj_Choque = new TgcText2d();
            Msj_Choque.Text = "Cuidado con el terreno!!";
            Msj_Choque.Position = new Point((int)player.GetPosition().X, altoPantalla / 3);
            Msj_Choque.Color = Color.DarkRed;
            Msj_Choque.changeFont(new System.Drawing.Font("Cataclysmic", 30.0f));

            Msj_Triunfo = new TgcText2d();
            Msj_Triunfo.Text = "Felicitaciones has capturado todas las calaveras!!";
            Msj_Triunfo.Position = new Point((int)player.GetPosition().X, altoPantalla / 3);
            Msj_Triunfo.Color = Color.DarkViolet;
            Msj_Triunfo.changeFont(new System.Drawing.Font("Cataclysmic", 30.0f));

            Msj_Advertencia = new TgcText2d();
            Msj_Advertencia.Text = "No vayas tan alto, tu techo de vuelo es de 20.000 m!";
            Msj_Advertencia.Position = new Point((int)player.GetPosition().X, altoPantalla / 3);
            Msj_Advertencia.Color = Color.DarkRed;
            Msj_Advertencia.changeFont(new System.Drawing.Font("Cataclysmic", 30.0f));
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



            Boolean ya_existe_el_centro=false;
            foreach (Vector3 nueva_pos in posiciones_a_analizar)
            {

                foreach ( tipo_posicion_con_index_heightmap pos_centro in posiciones_centros)
                {

                    if (pos_centro.posicion_centro == nueva_pos) 
                    {
                        ya_existe_el_centro = true;
                        break;
                    }
                }
                if (!ya_existe_el_centro)
                {
                    tipo_posicion_con_index_heightmap nuevo_centro = new tipo_posicion_con_index_heightmap();
                    nuevo_centro.posicion_centro = nueva_pos;
                    nuevo_centro.cargar_index(width);
                    posiciones_centros.Add(nuevo_centro);
                    analisis_agregado_a_centros_nube(nueva_pos);
                }
                else
                {
                    ya_existe_el_centro = false;
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
                    _1_.Y = nuevo_centro.Y + generador.Next(45000) + 6000;
                    posiciones_centros_nubes.Add(_1_);
                }
                avance_random = Convert.ToInt32(generador.Next(10)) + 70;
            }
            else
            {
                avance_random--;
            }

        }

        //Si se quiere cargar la lista de terrenos hq, se manda la lista hq_terrain y el heightmap hq
        public void agregar_terrenos_en_lista_de_calidad(List<Terrain> lista, string heightmap, float escalaXZ, float escalaY)
        {
            Terrain Loader;

            foreach (string textura in texturas)
            {
                Loader = new Terrain();
                Loader.loadHeightmap(heightmap, escalaXZ, escalaY, new Vector3(0, 0, 0));
                Loader.loadTexture(textura);
                lista.Add(Loader);
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
            ScaleXZ_hq = 160f;
            ScaleXZ_mq = (ScaleXZ_hq * 2) + 0.7f;
            ScaleXZ_lq = (ScaleXZ_mq * 2) + 5f;


            currentScaleY = 3f;


            //Path de Textura default del terreno y Modifier para cambiarla
//            currentTexture = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "TerrainTexture.jpg";

            current_sea_texture = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "sea.jpg";
            current_sea_to_sand_texture = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "sea_to_sand.jpg";
            current_sand_texture = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "sand.jpg";
            current_sand_to_terrain_texture= GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "sand_to_terrain.jpg";
            current_terrain_texture = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "terrain.jpg";
            
            //Cargo las texturas en una lista para poder inicializar los terrenos de forma mas sencilla
            texturas.Add(current_sea_texture);
            texturas.Add(current_sea_to_sand_texture);
            texturas.Add(current_sand_texture);
            texturas.Add(current_sand_to_terrain_texture);
            texturas.Add(current_terrain_texture);


            if (!terreno_inicializado)
            {
                GuiController.Instance.Modifiers.addTexture("texture", current_terrain_texture);
            }



            //Carga terrenos de alta,media y baja definicion: cargar heightmap y textura de color

            Bitmap bitmap_hq = (Bitmap)Bitmap.FromFile(currentHeightmap_hq);

            Bitmap bitmap_mq = (Bitmap)Bitmap.FromFile(currentHeightmap_mq);

            Bitmap bitmap_lq = (Bitmap)Bitmap.FromFile(currentHeightmap_lq);

            //Este es el ancho de referencia gral
            width = ((bitmap_hq.Size.Width * ScaleXZ_hq) - 370);

            //No se van a renderizar mas de 5 terrenos"hacia adelante". Se utiliza para hallar intersecciones con el plano
            //valor_grande = 5 * width;

            
            agregar_terrenos_en_lista_de_calidad(hq_terrains,currentHeightmap_hq,ScaleXZ_hq,currentScaleY);

            agregar_terrenos_en_lista_de_calidad(mq_terrains, currentHeightmap_mq, ScaleXZ_mq, currentScaleY);

            agregar_terrenos_en_lista_de_calidad(lq_terrains, currentHeightmap_lq, ScaleXZ_lq, currentScaleY);


            //Hay que llamar primero a initPlane para que esto funcione correctamente

            pos_original = cam.getPosition();
            pos_original.Y = 0;


            //Seteo el primer valor random que se va a considerar como contador general..
            avance_random = Convert.ToInt32(generador.Next(5)) + 10;


            //Generar lista de posiciones de centros de terreno inicial
            Vector3 nuevo_punto;
            float inner_width = width;
            posiciones_centros = new List<tipo_posicion_con_index_heightmap>();
            posiciones_centros_iniciales = new List<tipo_posicion_con_index_heightmap>();
            posiciones_centros_nubes = new List<Vector3>();

            pos_original.X = pos_original.X - (width * 9);
            for (int i = 0; i < 18; i++)
            {

                nuevo_punto = new Vector3();
                nuevo_punto = pos_original;
                tipo_posicion_con_index_heightmap nuevo_centro = new tipo_posicion_con_index_heightmap();
                nuevo_centro.posicion_centro = nuevo_punto;
                nuevo_centro.cargar_index(width);
                posiciones_centros.Add(nuevo_centro);
                posiciones_centros_iniciales.Add(nuevo_centro);
                analisis_agregado_a_centros_nube(nuevo_punto);
                


                for (int j = 0; j < 9; j++)
                {

                    nuevo_punto = new Vector3();
                    nuevo_punto = pos_original;
                    nuevo_punto.Z = pos_original.Z + inner_width;
                    nuevo_centro = new tipo_posicion_con_index_heightmap();
                    nuevo_centro.posicion_centro = nuevo_punto;
                    nuevo_centro.cargar_index(width);
                    posiciones_centros.Add(nuevo_centro);
                    posiciones_centros_iniciales.Add(nuevo_centro);
                    analisis_agregado_a_centros_nube(nuevo_punto);

                    nuevo_punto = new Vector3();
                    nuevo_punto = pos_original;
                    nuevo_punto.Z = pos_original.Z - inner_width;
                    nuevo_centro = new tipo_posicion_con_index_heightmap();
                    nuevo_centro.posicion_centro = nuevo_punto;
                    nuevo_centro.cargar_index(width);
                    posiciones_centros.Add(nuevo_centro);
                    posiciones_centros_iniciales.Add(nuevo_centro);
                    analisis_agregado_a_centros_nube(nuevo_punto);

                    inner_width = inner_width + width;
                }
                inner_width = width;
                pos_original.X = pos_original.X + width;
            }

            //foreach(tipo_posicion_con_index_heightmap posicion in posiciones_centros){
            //    posiciones_centros_iniciales.Add(posicion);
            //}

            terreno_inicializado = true;

            //Generar lista de posiciones de centros de nubes inicial
            posiciones_centros_nubes = new List<Vector3>();
            for (int x = 0; x < posiciones_centros.Count; x = x + avance_random)
            {
                nuevo_punto = new Vector3();
                nuevo_punto.X = posiciones_centros[x].posicion_centro.X;
                nuevo_punto.Y = posiciones_centros[x].posicion_centro.Y + generador.Next(45000) + 6000;
                nuevo_punto.Z = posiciones_centros[x].posicion_centro.Z;
                posiciones_centros_nubes.Add(nuevo_punto);
                avance_random = Convert.ToInt32(generador.Next(5)) + 70;//ERA 10---------------
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
            nube = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "nube-TgcScene.xml").Meshes[0];

            ////Seteo efecto zbuffer a la nube
            //nube.Effect = effect;

            //Posicionamiento de la nube
            nube.Position = new Vector3(0, 0, 0);
            nube.Scale = new Vector3(7, 7, 7);

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

            GuiController.Instance.ThirdPersonCamera.setCamera(cam.getPosition(), 0,0);
            
            normal_actual = look_at_actual - pos_actual;

            //plano_vision = Plane.FromPointNormal(pos_actual, normal_actual);

            proy_pos_actual = pos_actual;
            proy_pos_actual.Y = 0;

            List<tipo_posicion_con_index_heightmap> centros_terreno_aux = new List<tipo_posicion_con_index_heightmap>();
            List<Vector3> a_revisar_para_generar = new List<Vector3>();
            List<Vector3> aux = new List<Vector3>();


            //genero las posiciones de los centros que se requieran que se agreguen "a lo lejos"

            for(int i = 0; i < posiciones_centros.Count; i++)
            //foreach (tipo_posicion_con_index_heightmap posicion in posiciones_centros)
            {
                tipo_posicion_con_index_heightmap posicion = posiciones_centros[i];
                //Esta forma de averiguar que puntos estan delante de la camara funciona, pero no resultó performante, por lo que se reemplazo la condicion del if
                //if (esta_delante_del_plano(plano_vision, posicion))
                if (dist_menor_a_n_width(proy_pos_actual, posicion.posicion_centro, 9))
                {
                    if (dist_mayor_a_n_width(proy_pos_actual, posicion.posicion_centro, 7))
                    {
                        a_revisar_para_generar.Add(posicion.posicion_centro);
                    }
                    centros_terreno_aux.Add(posicion);
                }
            }

            //piso los centros qeu ya estaban, con el array que tiene los que quedan
            posiciones_centros = centros_terreno_aux;
            /*
            //Borrado de centros de terreno alejados
            foreach (int posicion_a_borrar in centros_terreno_a_borrar)
            {
                posiciones_centros.Remove(posicion_a_borrar);
            }*/


            //centros_terreno_a_borrar.Clear();

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
                if (!dist_mayor_a_n_width(proy_pos_actual, proyeccion_posicion_nube, 10))
                {
                    aux.Add(posicion);
                }

            }
            posiciones_centros_nubes = aux;
            
            /*
            foreach (Vector3 posicion_a_borrar in aux)
            {
                posiciones_centros_nubes.Remove(posicion_a_borrar);
            }
            */
            ultimo_centro_de_posiciones_centros = 0;
            avance_random = 0;

            GuiController.Instance.Fog.Enabled = true;
            GuiController.Instance.Fog.updateValues();
            foreach (Vector3 centro_nube in posiciones_centros_nubes)
            {
                nube.Position = centro_nube;
                nube.render();
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

            List<Vector3> centros_terrains_colisionables_aux = new List<Vector3>();

            //Renderizado de terreno
           foreach (tipo_posicion_con_index_heightmap posicion in posiciones_centros)
            {
                if (dist_menor_a_n_width(proy_pos_actual, posicion.posicion_centro, 4))
                {
                    mq_terrains[posicion.index].render(posicion.posicion_centro);
                    //terrain_mq.render(posicion);

                    centros_terrains_colisionables_aux.Add(posicion.posicion_centro);
                    //terrain_hq.render();
                }
                else
                {
                    lq_terrains[posicion.index].render(posicion.posicion_centro);
                    //terrain_lq.render(posicion);
                  /*  if (dist_menor_a_n_width(proy_pos_actual, posicion, 4))
                    {
                        terrain_mq.render(posicion);
                        //terrain_mq.render();
                    }
                    else
                    {
                        terrain_lq.render(posicion);
                        //terrain_lq.render();
                    }*/

                }
            }

           if (centros_terrains_colisionables_aux.Count != 0) {
               centros_terrains_colisionables.Clear();
               centros_terrains_colisionables = centros_terrains_colisionables_aux;              
           }

        }

        public void closeTerrain()
        {
            hq_terrains.Clear();
            mq_terrains.Clear();
            lq_terrains.Clear();

/*            terrain_hq.dispose();
            terrain_mq.dispose();
            terrain_lq.dispose();*/
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

            Vector3 camera;
            Vector3 target;

            camera = plane + CAM_DELTA.Y * y + CAM_DELTA.Z * z;
            target = plane + CAM_DELTA.Y * y;
            cam.SetCenterTargetUp(camera, target, y, true);
            cam.updateViewMatrix(d3dDevice);

            if (player.GetPosition().Y >= 18000) {
                mostrar_msj_advertencia = true;
                hora_advertencia = DateTime.Now;
                if (player.GetPosition().Y >= 20000) {
                    motor.stop();
                    motor.closeFile();
                    sound = GuiController.Instance.Mp3Player;
                    sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                    "Jet_Pilot\\Sonido\\Motor_Apagandose.mp3";
                    sound.play(false);
                    reset();
                }
            }

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
            colisionador = new Colisionador(hq_terrains[0], width, currentScaleY);
        }

        private void updateColision()
        {
            //hago colisionar el avion
            if (colisionador.colisionar(player.getMesh().BoundingBox, centros_terrains_colisionables))
            {
                mostrar_msj_choque = true;
                hora_choque = DateTime.Now;
                motor.stop();
                motor.closeFile();
                sound = GuiController.Instance.Mp3Player;
                sound.FileName = GuiController.Instance.AlumnoEjemplosMediaDir +
                "Jet_Pilot\\Sonido\\Motor_Apagandose.mp3";
                sound.play(false);
                reset();
            }
        }


        //Metodos para el Skybox

        public void initSkybox()
        {
            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            GuiController.Instance.CustomRenderEnabled = true;

            //Crear textura para almacenar el zBuffer. Es una textura que se usa como RenderTarget y que tiene un formato de 1 solo float de 32 bits.
            //En cada pixel no vamos a guardar un color sino el valor de Z de la escena
            //La creamos con un solo nivel de mipmap (el original)
            zBufferTexture = new Texture(d3dDevice, d3dDevice.Viewport.Width, d3dDevice.Viewport.Height, 1, Usage.RenderTarget, Format.R32F, Pool.Default);

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

            //Obtener valores de Modifiers
            float valorFloat = (float)GuiController.Instance.Modifiers["valorFloat"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];

            skyBox2.renderSkybox(player.GetPosition());
           
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
                            mostrar_msj_triunfo = true;
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
                numberx = generador.Next(20000);
                numbery = generador.Next(13500) + 1500; //Para que no toque el terreno
                numberz = generador.Next(20000);
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
