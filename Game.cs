// Include the namespaces (code libraries) you need below.
using System;
using System.Numerics;
using Raylib_cs;

// The namespace your code is in.
namespace MohawkGame2D
{
    /// <summary>
    ///     Your game code goes inside this class!
    /// </summary>
    public class Game
    {
        //  constants 
        const int NUM_BUBBLES = 60;
        const int MAX_PELLETS = 120;

        // state 
        Vector2[] bubbles = new Vector2[NUM_BUBBLES]; // array #1
        Pellet[] pellets = new Pellet[MAX_PELLETS]; // array #2
        System.Random rng = new System.Random();
        int frame = 0;

        //  colours
        readonly Color WATER = new Color(0x14, 0x80, 0x78, 0xFF);
        readonly Color GLASS = new Color(0x7A, 0xC9, 0xDF, 0xFF);
        readonly Color SAND = new Color(0xD2, 0xB0, 0x80, 0xFF);
        readonly Color SEAWEED = new Color(0x1E, 0xA0, 0x5A, 0xFF);
        readonly Color BUBBLE = new Color(0xBE, 0xE0, 0xEF, 0xFF);
        readonly Color FISH1 = new Color(0xFF, 0xA5, 0x00, 0xFF);
        readonly Color FISH2 = new Color(0xFF, 0x0F, 0x00, 0xFF);
        readonly Color FOOD = new Color(0x7A, 0x4F, 0x2B, 0xFF); // brown pellets

        // pellet record
        struct Pellet
        {
            public bool Active;
            public Vector2 Pos;
            public float Vy;
        }
        


        /// <summary>
        ///     Setup runs once before the game loop begins.
        /// </summary>
        public void Setup()
        {
            int W = Raylib.GetRenderWidth();
            int H = Raylib.GetRenderHeight();
            int m = 8;

            for (int i = 0; i < NUM_BUBBLES; i++)
            {
                bubbles[i] = new Vector2(
                    rng.Next(m + 12, W - m - 12),
                    rng.Next(m + 12, H - m - 12)
                );
            }

            for (int i = 0; i < MAX_PELLETS; i++)
                pellets[i].Active = false;
        }
        

        /// <summary>
        ///     Update runs every frame.
        /// </summary>
        public void Update()
        {
            frame++;

            int W = Raylib.GetRenderWidth();
            int H = Raylib.GetRenderHeight();
            int m = 8;
            int tankW = W - m * 2;
            int tankH = H - m * 2;
            int sandH = Math.Max(6, tankH / 8);
            int sandTop = m + tankH - sandH;

            // 1) clear background 
            Raylib.ClearBackground(WATER);
            Raylib.DrawRectangle(m, m, tankW, tankH, WATER);

            // 2) input: drop pellets on click (a small handful)
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mouse = Raylib.GetMousePosition();
                // clamp inside tank
                float px = Math.Clamp(mouse.X, m + 6, m + tankW - 6);
                float py = Math.Clamp(mouse.Y, m + 6, m + tankH - 6);

                int count = 6; // how many per click
                for (int n = 0; n < count; n++)
                    SpawnPellet(new Vector2(px + rng.Next(-8, 9), py + rng.Next(-6, 7)));
            }

            // 3) update bubbles (rise + wrap)
            for (int i = 0; i < NUM_BUBBLES; i++)
            {
                bubbles[i].Y -= 1.2f;
                if (bubbles[i].Y < m + 6)
                {
                    bubbles[i].Y = m + tankH - 10;
                    bubbles[i].X = rng.Next(m + 12, m + tankW - 12);
                }
            }

            // 4) update pellets (fall to sand)
            for (int i = 0; i < MAX_PELLETS; i++)
            {
                if (!pellets[i].Active) continue;

                pellets[i].Vy += 0.15f;                  // gravity
                pellets[i].Pos.Y += pellets[i].Vy;

                if (pellets[i].Pos.Y >= sandTop - 3)     // stop on sand
                {
                    pellets[i].Pos.Y = sandTop - 3;
                    pellets[i].Vy = 0;
                }
            }

            // 5) draw tank + scenery
            Raylib.DrawRectangleLines(m, m, tankW, tankH, GLASS);           // glass outline
            Raylib.DrawRectangle(m, sandTop, tankW, sandH, SAND);           // sand

            // seaweed clumps 
            int step = Math.Max(24, tankH / 80);
            for (int x = m + 30; x < m + tankW - 40; x += step)
            {
                Raylib.DrawRectangle(x, sandTop - 40, 8, 40, SEAWEED);
                Raylib.DrawRectangle(x + 10, sandTop - 30, 6, 30, SEAWEED);
            }

            // 6) draw bubbles
            for (int i = 0; i < NUM_BUBBLES; i++)
                Raylib.DrawCircle((int)bubbles[i].X, (int)bubbles[i].Y, 3, BUBBLE);

            // 7) draw pellets
            for (int i = 0; i < MAX_PELLETS; i++)
                if (pellets[i].Active)
                    Raylib.DrawCircle((int)pellets[i].Pos.X, (int)pellets[i].Pos.Y, 3, FOOD);

            // 8) draw fish 
            // gentle swim wiggle using sin()
            int wiggle1 = (int)(MathF.Sin(frame * 0.05f) * 6);
            int wiggle2 = (int)(MathF.Sin(frame * 0.04f + 1.3f) * 8);

            DrawFish(m + tankW / 3 + wiggle1, sandTop - 80, FISH1, faceRight: true);
            DrawFish(m + tankW * 2 / 3 + wiggle2, sandTop - 120, FISH2, faceRight: false);
        }

        // ---------- helpers ----------
        void SpawnPellet(Vector2 pos)
        {
            for (int i = 0; i < MAX_PELLETS; i++)
            {
                if (pellets[i].Active) continue;
                pellets[i].Active = true;
                pellets[i].Pos = pos;
                pellets[i].Vy = 0f;
                return;
            }
            // if full, overwrite a random pellet
            int j = rng.Next(0, MAX_PELLETS);
            pellets[j].Active = true;
            pellets[j].Pos = pos;
            pellets[j].Vy = 0f;
        }

        // simple fish: oval body + triangle tail + eye
        void DrawFish(int cx, int cy, Color colour, bool faceRight)
        {
            int dir = faceRight ? 1 : -1;

            // body
            Raylib.DrawEllipse(cx, cy, 32, 20, colour);

            // tail (behind)
            Vector2 p1 = new Vector2(cx - 28 * dir, cy);
            Vector2 p2 = new Vector2(cx - 50 * dir, cy - 12);
            Vector2 p3 = new Vector2(cx - 50 * dir, cy + 12);
            Raylib.DrawTriangle(p1, p2, p3, colour);

            // eye
            Raylib.DrawCircle(cx + 18 * dir, cy - 4, 3, Color.Black);
        }
    }
}
        
    


