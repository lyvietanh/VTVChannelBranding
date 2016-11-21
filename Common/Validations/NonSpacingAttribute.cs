using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NonSpacingAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                string text = (string)value;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ')
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
