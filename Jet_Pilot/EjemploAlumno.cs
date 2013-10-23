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

namespace AlumnoEjemplos.Jet_Pilot
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class Jet_Pilot : TgcExample
    {
        private TgcSkyBox skyBox;
        private TgcMesh avion;
        float velocidadActual = 200f;
        public string currentHeightmap { get; set; }
        public TgcSimpleTerrain terrain { get; set; }
        //Width es el ancho de referencia de cada seccion de terreno. Se inicializa en init
        float width, valor_grande;

        //Estas variables se utilizan para manejar el plano en el que se encuentra la cámara y tambien para medir distancias
        Vector3 pos_original, pos_actual, proy_pos_actual, look_at_actual, normal_actual;
        Plane plano_vision;
        Vector3 punto_para_proy, punto_proy_en_pl, vector_final;

        //Muestras de terreno de alta, media y baja calidad        
        Terrain terrain_hq, terrain_mq, terrain_lq;

        //Esta es la lista de las posiciones que vana a ocupar los terrenos que posiblemente se renderizaran
        List<Vector3> posiciones_centros;

        string currentHeightmap_hq, currentHeightmap_mq, currentHeightmap_lq;
        string currentTexture;
        float ScaleXZ_hq, ScaleXZ_mq, ScaleXZ_lq;
        float currentScaleY;
        Colisionador colisionador;
        TgcText2d texto_posicion;
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
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
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;
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

            colisionador = new Colisionador(terrain_hq, width, currentScaleY);

            ///////////////USER VARS//////////////////
            /*
            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("variablePrueba");

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("variablePrueba", 5451);
            */


            ///////////////MODIFIERS//////////////////
            /*
            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("radioEsfera", 0f, 500f, 150f);
            /*
            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
            */

            
            /*
             * Copnfiguracion de la camara
             */
            Vector3 centro_camara;
            centro_camara.X = 0;
            centro_camara.Y = 0;
            centro_camara.Z = 0;
            centro_camara.Y = centro_camara.Y + 495.0046f;

            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(centro_camara, 100, -400);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 100, 0);
            
            //proyeccion de la camara sobre el plano xz
            pos_original = GuiController.Instance.ThirdPersonCamera.getPosition();
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

            /*
             * Configuracion del avion
             */
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene.Meshes[0];
            avion.Position = centro_camara;
            avion.rotateY((float)Math.PI);  //el modelo aparece invertido sino

            //Crear SkyBox 
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);

            //Configurar color
            //skyBox.Color = Color.OrangeRed;
            //Configurar las texturas para cada una de las 6 caras
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            //Hay veces es necesario invertir las texturas Front y Back si se pasa de un sistema RightHanded a uno LeftHanded
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            //Actualizar todos los valores para crear el SkyBox
            skyBox.updateValues();

            //texto con la posicion del avion
            texto_posicion = new TgcText2d();
            texto_posicion.Align = TgcText2d.TextAlign.RIGHT;
            texto_posicion.Position = new Point(0, 0);
        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
            //Device de DirectX para renderizar
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            //Multiplicar la velocidad por el tiempo transcurrido, para no acoplarse al CPU
            float speed = velocidadActual * elapsedTime;

            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia
            //Calcular proxima posicion del avion segun Input
            Vector3 move = new Vector3(0, 0, 0);
            bool moving = false;
            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                move.Z = speed;
                avion.Rotation = new Vector3(0, (float)Math.PI, 0);
                moving = true;
            }//Atras
            else if (d3dInput.keyDown(Key.S))
            {
                move.Z = -speed;
                avion.Rotation = new Vector3(0, 0, 0);
                moving = true;
            }//Izquierda
            else if (d3dInput.keyDown(Key.A))
            {
                move.X = -speed;
                avion.Rotation = new Vector3(0, (float)Math.PI / 2, 0);
                moving = true;
            }//Derecha
            else if (d3dInput.keyDown(Key.D))
            {
                move.X = speed;
                avion.Rotation = new Vector3(0, -(float)Math.PI / 2, 0);
                moving = true;
            }//abajo
            else if(d3dInput.keyDown(Key.LeftControl))
            {
                move.Y = -speed;
                avion.Rotation = new Vector3(0, (float)Math.PI, 0);
                moving = true;
            }//arriba
            else if (d3dInput.keyDown(Key.Space))
            {
                move.Y = speed;
                avion.Rotation = new Vector3(0, (float)Math.PI, 0);
                moving = true;
            }

            //Si hubo desplazamientos
            if (moving)
            {
                //Mover personaje
                Vector3 lastPos = avion.Position;
                avion.move(move);
            }

            //Renderizar modelo
            avion.render();
            texto_posicion.Text = "Posicion: (" + avion.Position.X + "," + avion.Position.Y + "," + avion.Position.Z + ")";
            texto_posicion.render();
            /*float radio = (float) GuiController.Instance.Modifiers.getValue("radioEsfera");*/
            (new TgcBoundingSphere(avion.Position, 150f)).render();
            //Renderizar BoundingBox
            avion.BoundingBox.render();           

            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = avion.Position;

            //Renderizar terreno

            pos_actual = GuiController.Instance.ThirdPersonCamera.getPosition();
            look_at_actual = GuiController.Instance.ThirdPersonCamera.getLookAt();

            normal_actual = look_at_actual - pos_actual;

            plano_vision = Plane.FromPointNormal(pos_actual, normal_actual);

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


            List<Vector3> centros_terrains_colisionables = new List<Vector3>(); 
            //dejo esto aca, despues veo si es bueno promoverla a variable de instancia
            
            //renderizo terrenos de alta, media y baja calidad de acuerdo a la distancia a la que se encuentren de la proyeccion de la camara en el plano xz
            foreach (Vector3 posicion in posiciones_centros)
            {
                if (dist_menor_a_n_width(proy_pos_actual, posicion, 2))
                {
                    terrain_hq.render(posicion);
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
            if (colisionador.colisionar(avion, centros_terrains_colisionables))
                avion.Position = new Vector3(0f, 495.0046f, 0f);    
            //si choca lo vuelvo a la posicion original. y que el proximo ciclo empiece de vuelta

            //skyBox.render();
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            terrain_hq.dispose();
            terrain_mq.dispose();
            terrain_lq.dispose();
        }

        private bool esta_delante_del_plano(Plane plano_actual, Vector3 punto)
        {

            //genero un punto atras del plano que este en una linea perpendicular al mismo y que contenga al punto analizado
            punto_para_proy.X = (valor_grande * normal_actual.X) + punto.X;
            punto_para_proy.Y = (valor_grande * normal_actual.Y) + punto.Y;
            punto_para_proy.Z = (valor_grande * normal_actual.Z) + punto.Z;

            punto_proy_en_pl = Plane.IntersectLine(plano_vision, punto_para_proy, punto);

            //este vector apunta desde el plano al punto (de forma perpendicular al plano)
            vector_final = punto - punto_proy_en_pl;

            //si el punto esta "por delante" del plano, devuelvo true
            if ((vector_final.X / normal_actual.X >= 0))
            {
                return true;
            }

            return false;
        }

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

    }
}
