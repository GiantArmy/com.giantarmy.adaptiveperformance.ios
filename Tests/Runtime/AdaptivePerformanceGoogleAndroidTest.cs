using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

class AdaptivePerformanceiOSTests
{

    [UnityTest]
    public IEnumerator DummyAPiOSTest()
    {
        yield return null;
        Assert.AreEqual(1, 1);
    }
}
