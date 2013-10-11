using System;
using System.Collections.Generic;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Jet_Pilot
{
    /// <summary>
    /// Colisionador para ver si el avion colisiona con el terreno
    /// </summary>
    public class Colisionador
    {
        const float EPSILON = 0.05f;

        Terrain template_terreno;
        float tolerancia = 5f;
        private float tamanio_terrenos;
        List<Vector3> centros_probables;
        
        /// <summary>
        /// crea un nuevo colisionador
        /// </summary>
        /// <param name="terreno">el tereno desde el que voy a tomar los vertices para probar la colision</param>
        /// <param name="tamanio">el tamanio de la esfera con la que hago las pruebas, idealmente deberia contener al objeto a colisionar</param>
        public Colisionador(Terrain terreno, float tamanio)
        {
            template_terreno = terreno;
            tamanio_terrenos = tamanio;
            centros_probables = new List<Vector3>();
        }

        public bool colisionar(TgcMesh objeto,List<Vector3> centros)
        {
            foreach (Vector3 centro in centros)
            {
                if((objeto.Position.Y - centro.Y) < tolerancia && Vector3.LengthSq(objeto.Position - centro) < Math.Sqrt(tamanio_terrenos/2))
                {//si el centro esta cerca en altura, y en horizontal lo guardo para colisionar
                    centros_probables.Add(centro);
                }
                //veo si colisionan los terrenos
            }
            return false;
        }
    }
}