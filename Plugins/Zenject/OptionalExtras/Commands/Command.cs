using System;

namespace Zenject
{
    public abstract class Command<T> : IPoolable<IMemoryPool, T>, IDisposable
    {
        private IMemoryPool _pool;
        
        protected T signal;

        public void ExecuteCommand()
        {
            Execute();
            
            Dispose();
        }

        protected abstract void Execute();

        public void OnDespawned()
        {
            _pool = null;
        }

        public void OnSpawned(IMemoryPool pool, T signal_)
        {
            this.signal = signal_;
            _pool = pool;
        }

        public void Dispose()
        {
            _pool?.Despawn(this);
        }
    }
    
    public class Pool<TCommand, TSignal> : PoolableMemoryPool<IMemoryPool, TSignal, TCommand> where TCommand : Command<TSignal>
    {
    }    
}