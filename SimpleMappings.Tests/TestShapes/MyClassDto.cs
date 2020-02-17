using System.Collections.Generic;

namespace SimpleMappings.Tests.TestShapes
{
    public class MyClassDto
    {
        public string name { get; set; }
        public int _age { get; set; }
        public IEnumerable<int> numbers { get; set; }
        public IReadOnlyList<MyStruct> structs { get; set; }
        public double sumOfStructs { get; set; }
    }
}