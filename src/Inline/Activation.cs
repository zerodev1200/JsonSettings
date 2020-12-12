﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Utilities;
using Module = nucs.JsonSettings.Modulation.Module;

namespace nucs.JsonSettings {
    public static class Activation {
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type t) => t.GetConstructors().Concat(t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        /// <summary>
        ///     Does the type have public/private/protected/internal.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool HasDefaultConstructor(this Type t) {
            var ctrs = t.GetAllConstructors();
            ctrs=ctrs.Where(c=>c.GetParameters().Length == 0 || c.GetParameters().All(p => p.IsOptional)).ToArray();
            return ReflectionHelpers.IsValueType(t) || (ctrs.Any(c => c.GetParameters().Length == 0 || c.GetParameters().All(p => p.IsOptional)));
        }

        public static object CreateInstance(this Type t) {
            var ctrs = t.GetAllConstructors().Where(c=>c.GetParameters().Length == 0 || c.GetParameters().All(p => p.IsOptional)).ToArray();
            if (ReflectionHelpers.IsValueType(t) || ctrs.Any(c => c.IsPublic)) //is valuetype or has public constractor.
                return Activator.CreateInstance(t);
            var prv = ctrs.FirstOrDefault(c => c.IsAssembly ||c.IsFamily || c.IsPrivate); //check protected/internal/private constructor
            if (prv == null)
                throw new ReflectiveException($"Type {t.FullName} does not have empty constructor (public or private)");
            return prv.Invoke(null);
        }

        public static object CreateInstance(this Type t, object[] args) {
            if (args == null || args.Length==0) return t.CreateInstance();
            try {
                return Activator.CreateInstance(t, args);
            } catch (AmbiguousMatchException) {
                return t.GetAllConstructors().Where(ci => {
                    //todo test and check for constructors with default values too.
                    var p = ci.GetParameters();
                    if (p.Length != args.Length)
                        return false;

                    //all args are null
                    if (args.All(arg => arg == null))
                        return true;

                    //smart match
                    for (int i = 0; i < p.Length; i++) {
                        var arg = args[i];
                        var param = p[i];
                        if (arg == null)
                            continue;
                        if (arg.GetType() != param.ParameterType)
                            goto _nomatch;
                    }

                    return true;

                    _nomatch:
                    return false;
                }).FirstOrDefault()?.Invoke(args);
            }
        }
    }

    public class ReflectiveException : Exception {
        public ReflectiveException() { }
        public ReflectiveException(string message) : base(message) { }
        public ReflectiveException(string message, Exception inner) : base(message, inner) { }
    }
}