using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qx;

namespace UnitTestProject1
{
    [TestClass]
    public class SpecificationTest
    {
        [TestMethod]
        public void QueueCreation()
        {
            IMessageQueue queue = new MessageQueue();
        }
    }
}
