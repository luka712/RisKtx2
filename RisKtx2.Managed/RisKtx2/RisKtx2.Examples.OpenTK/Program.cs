using OpenTK.Windowing.Desktop;
using RisKtx2.Examples.OpenTK;

var nativeSettings = new NativeWindowSettings()
{
    Size = new OpenTK.Mathematics.Vector2i(800, 600),
    Title = "OpenTK Sprite Example"
};

using var window = new Game(nativeSettings);
window.Run();