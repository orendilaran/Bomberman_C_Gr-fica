using CG.Core;
using OpenTK.Mathematics;

namespace CG.Drawables
{
    //Classe de Desenhável 3D, que possui uma malha e uma transformação, que são usados na sua operação de desenho.
    internal class Drawable3D : DrawableMesh
    {
        Transform transform = new Transform();
        public Transform Transform => transform;

        public Drawable3D(Material? material, Mesh? mesh) : base(material, mesh) { }
        public Drawable3D(Material? material, Mesh[] meshes) : base(material, meshes) { }

        protected override void InternalDraw(Material drawMaterial)
        {
            drawMaterial.Program.ApplyTransform(transform);
            base.InternalDraw(drawMaterial);
        }

        public override float DistanceSquaredTo(Vector3 position)
        {
            return Vector3.Distance(transform.position, position);
        }
    }
}
