﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieServiceGrpcClient.DomainLayer.Managers.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public sealed class InvalidMovieException : MovieServiceBusinessBaseException
    {
        public override string Reason => "Invalid Movie";

        public InvalidMovieException() { }
        public InvalidMovieException(string message) : base(message) { }
        public InvalidMovieException(string message, Exception inner) : base(message, inner) { }
        private InvalidMovieException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
