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
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            Muro1 = Primitive.CreateRectangularPrism(2f,50f,1f);
            chão = Primitive.CreatePlane(100f,100f);
            mesh = Primitive.CreateCube(2f);
            //mesh2 = Primitive.CreateSphere(1f, 32, 16);
            mesh2 = Mesh.LoadFromFile("./assets/models/model.glb")[0];

            //  Agora que temos uma malha descrita, precisamos explicar para a placa de vídeo como desenhar os vértices. É aí que
            //entram os shaders, programinhas que transformam os dados da malha em triângulos coloridos na tela! Existem vários
            //tipos de shaders, e os mais essenciais são os shaders de vértice e fragmento.

            //  O vertex shader é responsável por "traduzir" os dados da malha em coordenadas de tela. Para a openGL, triângulos na tela
            //são desenhados utilizando coordenadas normalizadas, que vão de -1 a 1 horizontalmente(esquerda para a direita) e também de
            //-1 a 1 verticalmente(baixo para o topo).
            //  Neste shader apenas lemos o valor da posição da malha, que recebemos ao utilizar a palavra-chave "in" e "location = 0",
            //o id que definimos na função GL.VertexAttribPointer. Passamos o valor diretamente para a variável gl_Position, que indica
            //a posição de tela do vértice. Note que gl_Position é um vetor 4D, isso será importante quando passarmos para o mundo 3D!
            //  O vertex shader é executado uma vez para cada vértice da malha. No nosso caso, será executado até 6 vezes(possívelmente
            //menos devido ao processo de caching de resultados), 3 para cada um dos 2 triângulos do retângulo.
            Shader shaderVertex = Shader.LoadFromFile("./assets/shaders/shader.vert", ShaderType.VertexShader);

            //  E agora o Fragment Shader, responsável por definir a cor dos píxels de um triângulo. Nele, temos uma variável de saída,
            //out_Color, indicada pela keyword out, que vai definir a cor a ser desenhada na tela. No momento, estamos definindo a cor
            //como (r=0, g=0, b=1, a=1), ou seja, azul.
            //  O fragment shader é executado uma vez para cada fragmento/pixel de um triângulo.
            Shader shaderFragment = Shader.LoadFromFile("./assets/shaders/shader.frag", ShaderType.FragmentShader);


            //  Após a compilação dos dois shaders, os combinamos em um programa só, para que sejam executados em sequência. Um programa
            //de shader pode falhar em seu processo de Link, caso os shaders não sejam compatíveis uns com os outros.
            program = new ShaderProgram(shaderVertex, shaderFragment);

            //  Após a criação do programa, não precisamos mais dos shaders individuais e, por isso, os deletamos.
            shaderVertex.Delete();
            shaderFragment.Delete();

            //  Carregamento de textura do disco
            texture = new Texture("./assets/textures/image.jpg");
            texture3 = new Texture("./assets/textures/tijolo.jpeg");

            //  Criação de materiais, com os valores para as propriedades do shader.
            material1 = new Material(program);
            material1.SetFloat("u_SpecularIntensity", 0.5f);
            material1.SetVec3("u_Color", 1f, 0f, 0f);
            material1.SetTexture("u_Texture", texture);

            Texture texture2 = new Texture("./assets/textures/house.png");

            //  Como o segundo material não atribui o valor de "u_Texture", ele vai
            //utilizar o valor padrão, que é uma textura branca.
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
           

            drawable1 = new(material1, mesh);
            drawable1.Transform.position = new Vector3(2f, 0f, 1f);
            drawable2 = new(material2, mesh2);

            Drawable3D muro = new(material4, Muro1);
            muro.Transform.position = new Vector3(-5f, 25f, 0f);
            muro.Transform.rotation.X = 0f;

            Drawable3D floor = new(material3, chão);
            floor.Transform.position = new Vector3(0f, -1f, 0f);
            floor.Transform.rotation.X = 0f;

            camera = new PerspectiveCamera((float)Size.X / Size.Y, 60f);
            camera.position.Z = 10f;
            camera.position.Y = 10f;

            //Aqui é pra renderizar os objetos

            scene.AddDrawable(drawable1);
            scene.AddDrawable(drawable2);
            scene.AddDrawable(floor);
            scene.AddDrawable(muro);

            scene.DirectionalLight.rotation.X = -90f;

            CursorState = CursorState.Grabbed;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            //  As funções de IsKeyDown detectam se uma tecla está sendo segurada
            //  Multiplicamos o valor de deslocamento pelo delta time para que o
            //movimento não varie com o frame rate.
            float delta = (float)args.Time;

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

            camera.rotation.Y -= MouseState.Delta.X * 0.1f;
            camera.rotation.X -= MouseState.Delta.Y * 0.1f;
            camera.rotation.X = MathF.Max(MathF.Min(camera.rotation.X, 90f), -90f);

            scene.DirectionalLight.rotation.X += delta * 5f;

            drawable1.Transform.rotation.Y += delta * 10f;
        }

        //  A função OnRenderFrame é executada uma vez a cada frame(visual). Por isso, é aqui que faremos as operações de desenho
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            scene.Draw(camera);

            //  Como todos comandos de desenho são feitos em um buffer, ou tela, secundário, precisamos pedir que os buffers sejam
            //trocados para que o novo desenho seja exibido ao usuário.
            SwapBuffers();
        }
    }
}
