using System.Collections.Generic;

namespace SimpleMappings.Tests.TestShapes
{
    public class MyClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<int> Numbers { get; set; }
        public MyStruct[] Structs { get; set; }
    }
}