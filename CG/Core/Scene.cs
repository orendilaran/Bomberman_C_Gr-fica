using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CG.Core
{
    // Classe de Cena, usada para organizar o desenho de um conjunto de objetos. Usada principalmente para permitir
    //o desenho de um mesmo conjunto de objetos com diferentes perspectivas e materiais.
    internal class Scene
    {
        private List<Drawable> _drawables = new List<Drawable>();
        private List<ShaderProgram> _programs = new List<ShaderProgram>();

        private DirectionalLight _directionalLight = new DirectionalLight();
        public DirectionalLight DirectionalLight => _directionalLight;
        public Vector3 ambientLightColor = new Vector3(0.2f, 0.2f, 0.4f);

        public Scene() { }

        public void AddDrawable(Drawable drawable)
        {
            if (_drawables.Contains(drawable))
            {
                return;
            }
            _drawables.Add(drawable);
            ShaderProgram? program = drawable.material?.Program;
            if (program != null && !_programs.Contains(program))
            {
                _programs.Add(program);
            }
        }

        public void RemoveDrawable(Drawable drawable)
        {
            if (_drawables.Contains(drawable))
            {
                _drawables.Remove(drawable);
            }
        }

        private void SetupProgram(ShaderProgram program, Camera camera)
        {
            program.ApplyCamera(camera);
            program.ApplyDirectionalLight(_directionalLight);
            program.SetUniform("u_AmbientColor", ambientLightColor);
        }

        public void Draw(Camera camera, Material? materialOverride = null)
        {
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);

            // Caso estejamos usando um material único para toda a cena, fazemos o setup exclusivo dele.
            if (materialOverride != null)
            {
                SetupProgram(materialOverride.Program, camera);
            }
            else
            {
                foreach (var program in _programs)
                {
                    SetupProgram(program, camera);
                }
            }

            // Determinamos a cor de limpeza da tela e, na sequência, pedimos pra limpar os canais de cor e profundidade.
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Desenhamos cada um dos desenháveis registrados na cena.
            foreach (var drawable in _drawables)
            {
                drawable.Draw(materialOverride);
            }
        }
    }
}
