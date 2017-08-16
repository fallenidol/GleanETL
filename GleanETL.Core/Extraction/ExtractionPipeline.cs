namespace Glean.Core.Extraction
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class ExtractionPipeline
    {
        private readonly List<IExtract> extracts = new List<IExtract>();

        public void AddExtract(IExtract extract)
        {
            if (extract != null)
            {
                extracts.Add(extract);
            }
        }

        public void Clear()
        {
            extracts.Clear();
        }

        public void ProcessParallel()
        {
            Parallel.ForEach(
                extracts,
                extract =>
                {
                    Trace.WriteLine("*** STARTING " + extract.ToString());
                    extract.ExtractToTarget();

                    extract = null;
                });
        }

        public void ProcessSeries()
        {
            for (var i = 0; i < extracts.Count(); i++)
            {
                Trace.WriteLine("*** STARTING " + extracts[i]);
                extracts[i].ExtractToTarget();
                extracts[i] = null;
            }
        }
    }
}