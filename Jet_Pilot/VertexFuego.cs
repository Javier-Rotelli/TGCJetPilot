// Simple struct to hold position and two texture coordinates
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.Jet_Pilot
{
    public struct vertexFuego
    {
        public Vector3 Position;
        public Vector2 TexCoords0;
        public Vector2 TexCoords1;
        public Vector2 TexCoords2;
        public Vector2 TexCoords3;

        // We include a constructor to save time when creating number of these
        public vertexFuego(Vector3 Pos, Vector2 TexC0, Vector2 TexC1, Vector2 TexC2, Vector2 TexC3)
        {
            Position = Pos;
            TexCoords0 = TexC0;
            TexCoords1 = TexC1;
            TexCoords2 = TexC2;
            TexCoords3 = TexC3;
        }

        public static readonly int SizeInBytes = sizeof(float) * 11;
        public static readonly VertexFormats Format = VertexFormats.Position | VertexFormats.Texture0 | VertexFormats.Texture1 | VertexFormats.Texture2 | VertexFormats.Texture3;

        // Vertex Element array for our struct above
        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0,0,DeclarationType.Float3,
                            DeclarationMethod.Default,
                            DeclarationUsage.Position,
                            0),
            new VertexElement(0,sizeof(float)*(3),DeclarationType.Float2,
                            DeclarationMethod.Default,
                            DeclarationUsage.TextureCoordinate,
                            0),
            new VertexElement(0,sizeof(float)*(3+2),DeclarationType.Float2,
                            DeclarationMethod.Default,
                            DeclarationUsage.TextureCoordinate,
                            1),
            new VertexElement(0,sizeof(float)*(3+2+2),DeclarationType.Float2,
                            DeclarationMethod.Default,
                            DeclarationUsage.TextureCoordinate,
                            2),
            new VertexElement(0,sizeof(float)*(3+2+2+2),DeclarationType.Float2,
                            DeclarationMethod.Default,
                            DeclarationUsage.TextureCoordinate,
                            3),

        };
    }
}