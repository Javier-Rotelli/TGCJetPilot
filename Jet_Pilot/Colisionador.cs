using System;
using System.Collections.Generic;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using System.Drawing;

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

            this.escalaY = escalaY * 255;
            tamanio_terrenos = tamanio;
            centros_probables = new List<Vector3>();
        }

        public bool colisionar(TgcBoundingBox objeto,List<Vector3> centros)
        {
            if (objeto.Position.Y - (centros[0].Y + escalaY ) - tolerancia > EPSILON)
            {//si esta lejos en el eje y, no tiene sentido testear lo demas. 
                objeto.setRenderColor(Color.Yellow);
                return false;
            }
            objeto.setRenderColor(Color.Red);
            centros_probables.Clear();
            if (objeto.Position.Y < 0) { return true; } //Si el avion avanza demasiado rápido  y no se llega a checkear la colisión
            foreach (Vector3 centro in centros)
            {

                if((Math.Pow(objeto.Position.X - centro.X, 2) + Math.Pow(objeto.Position.Z - centro.Z,2)) - Math.Pow(tamanio_terrenos/2,2) < EPSILON)
                {//si el centro esta cerca en altura, y en horizontal lo guardo para colisionar
                    centros_probables.Add(centro);
                }
            }

            //veo si colisionan los terrenos
            Vector3 nuevaPosicion;
            List<Vector3> verticesEnEsfera = new List<Vector3>();
            TgcBoundingBox bounding = objeto.clone();
            foreach (Vector3 centro in centros_probables)
            {
                //desplazo el avion en vez del terreno para testear colisiones
                nuevaPosicion = Vector3.Subtract(objeto.Position,centro);
                
                bounding.move(Vector3.Multiply(centro,-1f));
                verticesEnEsfera.Clear();
                TgcBoundingSphere esfera = new TgcBoundingSphere(nuevaPosicion,tolerancia);
                Vector3 colision = new Vector3();
                for (int i = 0; i < verticesTerreno.Length; i += 3)
                {
                    if (TgcCollisionUtils.testSphereTriangle(esfera, verticesTerreno[i].Position, verticesTerreno[i + 1].Position, verticesTerreno[i + 2].Position,out colision))
                    {
                        if (TgcCollisionUtils.testTriangleAABB( verticesTerreno[i].Position, verticesTerreno[i + 1].Position, verticesTerreno[i + 2].Position,bounding))
                        {
                            return true;
                        }
                    }
                }
                /*
                foreach (CustomVertex.PositionTextured vertice in verticesTerreno)
                {
                    if ( Vector3.LengthSq(Vector3.Subtract(nuevaPosicion,vertice.Position)) - toleranciaSQ < EPSILON)
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
                }*/

            }
            return false;
        }
    }
}