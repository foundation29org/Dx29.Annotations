using System;
using System.IO;

using Dx29.Services;

namespace Dx29
{
    public class BlobLogger : Logger
    {
        public BlobLogger(BlobStorage storage, string container, string path, LogMode mode = LogMode.Info, bool indented = true)
            : base(new StringWriter(), mode, indented)
        {
            Storage = storage;
            Container = container;
            Path = path;
        }

        public BlobStorage Storage { get; }

        public string Container { get; }
        public string Path { get; }

        public override void Log(LogMode mode, string message, object details = null)
        {
            base.Log(mode, message, details);
            Storage.UploadString(Container, Path, $"{{\n{base.Writer.ToString()}}}");
        }
    }
}
