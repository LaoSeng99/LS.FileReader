using System;

namespace LS.FileReader.Attributes
{
    public class HeaderColumnAttribute : Attribute
    {
        public string[] Aliases { get; }
        public HeaderColumnAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }
    }

}

