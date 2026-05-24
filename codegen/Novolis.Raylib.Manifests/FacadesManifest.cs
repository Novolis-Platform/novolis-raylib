using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static FacadeTypesFragment Facades { get; } = new(
        Id: "facades",
        Types: new FacadeTypeSpec[]
        {
            new(
                Name: "Graphics",
                Namespace: "Novolis.Raylib.Rendering",
                Folder: "Rendering",
                TypeSummary: "2D drawing and framebuffer operations (raylib drawing module).",
                Usings: new string[]
                {
                    "System.Drawing",
                    "Novolis.Raylib.Interop",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("BeginDrawing", "void BeginDrawing()", "Raylib6Native.BeginDrawing()", "Setup canvas (framebuffer) to start drawing"),
                    new("EndDrawing", "void EndDrawing()", "Raylib6Native.EndDrawing()", "End canvas drawing and swap buffers (double buffering)"),
                    new("ClearBackground", "void ClearBackground(Color color)", "Raylib6Native.ClearBackground(color.ToRaylibColor())", "Set background color (framebuffer clear color)"),
                    new("DrawText", "void DrawText(string text, int posX, int posY, int fontSize, Color color)", "Raylib6Native.DrawText(text, posX, posY, fontSize, color.ToRaylibColor())", "Draw text (using default font)"),
                    new("MeasureText", "int MeasureText(string text, int fontSize)", "Raylib6Native.MeasureText(text, fontSize)", "Measure string width for default font"),
                    new("DrawRectangle", "void DrawRectangle(int posX, int posY, int width, int height, Color color)", "Raylib6Native.DrawRectangle(posX, posY, width, height, color.ToRaylibColor())", "Draw a color-filled rectangle"),
                    new("DrawRectangleRec", "void DrawRectangleRec(RectangleF rec, Color color)", "Raylib6Native.DrawRectangleRec(rec, color.ToRaylibColor())", "Draw a color-filled rectangle"),
                    new("DrawRectangleLines", "void DrawRectangleLines(int posX, int posY, int width, int height, Color color)", "Raylib6Native.DrawRectangleLines(posX, posY, width, height, color.ToRaylibColor())", "Draw rectangle outline"),
                    new("DrawLine", "void DrawLine(int startPosX, int startPosY, int endPosX, int endPosY, Color color)", "Raylib6Native.DrawLine(startPosX, startPosY, endPosX, endPosY, color.ToRaylibColor())", "Draw a line"),
                    new("BeginScissorMode", "void BeginScissorMode(int x, int y, int width, int height)", "Raylib6Native.BeginScissorMode(x, y, width, height)", "Begin scissor mode (define screen area for following drawing)"),
                    new("EndScissorMode", "void EndScissorMode()", "Raylib6Native.EndScissorMode()", "End scissor mode"),
                    new("DrawCircle", "void DrawCircle(int centerX, int centerY, float radius, Color color)", "Raylib6Native.DrawCircle(centerX, centerY, radius, color.ToRaylibColor())", "Draw a color-filled circle"),
                    new("DrawFPS", "void DrawFPS(int posX, int posY)", "Raylib6Native.DrawFPS(posX, posY)", "Draw current FPS"),
                }
            ),
            new(
                Name: "Window",
                Namespace: "Novolis.Raylib.Windowing",
                Folder: "Windowing",
                TypeSummary: "Window lifecycle, configuration, and display state.",
                Usings: new string[]
                {
                    "Novolis.Raylib.Interop",
                    "Novolis.Raylib.Interact",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("SetConfigFlags", "void SetConfigFlags(uint flags)", "Raylib6Native.SetConfigFlags(flags)", "raylib SetConfigFlags — call before Init for pre-window flags (e.g. WindowStateFlags.Hidden / FLAG_WINDOW_HIDDEN)."),
                    new("Init", "void Init(int width, int height, string title)", "Raylib6Native.InitWindow(width, height, title)", "Initialize window and OpenGL context"),
                    new("ShouldClose", "bool ShouldClose()", "Raylib6Native.WindowShouldClose()", "Check if application should close (KEY_ESCAPE pressed or windows close icon clicked)"),
                    new("Close", "void Close()", "Raylib6Native.CloseWindow()", "Close window and unload OpenGL context"),
                    new("IsReady", "bool IsReady()", "Raylib6Native.IsWindowReady()", "Check if window has been initialized successfully"),
                    new("IsHidden", "bool IsHidden()", "Raylib6Native.IsWindowHidden()", "Check if window is currently hidden"),
                    new("GetScreenWidth", "int GetScreenWidth()", "Raylib6Native.GetScreenWidth()", "Get current screen width"),
                    new("GetScreenHeight", "int GetScreenHeight()", "Raylib6Native.GetScreenHeight()", "Get current screen height"),
                    new("SetTitle", "void SetTitle(string title)", "Raylib6Native.SetWindowTitle(title)", "Set title for window"),
                    new("ToggleFullscreen", "void ToggleFullscreen()", "Raylib6Native.ToggleFullscreen()", "Toggle window state: fullscreen/windowed, resizes monitor to match window resolution"),
                    new("IsFullscreen", "bool IsFullscreen()", "Raylib6Native.IsWindowFullscreen()", "Check if window is currently fullscreen"),
                    new("SetState", "void SetState(uint flags)", "Raylib6Native.SetWindowState(flags)", "raylib SetWindowState (e.g. FLAG_WINDOW_RESIZABLE from raylib headers)."),
                    new("PollInputEvents", "void PollInputEvents()", "Raylib6Native.PollInputEvents()", "Register all input events"),
                    new("SetExitKey", "void SetExitKey(KeyboardKey key)", "Raylib6Native.SetExitKey((int)key)", "Sets the key that closes the window (default Escape / KeyboardKey.Escape)."),
                }
            ),
            new(
                Name: "Input",
                Namespace: "Novolis.Raylib.Interact",
                Folder: "Interact",
                TypeSummary: "Keyboard, mouse, and cursor input.",
                Usings: new string[]
                {
                    "System.Numerics",
                    "Novolis.Raylib.Interop",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("IsKeyPressed", "bool IsKeyPressed(KeyboardKey key)", "Raylib6Native.IsKeyPressed((int)key)", "Check if a key has been pressed once"),
                    new("IsKeyDown", "bool IsKeyDown(KeyboardKey key)", "Raylib6Native.IsKeyDown((int)key)", "Check if a key is being pressed"),
                    new("IsKeyReleased", "bool IsKeyReleased(KeyboardKey key)", "Raylib6Native.IsKeyReleased((int)key)", "Check if a key has been released once"),
                    new("GetCharPressed", "int GetCharPressed()", "Raylib6Native.GetCharPressed()", "Unicode codepoint from the last key pressed with Alt or Ctrl (raylib GetCharPressed)."),
                    new("GetKeyPressed", "int GetKeyPressed()", "Raylib6Native.GetKeyPressed()", "Next keycode from the key queue (raylib GetKeyPressed); returns 0 when empty."),
                    new("IsCursorOnScreen", "bool IsCursorOnScreen()", "Raylib6Native.IsCursorOnScreen()", "Check if cursor is on the screen"),
                    new("IsMouseButtonPressed", "bool IsMouseButtonPressed(MouseButton button)", "Raylib6Native.IsMouseButtonPressed((int)button)", "Check if a mouse button has been pressed once"),
                    new("IsMouseButtonDown", "bool IsMouseButtonDown(MouseButton button)", "Raylib6Native.IsMouseButtonDown((int)button)", "Check if a mouse button is being pressed"),
                    new("GetMouseWheelMove", "float GetMouseWheelMove()", "Raylib6Native.GetMouseWheelMove()", "Get mouse wheel movement for X or Y, whichever is larger"),
                    new("GetMousePosition", "Vector2 GetMousePosition()", "Raylib6Native.GetMousePosition()", "Get mouse position XY"),
                    new("GetMouseDelta", "Vector2 GetMouseDelta()", "Raylib6Native.GetMouseDelta()", "Get mouse delta between frames"),
                    new("DisableCursor", "void DisableCursor()", "Raylib6Native.DisableCursor()", "Disables cursor (lock cursor)"),
                    new("EnableCursor", "void EnableCursor()", "Raylib6Native.EnableCursor()", "Enables cursor (unlock cursor)"),
                    new("SetMousePosition", "void SetMousePosition(int x, int y)", "Raylib6Native.SetMousePosition(x, y)", "Set mouse position XY"),
                }
            ),
            new(
                Name: "World",
                Namespace: "Novolis.Raylib.Rendering",
                Folder: "Rendering",
                TypeSummary: "3D scene rendering; call Begin with a Camera, then draw, then End.",
                Usings: new string[]
                {
                    "System.Drawing",
                    "System.Numerics",
                    "Novolis.Raylib.Interop",
                    "Novolis.Raylib.Rendering",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("Begin", "void Begin(Camera camera)", "Raylib6Native.BeginMode3D(camera)", "Begin 3D mode with custom camera (3D)"),
                    new("End", "void End()", "Raylib6Native.EndMode3D()", "Ends 3D mode and returns to default 2D orthographic mode"),
                    new("DrawCube", "void DrawCube(Vector3 position, float width, float height, float length, Color color)", "Raylib6Native.DrawCube(position, width, height, length, color.ToRaylibColor())", "Draw cube"),
                    new("DrawCubeV", "void DrawCubeV(Vector3 position, Vector3 size, Color color)", "Raylib6Native.DrawCubeV(position, size, color.ToRaylibColor())", "Draw cube (Vector version)"),
                    new("DrawCubeWiresV", "void DrawCubeWiresV(Vector3 position, Vector3 size, Color color)", "Raylib6Native.DrawCubeWiresV(position, size, color.ToRaylibColor())", "Draw cube wires (Vector version)"),
                    new("DrawSphere", "void DrawSphere(Vector3 centerPos, float radius, Color color)", "Raylib6Native.DrawSphere(centerPos, radius, color.ToRaylibColor())", "Draw sphere"),
                    new("DrawSphereWires", "void DrawSphereWires(Vector3 centerPos, float radius, int rings, int slices, Color color)", "Raylib6Native.DrawSphereWires(centerPos, radius, rings, slices, color.ToRaylibColor())", "Draw sphere wires"),
                    new("DrawLine", "void DrawLine(Vector3 startPos, Vector3 endPos, Color color)", "Raylib6Native.DrawLine3D(startPos, endPos, color.ToRaylibColor())", "Draw a line in 3D world space"),
                    new("DrawCylinder", "void DrawCylinder(Vector3 position, float radiusTop, float radiusBottom, float height, int slices, Color color)", "Raylib6Native.DrawCylinder(position, radiusTop, radiusBottom, height, slices, color.ToRaylibColor())", "Draw a cylinder/cone"),
                    new("DrawGrid", "void DrawGrid(int slices, float spacing)", "Raylib6Native.DrawGrid(slices, spacing)", "Draw a grid (centered at (0, 0, 0))"),
                    new("DrawPlane", "void DrawPlane(Vector3 centerPos, Vector2 size, Color color)", "Raylib6Native.DrawPlane(centerPos, size, color.ToRaylibColor())", "Draw a plane on the XZ axis"),
                    new("DrawBillboard", "void DrawBillboard(Camera camera, Texture texture, Vector3 position, float scale, Color tint)", "Raylib6Native.DrawBillboard(camera, texture.Native, position, scale, tint.ToRaylibColor())", "Draw a camera-facing billboard texture"),
                    new("DrawBillboardPro", "void DrawBillboardPro(Camera camera, Texture texture, RectangleF source, Vector3 position, Vector3 up, Vector2 size, Vector2 origin, float rotation, Color tint)", "Raylib6Native.DrawBillboardPro(camera, texture.Native, source, position, up, size, origin, rotation, tint.ToRaylibColor())", "Draw a billboard with source rect, size, and rotation"),
                }
            ),
            new(
                Name: "Textures",
                Namespace: "Novolis.Raylib.Rendering",
                Folder: "Rendering",
                TypeSummary: "Texture load, draw, and unload helpers.",
                Usings: new string[]
                {
                    "System.Drawing",
                    "System.Numerics",
                    "Novolis.Raylib.Interop",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("Load", "Texture Load(string fileName)", "Texture.FromNative(Raylib6Native.LoadTexture(fileName))", "Load texture from file into GPU memory (VRAM)"),
                    new("Unload", "void Unload(Texture texture)", "Raylib6Native.UnloadTexture(texture.Native)", "Unload texture from GPU memory (VRAM)"),
                    new("IsValid", "bool IsValid(Texture texture)", "Raylib6Native.IsTextureValid(texture.Native)", "Check if a texture is valid (loaded in GPU)"),
                    new("Draw", "void Draw(Texture texture, int posX, int posY, Color tint)", "Raylib6Native.DrawTexture(texture.Native, posX, posY, tint.ToRaylibColor())", "Draw a Texture2D"),
                    new("DrawPro", "void DrawPro(Texture texture, RectangleF source, RectangleF dest, Vector2 origin, float rotation, Color tint)", "Raylib6Native.DrawTexturePro(texture.Native, source, dest, origin, rotation, tint.ToRaylibColor())", "Draw a part of a texture defined by a rectangle with 'pro' parameters"),
                }
            ),
            new(
                Name: "Time",
                Namespace: "Novolis.Raylib.Timing",
                Folder: "Timing",
                TypeSummary: "Frame timing and target FPS.",
                Usings: new string[]
                {
                    "Novolis.Raylib.Interop",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("SetTargetFPS", "void SetTargetFPS(int fps)", "Raylib6Native.SetTargetFPS(fps)", "Set target FPS (maximum)"),
                    new("GetFrameTime", "float GetFrameTime()", "Raylib6Native.GetFrameTime()", "Get time in seconds for last frame drawn (delta time)"),
                }
            ),
            new(
                Name: "AudioDevice",
                Namespace: "Novolis.Raylib.Audio",
                Folder: "Audio",
                TypeSummary: "Audio device initialization.",
                Usings: new string[]
                {
                    "Novolis.Raylib.Interop",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("Init", "void Init()", "Raylib6Native.InitAudioDevice()", "Initialize audio device and context"),
                    new("Close", "void Close()", "Raylib6Native.CloseAudioDevice()", "Close the audio device and context"),
                }
            ),
        }
    );
}
