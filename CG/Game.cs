using System.Security.Cryptography.X509Certificates;
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
        Mesh Bomba;
        Mesh gridMesh;
        Material gridMaterial;
        ShaderProgram program;
        Texture texture;
        Texture texture3;
        Texture Confete;
        Drawable3D drawable1;
        Drawable3D drawable2;
        
        Drawable3D player1Drawable; 
        Drawable3D player2Drawable; 

        Camera camera;
        Material material1;
        Material material2;
        Material material3;
        Material material4;
        Material material5; 
        Material material6; 
        Scene scene = new();
        
        private Random random = new(); 

        // --- LÓGICA DO GRID CORRIGIDA ---
        private const int GRID_CELLS_X = 13; 
        private const int GRID_CELLS_Z = 20; 

        private float gridWidth = GRID_CELLS_X;
        private float gridDepth = GRID_CELLS_Z;

        private int[,] logicalGrid; 
        
        private int p1GridX = 1;
        private int p1GridZ = 1;
        private int p2GridX = GRID_CELLS_X - 2;
        private int p2GridZ = GRID_CELLS_Z - 2;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            logicalGrid = new int[GRID_CELLS_X, GRID_CELLS_Z];

            // --- Geometria (Meshes) ---
            Muro1 = Primitive.CreateRectangularPrism(1f, 0.5f, gridDepth + 2); 
            Muro2 = Primitive.CreateRectangularPrism(gridWidth, 0.5f, 1f); 
            Muro3 = Primitive.CreateRectangularPrism(1f, 0.5f, gridDepth + 2); 
            Muro4 = Primitive.CreateRectangularPrism(gridWidth, 0.5f, 1f); 
            
            chão = Primitive.CreatePlane(gridWidth, gridDepth); 
            mesh = Primitive.CreateCube(1f); 
            gridMesh = Primitive.CreateGrid(gridWidth, gridDepth, GRID_CELLS_X, GRID_CELLS_Z); 
            mesh2 = Mesh.LoadFromFile("./assets/models/model.glb")[0];
            Bomba = Primitive.CreateSphere(1f);


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
            gridMaterial = new Material(program);
            gridMaterial.SetFloat("u_SpecularIntensity", 0.0f); 
            gridMaterial.SetVec3("u_Color", 0.3f, 0.3f, 0.3f); 
            
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
            
            material4 = new Material(program);
            material4.SetFloat("u_SpecularIntensity", 0.5f);
            material4.SetVec3("u_Color", 0.5f, 0.5f, 0.5f);
            material4.SetTexture("u_Texture", texture3);
            
            material5 = new Material(program);
            material5.SetFloat("u_SpecularIntensity", 0.3f);
            material5.SetVec3("u_Color", 0.2f, 0.2f, 0.2f); 
            
            material6 = new Material(program);
            material6.SetFloat("u_SpecularIntensity", 0.4f);
            material6.SetVec3("u_Color", 0.6f, 0.4f, 0.2f); 

            // --- Criação e Posicionamento de Objetos ---
            
            GenerateHardWalls(); 
            GenerateSoftWalls(); 

            player1Drawable = SpawnPlayer(p1GridX, p1GridZ, material1, 0.5f); 

            Material p2Material = new Material(program);
            p2Material.SetFloat("u_SpecularIntensity", 0.5f);
            p2Material.SetVec3("u_Color", 0f, 0f, 1f); 
            p2Material.SetTexture("u_Texture", texture);

            player2Drawable = SpawnPlayer(p2GridX, p2GridZ, p2Material, 0.5f);
            
            // Objetos do cenário
            AdicionarObjetoNaCena(gridMaterial, gridMesh, new Vector3(0f, 0.01f, 0f)); 
            AdicionarObjetoNaCena(material3, chão, new Vector3(0f, 0f, 0f));
            
            // Posições dos muros
            float halfWidth = gridWidth / 2f;
            float halfDepth = gridDepth / 2f;
            AdicionarObjetoNaCena(material4, Muro1, new Vector3(halfWidth + 0.5f, 1f, 0f)); 
            AdicionarObjetoNaCena(material4, Muro2, new Vector3(0f, 1f, halfDepth + 0.5f)); 
            AdicionarObjetoNaCena(material4, Muro3, new Vector3(-halfWidth - 0.5f, 1f, 0f)); 
            AdicionarObjetoNaCena(material4, Muro4, new Vector3(0f, 1f, -halfDepth - 0.5f)); 
            
            // --- Configuração da Câmera e Cena ---
            camera = new PerspectiveCamera((float)Size.X / Size.Y, 25f);
            camera.position.Z = 0f;
            camera.position.Y = 50.5f;
            camera.rotation.X = -90f;

            scene.DirectionalLight.rotation.X = -90f;
            CursorState = CursorState.Grabbed;
        }

        // --- MÉTODOS MOVIDOS PARA FORA DO CONSTRUTOR ---

        private void GenerateHardWalls()
        {
            for (int x = 1; x < GRID_CELLS_X - 1; x++) 
            {
                for (int z = 1; z < GRID_CELLS_Z - 1; z++)
                {
                    if (x % 2 == 0 && z % 2 == 0)
                    {
                        logicalGrid[x, z] = 2; 

                        Vector3 worldPosition = GridToWorld(x, z, 0.5f); 
                        AdicionarObjetoNaCena(material5, mesh, worldPosition);
                    }
                }
            }
        }

        private void GenerateSoftWalls()
        {
            float probability = 0.7f; 
            
            for (int x = 1; x < GRID_CELLS_X - 1; x++) 
            {
                for (int z = 1; z < GRID_CELLS_Z - 1; z++)
                {
                    if (logicalGrid[x, z] == 0)
                    {
                        if ((x == 1 && z == 2) || (x == 2 && z == 1))
                        {
                            continue; 
                        }
                        
                        int p2x = GRID_CELLS_X - 2;
                        int p2z = GRID_CELLS_Z - 2;
                        
                        if ((x == p2x - 1 && z == p2z) || (x == p2x && z == p2z - 1))
                        {
                            continue; 
                        }
                        
                        if (random.NextSingle() < probability) 
                        {
                            logicalGrid[x, z] = 1; 

                            Vector3 worldPosition = GridToWorld(x, z, 0.5f); 
                            AdicionarObjetoNaCena(material6, mesh, worldPosition);
                        }
                    }
                }
            }
        }
        
        private Drawable3D SpawnPlayer(int gridX, int gridZ, Material material, float yPosition = 0f)
        {
            Drawable3D newDrawable = AdicionarObjetoNaCena(material, mesh, GridToWorld(gridX, gridZ, yPosition));

            logicalGrid[gridX, gridZ] = 3; 
            
            return newDrawable;
        }

        /// <summary>
        /// Converte uma coordenada de célula do grid (ex: 0,0) para uma 
        /// posição no centro dessa célula no mundo 3D.
        /// </summary>
        private Vector3 GridToWorld(int gridX, int gridZ, float yPosition = 0f)
        {
            float worldX = (float)gridX - (gridWidth / 2f) + 0.5f;
            float worldZ = (float)gridZ - (gridDepth / 2f) + 0.5f;

            return new Vector3(worldX, yPosition, worldZ);
        }

        /// <summary>
        /// Tenta criar um objeto em uma célula específica do grid e retorna o Drawable3D.
        /// </summary>
        private Drawable3D SpawnObjectAt(int gridX, int gridZ, Mesh mesh, Material material, float yPosition = 0f)
        {
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

            Vector3 worldPosition = GridToWorld(gridX, gridZ, yPosition);

            Drawable3D newDrawable = AdicionarObjetoNaCena(material, mesh, worldPosition);

            logicalGrid[gridX, gridZ] = 1; 
            
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
            return drawable; 
        }

        private void HandlePlayerMovement(Keys up, Keys down, Keys left, Keys right, ref int gridX, ref int gridZ, Drawable3D playerDrawable, string playerName)
        {
            int newX = gridX;
            int newZ = gridZ;

            if (KeyboardState.IsKeyPressed(up))
            {
                newZ -= 1;
            }
            else if (KeyboardState.IsKeyPressed(down))
            {
                newZ += 1;
            }
            else if (KeyboardState.IsKeyPressed(left))
            {
                newX -= 1;
            }
            else if (KeyboardState.IsKeyPressed(right))
            {
                newX += 1;
            }

            if (newX < 0 || newX >= GRID_CELLS_X || newZ < 0 || newZ >= GRID_CELLS_Z)
            {
                return;
            }

            if (logicalGrid[newX, newZ] != 0)
            {
                Console.WriteLine($"{playerName} colidiu em [{newX}, {newZ}] (Valor: {logicalGrid[newX, newZ]})");
                return;
            }

            logicalGrid[gridX, gridZ] = 0;

            gridX = newX;
            gridZ = newZ;

            logicalGrid[gridX, gridZ] = 3;

            playerDrawable.Transform.position = GridToWorld(gridX, gridZ, 0.5f);
        }

        // --------------------------------------------------
        private void SpawnaBomba(int x,int y)
        {
            
        } 
        // --------------------------------------------------

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float delta = (float)args.Time;

            // --- Movimentação da Câmera --- (Comentei, para vai que precisa no futuro)
           // float cameraDelta = delta * 5f;
           // if (KeyboardState.IsKeyDown(Keys.W) && !KeyboardState.IsKeyPressed(Keys.W)) 
           // {
           //     camera.position += camera.Forward * cameraDelta;
           // }
           // if (KeyboardState.IsKeyDown(Keys.S) && !KeyboardState.IsKeyPressed(Keys.S))
           // {
           //     camera.position -= camera.Forward * cameraDelta;
           // }
           // if (KeyboardState.IsKeyDown(Keys.D) && !KeyboardState.IsKeyPressed(Keys.D))
           // {
           //     camera.position += camera.Right * cameraDelta;
           // }
           // if (KeyboardState.IsKeyDown(Keys.A) && !KeyboardState.IsKeyPressed(Keys.A))
           // {
           //     camera.position -= camera.Right * cameraDelta;
           // }
           // if (KeyboardState.IsKeyDown(Keys.E))
           // {
           //     camera.position += camera.Up * cameraDelta;
           // }
           // if (KeyboardState.IsKeyDown(Keys.Q))
           // {
           //     camera.position -= camera.Up * cameraDelta;
           // }
            
            HandlePlayerMovement(Keys.W, Keys.S, Keys.A, Keys.D, ref p1GridX, ref p1GridZ, player1Drawable, "Player 1");
            
            HandlePlayerMovement(Keys.Up, Keys.Down, Keys.Left, Keys.Right, ref p2GridX, ref p2GridZ, player2Drawable, "Player 2");

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                CursorState = CursorState.Normal;
            }

            // --- Rotação da Câmera --- (Comentei, para vai que precisa no futuro)
           // camera.rotation.Y -= MouseState.Delta.X * 0.1f;
           // camera.rotation.X -= MouseState.Delta.Y * 0.1f;
           // camera.rotation.X = MathF.Max(MathF.Min(camera.rotation.X, 90f), -90f);

            // --- Lógica de Limite do Grid ---
            float halfGridWidth = gridWidth / 2f;
            float halfGridDepth = gridDepth / 2f;
            camera.position.X = MathHelper.Clamp(camera.position.X, -halfGridWidth + 0.5f, halfGridWidth - 0.5f);
            camera.position.Z = MathHelper.Clamp(camera.position.Z, -halfGridDepth + 0.5f, halfGridDepth - 0.5f);

            // --- Rotação de Objetos ---
            //scene.DirectionalLight.rotation.X += delta * 5f;

            if (drawable1 != null)
            {
                drawable1.Transform.rotation.Y += delta * 10f;
            }
            
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            scene.Draw(camera);

            SwapBuffers();
        }
    }
}