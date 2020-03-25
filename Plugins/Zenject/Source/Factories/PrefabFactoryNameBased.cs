using System;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Zenject
{
    public class PrefabFactoryNameBased<TContract> : IFactory<Object, TContract>
        where TContract : Component, IPoolable<IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        public TContract Create(Object prefab)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn();

            instance.OnSpawned(pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TContract>
        {
            protected override void OnDespawned(TContract item)
            {
                base.OnDespawned(item);
                
                item.OnDespawned();
            }
        }
        
        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool> {}

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] private UnityEngine.Object prefab;

            private bool _validateRun = false;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TContract, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }

    public class PrefabFactoryNameBased<TParam, TContract> : IFactory<Object, TParam, TContract>
        where TContract : Component, IPoolable<TParam, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool = new Dictionary<string, Pool>();
        
        public TContract Create(Object prefab, TParam param1)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1);

            instance.OnSpawned(param1, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam, TContract>
        {
            protected override void OnDespawned(TContract item)
            {
                base.OnDespawned(item);

                item.OnDespawned();
            }
        }
        
        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool> {}

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TContract, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }        

    public class PrefabFactoryNameBased<TParam1, TParam2, TContract> : IFactory<Object, TParam1, TParam2, TContract>
        where TContract : Component, IPoolable<TParam1, TParam2, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        public TContract Create(Object prefab, TParam1 param1, TParam2 param2)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1, param2);

            instance.OnSpawned(param1, param2, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam1, TParam2, TContract>
        {
            protected override void OnDespawned(TContract item)
            {
                base.OnDespawned(item);
                
                item.OnDespawned();
            }            
        }
        
        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool> {}

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TContract, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }        

    public class PrefabFactoryNameBased<TParam1, TParam2, TParam3, TContract> : IFactory<Object, TParam1, TParam2, TParam3, TContract>
        where TContract : Component, IPoolable<TParam1, TParam2, TParam3, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        public TContract Create(Object prefab, TParam1 param1, TParam2 param2, TParam3 param3)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1, param2, param3);

            instance.OnSpawned(param1, param2, param3, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam1, TParam2, TParam3, TContract>
        {
            protected override void OnDespawned(TContract item)
            {
                base.OnDespawned(item);
                
                item.OnDespawned();
            }            
        }
        
        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool> {}

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TContract, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }      
    
    public class PrefabFactoryNameBased<TParam1, TParam2, TParam3, TParam4, TContract> : IFactory<Object, TParam1, TParam2, TParam3, TParam4, TContract>
        where TContract : Component, IPoolable<TParam1, TParam2, TParam3, TParam4, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        public TContract Create(Object prefab, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1, param2, param3, param4);

            instance.OnSpawned(param1, param2, param3, param4, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam1, TParam2, TParam3, TParam4, TContract>
        {
            protected override void OnDespawned(TContract item)
            {
                base.OnDespawned(item);
                
                item.OnDespawned();
            }            
        }
        
        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool> {}

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TContract, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }      
}