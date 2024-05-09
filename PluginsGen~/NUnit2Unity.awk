/using Microsoft.VisualStudio.TestTools.UnitTesting;/ {
  print "using NUnit.Framework;"
  print "using UnityEngine.TestTools;"
  next
}

/\[TestClass\]/ {
  sub("TestClass", "TestFixture")
  print $0
  next
}

/\[TestMethod\]/ {
  sub("TestMethod", "Test")
  print $0
  next
}

/Assert.ThrowsException/ {
  sub("Assert.ThrowsException", "Assert.Throws")
  print $0
  next
}

/Assert.AreEqual<.*>/ {
  sub("Assert.AreEqual<.*>", "Assert.AreEqual")
  print $0
  next
}

/Assert.AreNotEqual<.*>/ {
  sub("Assert.AreNotEqual<.*>", "Assert.AreNotEqual")
  print $0
  next
}

/Assert.IsInstanceOfType/ {
  sub("Assert.IsInstanceOfType", "Assert.IsInstanceOf")
  print $0
  next
}

/.*/ {
  print
}