using CG.Core;
using CG.Drawables;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CG
{
    internal class Game : GameWindow
    {
        Mesh mesh;
        Mesh mesh2;
        Mesh chão;
        Mesh Muro1;
        Mesh Muro2;
        Mesh Muro3;
        Mesh Muro4;
        Mesh gridMesh;
        Material gridMaterial;
        ShaderProgram program;
        Texture texture;
        Texture texture3;
        Drawable3D drawable1;
        Drawable3D drawable2;
        Camera camera;
        Material material1;
        Material material2;
        Material material3;
        Material material4;
        Scene scene = new();

        // --- LÓGICA DO GRID CORRIGIDA ---
        // Esta é a fonte da verdade para o tamanho do grid.
        private const int GRID_CELLS_X = 12;
        private const int GRID_CELLS_Z = 19;

        // Os tamanhos do mundo são baseados no número de células (1 célula = 1 unidade de tamanho)
        private float gridWidth = GRID_CELLS_X;  // 13f
        private float gridDepth = GRID_CELLS_Z;  // 20f

        private int[,] logicalGrid;
        // ---------------------------------

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            logicalGrid = new int[GRID_CELLS_X, GRID_CELLS_Z];

            // --- Geometria (Meshes) ---
            // Os tamanhos agora estão consistentes com o grid 13x20
            Muro1 = Primitive.CreateRectangularPrism(1f, 0.5f, gridDepth+2); // Muro lateral (profundidade 20)
            Muro2 = Primitive.CreateRectangularPrism(gridWidth, 0.5f, 1f); // Muro frontal (largura 13)
            Muro3 = Primitive.CreateRectangularPrism(1f, 0.5f, gridDepth+2); // Muro lateral (profundidade 20)
            Muro4 = Primitive.CreateRectangularPrism(gridWidth, 0.5f, 1f); // Muro traseiro (largura 13)
            
            chão = Primitive.CreatePlane(gridWidth, gridDepth); // Chão 13x20
            mesh = Primitive.CreateCube(1f); // Cubo de 1x1 para caber nas células
            gridMesh = Primitive.CreateGrid(gridWidth, gridDepth, GRID_CELLS_X, GRID_CELLS_Z); // Grid visual 13x20
            mesh2 = Mesh.LoadFromFile("./assets/models/model.glb")[0];


            // --- Shaders ---
            Shader shaderVertex = Shader.LoadFromFile("./assets/shaders/shader.vert", ShaderType.VertexShader);
            Shader shaderFragment = Shader.LoadFromFile("./assets/shaders/shader.frag", ShaderType.FragmentShader);
            program = new ShaderProgram(shaderVertex, shaderFragment);
            shaderVertex.Delete();
            shaderFragment.Delete();

            // --- Texturas ---
            texture = new Texture("./assets/textures/image.jpg");
            texture3 = new Texture("./assets/textures/tijolo.jpeg");
            Texture texture2 = new Texture("./assets/textures/house.png");

            // --- Materiais ---
            // (Grid sem textura e com uma cor sólida, como cinza)
            gridMaterial = new Material(program);
            gridMaterial.SetFloat("u_SpecularIntensity", 0.0f); // Sem brilho especular
            gridMaterial.SetVec3("u_Color", 0.3f, 0.3f, 0.3f); // Cinza escuro
            
            material1 = new Material(program);
            material1.SetFloat("u_SpecularIntensity", 0.5f);
            material1.SetVec3("u_Color", 1f, 0f, 0f);
            material1.SetTexture("u_Texture", texture);

            material2 = new Material(program);
            material2.SetFloat("u_SpecularIntensity", 0.5f);
            material2.SetVec3("u_Color", 1f, 1f, 1f);
            material2.SetTexture("u_Texture", texture2);

            material3 = new Material(program);
            material3.SetFloat("u_SpecularIntensity", 0.1f);
            material3.SetVec3("u_Color", 0f, 0.7f, 0.3f);
            
            //Material dos muros (reutilizando 'material4')
            material4 = new Material(program);
            material4.SetFloat("u_SpecularIntensity", 0.5f);
            material4.SetVec3("u_Color", 0.5f, 0.5f, 0.5f);
            material4.SetTexture("u_Texture", texture3);

            // --- Criação e Posicionamento de Objetos ---
            
            // Armazena a referência do objeto giratório em 'drawable1'
            drawable1 = SpawnObjectAt(8, 10, mesh, material1, 0.5f); // Y=0.5f para cubo de 1f ficar em cima do chão

            // Vamos criar outro no canto (0,0)
            SpawnObjectAt(0, 0, mesh, material1, 0.5f);

            // E outro no canto oposto (12, 19)
            // (Células X vão de 0 a 12; Células Z vão de 0 a 19)
            SpawnObjectAt(12, 19, mesh, material1, 0.5f);
            
            // Objetos do cenário
            AdicionarObjetoNaCena(gridMaterial, gridMesh, new Vector3(0f, 0.01f, 0f)); // Um pouco acima do chão
            AdicionarObjetoNaCena(material3, chão, new Vector3(0f, 0f, 0f));
            
            // Posições dos muros corrigidas para o grid 13x20
            float halfWidth = gridWidth / 2f;
            float halfDepth = gridDepth / 2f;
            AdicionarObjetoNaCena(material4, Muro1, new Vector3(halfWidth + 0.5f, 1f, 0f));  // Muro Direito
            AdicionarObjetoNaCena(material4, Muro2, new Vector3(0f, 1f, halfDepth + 0.5f));  // Muro Frontal
            AdicionarObjetoNaCena(material4, Muro3, new Vector3(-halfWidth - 0.5f, 1f, 0f)); // Muro Esquerdo
            AdicionarObjetoNaCena(material4, Muro4, new Vector3(0f, 1f, -halfDepth - 0.5f)); // Muro Traseiro
            
            // --- Configuração da Câmera e Cena ---
            camera = new PerspectiveCamera((float)Size.X / Size.Y, 60f);
            camera.position.Z = 10f;
            camera.position.Y = 10f;

            scene.DirectionalLight.rotation.X = -90f;
            CursorState = CursorState.Grabbed;
        }

        // --- MÉTODOS MOVIDOS PARA FORA DO CONSTRUTOR ---

        /// <summary>
        /// Converte uma coordenada de célula do grid (ex: 0,0) para uma 
        /// posição no centro dessa célula no mundo 3D.
        /// </summary>
        private Vector3 GridToWorld(int gridX, int gridZ, float yPosition = 0f)
        {
            // Fórmula: (CoordenadaDoGrid - (TotalDeCélulas / 2)) + 0.5 (para o centro)
            // Cada célula tem 1.0 de largura/profundidade
            float worldX = (float)gridX - (gridWidth / 2f) + 0.5f;
            float worldZ = (float)gridZ - (gridDepth / 2f) + 0.5f;

            return new Vector3(worldX, yPosition, worldZ);
        }

        /// <summary>
        /// Tenta criar um objeto em uma célula específica do grid e retorna o Drawable3D.
        /// </summary>
        private Drawable3D SpawnObjectAt(int gridX, int gridZ, Mesh mesh, Material material, float yPosition = 0f)
        {
            // 1. Verifica se a célula é válida e está vazia
            if (gridX < 0 || gridX >= GRID_CELLS_X || gridZ < 0 || gridZ >= GRID_CELLS_Z)
            {
                Console.WriteLine($"Erro: Tentativa de criar fora do grid em [{gridX}, {gridZ}]");
                return null;
            }

            if (logicalGrid[gridX, gridZ] != 0)
            {
                Console.WriteLine($"Erro: Célula [{gridX}, {gridZ}] já está ocupada.");
                return null;
            }

            // 2. Converte a coordenada do grid para a posição do mundo
            Vector3 worldPosition = GridToWorld(gridX, gridZ, yPosition);

            // 3. Usa sua função auxiliar existente para criar o objeto
            Drawable3D newDrawable = AdicionarObjetoNaCena(material, mesh, worldPosition);

            // 4. Marca a célula como ocupada no grid lógico
            logicalGrid[gridX, gridZ] = 1; // 1 = Ocupado
            
            return newDrawable;
        }

        /// <summary>
        /// Função auxiliar para adicionar objetos na cena. Agora retorna o Drawable3D.
        /// </summary>
        Drawable3D AdicionarObjetoNaCena(Material material, Mesh mesh, Vector3 position, float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f)
        {
            Drawable3D drawable = new(material, mesh);
            drawable.Transform.position = position;
            drawable.Transform.rotation.X = rotationX;
            drawable.Transform.rotation.Y = rotationY;
            drawable.Transform.rotation.Z = rotationZ;
            scene.AddDrawable(drawable);
            return drawable; // Retorna o objeto criado
        }

        // --------------------------------------------------

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float delta = (float)args.Time;

            // --- Movimentação da Câmera ---
            float cameraDelta = delta * 5f;
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                camera.position += camera.Forward * cameraDelta;
            }
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                camera.position -= camera.Forward * cameraDelta;
            }
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                camera.position += camera.Right * cameraDelta;
            }
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                camera.position -= camera.Right * cameraDelta;
            }
            if (KeyboardState.IsKeyDown(Keys.E))
            {
                camera.position += camera.Up * cameraDelta;
            }
            if (KeyboardState.IsKeyDown(Keys.Q))
            {
                camera.position -= camera.Up * cameraDelta;
            }

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                CursorState = CursorState.Normal;
            }

            // --- Rotação da Câmera ---
            camera.rotation.Y -= MouseState.Delta.X * 0.1f;
            camera.rotation.X -= MouseState.Delta.Y * 0.1f;
            camera.rotation.X = MathF.Max(MathF.Min(camera.rotation.X, 90f), -90f);

            // --- Lógica de Limite do Grid ---
            float halfGridWidth = gridWidth / 2f;
            float halfGridDepth = gridDepth / 2f;
            camera.position.X = MathHelper.Clamp(camera.position.X, -halfGridWidth, halfGridWidth);
            camera.position.Z = MathHelper.Clamp(camera.position.Z, -halfGridDepth, halfGridDepth);

            // --- Rotação de Objetos ---
            //scene.DirectionalLight.rotation.X += delta * 5f;

            // Verifica se drawable1 não é nulo antes de tentar girá-lo
            if (drawable1 != null)
            {
                drawable1.Transform.rotation.Y += delta * 10f;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            scene.Draw(camera);

            SwapBuffers();
        }
    }
}