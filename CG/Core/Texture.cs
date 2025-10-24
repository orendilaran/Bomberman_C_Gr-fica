using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace CG.Core
{
    //  Classe de configuração dos parâmetros de textura, com ela podemos escolher se a textura deve ser repetida ou espelhada
    //na horizontal ou vertical. Podemos também escolher o que deve acontecer quando a textura é desenhada em um tamanho maior
    //ou menor que o original.
    internal class TextureSettings
    {
        private List<(TextureParameterName, int)> _settings = new();

        public static TextureSettings Default
        {
            get
            {
                TextureSettings settings = new();

                settings.AddSetting(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                settings.AddSetting(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                settings.AddSetting(TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                settings.AddSetting(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                return settings;
            }
        }

        public void AddSetting(TextureParameterName name, int value)
        {
            _settings.Add((name, value));
        }

        public void Apply()
        {
            foreach (var setting in _settings)
            {
                GL.TexParameter(TextureTarget.Texture2D, setting.Item1, setting.Item2);
            }
        }
    }

    // Classe de textura, responsável pelo envio dos dados de textura para a placa de vídeo
    internal class Texture
    {
        int _id;

        public int Id => _id;

        public Texture(string path) : this(path, TextureSettings.Default) { }

        //  Construtor para o carregamento de uma textura a partir de um arquivo no computador
        public Texture(string path, TextureSettings settings)
        {
            ImageResult result = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

            _id = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _id);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, result.Width, result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, result.Data);
            settings.Apply();
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        // Construtor para a criação de uma textura em branco, com largura e altura especificada
        public Texture(int width, int height, TextureSettings settings, bool depth = false)
        {
            _id = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _id);


            PixelInternalFormat internalFormat = depth ? PixelInternalFormat.DepthComponent : PixelInternalFormat.Rgba;
            PixelFormat format = depth ? PixelFormat.DepthComponent : PixelFormat.Rgba;
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, PixelType.UnsignedByte, IntPtr.Zero);
            settings.Apply();
            uint[] data = { ~0u };
            GL.ClearTexImage(_id, 0, format, PixelType.UnsignedByte, data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Use(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            GL.BindTexture(TextureTarget.Texture2D, _id);
        }

        public void Delete()
        {
            GL.DeleteTexture(_id);
        }
    }
}
