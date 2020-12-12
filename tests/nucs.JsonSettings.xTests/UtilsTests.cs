using System;
using System.IO;
using System.Reflection;
using System.Security;
using FluentAssertions;
using nucs.JsonSettings.xTests.Utils;
using NUnit.Framework;


namespace nucs.JsonSettings.xTests
{
    [TestFixture]
    public class UtilsTests
    {
        [Test]
        public void Activation_PrivateConst()
        {
            var a = Activation.CreateInstance(typeof(PrivateConst));
            a.Should().BeOfType<PrivateConst>();
        }

        [Test]
        public void Activation_ProtectedConst()
        {
            var a = Activation.CreateInstance(typeof(ProtectedConst));
            a.Should().BeOfType<ProtectedConst>();
        }

        [Test]
        public void Activation_PublicConst()
        {
            var a = Activation.CreateInstance(typeof(PublicConst));
            a.Should().BeOfType<PublicConst>();
        }

        [Test]
        public void Activation_InternalConst()
        {
            var a = Activation.CreateInstance(typeof(InternalConst));
            a.Should().BeOfType<InternalConst>();
        }

        [Test]
        public void Activation_NoEmptyConst()
        {
            Action a = () => Activation.CreateInstance(typeof(NoEmptyConst));
            a.Should().Throw<ReflectiveException>();
        }

        [Test]
        public void Activation_FallbackWhenNullArgs()
        {
            var a = Activation.CreateInstance(typeof(PublicConst), null);
            a.Should().BeOfType<PublicConst>();
        }

        [Test]
        public void Activation_ObjectConstructor()
        {
            object a() => Activation.CreateInstance(typeof(AmbiousClass), new object[] { null });
            a().Should().BeOfType<AmbiousClass>();
        }

        [Test]
        public void Activation_AmbiousDefaultClass()
        {
            object a() => Activation.CreateInstance(typeof(AmbiousDefaultClass), new object[] { null });
            a().Should().BeOfType<AmbiousDefaultClass>();
        }

        [Test]
        public void Activation_AmbiousDefaultWithSameClass()
        {
            object a() => Activation.CreateInstance(typeof(AmbiousDefaultWithSameClass), new object[] { null, null });
            a().Should().BeOfType<AmbiousDefaultWithSameClass>();
        }

        [Test]
        public void Activation_NoMatchClass()
        {
            Action a = () => Activation.CreateInstance(typeof(NoMatchClass), new object[] { null });
            a.Should().Throw<MissingMethodException>();
        }

        class AmbiousClass { }

        class AmbiousDefaultClass { }

        class AmbiousDefaultWithSameClass { }

        class NoMatchClass { }

        class PrivateConst
        {
            private PrivateConst() { }
        }

        class InternalConst
        {
            internal InternalConst() { }
        }

        class ProtectedConst { }

        class PublicConst { }

        class NoEmptyConst { }
    }
}