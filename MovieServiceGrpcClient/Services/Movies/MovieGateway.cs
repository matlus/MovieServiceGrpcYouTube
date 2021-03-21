using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Grpc.Services.MovieService;
using MovieResource = Grpc.Services.Movie;
using Movie = DomainLayer.Managers.Models.Movie;
using Genre = DomainLayer.Managers.Enums.Genre;
using GrpcGenre = Grpc.Enums.Genre;
using MovieServiceGrpcClient.DomainLayer.Managers.Exceptions;

namespace MovieServiceGrpcClient.Services.Movies
{
    internal sealed class MovieGateway : IDisposable
    {
        private bool _disposed = false;
        private readonly GrpcChannel _grpcChannel;
        private readonly MovieServiceClient _movieServiceClient;
        public MovieGateway()
        {
            _grpcChannel = GrpcChannel.ForAddress("https://localhost:5001");
            _movieServiceClient = new MovieServiceClient(_grpcChannel);
        }

        public async Task<IEnumerable<Movie>> GetAllMovies()
        {
            var movieResponse = await _movieServiceClient.GetAllMoviesAsync(new Empty());
            return MapToMovies(movieResponse.Movies);
        }

        public async IAsyncEnumerable<Movie> GetAllMoviesStream()
        {
            var asyncServerStreamingCaller = _movieServiceClient.GetAllMoviesStream(new Empty());
            await foreach (var movieResource in asyncServerStreamingCaller.ResponseStream.ReadAllAsync())
            {
                yield return MapToMovie(movieResource);
            }
        }

        public async Task<IEnumerable<Movie>> GetMoviesByGenre(Genre genre)
        {
            var movieResponse = await _movieServiceClient.GetMoviesByGenreAsync(MapToMoviesByGenreRequest(genre));
            return MapToMovies(movieResponse.Movies);
        }

        private MoviesByGenreRequest MapToMoviesByGenreRequest(Genre genre)
        {
            GrpcGenre grpcGenre;

            switch (genre)
            {
                case Genre.Action:
                    grpcGenre = GrpcGenre.Action;
                    break;
                case Genre.Comedy:
                    grpcGenre = GrpcGenre.Comedy;
                    break;
                case Genre.Drama:
                    grpcGenre = GrpcGenre.Drama;
                    break;
                case Genre.SciFi:
                    grpcGenre = GrpcGenre.Scifi;
                    break;
                case Genre.Thriller:
                    grpcGenre = GrpcGenre.Thriller;
                    break;
                default:
                    throw new InvalidGenreException($"The Genre: {genre}, can not be mapped to a GrpcGenre");
            }

            return new MoviesByGenreRequest { Genre = grpcGenre };
        }

        public async Task<int> CreateMovie(Movie movie)
        {
            try
            {
                var createMovieResponse = await _movieServiceClient.CreateMovieAsync(MapToMovieResource(movie));
                return createMovieResponse.Id;
            }
            catch (RpcException e)
            {
                throw MapToDomainException(e);
            }
        }

        private static IEnumerable<Movie> MapToMovies(IEnumerable<MovieResource> movieResources)
        {
            foreach (var movieResource in movieResources)
            {
                yield return MapToMovie(movieResource);
            }
        }

        private static Movie MapToMovie(MovieResource movieResource)
        {
            return new Movie(
                title: movieResource.Title,
                imageUrl: movieResource.ImageUrl,
                genre: (Genre)movieResource.Genre,
                year: movieResource.Year);
        }

        private static MovieResource MapToMovieResource(Movie movie)
        {
            return new MovieResource()
            {
                Title = movie.Title,
                ImageUrl = movie.ImageUrl,
                Genre = (GrpcGenre)movie.Genre,
                Year = movie.Year
            };
        }

        private Exception MapToDomainException(RpcException e)
        {
            var reason = e.Trailers.GetValue("reason");
            var exceptionType = e.Trailers.GetValue("exception-type");

            if (e.Status.StatusCode == StatusCode.Internal)
            {
                return new Exception($"{reason}, with message: {e.Status.Detail}");
            }
            else
            {
                switch (exceptionType)
                {
                    case "InvalidMovieException":
                        return new InvalidMovieException(e.Status.Detail);
                    default:
                        return new Exception($"An Unexpected Exception for thrown while making a Grpc call to the Movie Service, with Message: {e.Status.Detail}");
                }                
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _grpcChannel?.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
