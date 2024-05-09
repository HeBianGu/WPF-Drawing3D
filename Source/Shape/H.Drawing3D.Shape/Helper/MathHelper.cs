using System;
using System.Linq;
using System.Threading.Tasks;
#if SHARPDX
using Vector2 = SharpDX.Vector2;
using DoubleOrSingle = System.Single;
#if NETFX_CORE
#if CORE
namespace HelixToolkit.SharpDX.Core
#else
namespace HelixToolkit.UWP
#endif
#else
namespace HelixToolkit.Wpf.SharpDX
#endif
#else
using Vector2 = System.Windows.Vector;
using DoubleOrSingle = System.Double;
using H.Drawing3D.Shape.Geometry;

namespace H.Drawing3D.Shape.Helper
#endif
{
    /// <summary>
    /// 
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Generates the noise map.
        /// From https://stackoverflow.com/questions/8659351/2d-perlin-noise
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="result">The result.</param>
        /// <param name="octaves">The octaves.</param>
        public static void GenerateNoiseMap(int width, int height, int octaves, out DoubleOrSingle[] result)
        {
            double[] data = new DoubleOrSingle[width * height];

            // track min and max noise value. Used to normalize the result to the 0 to 1.0 range.
            double min = DoubleOrSingle.MaxValue;
            double max = DoubleOrSingle.MinValue;

            // rebuild the permutation table to get a different noise pattern. 
            // Leave this out if you want to play with changing the number of octaves while 
            // maintaining the same overall pattern.
            Noise2d.Reseed();

            float frequency = 0.5f;
            float amplitude = 1f;
            //var persistence = 0.25f;

            for (int octave = 0; octave < octaves; octave++)
            {
                // parallel loop - easy and fast.
                _ = Parallel.For(0
                    , width * height
                    , (offset) =>
                    {
                        int i = offset % width;
                        int j = offset / width;
                        double noise = Noise2d.Noise(i * frequency * 1f / width, j * frequency * 1f / height);
                        noise = data[(j * width) + i] += noise * amplitude;

                        min = Math.Min(min, noise);
                        max = Math.Max(max, noise);

                    }
                );

                frequency *= 2;
                amplitude /= 2;
            }
            //Normalize
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = (data[i] - min) / (max - min);
            }
            result = data;
        }

    }
    /// <summary>
    /// implements improved Perlin noise in 2D. 
    /// Transcribed from http://www.siafoo.net/snippet/144?nolinenos#perlin2003
    /// From StackOverflow: https://stackoverflow.com/questions/8659351/2d-perlin-noise
    /// </summary>
    public static class Noise2d
    {
        private static readonly Random _random = new();
        private static int[] _permutation;

        private static readonly Vector2[] _gradients;

        static Noise2d()
        {
            CalculatePermutation(out _permutation);
            CalculateGradients(out _gradients);
        }

        private static void CalculatePermutation(out int[] p)
        {
            p = Enumerable.Range(0, 256).ToArray();

            // shuffle the array
            for (int i = 0; i < p.Length; i++)
            {
                int source = _random.Next(p.Length);

                (p[source], p[i]) = (p[i], p[source]);
            }
        }

        /// <summary>
        /// generate a new permutation.
        /// </summary>
        public static void Reseed()
        {
            CalculatePermutation(out _permutation);
        }

        private static void CalculateGradients(out Vector2[] grad)
        {
            grad = new Vector2[256];

            for (int i = 0; i < grad.Length; i++)
            {
                Vector2 gradient;

                do
                {
                    gradient = new Vector2((float)((_random.NextDouble() * 2) - 1), (float)((_random.NextDouble() * 2) - 1));
                }
                while (SharedFunctions.LengthSquared(ref gradient) >= 1);

                gradient.Normalize();

                grad[i] = gradient;
            }

        }

        private static DoubleOrSingle Drop(DoubleOrSingle t)
        {
            t = Math.Abs(t);
            return 1f - (t * t * t * ((t * ((t * 6) - 15)) + 10));
        }

        private static DoubleOrSingle Q(DoubleOrSingle u, DoubleOrSingle v)
        {
            return Drop(u) * Drop(v);
        }
        /// <summary>
        /// Noises the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public static DoubleOrSingle Noise(DoubleOrSingle x, DoubleOrSingle y)
        {
            Vector2 cell = new((DoubleOrSingle)Math.Floor(x), (DoubleOrSingle)Math.Floor(y));

            DoubleOrSingle total = 0;

            Vector2[] corners = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

            foreach (Vector2 n in corners)
            {
                Vector2 ij = cell + n;
                Vector2 uv = new(x - ij.X, y - ij.Y);

                int index = _permutation[(int)ij.X % _permutation.Length];
                index = _permutation[(index + (int)ij.Y) % _permutation.Length];

                Vector2 grad = _gradients[index % _gradients.Length];

                total += Q(uv.X, uv.Y) * SharedFunctions.DotProduct(ref grad, ref uv);
            }

            return Math.Max(Math.Min(total, 1f), -1f);
        }

    }
}
