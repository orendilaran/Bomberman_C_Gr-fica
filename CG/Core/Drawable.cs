using OpenTK.Mathematics;

namespace CG.Core
{
    // Classe de Desenhável, usada para organizar objetos que podem ser desenhados com um determinado material.
    //Sua principal função é a unificação das operações de desenho para melhor reaproveitamento de código.
    internal abstract class Drawable
    {
        public Material? material;

        public Drawable(Material? material)
        {
            this.material = material;
        }

        public void Draw(Material? materialOverride = null)
        {
            Material? drawMaterial = materialOverride ?? material;
            if (drawMaterial != null)
            {
                drawMaterial.Use();
                InternalDraw(drawMaterial);
            }
        }

        // Cada desenhável faz sua própria implementação da função InternalDraw, chamando os comandos necessários
        //para seu correto desenho na tela.
        protected abstract void InternalDraw(Material drawMaterial);

        public float DistanceTo(Vector3 position)
        {
            return MathF.Sqrt(DistanceSquaredTo(position));
        }

        public virtual float DistanceSquaredTo(Vector3 position)
        {
            return 0f;
        }
    }
}
