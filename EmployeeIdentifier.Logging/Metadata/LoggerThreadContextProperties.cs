
namespace EmployeeIdentifier.Logging.Metadata
{
    public class LoggerThreadContextProperties
    {
        private const string _slotName = "MyLogicalThreadContextProperties";

        public object this[string key]
        {
            get
            {
                // Don't create the dictionary if it does not already exist
                Dictionary<string, object> dictionary = GetProperties(false);
                if (dictionary != null)
                {
                    return dictionary[key];
                }
                return null;
            }
            set
            {
                // Force the dictionary to be created
                GetProperties(true)[key] = value;
            }
        }

        public object TryGetValue(string key)
        {
            Dictionary<string, object> dictionary = GetProperties(false);
            if (dictionary != null)
            {
                dictionary.TryGetValue(key, out object value);
                return value;
            }
            return null;
        }

        internal Dictionary<string, object> GetProperties(bool create)
        {
            Dictionary<string, object> properties = GetCallContextData();
            if (properties == null && create)
            {
                properties = new Dictionary<string, object>();
                SetCallContextData(properties);
            }
            return properties;

        }

        private static Dictionary<string, object> GetCallContextData()
        {
            return CallContext.GetData(_slotName) as Dictionary<string, object>;
        }

        private static void SetCallContextData(Dictionary<string, object> properties)
        {
            CallContext.SetData(_slotName, properties);
        }
    }
}
