using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Zenject
{
    public interface ILateInitializable
    {
        void LateInitialize();
    }
    
    // TODO: make it so this supports priority bound in BindExecutionOrder
    public class LateInitializableManager : IInitializable
    {
        private readonly List<ILateInitializable> lateInitializables;
        private readonly CoroutineRunner coroutineRunner;

        public LateInitializableManager(
            List<ILateInitializable> lateInitializables,
            CoroutineRunner coroutineRunner)
        {
            this.lateInitializables = lateInitializables;
            this.coroutineRunner = coroutineRunner;
        }
        
        public void Initialize()
        {
            coroutineRunner.RunCoroutine(LateInitialize());           
        }

        public void LateInitalize()
        {
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