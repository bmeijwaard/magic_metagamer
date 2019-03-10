using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace The_MTG_Metagamer_Shared.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetInnerExeptionsStackTrace(this Exception e)
        {
            return string.Join("####", e.GetInnerExceptions().Select(ex => ex.StackTrace));
        }

        public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
        {
            if (ex == null)            
                throw new ArgumentNullException("ex");            

            var innerException = ex;
            do
            {
                yield return innerException;
                innerException = innerException.InnerException;
            }
            while (innerException != null);
        }
    }
}
