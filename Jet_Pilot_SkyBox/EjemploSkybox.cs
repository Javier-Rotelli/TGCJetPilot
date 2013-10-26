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
using TgcViewer.Utils.Fog;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils;

namespace AlumnoEjemplos.MiGrupo
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {       
        private TgcMesh nube;
        private List<TgcMesh> meshes;
        private Texture zBufferTexture;
        private Effect effect;
        private Surface pOldRT;
        private Skybox skyBox2;       
        private List<TgcMesh> nubes = new List<TgcMesh>();
        private int anchoPantalla = GuiController.Instance.Panel3d.Width;
        private int altoPantalla = GuiController.Instance.Panel3d.Height;

        /// <summary>
        /// Categor�a a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el �rbol de la derecha de la pantalla.
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
        /// Completar con la descripci�n del TP
        /// </summary>
        public override string getDescription()
        {
            return "Simulador de vuelo";
        }

        /// <summary>
        /// M�todo que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            /////////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            ////Camara en primera persona, tipo videojuego FPS
            ////Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //////Por default la camara FPS viene desactivada
            //GuiController.Instance.FpsCamera.Enable = true;
            ////Configurar posicion y hacia donde se mira
            //GuiController.Instance.FpsCamera.setCamera(new Vector3(anchoPantalla / 2, altoPantalla / 2, anchoPantalla / 2), new Vector3(0, 0, 0));

            string avionPath = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\" + "AvionCaza-TgcScene.xml";

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;

            //Cargar shader de zbuffer
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\EjemploGetZBuffer.fx");
          
            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(avionPath);

            //cargo la mesh de la nube
            nube = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "nube-TgcScene.xml").Meshes[0];                       
           
            meshes = scene.Meshes;
            meshes.Add(nube);
            for (int i = 0; i < 8; i++)
            {
                nubes.Add(nube);
            }
            meshes.AddRange(nubes);

            //Le setea a todos los meshes de la scene el efecto de zbuffer
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Effect = effect;
            }                        

            //Crear textura para almacenar el zBuffer. Es una textura que se usa como RenderTarget y que tiene un formato de 1 solo float de 32 bits.
            //En cada pixel no vamos a guardar un color sino el valor de Z de la escena
            //La creamos con un solo nivel de mipmap (el original)
            zBufferTexture = new Texture(d3dDevice, d3dDevice.Viewport.Width, d3dDevice.Viewport.Height, 1, Usage.RenderTarget, Format.R32F, Pool.Default);
                      
            //Posicionar el avion
            meshes[0].Position = new Vector3(anchoPantalla / 2, altoPantalla / 2, anchoPantalla / 2);
            nube.Position = meshes[0].Position + new Vector3(10, 0, 0);            

            //Camara en tercera persona que apunta al avion
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(meshes[0].Position, 20.0f, 150.0f);

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


        /// <summary>
        /// M�todo que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aqu� todo el c�digo referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el �ltimo frame</param>
        public override void render(float elapsedTime)
        {

            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //con esto la camara en 3ra persona sigue al avion y por ende el skybox lo acompa�a
            GuiController.Instance.ThirdPersonCamera.setCamera(meshes[0].Position, 20.0f, 150.0f);   

            //Guardar render target original
            pOldRT = d3dDevice.GetRenderTarget(0);

            // 1) Mandar a dibujar todos los mesh para que se genere la textura de ZBuffer
            d3dDevice.BeginScene();

            //Seteamos la textura de zBuffer como render  target (en lugar de dibujar a la pantalla)
            Surface zBufferSurface = zBufferTexture.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, zBufferSurface);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Green, 1.0f, 0);

            //Render de cada mesh
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Technique = "GenerateZBuffer";
                mesh.render();
            }

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
            foreach (TgcMesh mesh in meshes)
            {
                mesh.Technique = "AlterColorByDepth";
                mesh.render();
            }

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

            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow))
            {
                //se mueve para atras
                meshes[0].move(new Vector3(0, 0, 500 * elapsedTime));
            }

            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.UpArrow))
            {
                //se mueve para adelante
                meshes[0].move(new Vector3(0, 0, -500 * elapsedTime));

            }

            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow))
            {
                //rota para el costado izquierdo
                meshes[0].rotateY(0.3f * elapsedTime);
                //meshes[0].moveOrientedY(50 * elapsedTime);
                //    GuiController.Instance.ThirdPersonCamera.rotateY(0.3f * elapsedTime);
            }

            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Boton izq apretado               
            }

            skyBox2.renderSkybox(meshes[0].Position);
            //skyBox2.Render();

            //Mostrar FPS
            d3dDevice.BeginScene();
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
            d3dDevice.EndScene();

        }

        /// <summary>
        /// M�todo que se llama cuando termina la ejecuci�n del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
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
