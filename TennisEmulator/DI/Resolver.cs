using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TennisEmulator.DI
{
    public static class Resolver
    {
        private static Dictionary<Type, object> intances = new Dictionary<Type, object>();
        public static void Config<T>(T intance) where T : class
        {
            Resolver.intances[typeof(T)] = intance;
        }

        public static T Resolve<T>() where T : class
        {
            return Resolver.intances.GetValueOrDefault(typeof(T)) as T;
        }
    }
}
