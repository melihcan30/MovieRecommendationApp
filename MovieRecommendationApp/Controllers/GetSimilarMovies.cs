using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MovieRecommendationApp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SimilarMoviesController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private const string OMDbApiBaseUrl = "http://www.omdbapi.com/";
		private const string OMDbApiKey = ""; // OMDb API anahtarınızı buraya ekleyin

		public SimilarMoviesController()
		{
			_httpClient = new HttpClient();
		}

		[HttpGet]
		public async Task<IActionResult> GetSimilarMovies(string movieTitle)
		{
			try
			{
				var movieDetails = await GetMovieDetails(movieTitle);
				if (movieDetails == null)
				{
					return NotFound("Film bulunamadı.");
				}

				var similarMovies = await GetSimilarMoviesByGenre(movieDetails.Genre);
				if (similarMovies == null || !similarMovies.Any())
				{
					return NotFound("Benzer film bulunamadı.");
				}

				return Ok(similarMovies);
			}
			catch (Exception ex)
			{
				// Hata yönetimi burada yapılabilir
				return StatusCode(500, "Bir hata oluştu.");
			}
		}

		private async Task<MovieDetails> GetMovieDetails(string movieTitle)
		{
			var apiUrl = $"{OMDbApiBaseUrl}?apikey={OMDbApiKey}&t={movieTitle}";
			var response = await _httpClient.GetAsync(apiUrl);
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsStringAsync();
			var movieDetails = JsonConvert.DeserializeObject<MovieDetails>(content);
			return movieDetails;
		}

		private async Task<List<Movie>> GetSimilarMoviesByGenre(string genre)
		{
			var apiUrl = $"{OMDbApiBaseUrl}?apikey={OMDbApiKey}&s=&type=movie&genre={genre}";
			var response = await _httpClient.GetAsync(apiUrl);
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsStringAsync();
			var searchResponse = JsonConvert.DeserializeObject<SearchResponse>(content);
			return searchResponse?.Search;
		}
	}

	public class MovieDetails
	{
		public string Title { get; set; }
		public string Genre { get; set; }
	}

	public class SearchResponse
	{
		public List<Movie> Search { get; set; }
	}

	public class Movie
	{
		public string Title { get; set; }
		public string Year { get; set; }
		public string ImdbID { get; set; }
		public string Type { get; set; }
		public string Poster { get; set; }
	}
}
