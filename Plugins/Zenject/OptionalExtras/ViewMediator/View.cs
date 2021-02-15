using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public abstract class ViewBase : MonoBehaviour, IDisposable
    {
        private IMemoryPool _pool;

        public virtual void OnDespawned()
        {
            _pool = null;
        }

        public virtual void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;
        }

        public virtual void Dispose()
        {
            _pool.Despawn(this);
        }
    }       
    
    public abstract class View : ViewBase, IPoolable<IMemoryPool>, IDisposable
    {
        private List<IMediator<View>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View>> mediators)
        {
            _mediators = mediators;
        }

        public override void OnDespawned()
        {
            base.OnDespawned();   

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public override void OnSpawned(IMemoryPool pool)
        {
            base.OnSpawned(pool);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }
    }    
    
    public abstract class View<TParam> : ViewBase, IPoolable<TParam, IMemoryPool>, IDisposable
    {
        private List<IMediator<View<TParam>, TParam>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View<TParam>, TParam>> mediators)
        {
            _mediators = mediators;
        }
        
        private TParam _param;
        
        public override void OnDespawned()
        {
            base.OnDespawned();

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public void OnSpawned(TParam param, IMemoryPool pool)
        {
            base.OnSpawned(pool);
            
            _param = param;
            
            foreach (var m in _mediators) m.SetParam(param);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }
    }
    
    public abstract class View<TParam1, TParam2> : ViewBase, IPoolable<TParam1, TParam2, IMemoryPool>, IDisposable
    {
        private List<IMediator<View<TParam1, TParam2>, TParam1, TParam2>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View<TParam1, TParam2>, TParam1, TParam2>> mediators)
        {
            _mediators = mediators;
        }
        
        private TParam1 _param1;
        private TParam2 _param2;
        private IMemoryPool _pool;
        
        public override void OnDespawned()
        {
            base.OnDespawned();

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public void OnSpawned(TParam1 param1, TParam2 param2, IMemoryPool pool)
        {
            base.OnSpawned(pool);
            
            _param1 = param1;
            _param2 = param2;
            
            foreach (var m in _mediators) m.SetParams(param1, param2);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }
    }    
}