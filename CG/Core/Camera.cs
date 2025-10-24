using OpenTK.Mathematics;

namespace CG.Core
{
    // Classe de Câmera, para facilitar o uso e acesso às matrizes de visão e projeção.
    // Como herdamos de Transform, a câmera tem os componentes de posição, rotação e escala, embora esta última
    //não seja utilizada.
    internal abstract class Camera : Transform
    {
        public float aspectRatio = 1f;
        public float near = 0.1f;
        public float far = 100f;

        public Camera(float aspectRatio)
        {
            this.aspectRatio = aspectRatio;
        }

        public Matrix4 ViewMatrix => Matrix4.LookAt(position, position + Forward, Up);
        public abstract Matrix4 ProjectionMatrix { get; }
    }

    // Classe de câmera ortográfica, que tem sua projeção feita dentro de um prisma retangular.
    internal class OrthographicCamera : Camera
    {
        public float size = 10f;

        public OrthographicCamera(float aspectRatio, float size) : base(aspectRatio)
        {
            this.size = size;
        }

        public override Matrix4 ProjectionMatrix => Matrix4.CreateOrthographic(aspectRatio * size, size, near, far);
    }

    // Classe de câmera em perspectiva, que usa uma projeção que deixa objetos distantes menores.
    internal class PerspectiveCamera : Camera
    {
        public float fieldOfView = 60f;

        public PerspectiveCamera(float aspectRatio, float fieldOfView) : base(aspectRatio)
        {
            this.fieldOfView = fieldOfView;
        }

        public override Matrix4 ProjectionMatrix => Matrix4.CreatePerspectiveFieldOfView(fieldOfView * (MathF.PI / 180f), aspectRatio, near, far);
    }
}
