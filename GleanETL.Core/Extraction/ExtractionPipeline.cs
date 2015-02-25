using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GleanETL.Core.Extraction
{
    public class ExtractionPipeline
    {
        #region Fields

        private readonly List<IExtract> _extracts = new List<IExtract>();

        #endregion Fields

        #region Methods

        public void AddExtract(IExtract extract)
        {
            if (extract != null)
            {
                _extracts.Add(extract);
            }
        }

        public void Clear()
        {
            _extracts.Clear();
        }

        public void ProcessParallel()
        {
            Parallel.ForEach(_extracts, extract =>
            {
                Trace.WriteLine("*** STARTING " + extract.ToString());
                extract.ExtractToTarget();

                extract = null;
            });
        }

        public void ProcessSeries()
        {
            for (var i = 0; i < _extracts.Count(); i++)
            {
                Trace.WriteLine("*** STARTING " + _extracts[i].ToString());
                _extracts[i].ExtractToTarget();
                _extracts[i] = null;
            }
        }

        #endregion Methods
    }
}