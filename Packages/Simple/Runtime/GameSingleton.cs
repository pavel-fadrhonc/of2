//  </copyright>
//  <author>Pavel</author>
//  <email>pavel.fadrhonc@gmail.com</email>
//  <date>12/8/2013 11:26:46 PM</date>
//  <summary>Differs from SceneSingleton in that it is not destroyed on new scene load
// and is persistent throughout the whole game.
// However, </summary>
using UnityEngine;

public class GameSingleton<T> : SceneSingleton<T> where T : OakMonoBehaviour
{
    #region UNITY METHODS

    protected override void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();

        DontDestroyOnLoad(this);
    }

    #endregion
}

