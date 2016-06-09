using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RapidDoc.Extensions
{
    public class HierarchyComparer : IComparer<String>
    {
        public int Compare(String a, String b)
        {
            string[] arrayOne = a.Split('-');
            string[] arrayTwo = b.Split('-');

            int arrayOneCount = arrayOne.Count();
            int arrayTwoCount = arrayTwo.Count();
            int minLevel = Math.Min(arrayOneCount, arrayTwoCount);

            for (int i = 0; i < minLevel; i++ )
            {
                int valOne = Convert.ToInt32(arrayOne[i]);
                int valTwo = Convert.ToInt32(arrayTwo[i]);

                if (valOne < valTwo)
                    return -1;
                else if (valOne > valTwo)
                    return 1;
            }

            if (arrayOneCount < arrayTwoCount)
                return -1;
            else if (arrayOneCount > arrayTwoCount)
                return 1;

            return 0;
        }
    }
}