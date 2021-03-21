using DomainLayer;
using DomainLayer.Managers.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Grpc.Services.MovieService;
using MovieResource = Grpc.Services.Movie;
using Movie = DomainLayer.Managers.Models.Movie;
using Genre = DomainLayer.Managers.Enums.Genre;
using GrpcGenre = Grpc.Enums.Genre;
using DomainLayer.Managers.Exceptions;
using DomainLayer.Managers.Parsers;

namespace MovieServiceYouTube.Services
{
    public class MovieServiceGrpc : MovieServiceBase
    {
        private readonly ILogger _logger;
        private readonly DomainFacade _domainFacade;
        public MovieServiceGrpc(ILogger<MovieServiceGrpc> logger, DomainFacade domainFacade)
        {
            _logger = logger;
            _domainFacade = domainFacade;
        }

        public override async Task<MovieResponse> GetAllMovies(Empty request, ServerCallContext context)
        {
            var movies = await _domainFacade.GetAllMovies();
            return MapToMovieResponse(movies);
        }

        public override async Task GetAllMoviesStream(Empty request, IServerStreamWriter<MovieResource> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await foreach (var movie in _domainFacade.GetAllMoviesStream())
                {
                    await responseStream.WriteAsync(MapToMovieResource(movie));
                }
            }
        }

        public override async Task<MovieResponse> GetMoviesByGenre(MoviesByGenreRequest request, ServerCallContext context)
        {
            var movies = await _domainFacade.GetMoviesByGenre(GenreParser.Parse(request.Genre.ToString()));
            return MapToMovieResponse(movies);
        }

        public override async Task<CreateMovieResponse> CreateMovie(MovieResource request, ServerCallContext context)
        {
            try
            {
                var id = await _domainFacade.CreateMovie(MapToMovie(request));
                return new CreateMovieResponse { Id = id };
            }
            catch (Exception e)
            {
                throw MapToRcpException(e);
            }
        }

        private static MovieResponse MapToMovieResponse(IEnumerable<Movie> movies)
        {
            var movieResponse = new MovieResponse();
            foreach (var movie in movies)
            {
                movieResponse.Movies.Add(MapToMovieResource(movie));
            }

            return movieResponse;
        }

        private static MovieResource MapToMovieResource(Movie movie)
        {
            return new MovieResource { Title = movie.Title, ImageUrl = movie.ImageUrl, Year = movie.Year, Genre = MapToGrpcGenre(movie.Genre) };
        }

        private static Movie MapToMovie(MovieResource movieResource)
        {
            return new Movie(
                title: movieResource.Title,
                imageUrl: movieResource.ImageUrl,
                genre: (Genre)movieResource.Genre,
                year: movieResource.Year);
        }

        private static GrpcGenre MapToGrpcGenre(Genre genre)
        {
            switch (genre)
            {
                case Genre.Action:
                    return GrpcGenre.Action;
                case Genre.Comedy:
                    return GrpcGenre.Comedy;
                case Genre.Drama:
                    return GrpcGenre.Drama;
                case Genre.SciFi:
                    return GrpcGenre.Scifi;
                case Genre.Thriller:
                    return GrpcGenre.Thriller;
                default:
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Unable to map DomainLayer.Managers.Enums.Genre: {genre}, to Grpc.Enums.Genre"));
            }
        }

        private Exception MapToRcpException(Exception e)
        {
            var metadata = new Metadata();
            Status status;

            var movieServiceBaseException = e as MovieServiceBaseException;
            if (movieServiceBaseException != null)
            {
                metadata.Add("reason", movieServiceBaseException.Reason);
                status = new Status(StatusCode.InvalidArgument, e.Message);
            }
            else
            {
                metadata.Add("reason", "Unexpected Exception");
                status = new Status(StatusCode.Internal, e.Message);
            }

            metadata.Add("exception-type", e.GetType().Name);
            return new RpcException(status, metadata);
        }
    }
}
