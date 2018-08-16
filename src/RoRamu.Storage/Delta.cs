namespace RoRamu.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Delta<TEntity>
    {
        private readonly IEnumerable<PropertyInfo> _propertyInfos;
        private readonly IReadOnlyDictionary<string, Type> _propertyTypes;

        private readonly IDictionary<string, object> _updatedData = new Dictionary<string, object>();

        public Delta()
        {
            this._propertyInfos = GetPropertiesFromEntity();
            this._propertyTypes = this._propertyInfos.ToDictionary(
                prop => prop.Name,
                prop => prop.PropertyType);
        }

        public IEnumerable<string> GetPropertyNames()
        {
            return this._propertyTypes.Keys;
        }

        public bool TrySetProperty(string propertyName, object newValue)
        {
            if (this._propertyTypes.ContainsKey(propertyName) && // property should exist
                this._propertyTypes[propertyName].IsAssignableFrom(newValue.GetType())) // value must be of the correct type
            {
                this._updatedData[propertyName] = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ResetProperty(string propertyName)
        {
            if (!this._propertyTypes.ContainsKey(propertyName))
            {
                throw new ArgumentException($"There is no property called '{propertyName}' on the type {typeof(TEntity).FullName}", nameof(propertyName));
            }

            this._updatedData.Remove(propertyName);
        }

        public void ResetAllProperties()
        {
            this._updatedData.Clear();
        }

        public void UpdateEntity(TEntity entity)
        {
            foreach (PropertyInfo property in this._propertyInfos)
            {
                // Get the property name
                string propertyName = property.Name;

                // Get the updated property value
                object value = this._updatedData.ContainsKey(propertyName)
                    // If the value has been updated, get it
                    ? this._updatedData[propertyName]
                    // Otherwise, get the value from the original entity
                    : property.GetValue(entity);

                property.SetValue(propertyName, value);
            }
        }

        private static IEnumerable<PropertyInfo> GetPropertiesFromEntity()
        {
            Type entityType = typeof(TEntity);
            PropertyInfo[] propertyInfo = entityType.GetProperties(BindingFlags.Public);

            return propertyInfo
                // Dedupe the properties in case some from the base classes were either overridden or hidden
                .GroupBy(
                    prop => prop.Name,
                    (propName, props) => props.OrderBy(prop =>
                    {
                        Type currentType = entityType;
                        int timesInherited = 0;
                        while (prop.DeclaringType != currentType)
                        {
                            currentType = currentType.BaseType;
                            timesInherited++;
                        }

                        return timesInherited;
                    }).First());
        }
    }
}
