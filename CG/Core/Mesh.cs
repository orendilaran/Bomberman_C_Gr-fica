using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CG.Core
{
    // Classe de malha, representando o conjunto de vértices e índices conforme especificação.
    internal class Mesh
    {
        int _count = 0;
        int _vao = 0;
         PrimitiveType _primitiveType; //Para criar linhas

        public Mesh(float[] vertices, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles)
            {
            _count = indices.Length;
            _primitiveType = primitiveType; //PAra criar linhas

            //  Começamos criando um ArrayBuffer(vbo), um espaço na placa de vídeo para armazenar os vértices.
            //  A função GL.BindBuffer diz que estamos operando no array previamente criado.
            //  Com a função GL.BufferData faz de fato o envio dos dados como uma sequência de bytes para a placa de vídeo.
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            //  A placa de vídeo não sabe o significado dos dados enviados, por isso precisamos de um VertexArrayObject(vao).
            //  Com o VertexArrayObject, descrevemos quais elementos estão presentes nos vértices. No exemplo, vamos informar
            //que os dados representam vetores 2D com as posições dos vértices.
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            //  É com a função GL.VertexAttribPointer que "ensinamos" a placa de vídeo a usar os dados brutos que fornecemos.
            //  Vamos criar atributos para os dados, que podem ser posições, cores, texturas, etc.
            //  A função é bem longa, então atenção ao significado dos parâmetros:
            //  id: Um identificador/índice para o Atributo
            //  size: O "tamanho" do dado, significando quantos elementos tem. Como estamos trabalhando com vetores 3D, usamos 3
            //  type: O tipo do dado. Estamos usando floats para descrever os vértices, mas existem outros tipos disponíveis.
            //  normalized: Indica se os valores estão normalizados, ou seja, vão de 0 a 1. Não é o nosso caso.
            //  stride: Indica o espaço, em bytes, que precisamos percorrer para encontrar o "próximo vértice". No exemplo, é
            //preciso pular 2 vezes o tamanho de um float para chegar ao próximo vértice.
            //  offset: O espaço, em bytes que precisamos pular para chegar ao início dos dados. No exemplo, não precisamos pular
            //nada, mas este parâmetro será útil quando criarmos mais atributos para a malha.

            // Atributo 0: Posição do vértice
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);

            // Atributo 1: Cor/Normal do vértice
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);

            // Atributo 2: Coordenadas de textura
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 6);

            //  O envio dos dados de triângulo para a placa de vídeo segue o mesmo processo do envio dos vértices, só que agora utilizamos
            //o BufferTarget.ElementArrayBuffer, pois estamos lidando com elementos(triângulos) e não vértices.
            int ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);
        }

        public void Draw()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElements(_primitiveType, _count, DrawElementsType.UnsignedInt, 0);// Comando de desenho do triângulo, a partir do Element Buffer Object(ebo)
        }

        public static Mesh[] LoadFromFile(string path, float scale = 1f)
        {
            Assimp.AssimpContext importer = new();
            importer.Scale = scale;
            try
            {
                Assimp.PostProcessSteps flags =
                    Assimp.PostProcessSteps.Triangulate |
                    Assimp.PostProcessSteps.GenerateNormals |
                    Assimp.PostProcessSteps.PreTransformVertices
                ;
                Assimp.Scene scene = importer.ImportFile(path, flags);

                Mesh ProcessMesh(Assimp.Mesh m)
                {
                    if (!m.HasNormals || !m.HasVertices || !m.HasTextureCoords(0))
                    {
                        return Primitive.CreateCube(1f);
                    }

                    List<float> vertices = new();
                    for (int i = 0; i < m.VertexCount; i++)
                    {
                        var v = m.Vertices[i];
                        var n = m.Normals[i];
                        var t = m.TextureCoordinateChannels[0][i];
                        vertices.AddRange([v.X, v.Y, v.Z, n.X, n.Y, n.Z, t.X, t.Y]);
                    }

                    uint[] indices = m.GetUnsignedIndices().ToArray();

                    return new Mesh(vertices.ToArray(), indices);
                }

                Mesh[] ProcessNode(Assimp.Scene s, Assimp.Node n)
                {
                    List<Mesh> meshes = new();
                    foreach (int i in n.MeshIndices)
                    {
                        meshes.Add(ProcessMesh(s.Meshes[i]));
                    }
                    foreach (Assimp.Node other in s.RootNode.Children)
                    {
                        meshes.AddRange(ProcessNode(s, other));
                    }
                    return meshes.ToArray();
                }

                return ProcessNode(scene, scene.RootNode);
            }
            catch
            {
                return [];
            }
        }
    }

    internal class Primitive
    {
        public static Mesh CreateGrid(float width, float depth, int divisionsX, int divisionsZ)
    {
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();
        
        float halfWidth = width / 2f;
        float halfDepth = depth / 2f;
        float stepX = width / divisionsX;
        float stepZ = depth / divisionsZ;
        
        uint indexCounter = 0;

        // Loop para criar as linhas paralelas ao eixo Z
        for (int i = 0; i <= divisionsX; i++)
        {
            float x = -halfWidth + (i * stepX);

            // Posição (X, Y, Z), Normal (0, 1, 0), UV (0, 0)
            vertices.AddRange(new float[] { x, 0f, -halfDepth,   0f, 1f, 0f,   0f, 0f });
            vertices.AddRange(new float[] { x, 0f,  halfDepth,   0f, 1f, 0f,   0f, 0f });

            indices.Add(indexCounter++);
            indices.Add(indexCounter++);
        }

        // Loop para criar as linhas paralelas ao eixo X
        for (int i = 0; i <= divisionsZ; i++)
        {
            float z = -halfDepth + (i * stepZ);

            vertices.AddRange(new float[] { -halfWidth, 0f, z,   0f, 1f, 0f,   0f, 0f });
            vertices.AddRange(new float[] {  halfWidth, 0f, z,   0f, 1f, 0f,   0f, 0f });

            indices.Add(indexCounter++);
            indices.Add(indexCounter++);
        }

        // Informa ao construtor da Mesh que ela deve ser desenhada como LINHAS.
        return new Mesh(vertices.ToArray(), indices.ToArray(), PrimitiveType.Lines);
    }
        public static Mesh CreatePlane(float width = 1f, float depth = 1f)
        {
            float halfW = width / 2f;
            float halfD = depth / 2f;

            float[] vertices =
            {   // posição           // normal    // uv
                -halfW, 0f,  halfD,  0f, 1f, 0f,  0f, 0f,// 0
                 halfW, 0f,  halfD,  0f, 1f, 0f,  1f, 0f,// 1
                 halfW, 0f, -halfD,  0f, 1f, 0f,  1f, 1f,// 2
                -halfW, 0f, -halfD,  0f, 1f, 0f,  0f, 1f,// 3
            };

            uint[] indices =
            {
                0, 1, 2,
                0, 2, 3,
            };

            return new Mesh(vertices, indices);
        }

        public static Mesh CreatePlane(float size = 1f)
        {
            return CreatePlane(size, size);
        }


        public static Mesh CreateCylinder(float radius = 0.5f, float height = 1.0f, int segments = 16)
        {
            float halfHeight = height / 2.0f;

            List<float> vertices = new();
            for (int i = 0; i <= segments; i++)
            {
                float value = (float)i / segments;
                float angle = value * MathF.Tau;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);
                float x = cos * radius;
                float z = -sin * radius;

                // 0 - Face lateral (topo)
                vertices.AddRange(new float[] { x, halfHeight, z });// Posição
                vertices.AddRange(new float[] { cos, 0.0f, -sin });// Normal
                vertices.AddRange(new float[] { value, 1.0f });// UV

                // 1 - Face lateral (baixo)
                vertices.AddRange(new float[] { x, -halfHeight, z });// Posição
                vertices.AddRange(new float[] { cos, 0.0f, -sin });// Normal
                vertices.AddRange(new float[] { value, 0.0f });// UV

                float uvX = cos * 0.5f + 0.5f;
                float uvY = sin * 0.5f + 0.5f;

                // 2 - Face superior
                vertices.AddRange(new float[] { x, halfHeight, z });// Posição
                vertices.AddRange(new float[] { 0.0f, 1.0f, 0.0f });// Normal
                vertices.AddRange(new float[] { uvX, uvY });// UV

                // 3 - Face inferior
                vertices.AddRange(new float[] { x, -halfHeight, z });// Posição
                vertices.AddRange(new float[] { 0.0f, -1.0f, 0.0f });// Normal
                vertices.AddRange(new float[] { -uvX, uvY });// UV
            }

            List<uint> indices = new();
            // Triângulos laterais
            for (uint i = 0; i < segments; i++)
            {
                uint i0 = i * 4;
                uint i1 = i0 + 1;
                uint i2 = i0 + 4;
                uint i3 = i0 + 5;

                indices.AddRange(new uint[] { i0, i1, i2 });
                indices.AddRange(new uint[] { i1, i3, i2 });
            }
            // Triângulos superiores
            for (uint i = 0; i < segments; i++)
            {
                uint i0 = 2;
                uint i1 = i * 4 + 2;
                uint i2 = (i + 1) * 4 + 2;

                indices.AddRange(new uint[] { i0, i1, i2 });
            }
            // Triângulos inferiores
            for (uint i = 0; i < segments; i++)
            {
                uint i0 = 3;
                uint i1 = i * 4 + 3;
                uint i2 = (i + 1) * 4 + 3;

                indices.AddRange(new uint[] { i1, i0, i2 });
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        public static Mesh CreateSphere(float radius = 0.5f, uint segments = 32, uint rings = 16)
        {
            List<float> vertices = new();
            for (uint i = 0; i <= rings; i++)
            {
                float valueY = (float)i / rings;
                float mult = MathF.Sin(valueY * MathF.PI);
                float cosY = MathF.Cos(valueY * MathF.PI);
                float y = -cosY;
                for (uint j = 0; j <= segments; j++)
                {
                    float valueX = (float)j / segments;

                    float cosX = MathF.Cos(MathF.Tau * valueX);
                    float sinZ = MathF.Sin(MathF.Tau * valueX);

                    float x = cosX * mult;
                    float z = -sinZ * mult;

                    vertices.AddRange(new float[] { x * radius, y * radius, z * radius });// Posição
                    vertices.AddRange(new float[] { x, y, z });// Normal
                    vertices.AddRange(new float[] { valueX, valueY });// UV
                }
            }

            List<uint> indices = new();
            for (uint i = 0; i < rings; i++)
            {
                for (uint j = 0; j < segments; j++)
                {
                    uint i0 = i * (segments + 1) + j;
                    uint i1 = i * (segments + 1) + j + 1;
                    uint i2 = (i + 1) * (segments + 1) + j + 1;
                    uint i3 = (i + 1) * (segments + 1) + j;

                    indices.AddRange(new uint[] { i0, i1, i2 });
                    indices.AddRange(new uint[] { i0, i2, i3 });
                }
            }
            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        public static Mesh CreateCone(float radius = 0.5f, float height = 1.0f, uint segments = 16)
        {
            List<float> vertices = new();
            List<uint> indices = new();

            float half = height / 2f;
            for (uint i = 0; i < segments + 1; i++)
            {
                float value = i / (float)segments;
                float x = MathF.Sin(value * MathF.Tau);
                float z = MathF.Cos(value * MathF.Tau);

                Vector3 normal = new Vector3(x * height, radius, z * height).Normalized();

                // Parede inferior
                vertices.AddRange(new float[] { x * radius, -half, z * radius });// Posição
                vertices.AddRange(new float[] { normal.X, normal.Y, normal.Z });// Normal
                vertices.AddRange(new float[] { x / 2f + 0.5f, -z / 2f + 0.5f });// UV
                // Parede superior
                vertices.AddRange(new float[] { 0f, half, 0f });// Posição
                vertices.AddRange(new float[] { normal.X, normal.Y, normal.Z });// Normal
                vertices.AddRange(new float[] { 0.5f, 0.5f });// UV
                // Baixo
                vertices.AddRange(new float[] { x * radius, -half, z * radius });// Posição
                vertices.AddRange(new float[] { 0f, -1f, 0f });// Normal
                vertices.AddRange(new float[] { x / 2f + 0.5f, z / 2f + 0.5f });// UV
            }

            // Paredes
            for (uint i = 0; i < segments; i++)
            {
                uint one = i * 3;
                uint two = (i + 1) * 3;
                uint three = one + 1;

                indices.AddRange(new[] { one, two, three });
            }
            // Baixo
            for (uint i = 0; i < segments - 1; i++)
            {
                uint one = 2;
                uint two = (i + 2) * 3 + 2;
                uint three = (i + 1) * 3 + 2;

                indices.AddRange(new[] { one, two, three });
            }
            return new Mesh(vertices.ToArray(), indices.ToArray());
        }
        //CUBO!!
        public static Mesh CreateCube(float size)
        {
        return CreateRectangularPrism(size, size, size);

        }
        public static Mesh CreateRectangularPrism(float Largura,float Altura, float Espessura)
        {
        float halfL = Largura / 2f;
        float halfA = Altura / 2f;
        float halfE = Espessura / 2f;

        float[] vertices =
        { // posição // normal // uv
        // face +Z
        -halfL, -halfA, halfE, 0f, 0f, 1f, 0f, 0f,// 0
        halfL, -halfA, halfE, 0f, 0f, 1f, 1f, 0f,
        halfL, halfA, halfE, 0f, 0f, 1f, 1f, 1f,
        -halfL, halfA, halfE, 0f, 0f, 1f, 0f, 1f,
        // face -Z
        halfL, -halfA, -halfE, 0f, 0f, -1f, 0f, 0f,// 4
        -halfL, -halfA, -halfE, 0f, 0f, -1f, 1f, 0f,
        -halfL, halfA, -halfE, 0f, 0f, -1f, 1f, 1f,
        halfL, halfA, -halfE, 0f, 0f, -1f, 0f, 1f,
        // face +X
        halfL, -halfA, halfE, 1f, 0f, 0f, 0f, 0f,// 8
        halfL, -halfA, -halfE, 1f, 0f, 0f, 1f, 0f,
        halfL, halfA, -halfE, 1f, 0f, 0f, 1f, 1f,
        halfL, halfA, halfE, 1f, 0f, 0f, 0f, 1f,
        // face -X
        -halfL, -halfA, -halfE, -1f, 0f, 0f, 0f, 0f,// 12
        -halfL, -halfA, halfE, -1f, 0f, 0f, 1f, 0f,
        -halfL, halfA, halfE, -1f, 0f, 0f, 1f, 1f,
        -halfL, halfA, -halfE, -1f, 0f, 0f, 0f, 1f,
        // face +Y
        -halfL, halfA, halfE, 0f, 1f, 0f, 0f, 0f,// 16
        halfL, halfA, halfE, 0f, 1f, 0f, 1f, 0f,
        halfL, halfA, -halfE, 0f, 1f, 0f, 1f, 1f,
        -halfL, halfA, -halfE, 0f, 1f, 0f, 0f, 1f,
        // face -Y
        halfL, -halfA, halfE, 0f, -1f, 0f, 0f, 0f,// 20
        -halfL, -halfA, halfE, 0f, -1f, 0f, 1f, 0f,
        -halfL, -halfA, -halfE, 0f, -1f, 0f, 1f, 1f,
        halfL, -halfA, -halfE, 0f, -1f, 0f, 0f, 1f,
        };

        uint[] indices =
        {
        0, 1, 2,
        0, 2, 3,

        4, 5, 6,
        4, 6, 7,

        8, 9, 10,
        8, 10, 11,

        12, 13, 14,
        12, 14, 15,

        16, 17, 18,
        16, 18, 19,

        20, 21, 22,
        20, 22, 23,
        };

        return new Mesh(vertices, indices);
        }
    }
}
