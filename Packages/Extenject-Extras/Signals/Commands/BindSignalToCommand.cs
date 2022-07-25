using System;
using ModestTree;

namespace Zenject
{
    public partial class BindSignalToBinder<TSignal>
    {
        public SignalCopyBinder ToCommand<TCommand>() where TCommand : Command<TSignal>
        {
            Assert.That(!_bindStatement.HasFinalizer);
            _bindStatement.SetFinalizer(new NullBindingFinalizer());

            _container.BindMemoryPool<TCommand, Pool<TCommand, TSignal>>();

            var bindInfo = _container.Bind<IDisposable>()
                .To<SignalCommandCallbackWrapper<TCommand, TSignal>>()
                .AsCached()
                .WithArguments(_signalBindInfo)
                .NonLazy().BindInfo;
            
            _AotWorkAroundForBindCommand<TSignal, TCommand>();

            return new SignalCopyBinder(bindInfo);
        }

        /// Instead of not using structs in generic arguments of class I decided to do this workaround
        /// By using the class all generic types get concretized and therefore not stripped on AOT platforms 
        static void _AotWorkAroundForBindCommand<TSig, TCommand>() where TCommand : Command<TSig>
        {
            var pool = new Pool<TCommand, TSig>();
            var wrapper = new SignalCommandCallbackWrapper<TCommand, TSig>(null, null, null, null);
        }     
    }
}