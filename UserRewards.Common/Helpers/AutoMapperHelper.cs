using AutoMapper;
using System;
using System.Linq;
using System.Reflection;

namespace UserRewards.Common.Helpers
{
    public static class AutoMapperHelper
    {
        /// <summary>
        /// Finds all profiles in the assembly and adds them to the MapperConfiguration
        /// </summary>
        /// <returns>MapperConfiguration with every profile</returns>
        public static MapperConfiguration ConfigureAutomapper()
        {
            var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assembliesToScan.Where(a => !a.IsDynamic).SelectMany(a => a.ExportedTypes).ToArray();

            var profiles =
                allTypes
                    .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                    .Where(t => !t.GetTypeInfo().IsAbstract);



            return new MapperConfiguration(cfg =>
            {
                foreach (var profile in profiles)
                {
                    cfg.AddProfile(profile);
                }
            });
        }
    }
}
