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
                this.extracts.Add(extract);
            }
        }

        public void Clear()
        {
            this.extracts.Clear();
        }

        public void ProcessParallel()
        {
            Parallel.ForEach(
                this.extracts,
                extract =>
                {
                    Trace.WriteLine("*** STARTING " + extract.ToString());
                    extract.ExtractToTarget();

                    extract = null;
                });
        }

        public void ProcessSeries()
        {
            for (var i = 0; i < this.extracts.Count(); i++)
            {
                Trace.WriteLine("*** STARTING " + this.extracts[i]);
                this.extracts[i].ExtractToTarget();
                this.extracts[i] = null;
            }
        }
    }
}