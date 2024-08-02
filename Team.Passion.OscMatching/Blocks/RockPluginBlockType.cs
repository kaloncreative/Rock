using System.Linq;

namespace Rock.Blocks
{
    
    public class RockPluginBlockType : RockBlockType
    {
        /// <inheritdoc />
        public override string ObsidianFileUrl => GetObsidianFileUrl();

        private string GetObsidianFileUrl()
        {
            var type = GetType();

            // Get all the namespaces after the first one with the name "Blocks".
            // Standard namespacing for blocks is to be one of:
            // Rock.Blocks.x.y.z
            // com.rocksolidchurchdemo.Blocks.x.y.z
            var namespaces = type.Namespace.Split('.')
                .SkipWhile(n => n != "Blocks")
                .Skip(1)
                .ToList();

            // Filename convention is camelCase.
            var fileName = $"{type.Name.Substring(0, 1).ToLower()}{type.Name.Substring(1)}";

            return $"/Plugins/team_passion/{namespaces.AsDelimited("/")}/{fileName}.obs";
        }
    }
}
