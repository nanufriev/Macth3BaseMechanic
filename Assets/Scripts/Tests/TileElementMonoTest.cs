using Core.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class TileElementMonoTest
    {
        private GameObject _gameObject;
    
        [SetUp]
        public void Setup()
        {
            _gameObject = Object.Instantiate(new GameObject());
            _gameObject.AddComponent<SpriteRenderer>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject.gameObject);
        }
    
        [Test]
        public void TestEquality()
        {
            var component1 = _gameObject.AddComponent<TileElementMono>();
            component1.Init(1,1, Color.black, Camera.main);
            
            var component2 = _gameObject.AddComponent<TileElementMono>();
            component2.Init(5,2, Color.black, Camera.main);
            
            var component3 = _gameObject.AddComponent<TileElementMono>();
            component3.Init(1,1, Color.black, Camera.main);
        
            Assert.IsTrue(component1.Equals(component3));
            Assert.IsFalse(component1.Equals(component2));
        }

        [Test]
        public void TestNonEqualityWithOtherObject()
        {
            var component = _gameObject.AddComponent<TileElementMono>();
            component.Init(1,1, Color.black, Camera.main);
        
            var otherObject = new object();

            Assert.IsFalse(component.Equals(otherObject));
        }
        
        [Test]
        public void TestSetPosition()
        {
            var component = _gameObject.AddComponent<TileElementMono>();
            component.Init(7,5, Color.black, Camera.main);
        
            Assert.AreEqual(component.PositionX, 7);
            Assert.AreEqual(component.PositionY, 5);
            
            component.SetNewPositionIndex(10,1);
        
            Assert.AreEqual(component.PositionX, 10);
            Assert.AreEqual(component.PositionY, 1);
        }
        
        [Test]
        public void TestDispose()
        {
            var component = _gameObject.AddComponent<TileElementMono>();
            component.Init(7,5, Color.black, Camera.main);
        
            Assert.AreEqual(component.PositionX, 7);
            Assert.AreEqual(component.PositionY, 5);
            
            component.Dispose();
        
            Assert.AreEqual(component.PositionX, -1);
            Assert.AreEqual(component.PositionY, -1);
        }
    }
}
