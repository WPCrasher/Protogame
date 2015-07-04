namespace Protogame
{
    using System;
    using Ninject.Modules;

    /// <summary>
    /// The protogame asset io c module.
    /// </summary>
    public class ProtogameAssetIoCModule : NinjectModule
    {
        /// <summary>
        /// The load.
        /// </summary>
        public override void Load()
        {
            this.Bind<IAssetLoader>().To<FontAssetLoader>();
            this.Bind<IAssetLoader>().To<LanguageAssetLoader>();
            this.Bind<IAssetLoader>().To<TextureAssetLoader>();
            this.Bind<IAssetLoader>().To<LevelAssetLoader>();
            this.Bind<IAssetLoader>().To<AudioAssetLoader>();
            this.Bind<IAssetLoader>().To<TilesetAssetLoader>();
            this.Bind<IAssetLoader>().To<EffectAssetLoader>();
            this.Bind<IAssetLoader>().To<AIAssetLoader>();
            this.Bind<IAssetLoader>().To<ModelAssetLoader>();
            this.Bind<IAssetLoader>().To<TextureAtlasAssetLoader>();
            this.Bind<IAssetLoader>().To<VariableAssetLoader>();
            this.Bind<IAssetLoader>().To<ConfigurationAssetLoader>();
            this.Bind<IAssetSaver>().To<FontAssetSaver>();
            this.Bind<IAssetSaver>().To<LanguageAssetSaver>();
            this.Bind<IAssetSaver>().To<TextureAssetSaver>();
            this.Bind<IAssetSaver>().To<LevelAssetSaver>();
            this.Bind<IAssetSaver>().To<AudioAssetSaver>();
            this.Bind<IAssetSaver>().To<TilesetAssetSaver>();
            this.Bind<IAssetSaver>().To<EffectAssetSaver>();
            this.Bind<IAssetSaver>().To<ModelAssetSaver>();
            this.Bind<IAssetSaver>().To<TextureAtlasAssetSaver>();
            this.Bind<IAssetSaver>().To<VariableAssetSaver>();
            this.Bind<IAssetSaver>().To<ConfigurationAssetSaver>();
            this.Bind<IRawAssetLoader>().To<RawAssetLoader>();
            this.Bind<IRawAssetSaver>().To<RawAssetSaver>();
            this.Bind<ITransparentAssetCompiler>().To<DefaultTransparentAssetCompiler>();

#if PLATFORM_WINDOWS || PLATFORM_MACOS || PLATFORM_LINUX
#if DEBUG
            this.Bind<ILoadStrategy>().To<RawTextureLoadStrategy>();
            this.Bind<ILoadStrategy>().To<RawEffectLoadStrategy>();
            this.Bind<ILoadStrategy>().To<RawModelLoadStrategy>();
            this.Bind<ILoadStrategy>().To<RawLevelLoadStrategy>();
            this.Bind<ILoadStrategy>().To<RawAudioLoadStrategy>();
            this.Bind<ILoadStrategy>().To<RawConfigurationLoadStrategy>();
#endif
            this.Bind<ILoadStrategy>().To<LocalSourceLoadStrategy>();
            this.Bind<ILoadStrategy>().To<EmbeddedSourceLoadStrategy>();
            this.Bind<ILoadStrategy>().To<LocalCompiledLoadStrategy>();
            this.Bind<ILoadStrategy>().To<EmbeddedCompiledLoadStrategy>();
            this.Bind<ILoadStrategy>().To<AssemblyLoadStrategy>();
#elif PLATFORM_ANDROID || PLATFORM_OUYA
            this.Bind<ILoadStrategy>().To<AndroidSourceLoadStrategy>();
            this.Bind<ILoadStrategy>().To<AndroidCompiledLoadStrategy>();
            this.Bind<ILoadStrategy>().To<EmbeddedCompiledLoadStrategy>();
            this.Bind<ILoadStrategy>().To<EmbeddedSourceLoadStrategy>();
#endif

            // MonoGame compilation requires 64-bit for content compilation.
            if (IntPtr.Size == 8)
            {
#if PLATFORM_WINDOWS || PLATFORM_LINUX
                this.Bind<IAssetCompiler<TextureAsset>>().To<TextureAssetCompiler>();
                this.Bind<IAssetCompiler<ModelAsset>>().To<ModelAssetCompiler>();
                this.Bind<IAssetCompiler<AudioAsset>>().To<AudioAssetCompiler>();
                this.Bind<IAssetCompiler<TextureAtlasAsset>>().To<TextureAtlasAssetCompiler>();
#if PLATFORM_WINDOWS
                this.Bind<IAssetCompiler<EffectAsset>>().To<EffectAssetCompiler>();
                this.Bind<IAssetCompiler<FontAsset>>().To<FontAssetCompiler>();
#elif PLATFORM_LINUX
                this.Bind<IAssetCompiler<EffectAsset>>().To<EffectAssetRemoteCompiler>();
                this.Bind<IAssetCompiler<FontAsset>>().To<FontAssetRemoteCompiler>();
#endif
#endif
            }
        }
    }
}