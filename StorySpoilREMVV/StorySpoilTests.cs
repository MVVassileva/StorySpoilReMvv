using System;
using System.Net;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoilReMvv.Models;

namespace StorySpoilREMVV
{
    [TestFixture]
    public class StorySpoilTests
    {
        private RestClient client;
        private static string? lastCreatedStorySpoilerId;

        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIyYTVmZjhiMy0wODEyLTQyNTYtODkzOC05Mzc0ZDAxMWRhYTEiLCJpYXQiOiIwOC8xNi8yMDI1IDA4OjI1OjQ5IiwiVXNlcklkIjoiNTA3ZDQxNjUtYzFiMy00MzU0LThlMjEtMDhkZGRiMWExM2YzIiwiRW1haWwiOiJyZWdleGFtbXZ2QGVtYWlsLmNvbSIsIlVzZXJOYW1lIjoicmVnRXhhbU12diIsImV4cCI6MTc1NTM1NDM0OSwiaXNzIjoiU3RvcnlTcG9pbF9BcHBfU29mdFVuaSIsImF1ZCI6IlN0b3J5U3BvaWxfV2ViQVBJX1NvZnRVbmkifQ.YqykKbTwIDCWEMbyR6HqW7a65o3joMTsO0r7rWbr244";

        private const string loginUserName = "regExamMvv";
        private const string LoginPassword = "strongPass1234!";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(loginUserName, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken),
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string userName, string password)
        {
            var tempCLient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });

            var response = tempCLient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Failed to retrieve JWT token from the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Content: {response.Content}");
            }
        }


        //All Tests Here

        [Test, Order (1)]
        public void Test_CreateANewStorySpoiler_SuccessfullyCreated()
        {
            //Arrange
            var newStorySpoiler = new StoryDTO
            {
                Title = "New Story Spoiler Created",
                Description = "Testing a new story spoiler creation.",
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStorySpoiler);
            var response = this.client.Execute(request);
            
            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createdResponse.StoryId, Is.Not.Empty, "The created story spoiler has a StoryId.");
            Assert.That(createdResponse.Msg, Is.EqualTo("Successfully created!"));

            lastCreatedStorySpoilerId = createdResponse.StoryId;
        }

        [Test, Order(2)]
        public void Test_EditAStorySpoilerById_SuccessfullyEdited()
        {
            //Arrange
            var editStrorySpoiler = new StoryDTO
            {
                Title = "Edit the last created story spoiler",
                Description = "Testing editing the last created strory spoiler by ID.",
                Url = ""
            };

            //Act
            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStorySpoilerId}", Method.Put);
            request.AddJsonBody(editStrorySpoiler);
            var response = this.client.Execute(request);

            var editedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editedResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void Test_GetAllStories_Success()
        {
            //Arrange
            //Act
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = this.client.Execute(request);

            var responseStories = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseStories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void Test_DeleteStorySpoilerById_SuccessfullyDeleted()
        {
            //Arrange
            //Act
            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStorySpoilerId}", Method.Delete);
            var response = this.client.Execute(request);

            var deletedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deletedResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void Test_CreatingAStoryWithMissingFields_BadRequest()
        {
            //Arrange
            var missingFieldsStrory = new StoryDTO
            {
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(missingFieldsStrory);
            var response = this.client.Execute(request);
            
            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void Test_EditANonExistingStorySpoilerById_NotFound()
        {
            //Arrange
            var editStrorySpoiler = new StoryDTO
            {
                Title = "Edit a non-existing story spoiler",
                Description = "Testing editing a non-existing strory spoiler by ID.",
                Url = ""
            };

            //Act
            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStorySpoilerId}", Method.Put);
            request.AddJsonBody(editStrorySpoiler);
            var response = this.client.Execute(request);

            var editedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(editedResponse.Msg, Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void Test_DeleteANonExistingStorySpoilerById_BadRequest()
        {
            //Arrange
            //Act
            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStorySpoilerId}", Method.Delete);
            var response = this.client.Execute(request);

            var deletedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deletedResponse.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            this.client?.Dispose();
        }

    }
}