using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace SAPAssistant.Security
{
    public interface ICspBuilder
    {
        string Build();
    }

    public class CspBuilder : ICspBuilder
    {
        private readonly CspOptions _options;

        public CspBuilder(IOptions<CspOptions> options)
        {
            _options = options.Value;
        }

        public string Build()
        {
            var directives = new List<string>();

            if (_options.DefaultSrc?.Count > 0)
                directives.Add($"default-src {string.Join(' ', _options.DefaultSrc)}");

            if (_options.ScriptSrc?.Count > 0)
                directives.Add($"script-src {string.Join(' ', _options.ScriptSrc)}");

            if (_options.StyleSrc?.Count > 0)
                directives.Add($"style-src {string.Join(' ', _options.StyleSrc)}");

            if (_options.FontSrc?.Count > 0)
                directives.Add($"font-src {string.Join(' ', _options.FontSrc)}");

            return string.Join("; ", directives) + ";";
        }
    }
}
