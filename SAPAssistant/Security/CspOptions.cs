using System.Collections.Generic;

namespace SAPAssistant.Security
{
    public class CspOptions
    {
        public List<string> DefaultSrc { get; set; } = new();
        public List<string> ScriptSrc { get; set; } = new();
        public List<string> StyleSrc { get; set; } = new();
        public List<string> FontSrc { get; set; } = new();
    }
}
