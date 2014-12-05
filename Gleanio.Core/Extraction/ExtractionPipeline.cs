using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gleanio.Core.Extraction
{
    public class ExtractionPipeline
    {
        #region Fields

        private readonly List<IExtract> _extracts = new List<IExtract>();

        #endregion Fields

        #region Methods

        public void AddExtract(IExtract extract)
        {
            _extracts.Add(extract);
        }

        public void Clear()
        {
            _extracts.Clear();
        }

        public void ProcessParallel()
        {
            Parallel.ForEach(_extracts, extract =>
            {
                extract.ExtractToTarget();

                extract = null;
            });
        }

        public void ProcessSeries()
        {
            for (var i = 0; i < _extracts.Count(); i++)
            {
                _extracts[i].ExtractToTarget();
                _extracts[i] = null;
            }
        }

        #endregion Methods
    }
}