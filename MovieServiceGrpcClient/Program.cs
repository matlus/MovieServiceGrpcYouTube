using DomainLayer.Managers.Enums;
using DomainLayer.Managers.Models;
using MovieServiceGrpcClient.Services.Movies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieServiceGrpcClient
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            using var movieGateway = new MovieGateway();

            await GetAllMovies(movieGateway);
            await GetAllMoviesStream(movieGateway);
            await GetMoviesByGenre (movieGateway);
            await CreateMovie (movieGateway);

            Console.ReadKey();
        }

        private static async Task GetAllMovies(MovieGateway movieGateway)
        {
            Console.WriteLine("\r\nGetAllMovies\r\n");
            var movies = await movieGateway.GetAllMovies();
            PrintMovies(movies);
            Console.WriteLine("\r\n___________________________________________\r\n");
        }

        private static async Task GetAllMoviesStream(MovieGateway movieGateway)
        {
            Console.WriteLine("\r\nGetAllMoviesStream\r\n");
            var asyncEnumerableMovies = movieGateway.GetAllMoviesStream();
            await PrintMovies(asyncEnumerableMovies);
            Console.WriteLine("\r\n___________________________________________\r\n");
        }

        private static async Task GetMoviesByGenre(MovieGateway movieGateway)
        {
            Console.WriteLine("\r\nGetMoviesByGenre\r\n");
            var movies = await movieGateway.GetMoviesByGenre(Genre.SciFi);
            PrintMovies(movies);
            Console.WriteLine("\r\n___________________________________________\r\n");
        }

        private static async Task CreateMovie(MovieGateway movieGateway)
        {
            Console.WriteLine("\r\nCreateMovie\r\n");
            var newGuid = Guid.NewGuid().ToString("N");
            var id = await movieGateway.CreateMovie(
                new Movie(
                    title: $"Title_{newGuid}",
                    imageUrl: $"http://www.fromgrpc.com/images/{newGuid}",
                    genre: Genre.Drama,
                    year: 2021));

            Console.WriteLine($"Id for newly created Movie: {id}");
            Console.WriteLine("\r\n___________________________________________\r\n");
        }

        private static void PrintMovies(IEnumerable<Movie> movies)
        {
            foreach (var movie in movies)
            {
                PrintMovie(movie);
            }
        }
        
        private static async Task PrintMovies(IAsyncEnumerable<Movie> asyncEnumerableMovies)
        {
            var artificialLimit = 0;

            await foreach (var movie in asyncEnumerableMovies)
            {
                PrintMovie(movie);
                artificialLimit++;
                if (artificialLimit == 9) break;
            }
        }

        private static void PrintMovie(Movie movie) => Console.WriteLine($"Title: {movie.Title}\tYear: {movie.Year}\t Image Url: {movie.ImageUrl}\tGenre: {movie.Genre}");
    }
}
