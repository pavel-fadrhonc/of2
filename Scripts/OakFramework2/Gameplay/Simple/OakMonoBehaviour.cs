//  <author>Pavel Fadrhonc</author>
//  <email>pavel.fadrhonc@gmail.com</email>
//  <date>16.7.2013</date>
//  <summary>Oak version of Monobehaviour uses a thin layer for many convenience purposes inmproving original MonoBehaviour</summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public delegate void OakTask();

public class OakMonoBehaviour : MonoBehaviour
{
    protected Transform _thisTransform;
    public Transform CachedTransform { get { return _thisTransform; } }

    protected virtual void Awake()
    {
        _thisTransform = this.transform;
    }

    /// <summary>
    /// Removes from GameObjectCache so it cannot be querried
    /// </summary>
    protected virtual void OnDestroy()
    {
        GameObjectCache.Instance.Remove(gameObject);
    }
}
