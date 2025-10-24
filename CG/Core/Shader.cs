using OpenTK.Graphics.OpenGL;

namespace CG.Core
{
    // Classe de shader, representando um único arquivo de shader, seja do tipo vértice, fragmento ou outro.
    internal class Shader
    {
        int _id;
        public int Id => _id;

        public Shader(string source, ShaderType type)
        {
            _id = GL.CreateShader(type);
            GL.ShaderSource(_id, source);
            GL.CompileShader(_id);
            GL.GetShader(_id, ShaderParameter.CompileStatus, out int param);
            if (param != (int)All.True)
            {
                Console.WriteLine($"Erro de compilação de shader({type}): {GL.GetShaderInfoLog(_id)}");
            }
        }

        public static Shader LoadFromFile(string path, ShaderType type)
        {
            return new Shader(File.ReadAllText(path), type);
        }

        public void Delete()
        {
            GL.DeleteShader(_id);
        }
    }
}
