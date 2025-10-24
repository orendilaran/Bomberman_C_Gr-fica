using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CG.Core
{
    //  Classe de material. Materiais são uma coleção de parâmetros de variáveis uniforme que deixam mais prático o processo de
    //desenho dos objetos. Com materiais, não precisamos mais atribuir os valores de cada uma das variáveis individualmente.
    internal class Material
    {
        private static Texture? _defaultTexture = null;

        private Dictionary<string, int> _intUniforms = new();
        private Dictionary<string, float> _floatUniforms = new();
        private Dictionary<string, Vector2> _vec2Uniforms = new();
        private Dictionary<string, Vector3> _vec3Uniforms = new();
        private Dictionary<string, Vector4> _vec4Uniforms = new();
        private Dictionary<string, Texture> _textureUniforms = new();

        private ShaderProgram _program;
        public ShaderProgram Program => _program;

        // Como algumas variáveis uniforme não são de responsabilidade do material, mas são compartilhadas, fazemos
        //uma checagem por nomes que não devem ser adicionados à lista do material.
        private bool IsNameValid(string name)
        {
            string[] illegalNames =
            [
                "u_Model",
                "u_Rotation",
                "u_View",
                "u_Projection",
                "u_ViewPosition",
                "u_ViewInverseRotation",
                "u_AmbientColor",
                "u_LightDirection",
                "u_LightColor",
                "u_LightMap",
            ];
            return !illegalNames.Contains(name);
        }

        public Material(ShaderProgram program)
        {
            _program = program;

            GL.GetProgram(_program.Id, GetProgramParameterName.ActiveUniforms, out int count);
            for (int i = 0; i < count; i++)
            {
                GL.GetActiveUniform(_program.Id, i, 100, out int _, out int _, out ActiveUniformType type, out string name);
                if(!IsNameValid(name))
                {
                    continue;
                }

                switch (type)
                {
                    case ActiveUniformType.Int:
                        SetInt(name, 0);
                        break;
                    case ActiveUniformType.Float:
                        SetFloat(name, 0f);
                        break;
                    case ActiveUniformType.FloatVec2:
                        SetVec2(name, new Vector2(0f, 0f));
                        break;
                    case ActiveUniformType.FloatVec3:
                        SetVec3(name, new Vector3(1f));
                        break;
                    case ActiveUniformType.FloatVec4:
                        SetVec4(name, new Vector4(1f));
                        break;
                    case ActiveUniformType.Sampler2D:
                        if (_defaultTexture == null)
                        {
                            _defaultTexture = new Texture(1, 1, TextureSettings.Default);
                        }
                        SetTexture(name, _defaultTexture);
                        break;
                }
            }
        }

        public void Use()
        {
            _program.Use();
            foreach (var uniform in _intUniforms)
            {
                _program.SetUniform(uniform.Key, uniform.Value);
            }
            foreach (var uniform in _floatUniforms)
            {
                _program.SetUniform(uniform.Key, uniform.Value);
            }
            foreach (var uniform in _vec2Uniforms)
            {
                _program.SetUniform(uniform.Key, uniform.Value);
            }
            foreach (var uniform in _vec3Uniforms)
            {
                _program.SetUniform(uniform.Key, uniform.Value);
            }
            foreach (var uniform in _vec4Uniforms)
            {
                _program.SetUniform(uniform.Key, uniform.Value);
            }

            //  Texturas precisam ser tratadas de forma especial, pois não as passamos elas diretamente para a placa de vídeo
            //por questão de performance. Aqui, colocamos cada uma das texturas em uma unit, uma "caixinha", e falamos para o
            //shader de qual "caixinha" ele deve pegar a textura.
            {
                int i = 0;
                foreach (var uniform in _textureUniforms)
                {
                    uniform.Value.Use(i);
                    _program.SetUniform(uniform.Key, i);
                    i++;
                }
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public void SetInt(string name, int value)
        {
            _intUniforms[name] = value;
        }

        public void SetFloat(string name, float value)
        {
            _floatUniforms[name] = value;
        }

        public void SetVec2(string name, Vector2 value)
        {
            _vec2Uniforms[name] = value;
        }

        public void SetVec2(string name, float x, float y)
        {
            SetVec2(name, new Vector2(x, y));
        }

        public void SetVec3(string name, Vector3 value)
        {
            _vec3Uniforms[name] = value;
        }

        public void SetVec3(string name, float x, float y, float z)
        {
            SetVec3(name, new Vector3(x, y, z));
        }

        public void SetVec4(string name, Vector4 value)
        {
            _vec4Uniforms[name] = value;
        }

        public void SetVec4(string name, float x, float y, float z, float w)
        {
            SetVec4(name, new Vector4(x, y, z, w));
        }

        public void SetTexture(string name, Texture value)
        {
            _textureUniforms[name] = value;
        }
    }
}
