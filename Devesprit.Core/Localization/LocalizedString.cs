using System.Collections.Generic;

namespace Devesprit.Core.Localization
{
    public partial class LocalizedString : Dictionary<int, string>
    {
        public LocalizedString() { }

        public LocalizedString(string defaultValue)
        {
            Add(0, defaultValue);
        }

        public override string ToString()
        {
            if (ContainsKey(0))
            {
                return this[0];
            }
            return string.Empty;
        }
    }
}
