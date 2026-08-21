using LogicBuilder.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace Enrollment.Contexts
{
    public class EntityConfigurationHandler(DbContext context)
    {

        #region Properties
        protected DbContext Context { get; private set; } = context;
        #endregion Properties

        #region Methods
        public virtual void Configure(ModelBuilder modelBuilder)
        {
            foreach (Type propertyType in this.Context.GetType()
                .GetProperties()
                .Select(property => property.PropertyType)
                .Where(t => t.Name == "DbSet`1"))
            {
                Type modelType = propertyType.GetGenericArguments()[0];
                if (!typeof(BaseData).IsAssignableFrom(modelType))
                    continue;

                modelBuilder.Entity(modelType).Ignore(nameof(BaseData.EntityState));
            }

            Type interfaceType = typeof(Configuations.ITableConfiguration);
            interfaceType.Assembly.GetTypes().Where
            (
                p => interfaceType.IsAssignableFrom(p)
                && !p.GetTypeInfo().IsAbstract
                && !p.GetTypeInfo().IsGenericTypeDefinition
                && !p.GetTypeInfo().IsInterface
            )
            .ToList()
            .ForEach
            (
                t =>
                {
                    MethodInfo mi = t.GetMethod(nameof(Configuations.ITableConfiguration.Configure))!;//ITableConfiguration implements Configure
                    mi.Invoke(Activator.CreateInstance(t), [modelBuilder]);
                }
            );
        }
        #endregion Methods
    }
}
