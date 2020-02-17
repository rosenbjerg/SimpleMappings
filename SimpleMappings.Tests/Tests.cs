using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleMappings.Tests.TestShapes;

namespace SimpleMappings.Tests
{
    public class Tests
    {
        [Test]
        public void ThrowsExceptionIfUnmappedProperty()
        {
            Assert.Throws<MappingException>(() =>
            {
                MappingBuilder<MyClass, MyClassDto>.New()
                    .AutomapRemaining()
                    .ThrowIfUnmapped()
                    .UsingFactory<MyClassDto>();
            });
        }
        [Test]
        public void BasicMappingTest()
        {
            var mapper = MappingBuilder<MyClass, MyClassDto>.New()
                .MapProperty(mc => mc.Structs.Sum(s => s.Stat), mcd => mcd.sumOfStructs)
                .AutomapRemaining()
                .ThrowIfUnmapped()
                .UsingFactory<MyClassDto>();
            
            var model = new MyClass
            {
                Age = 23,
                Name = "daw",
                Numbers = new List<int> {1, 2, 3},
                Structs = new[] {new MyStruct {Stat = 2.3}, new MyStruct {Stat = 5.4}}
            };

            var dto = mapper.Map(model);
            
            Assert.AreEqual(model.Age, dto._age);
            Assert.AreEqual(model.Name, dto.name);
            Assert.AreEqual(model.Numbers.Count, dto.numbers.Count());
            Assert.AreEqual(model.Structs.Length, dto.structs.Count);
            Assert.AreEqual(model.Structs.Sum(s => s.Stat), dto.sumOfStructs);
        }
    }
}