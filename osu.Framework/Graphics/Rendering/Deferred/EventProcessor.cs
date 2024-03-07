// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using osu.Framework.Graphics.Rendering.Deferred.Allocation;
using osu.Framework.Graphics.Rendering.Deferred.Events;
using osu.Framework.Graphics.Veldrid.Buffers;
using osu.Framework.Graphics.Veldrid.Pipelines;
using osu.Framework.Graphics.Veldrid.Textures;

namespace osu.Framework.Graphics.Rendering.Deferred
{
    /// <summary>
    /// Processes the render events for a single frame.
    /// </summary>
    internal readonly ref struct EventProcessor
    {
        private readonly DeferredContext context;
        private readonly GraphicsPipeline graphics;

        public EventProcessor(DeferredContext context)
        {
            this.context = context;
            graphics = context.Renderer.Graphics;
        }

        public void ProcessEvents()
        {
            printEventsForDebug();
            processUploads();
            processEvents();
        }

        private void printEventsForDebug()
        {
            if (string.IsNullOrEmpty(FrameworkEnvironment.DeferredRendererEventsOutputPath))
                return;

            EventList.Enumerator enumerator = context.RenderEvents.CreateEnumerator();

            StringBuilder builder = new StringBuilder();
            int indent = 0;

            while (enumerator.Next())
            {
                string info;
                int indentChange = 0;

                switch (enumerator.Current().Type)
                {
                    case RenderEventType.DrawNodeAction:
                    {
                        enumerator.Current().Decompose(out DrawNodeActionEvent e);

                        info = $"DrawNode.{e.Action} ({context.Dereference<DrawNode>(e.DrawNode)})";

                        switch (e.Action)
                        {
                            case DrawNodeActionType.Enter:
                                indentChange += 2;
                                break;

                            case DrawNodeActionType.Exit:
                                indentChange -= 2;
                                break;
                        }

                        break;
                    }

                    default:
                    {
                        info = $"{enumerator.Current().Type.ToString()}";
                        break;
                    }
                }

                indent += Math.Min(0, indentChange);
                builder.AppendLine($"{new string(' ', indent)}{info}");
                indent += Math.Max(0, indentChange);
            }

            File.WriteAllText(FrameworkEnvironment.DeferredRendererEventsOutputPath, builder.ToString());
        }

        private void processUploads()
        {
            EventList.Enumerator enumerator = context.RenderEvents.CreateEnumerator();

            while (enumerator.Next())
            {
                switch (enumerator.Current().Type)
                {
                    case RenderEventType.AddPrimitiveToBatch:
                    {
                        enumerator.Current().Decompose(out AddPrimitiveToBatchEvent e);
                        IDeferredVertexBatch batch = context.Dereference<IDeferredVertexBatch>(e.VertexBatch);
                        batch.Write(e.Memory);
                        break;
                    }

                    case RenderEventType.SetUniformBufferData:
                    {
                        enumerator.Current().Decompose(out SetUniformBufferDataEvent e);
                        IDeferredUniformBuffer buffer = context.Dereference<IDeferredUniformBuffer>(e.Buffer);
                        UniformBufferReference range = buffer.Write(e.Data.Memory);
                        enumerator.Replace(new RenderEvent(e with { Data = new UniformBufferData(range) }));
                        break;
                    }

                    case RenderEventType.SetShaderStorageBufferObjectData:
                    {
                        enumerator.Current().Decompose(out SetShaderStorageBufferObjectDataEvent e);
                        IDeferredShaderStorageBufferObject buffer = context.Dereference<IDeferredShaderStorageBufferObject>(e.Buffer);
                        buffer.Write(e.Index, e.Memory);
                        break;
                    }
                }
            }

            context.VertexManager.Commit();
            context.UniformBufferManager.Commit();
        }

        private void processEvents()
        {
            EventList.Enumerator enumerator = context.RenderEvents.CreateEnumerator();

            while (enumerator.Next())
            {
                switch (enumerator.Current().Type)
                {
                    case RenderEventType.SetFrameBuffer:
                    {
                        enumerator.Current().Decompose(out SetFrameBufferEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.ResizeFrameBuffer:
                    {
                        enumerator.Current().Decompose(out ResizeFrameBufferEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetShader:
                    {
                        enumerator.Current().Decompose(out SetShaderEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetTexture:
                    {
                        enumerator.Current().Decompose(out SetTextureEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetUniformBuffer:
                    {
                        enumerator.Current().Decompose(out SetUniformBufferEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.Clear:
                    {
                        enumerator.Current().Decompose(out ClearEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetDepthInfo:
                    {
                        enumerator.Current().Decompose(out SetDepthInfoEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetScissor:
                    {
                        enumerator.Current().Decompose(out SetScissorEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetScissorState:
                    {
                        enumerator.Current().Decompose(out SetScissorStateEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetStencilInfo:
                    {
                        enumerator.Current().Decompose(out SetStencilInfoEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetViewport:
                    {
                        enumerator.Current().Decompose(out SetViewportEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetBlend:
                    {
                        enumerator.Current().Decompose(out SetBlendEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetBlendMask:
                    {
                        enumerator.Current().Decompose(out SetBlendMaskEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.Flush:
                    {
                        enumerator.Current().Decompose(out FlushEvent e);
                        processEvent(e);
                        break;
                    }

                    case RenderEventType.SetUniformBufferData:
                    {
                        enumerator.Current().Decompose(out SetUniformBufferDataEvent e);
                        processEvent(e);
                        break;
                    }
                }
            }
        }

        private void processEvent(in SetFrameBufferEvent e)
            => graphics.SetFrameBuffer(context.Dereference<DeferredFrameBuffer?>(e.FrameBuffer));

        private void processEvent(in ResizeFrameBufferEvent e)
            => context.Dereference<DeferredFrameBuffer>(e.FrameBuffer).Resize(e.Size);

        private void processEvent(in SetShaderEvent e)
            => graphics.SetShader(context.Dereference<DeferredShader>(e.Shader).Resource);

        private void processEvent(in SetTextureEvent e)
            => graphics.AttachTexture(e.Unit, context.Dereference<IVeldridTexture>(e.Texture));

        private void processEvent(in SetUniformBufferEvent e)
            => graphics.AttachUniformBuffer(context.Dereference<string>(e.Name), context.Dereference<IVeldridUniformBuffer>(e.Buffer));

        private void processEvent(in ClearEvent e)
            => graphics.Clear(e.Info);

        private void processEvent(in SetDepthInfoEvent e)
            => graphics.SetDepthInfo(e.Info);

        private void processEvent(in SetScissorEvent e)
            => graphics.SetScissor(e.Scissor);

        private void processEvent(in SetScissorStateEvent e)
            => graphics.SetScissorState(e.Enabled);

        private void processEvent(in SetStencilInfoEvent e)
            => graphics.SetStencilInfo(e.Info);

        private void processEvent(in SetViewportEvent e)
            => graphics.SetViewport(e.Viewport);

        private void processEvent(in SetBlendEvent e)
            => graphics.SetBlend(e.Parameters);

        private void processEvent(in SetBlendMaskEvent e)
            => graphics.SetBlendMask(e.Mask);

        private void processEvent(in FlushEvent e)
            => context.Dereference<IDeferredVertexBatch>(e.VertexBatch).Draw(e.VertexCount);

        private void processEvent(in SetUniformBufferDataEvent e)
        {
            IDeferredUniformBuffer buffer = context.Dereference<IDeferredUniformBuffer>(e.Buffer);

            buffer.Activate(e.Data.Range.Chunk);
            graphics.SetUniformBufferOffset(buffer, (uint)e.Data.Range.OffsetInChunk);
        }
    }
}
