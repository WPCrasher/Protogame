﻿using System;
using System.Collections.Generic;
using System.Linq;
using Protogame.ATFLevelEditor;
using Protoinject;

namespace Protogame
{
    public class EntityGroup : IContainsEntities, IEventListener<IGameContext>, IEventListener<INetworkEventContext>, IHasLights, IEntity, IServerEntity, INetworkIdentifiable, ISynchronisedObject, IPrerenderableEntity
    {
        private readonly INode _node;

        private INode<IEntity>[] _entityCache = new INode<IEntity>[0];

        private INode<IServerEntity>[] _serverEntityCache = new INode<IServerEntity>[0];

        public EntityGroup(INode node, IEditorQuery<EntityGroup> editorQuery)
        {
            _node = node;
            Transform = new DefaultTransform();

            // EditorGroup is used to represent game groups in the editor
            // and we need to map the transform to this object.
            if (editorQuery.Mode == EditorQueryMode.LoadingConfiguration)
            {
                editorQuery.MapTransform(this, x => this.Transform.Assign(x));

                _node.ChildrenChanged += ChildrenChanged;
                ChildrenChanged(null, null);
            }
        }

        private void ChildrenChanged(object sender, EventArgs e)
        {
            _entityCache = _node.Children.Where(x => typeof(IEntity).IsAssignableFrom(x.Type)).Cast<INode<IEntity>>().ToArray();
            _serverEntityCache = _node.Children.Where(x => typeof(IServerEntity).IsAssignableFrom(x.Type)).Cast<INode<IServerEntity>>().ToArray();
        }

        public ITransform Transform { get; }

        public IFinalTransform FinalTransform
        {
            get { return this.GetAttachedFinalTransformImplementation(_node); }
        }

        public void Update(IServerContext serverContext, IUpdateContext updateContext)
        {
            for (var i = 0; i < _entityCache.Length; i++)
            {
                _serverEntityCache[i].Value.Update(serverContext, updateContext);
            }
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            for (var i = 0; i < _entityCache.Length; i++)
            {
                _entityCache[i].Value.Render(gameContext, renderContext);
            }
        }

        public void Prerender(IGameContext gameContext, IRenderContext renderContext)
        {
            foreach (var child in _node.Children.Select(x => x.UntypedValue).OfType<IPrerenderableEntity>())
            {
                child.Prerender(gameContext, renderContext);
            }
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            for (var i = 0; i < _entityCache.Length; i++)
            {
                _entityCache[i].Value.Update(gameContext, updateContext);
            }
        }

        public bool Handle(IGameContext context, IEventEngine<IGameContext> eventEngine, Event @event)
        {
            foreach (var child in _node.Children.Select(x => x.UntypedValue).OfType<IEventListener<IGameContext>>())
            {
                if (child.Handle(context, eventEngine, @event))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Handle(INetworkEventContext context, IEventEngine<INetworkEventContext> eventEngine, Event @event)
        {
            foreach (var child in _node.Children.Select(x => x.UntypedValue).OfType<IEventListener<INetworkEventContext>>())
            {
                if (child.Handle(context, eventEngine, @event))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<ILight> GetLights()
        {
            foreach (var child in _node.Children.Select(x => x.UntypedValue).OfType<IHasLights>())
            {
                foreach (var light in child.GetLights())
                {
                    yield return light;
                }
            }
        }

        public void ReceiveNetworkIDFromServer(IGameContext gameContext, IUpdateContext updateContext, int identifier, int initialFrameTick)
        {
            throw new InvalidOperationException(
                "Entity groups can not receive network IDs.  This indicates an error in the code.");
        }

        public void ReceivePredictedNetworkIDFromClient(IServerContext serverContext, IUpdateContext updateContext, MxClient client,
            int predictedIdentifier)
        {
            throw new InvalidOperationException(
                "Entity groups can not receive predicted network IDs.  This indicates an error in the code.");
        }

        public void DeclareSynchronisedProperties(ISynchronisationApi synchronisationApi)
        {
            throw new InvalidOperationException(
                "Entity groups can not declare synchronised properties.  Do not attach a network synchronisation component to an entity group.");
        }
    }
}
