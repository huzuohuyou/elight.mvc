using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCenterSystem.Utils
{
    public delegate bool EqualsComparer<T>(T x, T y);

    public class Compare<T> : IEqualityComparer<T>
    {
        private EqualsComparer<T> _equalsComparer;

        public Compare(EqualsComparer<T> equalsComparer)
        {
            this._equalsComparer = equalsComparer;
        }

        public bool Equals(T x, T y)
        {
            if (null != this._equalsComparer)
                return this._equalsComparer(x, y);
            else
                return false;
        }

        public int GetHashCode(T obj)
        {
            return obj.ToString().GetHashCode();
        }

        public static bool CompareValue(T obj1, T obj2)
        {
            var isMatch = true;
            try
            {
                var type1 = obj1.GetType();
                var type2 = obj2.GetType();
                var properties1 = type1.GetProperties();
                var properties2 = type2.GetProperties();
                for (var i = 0; i < properties1.Length; i++)
                {
                    if (properties1[i].GetValue(obj1, null) == null)
                    {
                        if (properties2[i].GetValue(obj2, null) != null)
                            isMatch = false;
                    }
                    else if (properties2[i].GetValue(obj2, null) == null)
                    {
                        if (properties1[i].GetValue(obj1, null) != null)
                            isMatch = false;
                    }
                    else if (!properties1[i].GetValue(obj1, null).Equals(properties2[i].GetValue(obj2, null)))
                    {
                        isMatch = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return isMatch;
        }

    }
}
