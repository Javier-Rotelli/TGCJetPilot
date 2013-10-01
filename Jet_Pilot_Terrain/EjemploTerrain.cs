using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using AlumnoEjemplos.Jet_Pilot;

namespace Examples.Outdoor
{
   
    /// <summary>
    /// Generador de terreno.
    /// 
    /// Crea tres muestras de un terreno, de alta media y baja calidad,
    /// en base a una textura de Heightmap y le aplica arriba a cada 
    /// una de ellas una textura de color (DiffuseMap).
    /// Luego estas tres muestras se renderizan dando la sensación de 
    /// "terreno infinito".
    /// Se utiliza la herramienta TgcSimpleTerrain, pero modificada.
    /// 
    /// Autor: Jorge Baez
    /// 
    /// </summary>
    public class EjemploSimpleTerrain : TgcExample
    {
        //Width es el ancho de referencia de cada seccion de terreno. Se inicializa en init
        float width,valor_grande;
        
        //Estas variables se utilizan para manejar el plano en el que se encuentra la cámara y tambien para medir distancias
        Vector3 pos_original,pos_actual,proy_pos_actual,look_at_actual,normal_actual;
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


        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Jet_Pilot_Terrain";
        }

        public override string getDescription()
        {
            return "Se crea un terreno en base a una textura de HeightMap utilizando la herramietna del Framework TgcSimpleTerrain, pero modificada";
        }

        
        
        private bool esta_delante_del_plano(Plane plano_actual, Vector3 punto) {

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

        private bool dist_menor_a_n_width(Vector3 pos_camara, Vector3 pos_espacio, int n) {

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

        private void generar_puntos_alrededor(Vector3 posicion) {
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
        
     
        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Path de Heightmap high quality del terreno
            currentHeightmap_hq = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "Heightmap_hq.jpg";
            

            //Path de Heightmap medium quality del terreno
            currentHeightmap_mq = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "Heightmap_mq.jpg";
            

            //Path de Heightmap low quality del terreno
            currentHeightmap_lq = GuiController.Instance.AlumnoEjemplosMediaDir + "Jet_Pilot\\" + "Heightmaps\\" + "Heightmap_lq.jpg";
            
            

            //Escala del mapa
            ScaleXZ_hq = 40f;
            ScaleXZ_mq = (ScaleXZ_hq * 2) + 0.7f;
            ScaleXZ_lq = (ScaleXZ_mq * 2) + 5f ;

            
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
            
            


            //Configurar FPS Camara
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 7000f;
            GuiController.Instance.FpsCamera.JumpSpeed = 1000f;
            Vector3 centro_camara;
            centro_camara.X = 0;
            centro_camara.Y = 0;
            centro_camara.Z = 0;
            centro_camara.Y=centro_camara.Y+495.0046f;
            GuiController.Instance.FpsCamera.setCamera(centro_camara, new Vector3(164.9481f, 35.3185f, -61.5394f));
            
            //proyeccion de la camara sobre el plano xz
            pos_original = GuiController.Instance.FpsCamera.getPosition();
            pos_original.Y = 0;
            
            //Generar lista de posiciones inicial
            Vector3 nuevo_punto;
            float inner_width = width;
            posiciones_centros=new List<Vector3>();

            pos_original.X = pos_original.X - (width * 4);
            for (int i = 0; i < 9; i++){

                nuevo_punto = new Vector3();
                nuevo_punto = pos_original;
                posiciones_centros.Add(nuevo_punto);


                for (int j = 0; j < 4; j++){
                    
                    nuevo_punto=new Vector3();
                    nuevo_punto=pos_original;
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

        
           
        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar terreno

            pos_actual = GuiController.Instance.FpsCamera.getPosition();
            look_at_actual=GuiController.Instance.FpsCamera.getLookAt();

            normal_actual=look_at_actual-pos_actual;

            plano_vision = Plane.FromPointNormal(pos_actual, normal_actual);

            proy_pos_actual = pos_actual;
            proy_pos_actual.Y = 0;

            List<Vector3>a_borrar=new List<Vector3>();
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

        public override void close()
        {
            terrain_hq.dispose();
            terrain_mq.dispose();
            terrain_lq.dispose();
        }

       
        
        
    }
}
