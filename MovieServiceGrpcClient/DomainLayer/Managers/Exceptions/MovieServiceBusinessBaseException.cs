using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieServiceGrpcClient.DomainLayer.Managers.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public abstract class MovieServiceBusinessBaseException : MovieServiceBaseException
    {
        protected MovieServiceBusinessBaseException() { }
        protected MovieServiceBusinessBaseException(string message) : base(message) { }
        protected MovieServiceBusinessBaseException(string message, Exception inner) : base(message, inner) { }
        protected MovieServiceBusinessBaseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
