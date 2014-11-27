namespace Gleanio.Framework.Extraction
{
    using System;

    using Gleanio.Framework.EventArgs;
    using Gleanio.Framework.Source;
    using Gleanio.Framework.Target;

    public abstract class Extract : IExtract
    {
        #region Constructors

        protected Extract(TextFile source, IExtractTarget target)
        {
            Source = source;
            Target = target;
        }

        #endregion Constructors

        #region Events

        public event EventHandler<ExtractCompleteEventArgs> ExtractComplete;

        #endregion Events

        #region Properties

        public TextFile Source
        {
            get; private set;
        }

        public IExtractTarget Target
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        public abstract void ExtractToTarget();

        protected void OnExtractComplete()
        {
            if (ExtractComplete != null)
            {
                var args = new ExtractCompleteEventArgs();
                ExtractComplete.Invoke(this, args);
            }
        }

        protected void OnExtractComplete(ExtractCompleteEventArgs args)
        {
            if (ExtractComplete != null)
            {
                ExtractComplete.Invoke(this, args);
            }
        }

        #endregion Methods
    }
}