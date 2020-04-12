using System;

namespace ShaderBox
{
    public enum ShaderType
    {
        Vertex,
        Pixel
    }

    public static class ShaderTypeEx
    {
        private static string[] ShaderTypeNames;

        private static string ShaderTypeNamesAsSingleStringSeparatedByZeroes;

        public static string[] GetShaderTypeNames()
        {
            if (ShaderTypeNames == null)
            {
                ShaderTypeNames = System.Enum.GetNames(typeof(ShaderType));
            }
            
            return ShaderTypeNames;
        }

        public static string GetShaderTypeNamesAsSingleStringSeparatedByZeroes()
        {
            if (string.IsNullOrEmpty(ShaderTypeNamesAsSingleStringSeparatedByZeroes))
            {
                var names = GetShaderTypeNames();
                foreach (var name in names)
                {
                    ShaderTypeNamesAsSingleStringSeparatedByZeroes += name + '\0';
                }
            }

            return ShaderTypeNamesAsSingleStringSeparatedByZeroes;
        }

        public static int IndexOf(string value)
        {
            //int intValue = (int)System.Enum.Parse(typeof(ShaderType), value);
            //return intValue;

            return Array.IndexOf(GetShaderTypeNames(), value);
        }
    }
}
