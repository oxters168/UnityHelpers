using System.Linq;

namespace UnityHelpers
{
    [System.Serializable]
    public class ValuesVault
    {
        public ValueWrapper[] inputValues;

        public ValueWrapper GetValue(string name)
        {
            return inputValues.FirstOrDefault(value => value.name == name);
        }
    }
}