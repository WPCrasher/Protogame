﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Protogame
{
    /// <summary>
    /// The default implementation of an <see cref="IRenderPipeline"/>.
    /// </summary>
    /// <module>Graphics</module>
    /// <internal>True</internal>
    /// <interface_ref>Protogame.IRenderPipeline</interface_ref>
    public class DefaultRenderPipeline : IRenderPipeline
    {
        private readonly List<IRenderPass> _standardRenderPasses;

        private readonly List<IRenderPass> _postProcessingRenderPasses;

        private readonly List<IRenderPass> _transientStandardRenderPasses;

        private readonly List<IRenderPass> _transientPostProcessingRenderPasses;

        private RenderTarget2D _primary;

        private RenderTarget2D _secondary;

        public DefaultRenderPipeline()
        {
            _standardRenderPasses = new List<IRenderPass>();
            _postProcessingRenderPasses = new List<IRenderPass>();
            _transientStandardRenderPasses = new List<IRenderPass>();
            _transientPostProcessingRenderPasses = new List<IRenderPass>();
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            _primary = UpdateRenderTarget(_primary, gameContext);
            _secondary = UpdateRenderTarget(_secondary, gameContext);

            var standardRenderPasses = _standardRenderPasses.ToArray();
            var postProcessingRenderPasses = _postProcessingRenderPasses.ToArray();
            IRenderPass[] transientStandardRenderPasses;
            IRenderPass[] transientPostProcessingRenderPasses;
            IRenderPass previousPass = null;
            IRenderPass nextPass = null;

            var entities = gameContext.World.Entities.ToArray();

            renderContext.PushRenderTarget(_primary);

            for (var i = 0; i < standardRenderPasses.Length; i++)
            {
                var pass = standardRenderPasses[i];
                pass.BeginRenderPass(renderContext, previousPass);
                previousPass = pass;
                RenderPass(gameContext, renderContext, entities);
                if (i < standardRenderPasses.Length - 1)
                {
                    nextPass = standardRenderPasses[i + 1];
                }
                else if (_transientStandardRenderPasses.Count > 0)
                {
                    nextPass = _transientStandardRenderPasses[0];
                }
                else if (postProcessingRenderPasses > 0)
                {
                    nextPass = postProcessingRenderPasses[0];
                }
                else if (_transientPostProcessingRenderPasses.Count > 0)
                {
                    nextPass = _transientPostProcessingRenderPasses[0];
                }
                else
                {
                    nextPass = null;
                }
                pass.EndRenderPass(renderContext, nextPass);
            }

            var loop = 100;
            while (_transientStandardRenderPasses.Count > 0 && loop-- >= 0)
            {
                transientStandardRenderPasses = _transientStandardRenderPasses.ToArray();
                _transientStandardRenderPasses.Clear();

                for (var i = 0; i < transientStandardRenderPasses.Length; i++)
                {
                    var pass = transientStandardRenderPasses[i];
                    pass.BeginRenderPass(renderContext, previousPass);
                    previousPass = pass;
                    RenderPass(gameContext, renderContext, entities);
                    if (i < transientStandardRenderPasses.Length - 1)
                    {
                        nextPass = transientStandardRenderPasses[i + 1];
                    }
                    else if (_transientStandardRenderPasses.Count > 0)
                    {
                        nextPass = _transientStandardRenderPasses[0];
                    }
                    else if (postProcessingRenderPasses > 0)
                    {
                        nextPass = postProcessingRenderPasses[0];
                    }
                    else if (_transientPostProcessingRenderPasses.Count > 0)
                    {
                        nextPass = _transientPostProcessingRenderPasses[0];
                    }
                    else
                    {
                        nextPass = null;
                    }
                    pass.EndRenderPass(renderContext, nextPass);
                }
            }
            if (loop < 0)
            {
                throw new InvalidOperationException(
                    "Exceeded the number of AppendRenderPass iterations (100).  Ensure you " +
                    "are not unconditionally calling AppendRenderPass within another render pass.");
            }

            renderContext.PopRenderTarget();

            // TODO: Implement post-processing render passes (to do, we need some way of passing
            // the previous source texture or render target to a post-processing render pass).

            // TODO: Use a shader to blit the render target AND DEPTH BUFFER to the GPU.
        }

        public IRenderPass AddRenderPass(IRenderPass renderPass)
        {
            if (renderPass.IsPostProcessingPass)
            {
                _postProcessingRenderPasses.Add(renderPass);
            }
            else
            {
                _standardRenderPasses.Add(renderPass);
            }
        }

        public void RemoveRenderPass(IRenderPass renderPass)
        {
            if (renderPass.IsPostProcessingPass)
            {
                _postProcessingRenderPasses.Remove(renderPass);
            }
            else
            {
                _standardRenderPasses.Remove(renderPass);
            }
        }

        public IRenderPass AppendRenderPass(IRenderPass renderPass)
        {
            if (renderPass.IsPostProcessingPass)
            {
                _transientPostProcessingRenderPasses.Add(renderPass);
            }
            else
            {
                _transientStandardRenderPasses.Add(renderPass);
            }
        }

        private void RenderPass(IGameContext gameContext, IRenderContext renderContext, IEntity[] entities)
        {
            gameContext.World.RenderBelow(gameContext, renderContext);

            foreach (var entity in entities)
            {
                entity.Render(gameContext, renderContext);
            }

            gameContext.World.RenderAbove(gameContext, renderContext);
        }

        private RenderTarget2D UpdateRenderTarget(RenderTarget2D renderTarget, IGameContext gameContext)
        {
            if (IsRenderTargetOutOfDate(renderTarget, gameContext))
            {
                if (renderTarget != null)
                {
                    renderTarget.Dispose();
                }

                renderTarget = new RenderTarget2D(
                    gameContext.Graphics.GraphicsDevice,
                    gameContext.Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                    gameContext.Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                    false,
                    gameContext.Graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                    gameContext.Graphics.GraphicsDevice.PresentationParameters.DepthStencilFormat);
            }

            return renderTarget;
        }

        private bool IsRenderTargetOutOfDate(RenderTarget2D renderTarget, IGameContext gameContext)
        {
            if (renderTarget == null)
            {
                return true;
            }
            else
            {
                if (renderTarget.Width != gameContext.Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth)
                {
                    return true;
                }

                if (renderTarget.Height != gameContext.Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight)
                {
                    return true;
                }

                if (renderTarget.Format != gameContext.Graphics.GraphicsDevice.PresentationParameters.BackBufferFormat)
                {
                    return true;
                }

                if (renderTarget.DepthStencilFormat != gameContext.Graphics.GraphicsDevice.PresentationParameters.DepthStencilFormat)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
