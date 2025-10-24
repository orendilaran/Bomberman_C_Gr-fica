using OpenTK.Mathematics;

namespace CG.Core
{
    // Classe para facilitar uso e acesso à transformações dos objetos no mundo, que utiliza seus componentes de
    //posição, rotação e escala para gerar uma matriz de transformação, que pode ser utilizada nos shaders.
    internal class Transform
    {
        public Vector3 position = new(0f);
        public Vector3 rotation = new(0f);
        public Vector3 scale = new(1f);

        public Matrix4 TranslationMatrix => Matrix4.CreateTranslation(position);
        public Matrix4 RotationMatrix =>
            Matrix4.CreateRotationX(rotation.X * (MathF.PI / 180.0f)) *
            Matrix4.CreateRotationY(rotation.Y * (MathF.PI / 180.0f)) *
            Matrix4.CreateRotationZ(rotation.Z * (MathF.PI / 180.0f))
        ;        
        public Matrix4 InverseRotationMatrix =>
            Matrix4.CreateRotationZ(-rotation.Z * (MathF.PI / 180.0f)) *
            Matrix4.CreateRotationY(-rotation.Y * (MathF.PI / 180.0f)) *
            Matrix4.CreateRotationX(-rotation.X * (MathF.PI / 180.0f))
        ;
        public Matrix4 ScaleMatrix => Matrix4.CreateScale(scale);

        public Matrix4 ModelMatrix => ScaleMatrix * RotationMatrix * TranslationMatrix;

        public Vector3 Right => (new Vector4(Vector3.UnitX, 1.0f) * RotationMatrix).Xyz;
        public Vector3 Up => (new Vector4(Vector3.UnitY, 1.0f) * RotationMatrix).Xyz;
        public Vector3 Forward => (new Vector4(-Vector3.UnitZ, 1.0f) * RotationMatrix).Xyz;
    }
}
