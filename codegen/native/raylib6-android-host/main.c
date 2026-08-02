/**********************************************************************************************
 * Novolis thin Android host for raylib (static-linked).
 * NativeActivity loads this .so; raylib's android_main calls main().
 **********************************************************************************************/

#include "raylib.h"

int main(int argc, char *argv[])
{
    (void)argc;
    (void)argv;

    const int screenWidth = 800;
    const int screenHeight = 450;

    InitWindow(screenWidth, screenHeight, "Novolis Raylib Android");

    SetTargetFPS(60);

    while (!WindowShouldClose())
    {
        BeginDrawing();
        ClearBackground((Color){ 26, 58, 110, 255 }); /* navy */
        DrawText("Novolis Raylib Android", 40, 40, 32, (Color){ 232, 240, 255, 255 });
        DrawText("android-arm64 host OK", 40, 90, 22, (Color){ 180, 210, 230, 255 });
        EndDrawing();
    }

    CloseWindow();
    return 0;
}
