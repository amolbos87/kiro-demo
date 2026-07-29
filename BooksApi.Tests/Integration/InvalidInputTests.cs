using System;
using System.Threading.Tasks;
using BooksApi.DTOs;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace BooksApi.Tests.Integration;

public class InvalidInputTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InvalidInputTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Feature: books-api, Property 2: Invalid input is always rejected
    [Property]
    public async Property InvalidTitle_IsRejected()
    {
        // Generate a CreateBookDto with empty Title
        var gen = from title in Arb.Generate<string>().Where(s => string.IsNullOrWhiteSpace(s))
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 300)
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 50000)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre
                  };

        await Property.ForAll(gen)
            .Until(dto => !ModelStateHelper.IsValid(dto))
            .Step((dto, _) =>
            {
                var client = _factory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync("/api/books", content).Result;

                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            })
            .QuickCheckThrowOnFailure();
    }

    // Feature: books-api, Property 2: Invalid input is always rejected
    [Property]
    public async Property InvalidAuthor_IsRejected()
    {
        var gen = from title in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 500)
                  from author in Arb.Generate<string>().Where(s => string.IsNullOrWhiteSpace(s))
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 50000)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre
                  };

        await Property.ForAll(gen)
            .Until(dto => !ModelStateHelper.IsValid(dto))
            .Step((dto, _) =>
            {
                var client = _factory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync("/api/books", content).Result;

                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            })
            .QuickCheckThrowOnFailure();
    }

    // Feature: books-api, Property 2: Invalid input is always rejected
    [Property]
    public async Property InvalidPages_IsRejected()
    {
        var gen = from title in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 500)
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 300)
                  from pages in Arb.Generate<int>().Where(p => p < 1 || p > 50000)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre
                  };

        await Property.ForAll(gen)
            .Until(dto => !ModelStateHelper.IsValid(dto))
            .Step((dto, _) =>
            {
                var client = _factory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync("/api/books", content).Result;

                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            })
            .QuickCheckThrowOnFailure();
    }

    // Feature: books-api, Property 2: Invalid input is always rejected
    [Property]
    public async Property EmptyLanguage_IsRejected()
    {
        var gen = from title in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 500)
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 300)
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 50000)
                  from language in Arb.Generate<string>().Where(s => string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre
                  };

        await Property.ForAll(gen)
            .Until(dto => !ModelStateHelper.IsValid(dto))
            .Step((dto, _) =>
            {
                var client = _factory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync("/api/books", content).Result;

                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            })
            .QuickCheckThrowOnFailure();
    }

    // Feature: books-api, Property 2: Invalid input is always rejected
    [Property]
    public async Property EmptyGenre_IsRejected()
    {
        var gen = from title in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 500)
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 300)
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 50000)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => string.IsNullOrWhiteSpace(s))
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre
                  };

        await Property.ForAll(gen)
            .Until(dto => !ModelStateHelper.IsValid(dto))
            .Step((dto, _) =>
            {
                var client = _factory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync("/api/books", content).Result;

                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            })
            .QuickCheckThrowOnFailure();
    }

    // Feature: books-api, Property 2: Invalid input is always rejected
    [Property]
    public async Property TitleExceedsMaxLength_IsRejected()
    {
        var gen = from title in Arb.Generate<string>().Where(s => s.Length > 500)
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 300)
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 50000)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre
                  };

        await Property.ForAll(gen)
            .Until(dto => !ModelStateHelper.IsValid(dto))
            .Step((dto, _) =>
            {
                var client = _factory.CreateClient();
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync("/api/books", content).Result;

                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            })
            .QuickCheckThrowOnFailure();
    }
}

public static class ModelStateHelper
{
    public static bool IsValid<T>(T dto)
    {
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
        var validationResults = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, validationContext, validationResults, true);
        return isValid;
    }
}
