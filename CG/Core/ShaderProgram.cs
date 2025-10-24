using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CG.Core
{
    internal class ShaderProgram
    {
        int _id;
        public int Id => _id;

        public ShaderProgram(params Shader[] shaders)
        {
            _id = GL.CreateProgram();
            foreach (Shader shader in shaders)
            {
                GL.AttachShader(_id, shader.Id);
            }

            GL.LinkProgram(_id);
            GL.GetProgram(_id, GetProgramParameterName.LinkStatus, out int param);
            if (param != (int)All.True)
            {
                Console.WriteLine($"Erro de link do ShaderProgram: {GL.GetProgramInfoLog(_id)}");
            }
        }

        public void Use()
        {
            GL.UseProgram(_id);// A função GL.UseProgram faz com que todas as subsequentes chamadas de desenho utilizem o programa informado.
        }

        public void Delete()
        {
            GL.DeleteProgram(_id);
        }

        public void SetUniform(string name, int value)
        {
            Use();
            GL.Uniform1(GL.GetUniformLocation(Id, name), value);
        }

        public void SetUniform(string name, float value)
        {
            Use();
            GL.Uniform1(GL.GetUniformLocation(Id, name), value);
        }

        public void SetUniform(string name, Vector2 value)
        {
            Use();
            GL.Uniform2(GL.GetUniformLocation(Id, name), value);
        }

        public void SetUniform(string name, Vector3 value)
        {
            Use();
            GL.Uniform3(GL.GetUniformLocation(Id, name), value);
        }

        public void SetUniform(string name, Vector4 value)
        {
            Use();
            GL.Uniform4(GL.GetUniformLocation(Id, name), value);
        }

        public void SetUniform(string name, Matrix3 value)
        {
            Use();
            GL.UniformMatrix3(GL.GetUniformLocation(Id, name), true, ref value);
        }

        public void SetUniform(string name, Matrix4 value)
        {
            Use();
            GL.UniformMatrix4(GL.GetUniformLocation(Id, name), true, ref value);
        }

        public void ApplyTransform(Transform transform)
        {
            SetUniform("u_Model", transform.ModelMatrix);
            SetUniform("u_Rotation", transform.RotationMatrix);
        }

        public void ApplyCamera(Camera camera)
        {
            SetUniform("u_View", camera.ViewMatrix);
            SetUniform("u_Projection", camera.ProjectionMatrix);
            SetUniform("u_ViewPosition", camera.position);
            SetUniform("u_ViewInverseRotation", camera.InverseRotationMatrix);
        }

        public void ApplyDirectionalLight(DirectionalLight light)
        {
            SetUniform("u_LightDirection", light.Direction);
            SetUniform("u_LightColor", light.color);
        }
    }
}
