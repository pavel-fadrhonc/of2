﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Zenject;

[TestFixture]
public class TestListBinding : ZenjectUnitTestFixture
{
    [Test]
    public void TestBindInstaceToListBindings()
    {
        Container.Bind<Bar>().AsSingle();

		Container.BindInstance(new Foo());
		Container.BindInstance(new Foo());

		var bar = Container.Resolve<Bar>()
		Assert.IsNotNull(bar.FooList);
		Assert.IsEqual(bar.FooList.Count, 2);
    }

    public class Foo {}
    
    public class Bar
    {
		public List<Foo> FooList {get; private set; }

        public Bar(
			List<Foo> fooList)
        {
            FooList = fooList;
        }
    }
}
