using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleMappings.Tests.TestShapes;

namespace SimpleMappings.Tests
{
    public class Tests
    {
        [Test]
        public void ThrowsIfAnyPropertyIsUnmapped()
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
        public void DoesNotThrowIfAllPropertiesAreMapped()
        {
            Assert.DoesNotThrow(() =>
            {
                MappingBuilder<MyClass, MyClassDto>.New()
                    .MapProperty(mc => mc.Structs.Sum(s => s.Stat), mcd => mcd.sumOfStructs)
                    .AutomapRemaining()
                    .ThrowIfUnmapped()
                    .UsingFactory<MyClassDto>();
            });
        }

        [Test]
        public void ManuallyMappedProperty()
        {
            var mapper = MappingBuilder<MyClass, MyClassDto>.New()
                .AutomapRemaining()
                .UsingFactory<MyClassDto>();

            var model = new MyClass
            {
                Age = 23,
                Name = "daw",
                Numbers = new List<int> {1, 2, 3},
            };

            var dto = mapper.Map(model);

            Assert.AreEqual(model.Age, dto._age);
            Assert.AreEqual(model.Name, dto.name);
            Assert.AreEqual(model.Numbers.Count, dto.numbers.Count());
            Assert.AreEqual(null, dto.structs);
        }

        [Test]
        public void OnlyDefaultValues()
        {
            var mapper = MappingBuilder<MyClass, MyClassDto>.New()
                .AutomapRemaining()
                .UsingFactory<MyClassDto>();

            var model = new MyClass();

            var dto = mapper.Map(model);

            Assert.AreEqual(0, dto._age);
            Assert.AreEqual(default, dto.name);
            Assert.AreEqual(default, dto.numbers);
            Assert.AreEqual(default, dto.structs);
            Assert.AreEqual(0.0d, dto.sumOfStructs);
        }

        [Test]
        public void AutoMappedProperties()
        {
            var mapper = MappingBuilder<MyClass, MyClassDto>.New()
                .MapProperty(mc => mc.Structs.Sum(s => s.Stat), mcd => mcd.sumOfStructs)
                .UsingFactory<MyClassDto>();

            var model = new MyClass
            {
                Structs = new[] {new MyStruct {Stat = 2.3}, new MyStruct {Stat = 5.4}}
            };

            var dto = mapper.Map(model);

            Assert.AreEqual(model.Structs.Sum(s => s.Stat), dto.sumOfStructs);
        }

        class Mapper : MapperBase
        {
            public Mapping<MyClass, MyClassDto> MyClassMapper => MappingBuilder<MyClass, MyClassDto>.New()
                .MapProperty(mc => mc.Structs.Sum(s => s.Stat), mcd => mcd.sumOfStructs)
                .UsingFactory<MyClassDto>();
        }

        [Test]
        public void BasicMapperTest()
        {
            var mapper = new Mapper();

            var model = new MyClass
            {
                Age = 23,
                Name = "daw",
                Numbers = new List<int> {1, 2, 3},
                Structs = new[] {new MyStruct {Stat = 2.3}, new MyStruct {Stat = 5.4}}
            };

            var dto = mapper.Map<MyClass, MyClassDto>(model);

            Assert.AreEqual(model.Structs.Sum(s => s.Stat), dto.sumOfStructs);
        }
    }
}