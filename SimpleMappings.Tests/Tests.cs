using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleMappings.Tests.TestShapes;

namespace SimpleMappings.Tests
{
    public class Tests
    {
        [Test]
        public void ThrowsIfOnePropertyIsUnmapped()
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
        public void ThrowsIfAllPropertiesAreUnmapped()
        {
            Assert.Throws<MappingException>(() =>
            {
                MappingBuilder<MyClass, MyClassDto>.New()
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
                    .MapProperty(dto => dto.sumOfStructs, model => model.Structs.Sum(s => s.Stat))
                    .AutomapRemaining()
                    .ThrowIfUnmapped()
                    .UsingFactory<MyClassDto>();
            });
        }

        [Test]
        public void AutoMappedProperties()
        {
            var mapper = MappingBuilder<MyClass, MyClassDto>.New()
                .AutomapRemaining()
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
            Assert.AreEqual(0.0d, dto.sumOfStructs);
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
        public void ManuallyMappedProperty()
        {
            var mapper = MappingBuilder<MyClass, MyClassDto>.New()
                .MapProperty(d => d.sumOfStructs, m => m.Structs.Sum(s => s.Stat))
                .UsingFactory<MyClassDto>();

            var model = new MyClass
            {
                Structs = new[] {new MyStruct {Stat = 2.3}, new MyStruct {Stat = 5.4}}
            };

            var dto = mapper.Map(model);

            Assert.AreEqual(0, dto._age);
            Assert.AreEqual(default, dto.name);
            Assert.AreEqual(default, dto.numbers);
            Assert.AreEqual(default, dto.structs);
            Assert.AreEqual(model.Structs.Sum(s => s.Stat), dto.sumOfStructs);
        }

        class Mapper : MapperBase
        {
            public Mapping<MyClass, MyClassDto> MyClassMapper => MappingBuilder<MyClass, MyClassDto>.New()
                .MapProperty(dto => dto.sumOfStructs, model => model.Structs.Sum(s => s.Stat))
                .AutomapRemaining()
                .UsingFactory<MyClassDto>();
        }

        [Test]
        public void ReflectedMapperTest()
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
            
            Assert.AreEqual(model.Age, dto._age);
            Assert.AreEqual(model.Name, dto.name);
            Assert.AreEqual(model.Numbers.Count, dto.numbers.Count());
            Assert.AreEqual(model.Structs.Length, dto.structs.Count);
            Assert.AreEqual(model.Structs.Sum(s => s.Stat), dto.sumOfStructs);
        }
        [Test]
        public void PropertyMapperTest()
        {
            var mapper = new Mapper();

            var model = new MyClass
            {
                Age = 23,
                Name = "daw",
                Numbers = new List<int> {1, 2, 3},
                Structs = new[] {new MyStruct {Stat = 2.3}, new MyStruct {Stat = 5.4}}
            };

            var dto = mapper.MyClassMapper.Map(model);

            Assert.AreEqual(model.Age, dto._age);
            Assert.AreEqual(model.Name, dto.name);
            Assert.AreEqual(model.Numbers.Count, dto.numbers.Count());
            Assert.AreEqual(model.Structs.Length, dto.structs.Count);
            Assert.AreEqual(model.Structs.Sum(s => s.Stat), dto.sumOfStructs);
        }
    }
}