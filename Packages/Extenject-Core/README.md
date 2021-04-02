Core version of [Extenject DI framework](https://github.com/svermeulen/Extenject)
Not necessarily up to newest version. I check once in a while and merge if I find it's worth (it's a bit of a hussle cause this package has different structure).

## My additions
 * PrefabFactoryNameBased [TODO]
 * PrefabFactoryPoolable [TODO]
 * [CoroutineRunner] (#coroutinerunner)
 
## CoroutineRunner
Allows for running coroutines from non-MonoBehaviour classes and some extended functionality over coroutines like Pausing them.
Usage: Inject CoroutineRunner and call RunCoroutine

```csharp
public class Foo : IInitializable
{
	[Inject]
	CoroutineRunner _runner;
	
	ofCoroutine _coroutine;
	int i = 0;
	
	public void Initialize()
	{
		_coroutine = _runner.RunCoroutine(DoSomething());
	}
	
	IEnumerator DoSomething()
	{
		while(true)
		{
			i++; // cause why not
		}
	}
}
```

then, at any point you can call 

```csharp
_runner.PauseCoroutine(_coroutine);
```

to pause the coroutine, or

```csharp
_runner.PauseCoroutineFor(_coroutine, 1.0f);
```

to pause it for given amount of seconds. Subsequent pausing cancels previous pausing. Unpausing cancels pause timer.
You can resume coroutine with

```csharp
_runner.ResumeCoroutine(_coroutine);
```

you can stop the coroutine by 

```csharp
_runner.StopCoroutine(_coroutine);
```

or the coroutine can stop naturally by

```csharp
yield break;
```

in both cases `ofCoroutine.CoroutineFinished` event is called.

CoroutineRunner uses Extenject [MemoryPools] (https://github.com/svermeulen/Extenject/blob/master/Documentation/MemoryPools.md) to provide `ofCoroutine` wrappers. After coroutine has been stopped the wrapper is returned back to pool to further reuse. Therefore it is possible to keep reference to already stopped coroutine and then manipulate it. That results in `NullReferenceException`. Even worse is the case when the `ofCoroutine` gets returned and then reused again and the same wrapper class is used to manipulate different Unity Coroutine. Therefore it is recommended best practice to always null your reference to the wrapper when coroutine stops.

```csharp
...
	public void Initialize()
	{
		_coroutine = _runner.RunCoroutine(DoSomething());
		_coroutine.CoroutineFinished += () => {_coroutine = null;};
	}
...
```

Alternatively, you can use `ofCoroutine.IsValid` property but the problem of that is that it gets reseted when it is reused as new instance.
