using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Newtonian_Particle_Simulator.Render.Objects;

namespace Newtonian_Particle_Simulator.Render
{
    class ParticleSimulator
    {
        public readonly int NumParticles;
        private readonly ShaderProgram shaderProgram;
        private Vector3 focalPoint;
        private float time = 0f;

        public unsafe ParticleSimulator(ReadOnlySpan<Particle> particles)
        {
            NumParticles = particles.Length;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string vertexShaderPath = Path.Combine(basePath, "res", "shaders", "particles", "vertex.glsl");
            string fragmentShaderPath = Path.Combine(basePath, "res", "shaders", "particles", "fragment.glsl");

            shaderProgram = new ShaderProgram(
                new Shader(ShaderType.VertexShader, File.ReadAllText(vertexShaderPath)),
                new Shader(ShaderType.FragmentShader, File.ReadAllText(fragmentShaderPath))
            );

            var particleBuffer = new BufferObject(BufferRangeTarget.ShaderStorageBuffer, 0);
            particleBuffer.ImmutableAllocate(sizeof(Particle) * (nint)NumParticles, particles[0], BufferStorageFlags.None);

            focalPoint = Vector3.Zero;
            IsRunning = true;
        }

        public bool IsRunning { get; set; }

        public void Run(float dT)
        {
            time += dT;
            UpdateFocalPoint();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            shaderProgram.Use();
            shaderProgram.Upload(0, dT);
            shaderProgram.Upload(1, focalPoint);
            shaderProgram.Upload(2, 1.0f); // Always active
            shaderProgram.Upload(3, IsRunning ? 1.0f : 0.0f);

            GL.DrawArrays(PrimitiveType.Points, 0, NumParticles);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        private void UpdateFocalPoint()
        {
            // Create a circular motion for the focal point
            float radius = 25.0f;
            float frequency = 0.1f;
            focalPoint.X = (float)Math.Cos(time * frequency) * radius;
            focalPoint.Y = (float)Math.Sin(time * frequency) * radius;
            focalPoint.Z = (float)Math.Sin(time * frequency * 0.5f) * radius * 0.5f;
        }

        public void UpdateProjectionView(Matrix4 projectionView)
        {
            shaderProgram.Upload(4, projectionView);
        }
    }
}