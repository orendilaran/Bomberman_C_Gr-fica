using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Assimp.Unmanaged;
using CG.Core;
using CG.Drawables;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace CG
{
    internal class Game : GameWindow
    {
        Mesh mesh;
        Mesh chão;
        Mesh Muro1;
        Mesh Muro2;
        Mesh Muro3;
        Mesh Muro4;
        Mesh Bomba;
        Mesh gridMesh;
        Mesh Confette;
        Mesh meshplano;
        Material gridMaterial;
        ShaderProgram program;
        Texture texture;
        Texture texture3;
        Texture Confete;
        Drawable3D drawable1;
        Drawable3D drawable2;
        Drawable3D player1Drawable; 
        Drawable3D player2Drawable; 
        Drawable3D bombaPlayer1;
        Drawable3D bombaPlayer2;
        Camera camera;
        Material material1;
        Material material2;
        Material material3;
        Material material4;
        Material material5; 
        Material material6; 
        Material material7;
        Material matConfete;
        Material matConfeteRed;
        Material matConfeteBlue;
        Material matimpacto;
        Material matexplosao;
        Scene scene = new();

        // timer e logica bomba
        private bool limiteP1 = false;
        private bool limiteP2 = false;

        private float timer1 = 300;
        private float timer2 = 300;

        private float timerexplosao1 = 100;
        private float timerexplosao2 = 100;
        
        private Random random = new(); 

        // --- LÓGICA DO GRID CORRIGIDA ---
        private const int GRID_CELLS_X = 13; 
        private const int GRID_CELLS_Z = 20; 

        private float gridWidth = GRID_CELLS_X;
        private float gridDepth = GRID_CELLS_Z;

        private int[,] logicalGrid; 

        private int[,] positionbloco;
        private int NumeroTopIndex;

        List<Drawable3D> ListaBlocoDestrutiveis;
        
        private int p1GridX = 0;
        private int p1GridZ = 0;
        private int p2GridX = GRID_CELLS_X - 1;
        private int p2GridZ = GRID_CELLS_Z - 1;

        private bool acaboujogo = false;
        private bool N = false; //cima
        private bool S = false; //baixo
        private bool E = false; //direita
        private bool W = false; //esquerda
        private int gambiarra;
        private int gambiarra2;

        private bool criado;
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            logicalGrid = new int[GRID_CELLS_X, GRID_CELLS_Z];
            positionbloco = new int[GRID_CELLS_X+50, GRID_CELLS_Z+50];
            ListaBlocoDestrutiveis = new List<Drawable3D>();
            NumeroTopIndex = 0;
            gambiarra = 0;
            gambiarra2 = 0;
            criado = false;
            // --- Geometria (Meshes) ---
            Muro1 = Primitive.CreateRectangularPrism(1f, 0.5f, gridDepth + 2); 
            Muro2 = Primitive.CreateRectangularPrism(gridWidth, 0.5f, 1f); 
            Muro3 = Primitive.CreateRectangularPrism(1f, 0.5f, gridDepth + 2); 
            Muro4 = Primitive.CreateRectangularPrism(gridWidth, 0.5f, 1f); 
            
            chão = Primitive.CreatePlane(gridWidth, gridDepth); 
            mesh = Primitive.CreateCube(1f); 
            gridMesh = Primitive.CreateGrid(gridWidth, gridDepth, GRID_CELLS_X, GRID_CELLS_Z); 
            Bomba = Primitive.CreateSphere(0.5f);
            Confette = Primitive.CreatePlane(0.12f);
            meshplano = Primitive.CreatePlane(1f);


            // --- Shaders ---
            Shader shaderVertex = Shader.LoadFromFile("./assets/shaders/shader.vert", ShaderType.VertexShader);
            Shader shaderFragment = Shader.LoadFromFile("./assets/shaders/shader.frag", ShaderType.FragmentShader);
            program = new ShaderProgram(shaderVertex, shaderFragment);
            shaderVertex.Delete();
            shaderFragment.Delete();

            // --- Texturas ---
            texture = new Texture("./assets/textures/image.jpg");
            texture3 = new Texture("./assets/textures/tijolo.jpeg");
            Confete = new Texture("./assets/textures/tijolo.jpeg");
            Texture texture2 = new Texture("./assets/textures/house.png");
            Texture impacto = new Texture("./assets/textures/impacto.png");
            Texture explosao = new Texture("./assets/textures/explosao.png");

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

            material7 = new Material(program);
            material7.SetFloat("u_SpecularIntensity", 0.4f);
            material7.SetVec3("u_Color", 0.05f, 0.05f, 0.1f); 

            matConfete = new Material(program);
            matConfete.SetFloat("u_SpecularIntensity", 0f);
            matConfete.SetVec3("u_Color", 0f, 1f, 0f);
            matConfete.SetTexture("u_Texture", Confete);

            matConfeteRed = new Material(program);
            matConfeteRed.SetFloat("u_SpecularIntensity", 0f);
            matConfeteRed.SetVec3("u_Color", 1f, 0f, 0f);
            matConfeteRed.SetTexture("u_Texture", Confete);

            matConfeteBlue = new Material(program);
            matConfeteBlue.SetFloat("u_SpecularIntensity", 0f);
            matConfeteBlue.SetVec3("u_Color", 0f, 0f, 1f);
            matConfeteBlue.SetTexture("u_Texture", Confete);

            matimpacto = new Material(program);
            matimpacto.SetFloat("u_SpecularIntensity", 0.05f);
            matimpacto.SetVec3("u_Color", 1f, 1f, 1f);
            matimpacto.SetTexture("u_Texture", impacto);

            matexplosao = new Material(program);
            matexplosao.SetFloat("u_SpecularIntensity", 0.05f);
            matexplosao.SetVec3("u_Color", 1f, 1f, 1f);
            matexplosao.SetTexture("u_Texture", explosao);

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

            // Coloca Bombas no void

           bombaPlayer1  = AdicionarObjetoNaCena(material7,Bomba,new Vector3(-20f,-20f,-20f));
           bombaPlayer2  = AdicionarObjetoNaCena(material7,Bomba,new Vector3(-20f,-20f,-20f));
            
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
            positionbloco[(int)position.X+20,(int)position.Z+20] = NumeroTopIndex;
            NumeroTopIndex = NumeroTopIndex+1;
            ListaBlocoDestrutiveis.Add(drawable);

            return drawable; 
        }

        private void HandlePlayerMovement(Keys up, Keys down, Keys left, Keys right, float time, ref int gridX, ref int gridZ, Drawable3D playerDrawable, string playerName)
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
                //Console.WriteLine($"{playerName} colidiu em [{newX}, {newZ}] (Valor: {logicalGrid[newX, newZ]})");
                return;
            }

            logicalGrid[gridX, gridZ] = 0;

            gridX = newX;
            gridZ = newZ;

            logicalGrid[gridX, gridZ] = 3;

            playerDrawable.Transform.position = GridToWorld(gridX, gridZ, 0.5f);
            
        }

        // --------------------------------------------------
        private void SpawnaBomba(float x,float z,Drawable3D aBomba)
        {
            aBomba.Transform.position = new Vector3 (x, 0f, z);
        } 
        // --------------------------------------------------

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float delta = (float)args.Time;
            if (!acaboujogo){
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
            
            HandlePlayerMovement(Keys.W, Keys.S, Keys.A, Keys.D, delta, ref p1GridX, ref p1GridZ, player1Drawable, "Player 1");
            
            HandlePlayerMovement(Keys.Up, Keys.Down, Keys.Left, Keys.Right, delta, ref p2GridX, ref p2GridZ, player2Drawable, "Player 2");

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
            if (player1Drawable == null || player2Drawable == null)
            {
                Drawable3D Confeteria = AdicionarObjetoNaCena(matConfete, Confette, new Vector3(0f,0f,0f));
            }


            // Controla As Bombas
            if (KeyboardState.IsKeyDown(Keys.E) && limiteP1 == false)
            {
                SpawnaBomba(player1Drawable.Transform.position.X,player1Drawable.Transform.position.Z,bombaPlayer1);
                limiteP1 = true;
                timer1 = 1;
            }
            if (KeyboardState.IsKeyDown(Keys.Space) && limiteP2 == false)
            {
                SpawnaBomba(player2Drawable.Transform.position.X,player2Drawable.Transform.position.Z,bombaPlayer2);
                limiteP2 = true;
                timer2 = 1;
            }

            // Animações e limitador de bombas, tambem a explosão
            if (limiteP1 == true)
            {
                logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth-0.5)] = 4;
                timer1 -= delta;
                if (timer1 <= 0)
                {
                    //cria explosão
                    for (int i = 1; i < 4;)
                    {
                        // pega as posições de ate 3 de distancia no grid dos adjacentes
                        if(!S && (int)bombaPlayer1.Transform.position.X+i +(int)halfGridWidth< gridWidth){
                        if (logicalGrid[(int)bombaPlayer1.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] != 0){
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] == 2){S = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] == 1){S = true;
                            logicalGrid[(int)bombaPlayer1.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer1.Transform.position.X+i+20,(int)bombaPlayer1.Transform.position.Z+20]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        if(!N && (int)bombaPlayer1.Transform.position.X-i +(int)halfGridWidth>=0){
                        if(logicalGrid[(int)bombaPlayer1.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] != 0){
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] == 2){N = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] == 1){N = true;
                            logicalGrid[(int)bombaPlayer1.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer1.Transform.position.X-i+20,(int)bombaPlayer1.Transform.position.Z+20]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        if(!E&&bombaPlayer1.Transform.position.Z+i+halfGridDepth<gridDepth){
                        if(logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth+1)] != 0){
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth+i)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth+i)] == 2){E = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth+i)] == 1){E = true;
                            logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth+i)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer1.Transform.position.X+20,(int)(bombaPlayer1.Transform.position.Z+i+20)]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        if(!W&&bombaPlayer1.Transform.position.Z-i+halfGridDepth>=0){
                        if(logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth-i)] != 0){
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth-i)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth-i)] == 2){W = true;}
                            if (logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth-i)] == 1){W = true;
                            logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth-i)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer1.Transform.position.X+20,(int)(bombaPlayer1.Transform.position.Z-i+20)]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        // Coloca sprite da explosão na coordenada
                        if (!N){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer1.Transform.position.X-i,1f,bombaPlayer1.Transform.position.Z));
                        gambiarra++;
                        }
                        if (!S){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer1.Transform.position.X+i,1f,bombaPlayer1.Transform.position.Z));
                        gambiarra++;
                        }
                        if (!E){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer1.Transform.position.X,1f,bombaPlayer1.Transform.position.Z+i),0f,90f);
                        gambiarra++;
                        }
                        if (!W){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer1.Transform.position.X,1f,bombaPlayer1.Transform.position.Z-i),0f,-90f);
                        gambiarra++;
                        }
                        i++;
                        timerexplosao1 = 0.15f;
                    }
                    // faz sumir a bomba
                    AdicionarObjetoNaCena(matexplosao, meshplano, new Vector3(bombaPlayer1.Transform.position.X,1.05f,bombaPlayer1.Transform.position.Z));
                    gambiarra++;
                    logicalGrid[(int)bombaPlayer1.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer1.Transform.position.Z+halfGridDepth-0.5)] = 0;
                    bombaPlayer1.Transform.position = new Vector3 (-10f,-10f,-10f);
                    limiteP1 = false;
                    N = false;
                    S = false;
                    E = false;
                    W = false;   
                }
            }
            if (limiteP2 == true)
            {
                logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth-0.5)] = 4;
                timer2 -= delta;
                if (timer2 <= 0)
                {
                    //cria explosão
                    for (int i = 1; i < 4;)
                    {
                        // pega as posições de ate 3 de distancia no grid dos adjacentes
                        if(!S && (int)bombaPlayer2.Transform.position.X+i +(int)halfGridWidth< gridWidth){
                        if (logicalGrid[(int)bombaPlayer2.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] != 0){
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] == 2){S = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] == 1){S = true;
                            logicalGrid[(int)bombaPlayer2.Transform.position.X+i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer2.Transform.position.X+i+20,(int)bombaPlayer2.Transform.position.Z+20]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        if(!N && (int)bombaPlayer2.Transform.position.X-i +(int)halfGridWidth>=0){
                        if(logicalGrid[(int)bombaPlayer2.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] != 0){
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] == 2){N = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] == 1){N = true;
                            logicalGrid[(int)bombaPlayer2.Transform.position.X-i+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer2.Transform.position.X-i+20,(int)bombaPlayer2.Transform.position.Z+20]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        if(!E&&bombaPlayer2.Transform.position.Z+i+halfGridDepth<gridDepth){
                        if(logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth+1)] != 0){
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth+i)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth+i)] == 2){E = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth+i)] == 1){E = true;
                            logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth+i)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer2.Transform.position.X+20,(int)(bombaPlayer2.Transform.position.Z+i+20)]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        if(!W&&bombaPlayer2.Transform.position.Z-i+halfGridDepth>=0){
                        if(logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth-i)] != 0){
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth-i)] == 3){acaboujogo = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth-i)] == 2){W = true;}
                            if (logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth-i)] == 1){W = true;
                            logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth-i)] = 0;
                            ListaBlocoDestrutiveis[positionbloco[(int)bombaPlayer2.Transform.position.X+20,(int)(bombaPlayer2.Transform.position.Z-i+20)]].Transform.position = new Vector3(-10f,-10f,-10f);
                            }
                        }}
                        // Coloca sprite da explosão na coordenada
                        if (!N){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer2.Transform.position.X-i,1f,bombaPlayer2.Transform.position.Z));
                        gambiarra2++;
                        }
                        if (!S){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer2.Transform.position.X+i,1f,bombaPlayer2.Transform.position.Z));
                        gambiarra2++;
                        }
                        if (!E){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer2.Transform.position.X,1f,bombaPlayer2.Transform.position.Z+i),0f,90f);
                        gambiarra2++;
                        }
                        if (!W){AdicionarObjetoNaCena(matimpacto, meshplano, new Vector3(bombaPlayer2.Transform.position.X,1f,bombaPlayer2.Transform.position.Z-i),0f,-90f);
                        gambiarra2++;
                        }
                        i++;
                        timerexplosao2 = 0.15f;
                    }
                    // faz sumir a bomba
                    AdicionarObjetoNaCena(matexplosao, meshplano, new Vector3(bombaPlayer2.Transform.position.X,1.05f,bombaPlayer2.Transform.position.Z));
                    gambiarra2++;
                    logicalGrid[(int)bombaPlayer2.Transform.position.X+(int)halfGridWidth,(int)(bombaPlayer2.Transform.position.Z+halfGridDepth-0.5)] = 0;
                    bombaPlayer2.Transform.position = new Vector3 (-10f,-10f,-10f);
                    limiteP2 = false;
                    N = false;
                    S = false;
                    E = false;
                    W = false;   
                }
            }
            if(timerexplosao1 > 0){timerexplosao1 -= delta;}
            while (gambiarra >0 && timerexplosao1 <=0){
                ListaBlocoDestrutiveis[NumeroTopIndex-gambiarra].Transform.position = new Vector3(-10f,-10f,-10f);
                gambiarra--;}
            if(timerexplosao2 > 0){timerexplosao2 -= delta;}
            while (gambiarra2 >0 && timerexplosao2 <=0){
                ListaBlocoDestrutiveis[NumeroTopIndex-gambiarra2].Transform.position = new Vector3(-10f,-10f,-10f);
                gambiarra2--;}
            }
            else if (acaboujogo)
            {
                int Confetes = 150;
                while (Confetes >0)
                {
                    
                    while(!criado){
                    AdicionarObjetoNaCena(matConfete,Confette, new Vector3(random.NextSingle() * gridWidth - gridWidth/2,25f ,-gridDepth/2 - (random.NextSingle() * 10f-2)));
                    AdicionarObjetoNaCena(matConfeteRed,Confette, new Vector3(random.NextSingle() * gridWidth - gridWidth/2,25f ,-gridDepth/2 - (random.NextSingle() * 10f)));
                    AdicionarObjetoNaCena(matConfeteBlue,Confette, new Vector3(random.NextSingle() * gridWidth - gridWidth/2,25f ,-gridDepth/2 - (random.NextSingle() * 10f-5)));
                    Confetes--;
                    if (Confetes <= 0) {criado= true;}}
                    while(Confetes > 0 && criado){
                    ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.position = new Vector3(ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.position.X + (random.NextSingle() -0.45f)*0.01f,25f,ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.position.Z + (random.NextSingle() -0.25f)*0.03f);
                    ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.rotation = new Vector3(ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.rotation.X + (random.NextSingle() -0.45f)*0.05f,ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.rotation.Y + (random.NextSingle() -0.45f)*0.5f,0f);
                    if (ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.position.Z > gridDepth/2-4.3){ListaBlocoDestrutiveis[NumeroTopIndex-Confetes].Transform.position = new Vector3(random.NextSingle() * gridWidth - gridWidth/2,25f,-gridDepth +14);}
                     ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.position = new Vector3(ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.position.X + (random.NextSingle() -0.45f)*0.01f,25f,ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.position.Z + (random.NextSingle() -0.25f)*0.03f);
                    ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.rotation = new Vector3(ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.rotation.X + (random.NextSingle() -0.55f)*0.75f,ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.rotation.Y + (random.NextSingle() -0.45f)*0.5f,0f);
                    if (ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.position.Z > gridDepth/2-4.3){ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-150].Transform.position = new Vector3(random.NextSingle() * gridWidth - gridWidth/2,25f,-gridDepth +14);}
                     ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.position = new Vector3(ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.position.X + (random.NextSingle() -0.45f)*0.01f,25f,ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.position.Z + (random.NextSingle() -0.25f)*0.03f);
                    ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.rotation = new Vector3(ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.rotation.X + (random.NextSingle() -0.65f)*0.05f,ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.rotation.Y + (random.NextSingle() -0.45f)*0.5f,0f);
                    if (ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.position.Z > gridDepth/2-4.3){ListaBlocoDestrutiveis[NumeroTopIndex-Confetes-300].Transform.position = new Vector3(random.NextSingle() * gridWidth - gridWidth/2,25f,-gridDepth +14);}
                    Confetes--;
                    }                  
                }
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