using System.Collections;
using System.Collections.Generic;
using ModestTree;
using Zenject;

namespace Zenject
{
    public interface ILateInitializable
    {
        void LateInitialize();
    }
    
    // TODO: make it so this supports priority bound in BindExecutionOrder
    public class LateInitializableManager
    {
        bool _hasInitialized;        
        
        private readonly List<ILateInitializable> lateInitializables;
        private readonly CoroutineRunner coroutineRunner;

        public LateInitializableManager(
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ILateInitializable> lateInitializables,
            CoroutineRunner coroutineRunner)
        {
            this.lateInitializables = lateInitializables;
            this.coroutineRunner = coroutineRunner;
        }
        
        public void DelayInitialize()
        {
            coroutineRunner.RunCoroutine(LateInitialize());           
        }

        public void LateInitalize()
        {
            Assert.That(!_hasInitialized);
            _hasInitialized = true;

            foreach (var lateInitializable in lateInitializables)
            {
                lateInitializable.LateInitialize();
            }
        }

        private IEnumerator LateInitialize()
        {
            yield return null;

            LateInitalize();
        }
    }
}