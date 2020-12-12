﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FluentAssertions;
using nucs.JsonSettings.Autosave;
using nucs.JsonSettings.xTests.Utils;
using NUnit.Framework;

namespace nucs.JsonSettings.xTests.Autosave {
    [TestFixture]
    public class AutosaveTests {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public AutosaveTests() { }

        [SetUp]
        public void Setup() { Console.SetOut(TestContext.Out); }

        [Test]
        public void ClassWithoutInterfacesOrVirtuals() {
            using (var f = new TempfileLife()) {
                var o = JsonSettings.Load<InvalidSettings>(f.FileName);
                o = o.EnableAutosave();
                o.GetType().Namespace.Should().NotBe("Castle.Proxies");
            }
        }

        [Test]
        public void ClassWithInterfacesOrVirtuals() {
            using (var f = new TempfileLife()) {
                var o = JsonSettings.Load<Settings>(f.FileName).EnableAutosave();
                o.GetType().Namespace.Should().Be("Castle.Proxies");
            }
        }

        [Test]
        public void Saving() {
            using (var f = new TempfileLife()) {
                var rpath = JsonSettings.ResolvePath(f);

                bool saved = false;
                var o = JsonSettings.Load<Settings>(f.FileName).EnableAutosave();
                o.AfterSave += destinition => {
                    saved = true;
                };
                o.Property.Should().BeEquivalentTo(null);
                Console.WriteLine(File.ReadAllText(rpath));

                o.Property = "test";
                saved.Should().BeTrue();
                var o2 = JsonSettings.Load<Settings>(f.FileName).EnableAutosave();
                o2.Property.Should().BeEquivalentTo("test");
                var jsn = File.ReadAllText(rpath);
                jsn.Contains("\"test\"").Should().BeTrue();
                Console.WriteLine(jsn);
            }
        }

        [Test]
        public void IgnoreSavingWhenAbstractPropertyChanges() {
            using (var f = new TempfileLife()) {
                bool saved = false;
                var o = JsonSettings.Load<Settings>(f.FileName).EnableAutosave();
                o.AfterSave += destinition => {
                    saved = true;
                };

                o.FileName = "test.jsn";
                saved.Should().BeFalse();
            }
        }

        [Test]
        public void AccessingAfterLoadingAndMarkingAutosave() {
            using (var f = new TempfileLife()) {
                var o = JsonSettings.Load<Settings>(f.FileName).EnableAutosave();
                o.Property.Should().BeEquivalentTo(null);
                o.Property = "test";
                var o2 = JsonSettings.Load<Settings>(f.FileName).EnableAutosave();
                o2.Property.Should().BeEquivalentTo("test");
            }
        }

        [Test]
        public void SavingInterface() {
            using (var f = new TempfileLife()) {
                var rpath = JsonSettings.ResolvePath(f);
                var o = JsonSettings.Load<InterfacedSettings>(f.FileName).EnableIAutosave<ISettings>();

                Console.WriteLine(File.ReadAllText(rpath));
                o.Property.Should().BeEquivalentTo(null);
                o.Property = "test";
                var o2 = JsonSettings.Load<InterfacedSettings>(f.FileName);
                o2.Property.Should().BeEquivalentTo("test");

                var jsn = File.ReadAllText(rpath);
                jsn.Contains("\"test\"").Should().BeTrue();
                Console.WriteLine(jsn);
            }
        }

        public interface ISettings {
            string Property { get; set; }

            void Method();
        }

        public class InvalidSettings : JsonSettings {
            #region Overrides of JsonSettings

            /// <summary>
            ///     Serves as a reminder where to save or from where to load (if it is loaded on construction and doesnt change between constructions).<br></br>
            ///     Can be relative to executing file's directory.
            /// </summary>
            public override string FileName { get; set; } = "somename.jsn";

            public string Property { get; set; }

            public void Method() { }

            public InvalidSettings() { }
            public InvalidSettings(string fileName) : base(fileName) { }

            #endregion
        }

        public class Settings : JsonSettings {
            #region Overrides of JsonSettings

            /// <summary>
            ///     Serves as a reminder where to save or from where to load (if it is loaded on construction and doesnt change between constructions).<br></br>
            ///     Can be relative to executing file's directory.
            /// </summary>
            public override string FileName { get; set; } = "somename.jsn";

            public virtual string Property { get; set; }

            public virtual void Method() { }

            public Settings() { }
            public Settings(string fileName) : base(fileName) { }

            #endregion
        }

        public class InterfacedSettings : JsonSettings, ISettings {
            #region Overrides of JsonSettings

            /// <summary>
            ///     Serves as a reminder where to save or from where to load (if it is loaded on construction and doesnt change between constructions).<br></br>
            ///     Can be relative to executing file's directory.
            /// </summary>
            public override string FileName { get; set; } = "somename.jsn";

            public virtual string Property { get; set; }

            public virtual void Method() { }

            public InterfacedSettings() { }
            public InterfacedSettings(string fileName) : base(fileName) { }

            #endregion
        }
    }
}