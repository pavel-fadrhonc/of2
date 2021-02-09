using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public abstract class View : MonoBehaviour, IPoolable<IMemoryPool>, IDisposable
    {
        private List<IMediator<View>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View>> mediators)
        {
            _mediators = mediators;
        }
        
        private IMemoryPool _pool;

        public void OnDespawned()
        {
            _pool = null;

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }
    }    
    
    public abstract class View<TParam> : MonoBehaviour, IPoolable<TParam, IMemoryPool>, IDisposable
    {
        private List<IMediator<View<TParam>, TParam>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View<TParam>, TParam>> mediators)
        {
            _mediators = mediators;
        }
        
        private IMemoryPool _pool;
        private TParam _param;
        
        public void OnDespawned()
        {
            _pool = null;

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public void OnSpawned(TParam param, IMemoryPool pool)
        {
            _pool = pool;
            _param = param;
            
            foreach (var m in _mediators) m.SetParam(param);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }
    }
    
    public abstract class View<TParam1, TParam2> : MonoBehaviour, IPoolable<TParam1, TParam2, IMemoryPool>, IDisposable
    {
        private List<IMediator<View<TParam1, TParam2>, TParam1, TParam2>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View<TParam1, TParam2>, TParam1, TParam2>> mediators)
        {
            _mediators = mediators;
        }
        
        private IMemoryPool _pool;
        private TParam1 _param1;
        private TParam2 _param2;
        
        public void OnDespawned()
        {
            _pool = null;

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public void OnSpawned(TParam1 param1, TParam2 param2, IMemoryPool pool)
        {
            _pool = pool;
            _param1 = param1;
            _param2 = param2;
            
            foreach (var m in _mediators) m.SetParams(param1, param2);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }
    }    
}