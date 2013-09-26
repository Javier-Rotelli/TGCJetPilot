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

namespace AlumnoEjemplos.MiGrupo
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        private TgcSkyBox skyBox;
        private TgcMesh avion;
        float velocidadActual = 200f;
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
            GuiController.Instance.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));

            */

            /*
             * Configuracion del avion
             */
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene.Meshes[0];
            avion.rotateY((float)Math.PI);  //el modelo aparece invertido sino

            /*
             * Copnfiguracion de la camara
             */
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(avion.Position, 100, -400);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 100, 0);
            
            /*
             * Heightmap
             */
            currentTexture = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "TerrainTexture2.jpg";
            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "Heightmap2.jpg";
            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, 20f, 1.3f, new Vector3(0, 0, 300));
            terrain.loadTexture(currentTexture);

            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";

            //Crear SkyBox 
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

            //Renderizar BoundingBox
            avion.BoundingBox.render();

            terrain.render();
            

            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = avion.Position;


            //skyBox.render();
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {

        }


        public string currentHeightmap { get; set; }

        public TgcSimpleTerrain terrain { get; set; }

        public string currentTexture { get; set; }
    }
}
