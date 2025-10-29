using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using CG;
using StbImageSharp;

// Setup inicial
StbImage.stbi_set_flip_vertically_on_load(1);

// Criação da janela e início do loop
NativeWindowSettings settings = new()
{
    Title = "Meu Jogo",
    ClientSize = new(1280, 720),
    Flags = ContextFlags.ForwardCompatible,
};
using Game game = new(GameWindowSettings.Default, settings);
game.Run();
