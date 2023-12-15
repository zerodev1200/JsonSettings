using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using FluentAssertions;
using nucs.JsonSettings.Fluent;
using nucs.JsonSettings.Modulation;
using nucs.JsonSettings.xTests.Utils;
using Newtonsoft.Json;
using NUnit.Framework;

namespace nucs.JsonSettings.xTests {
    [TestFixture]
    public class ConfigurableTests {
        [Test]
        public void OnConfigure_AddSingleConfig() {
            using var f = new TempfileLife();
            //used for autodelete file after test ends
            var o = JsonSettings.Configure<CasualConfiguredSettings>(f.FileName).LoadNow();
            o.Modulation.Modules.Should().ContainItemsAssignableTo<RijndaelModule>();
            (o.Modulation.Modules.Count == 1).Should().BeTrue();
        }

        [Test]
        public void OnConfigure_AddSingleConfig_PriorToLoadNow() {
            using var f = new TempfileLife();
            //used for autodelete file after test ends
            var o = JsonSettings.Configure<CasualConfiguredSettings>(f.FileName);
            o.Modulation.Modules.Should().ContainItemsAssignableTo<RijndaelModule>();
            (o.Modulation.Modules.Count == 1).Should().BeTrue();
        }

        [Test]
        public void OnConfigure_Only_WithEncyption() {
            using var f = new TempfileLife();
            //used for autodelete file after test ends
            var o = JsonSettings.Configure<CasualConfiguredSettings>(f.FileName).LoadNow();
            o.Modulation.Modules.Should().ContainItemsAssignableTo<RijndaelModule>();
            (o.Modulation.Modules.Count == 1).Should().BeTrue();

            o.SomeNumeralProperty = 1;
            o.SomeProperty = "with some value";
            o.SomeClassProperty = new SmallClass() { Name = "Small", Value = "Class" };
            o.Save();

            //validate
            o = JsonSettings.Configure<CasualConfiguredSettings>(f.FileName).LoadNow();
            o.Modulation.Modules.Should().ContainItemsAssignableTo<RijndaelModule>();
            o.SomeProperty.Should().Be("with some value");
            o.SomeNumeralProperty.Should().Be(1);
            o.SomeClassProperty.Should().BeOfType(typeof(SmallClass)).And.Match(obj => (obj as SmallClass).Name == "Small");
        }

        [Test]
        public void OnConfigure_Only_WithEncyption_CheckBeforeLoadNow() {
            using var f = new TempfileLife();
            //used for autodelete file after test ends
            var o = JsonSettings.Configure<CasualConfiguredSettings>(f.FileName);
            o.Modulation.Modules.Should().ContainItemsAssignableTo<RijndaelModule>();
            (o.Modulation.Modules.Count == 1).Should().BeTrue();
            o.LoadNow();
            o.SomeNumeralProperty = 1;
            o.SomeProperty = "with some value";
            o.SomeClassProperty = new SmallClass() { Name = "Small", Value = "Class" };
            o.Save();

            //validate
            o = JsonSettings.Configure<CasualConfiguredSettings>(f.FileName).LoadNow();
            o.Modulation.Modules.Should().ContainItemsAssignableTo<RijndaelModule>();
            o.SomeProperty.Should().Be("with some value");
            o.SomeNumeralProperty.Should().Be(1);
            o.SomeClassProperty.Should().BeOfType(typeof(SmallClass)).And.Match(obj => (obj as SmallClass).Name == "Small");
        }
    }

    public class CasualConfiguredSettings : CasualExampleSettings {
        protected override void OnConfigure() {
            base.OnConfigure(); //Important!
            this.WithEncryption("password");
        }

        private CasualConfiguredSettings() { }
        public CasualConfiguredSettings(string someprop) : base(someprop) { }
    }
}