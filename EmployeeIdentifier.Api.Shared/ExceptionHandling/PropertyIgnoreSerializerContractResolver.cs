using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;

namespace EmployeeIdentifier.Api.Shared.ExceptionHandling
{
    public class PropertyIgnoreSerializerContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> _ignores;

        public PropertyIgnoreSerializerContractResolver(Dictionary<Type, HashSet<string>> ignores)
        {
            NamingStrategy = new CamelCaseNamingStrategy();

            _ignores = ignores;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = i => false;
                property.Ignored = true;
            }

            return property;
        }

        private bool IsIgnored(Type type, string jsonPropertyName)
        {
            if (!_ignores.TryGetValue(type, out HashSet<string> value))
                return false;

            return value.Contains(jsonPropertyName.ToLower());
        }
    }
}
