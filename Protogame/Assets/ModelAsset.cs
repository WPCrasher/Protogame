﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Protogame
{
    using System;

    /// <summary>
    /// Represents a model asset.
    /// </summary>
    public class ModelAsset : MarshalByRefObject, IAsset, IModel
    {
        /// <summary>
        /// The runtime model associated with this asset.
        /// </summary>
        private Model m_Model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelAsset"/> class.
        /// <para>
        /// This constructor is called by <see cref="ModelAssetLoader"/> when loading a model asset.
        /// </para>
        /// </summary>
        /// <param name="name">
        /// The name of the model asset.
        /// </param>
        /// <param name="sourceData">
        /// The source raw data used for compilation, or null if there is no source information.
        /// </param>
        /// <param name="compiledData">
        /// The compiled raw data used at runtime, or null if there is no compiled information.
        /// </param>
        /// <param name="sourcedFromRaw">
        /// Whether or not this asset was sourced from a purely raw asset file (such as a PNG).
        /// </param>
        public ModelAsset(
            string name,
            byte[] rawData,
            PlatformData data, 
            bool sourcedFromRaw)
        {
            this.Name = name;
            this.RawData = rawData;
            this.PlatformData = data;
            this.SourcedFromRaw = sourcedFromRaw;

            if (this.PlatformData != null)
            {
                try
                {
                    this.ReloadModel();
                }
                catch (NoAssetContentManagerException)
                {
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the asset only contains compiled information.
        /// </summary>
        /// <value>
        /// Whether the asset only contains compiled information.
        /// </value>
        public bool CompiledOnly
        {
            get
            {
                return this.RawData == null;
            }
        }

        /// <summary>
        /// Gets the name of the asset.
        /// </summary>
        /// <value>
        /// The name of the asset.
        /// </value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the platform-specific data associated with this asset.
        /// </summary>
        /// <seealso cref="PlatformData"/>
        /// <value>
        /// The platform-specific data for this asset.
        /// </value>
        public PlatformData PlatformData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw model data.  This is the source information used to compile the asset.
        /// </summary>
        /// <value>
        /// The raw texture data.
        /// </value>
        public byte[] RawData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the asset only contains source information.
        /// </summary>
        /// <value>
        /// Whether the asset only contains source information.
        /// </value>
        public bool SourceOnly
        {
            get
            {
                return this.PlatformData == null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this asset was sourced from a raw file (such as a FBX file).
        /// </summary>
        /// <value>
        /// The sourced from raw.
        /// </value>
        public bool SourcedFromRaw
        {
            get;
            private set;
        }

        /// <summary>
        /// Reloads the model from the associated compiled data.
        /// </summary>
        public void ReloadModel()
        {
            if (this.PlatformData != null)
            {
                var serializer = new ModelSerializer();
                this.m_Model = serializer.Deserialize(this.PlatformData.Data);
            }
        }

        /// <summary>
        /// Attempt to resolve this asset to the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The target type of the asset.
        /// </typeparam>
        /// <returns>
        /// The current asset as a <see cref="T"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the current asset can not be casted to the designated type.
        /// </exception>
        public T Resolve<T>() where T : class, IAsset
        {
            if (typeof(T).IsAssignableFrom(typeof(ModelAsset)))
            {
                return this as T;
            }

            throw new InvalidOperationException("Asset already resolved to ModelAsset.");
        }

        public IAnimationCollection AvailableAnimations
        {
            get
            {
                return this.m_Model.AvailableAnimations;
            }
        }

        public void Draw(IRenderContext renderContext, Matrix transform, string animationName, TimeSpan secondFraction)
        {
            this.m_Model.Draw(renderContext, transform, animationName, secondFraction);
        }

        public void Draw(IRenderContext renderContext, Matrix transform, string animationName, int frame)
        {
            this.m_Model.Draw(renderContext, transform, animationName, frame);
        }

        /// <summary>
        /// Loads vertex and index buffers for all of animations in this model.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device.
        /// </param>
        public void LoadBuffers(GraphicsDevice graphicsDevice)
        {
            this.m_Model.LoadBuffers(graphicsDevice);
        }
    }
}