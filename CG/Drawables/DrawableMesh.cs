using CG.Core;

namespace CG.Drawables
{
    //Classe de Malha Desenhável, que possui uma malha, usada na sua operação de desenho.
    internal class DrawableMesh : Drawable
    {
        protected Mesh[] meshes = [];

        public DrawableMesh(Material? material, Mesh? mesh) : base(material)
        {
            if (mesh != null)
            {
                meshes = [ mesh ];
            }
        }

        public DrawableMesh(Material? material, Mesh[] meshes) : base(material)
        {
            this.meshes = (Mesh[])meshes.Clone();
        }

        protected override void InternalDraw(Material drawMaterial)
        {
            foreach (Mesh mesh in meshes)
            {
                mesh?.Draw();
            }
        }
    }
}
