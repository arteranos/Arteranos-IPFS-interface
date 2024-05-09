/\[Test\]/ {
  primed = 1
  next
}

/public void / {
  primed = 0
  print "        [Test]"
  print $0
  next
}

/public async Task / {
  if (primed == 0) { 
    print
    next 
  }
  
  primed = 0
  print "        [UnityTest]"
  print "        public System.Collections.IEnumerator Async_"$4
  print "        {"
  print "            yield return Unity.Asyncs.Async2Coroutine("$4");"
  print "        }"
  print ""
  print "        public async Task "$4
  next
}

// {
  primed = 0
  print
}