using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Newtonian_Particle_Simulator.Render;

namespace Newtonian_Particle_Simulator
{
    class MainWindow : GameWindow
    {
        public MainWindow() 
            : base(832, 832, new GraphicsMode(0, 0, 0, 0), "Newtonian-Particle-Simulator") { }

        private readonly Camera camera = new Camera(new Vector3(0, 0, 50), new Vector3(0, 1, 0));
        private Matrix4 projection;

        private int frames = 0;
        private readonly Stopwatch fpsTimer = Stopwatch.StartNew();
        private ParticleSimulator particleSimulator;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            particleSimulator.Run((float)e.Time);

            SwapBuffers();
            frames++;
            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (fpsTimer.ElapsedMilliseconds >= 1000)
            {
                int fps = frames;
                Title = $"Newtonian-Particle-Simulator FPS: {fps}";
                frames = 0;
                fpsTimer.Restart();
            }

            particleSimulator.UpdateProjectionView(camera.View * projection);

            base.OnUpdateFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine($"OpenGL: {Helper.APIVersion}");
            Console.WriteLine($"GLSL: {GL.GetString(StringName.ShadingLanguageVersion)}");
            Console.WriteLine($"GPU: {GL.GetString(StringName.Renderer)}");

            if (!Helper.IsCoreExtensionAvailable("GL_ARB_direct_state_access", 4.5))
                throw new NotSupportedException("Your system does not support GL_ARB_direct_state_access");

            if (!Helper.IsCoreExtensionAvailable("GL_ARB_buffer_storage", 4.4))
                throw new NotSupportedException("Your system does not support GL_ARB_buffer_storage");
            
            GL.PointSize(1.1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            VSync = VSyncMode.On;

            int numParticles = 1000000; // You can adjust this number

            Random rng = new Random();
            Particle[] particles = new Particle[numParticles];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = Helper.RandomUnitVector(rng) * 50.0f;
                particles[i].Velocity = Vector3.Zero;
            }
            particleSimulator = new ParticleSimulator(particles);

            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (Width != 0 && Height != 0)
            {
                GL.Viewport(0, 0, Width, Height);
                projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(103.0f), (float)Width / Height, 0.1f, 1000f);
            }

            base.OnResize(e);
        }
    }
}