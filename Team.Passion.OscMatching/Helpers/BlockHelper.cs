using System;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;

namespace Team.Passion.OscMatching.Helpers
{
    public class BlockHelper
    {
        private readonly RockContext _rockContext;
        private readonly BlockService _blockService;

        public BlockHelper(RockContext rockContext)
        {
            _rockContext = rockContext;
            _blockService = new BlockService(_rockContext);
        }

        public T GetBlockAdditionalSettings<T>(int blockId, T defaultValue = default) where T : class
        {
            T result = defaultValue;
            try
            {
                result = _blockService
                    .Queryable()
                    .Where(x => x.Id == blockId)
                    .Select(x => x.AdditionalSettings)
                    .FirstOrDefault()
                    ?.FromJsonOrNull<T>();
            }
            catch (Exception)
            {
                // Ignore
            }

            return result;
        }

        public void SetBlockAdditionalSettings<T>(int blockId, T configuration)
        {
            var block = _blockService.Get(blockId);
            block.AdditionalSettings = configuration.ToJson();
            _rockContext.SaveChanges();
        }
    }
}
