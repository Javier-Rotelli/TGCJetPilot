using System;
using System.Collections.Generic;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Jet_Pilot
{
    /// <summary>
    /// Colisionador para ver si el avion colisiona con el terreno
    /// </summary>
    public class Colisionador
    {
        const float EPSILON = 0.05f;

        CustomVertex.PositionTextured[] verticesTerreno;
        float tolerancia = 150f;
        float tamanio_terrenos;
        float escalaY;
        List<Vector3> centros_probables;
        
        /// <summary>
        /// crea un nuevo colisionador
        /// </summary>
        /// <param name="terreno">el tereno desde el que voy a tomar los vertices para probar la colision</param>
        /// <param name="tamanio">el tamanio del terreno base con el que hago las pruebas</param>
        /// <param name="escalaY">la escala de los terrenos en el eje y </param>
        public Colisionador(Terrain terreno, float tamanio, float escalaY)
        {
            verticesTerreno = (CustomVertex.PositionTextured[]) terreno.vbTerrain.Lock(0,LockFlags.ReadOnly);
            terreno.vbTerrain.Unlock();

            this.escalaY = escalaY/2;   //dividido dos, porque es para referenciar desde el centro
            tamanio_terrenos = tamanio/2;
            centros_probables = new List<Vector3>();
        }

        public bool colisionar(TgcMesh objeto,List<Vector3> centros)
        {
            if (Math.Abs(objeto.Position.Y - centros[0].Y) - tolerancia - escalaY > EPSILON)
            {//si esta lejos en el eje y, no tiene sentido testear lo demas
                return false;
            }
            centros_probables.Clear();
            foreach (Vector3 centro in centros)
            {
                if(Vector3.LengthSq(objeto.Position - centro) - Math.Pow(tamanio_terrenos,2) < EPSILON)
                {//si el centro esta cerca en altura, y en horizontal lo guardo para colisionar
                    centros_probables.Add(centro);
                }
            }

            //veo si colisionan los terrenos
            Vector3 nuevaPosicion;
            List<Vector3> verticesEnEsfera = new List<Vector3>();
            TgcBoundingBox bounding = objeto.BoundingBox;
            foreach (Vector3 centro in centros_probables)
            {
                //desplazo el avion en vez del terreno para testear colisiones
                nuevaPosicion = objeto.Position - centro;
                bounding.scaleTranslate(nuevaPosicion - objeto.Position,new Vector3(1,1,1));
                verticesEnEsfera.Clear();
                float toleranciaSQ = (float)Math.Pow(tolerancia, 2);
                foreach (CustomVertex.PositionTextured vertice in verticesTerreno)
                {
                    if ( Vector3.LengthSq(nuevaPosicion - vertice.Position) - toleranciaSQ < EPSILON)
                    {
                        verticesEnEsfera.Add(vertice.Position);
                    }
                }
                foreach (Vector3 vertice in verticesEnEsfera)
                {
                    if (TgcCollisionUtils.sqDistPointAABB(vertice, bounding) < EPSILON)
                    {
                        return true;
                    }
                }

            }
            return false;
        }
    }
}