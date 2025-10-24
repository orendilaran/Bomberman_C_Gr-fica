using OpenTK.Mathematics;

namespace CG.Core
{
    internal class DirectionalLight : Transform
    {
        public Vector3 Direction => Forward;
        public Vector3 color;

        public DirectionalLight(Vector3 color)
        {
            this.color = color;
        }

        public DirectionalLight() : this(Vector3.One) { }
    }
}
